using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Geometry;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;

namespace Attribute {
    // Domain conversion
    internal static partial class AttributeConvert {
        internal static IEnumerable ConvertDomain(GeometryData geometry, BaseAttribute sourceAttribute, AttributeDomain to) {
            if (sourceAttribute.Domain == AttributeDomain.Spline || to == AttributeDomain.Spline) {
                Debug.LogWarning("Cannot convert from a Spline domain or into a Spline domain.");
                // Note: I use .Yield() so I don't return the attribute itself, but an actual IEnumerable over the attribute values
                // null turns the action into a NoOp
                return sourceAttribute.Yield(null);
            }

            if (sourceAttribute.Domain == to) 
                return sourceAttribute.Yield(null);

            return sourceAttribute.Domain switch {
                AttributeDomain.Vertex => to switch {
                    AttributeDomain.Edge => ConvertDomain_VertexToEdge(geometry, sourceAttribute),
                    AttributeDomain.Face => ConvertDomain_VertexToFace(geometry, sourceAttribute),
                    AttributeDomain.FaceCorner => ConvertDomain_VertexToFaceCorner(geometry, sourceAttribute),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeDomain.Edge => to switch {
                    AttributeDomain.Vertex => ConvertDomain_EdgeToVertex(geometry, sourceAttribute),
                    AttributeDomain.Face => ConvertDomain_EdgeToFace(geometry, sourceAttribute),
                    AttributeDomain.FaceCorner => ConvertDomain_EdgeToFaceCorner(geometry, sourceAttribute),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeDomain.Face => to switch {
                    AttributeDomain.Vertex => ConvertDomain_FaceToVertex(geometry, sourceAttribute),
                    AttributeDomain.Edge => ConvertDomain_FaceToEdge(geometry, sourceAttribute),
                    AttributeDomain.FaceCorner => ConvertDomain_FaceToFaceCorner(geometry, sourceAttribute),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeDomain.FaceCorner => to switch {
                    AttributeDomain.Vertex => ConvertDomain_FaceCornerToVertex(geometry, sourceAttribute),
                    AttributeDomain.Edge => ConvertDomain_FaceCornerToEdge(geometry, sourceAttribute),
                    AttributeDomain.Face => ConvertDomain_FaceCornerToFace(geometry, sourceAttribute),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(sourceAttribute.Domain), sourceAttribute.Domain, null)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IEnumerable<TValue> ConvertDomain<TAttribute, TValue>(GeometryData geometry, TAttribute sourceAttribute, AttributeDomain to) 
            where TAttribute : BaseAttribute 
        {
            // TODO: Might be worth rewriting the non-generic implementation here just to be type-safe with the .Yield(null) calls. 
            return (IEnumerable<TValue>)ConvertDomain(geometry, sourceAttribute, to);
        }

        //!! Vertex Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable ConvertDomain_VertexToEdge(GeometryData geometry, BaseAttribute sourceAttribute) {
            return geometry.Edges.Select(edge => AverageParams(sourceAttribute.Type, sourceAttribute[edge.VertA], sourceAttribute[edge.VertB]));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable ConvertDomain_VertexToFace(GeometryData geometry, BaseAttribute sourceAttribute) {
            return geometry.Faces.Select(face => AverageParams(sourceAttribute.Type, sourceAttribute[face.VertA], sourceAttribute[face.VertB], sourceAttribute[face.VertC]));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable ConvertDomain_VertexToFaceCorner(GeometryData geometry, BaseAttribute sourceAttribute) {
            return geometry.FaceCorners.Select(faceCorner => sourceAttribute[faceCorner.Vert]);
        }

        //!! Edge Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable ConvertDomain_EdgeToVertex(GeometryData geometry, BaseAttribute sourceAttribute) {
            return geometry.Vertices.Select(vertex => Average(sourceAttribute.Type, vertex.Edges.Select(sourceAttribute.GetValue)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable ConvertDomain_EdgeToFace(GeometryData geometry, BaseAttribute sourceAttribute) {
            return geometry.Faces.Select(face => AverageParams(sourceAttribute.Type, sourceAttribute[face.EdgeA], sourceAttribute[face.EdgeB], sourceAttribute[face.EdgeC]));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable ConvertDomain_EdgeToFaceCorner(GeometryData geometry, BaseAttribute sourceAttribute) {
            return geometry.FaceCorners.Select(faceCorner => Average(sourceAttribute.Type, geometry.Vertices[faceCorner.Vert].Edges.Select(sourceAttribute.GetValue)));
        }
        
        //!! Face Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable ConvertDomain_FaceToVertex(GeometryData geometry, BaseAttribute sourceAttribute) {
            return geometry.Vertices.Select(vertex => Average(sourceAttribute.Type, vertex.Faces.Select(sourceAttribute.GetValue)));
        }

        private static readonly List<int> edgeIndexList = new List<int>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable ConvertDomain_FaceToEdge(GeometryData geometry, BaseAttribute sourceAttribute) {
            return geometry.Edges.Select(edge => {
                edgeIndexList.Clear();
                edgeIndexList.Add(edge.FaceA);
                if (edge.FaceB != -1) edgeIndexList.Add(edge.FaceB);
                return Average(sourceAttribute.Type, edgeIndexList.Select(sourceAttribute.GetValue));
            });
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable ConvertDomain_FaceToFaceCorner(GeometryData geometry, BaseAttribute sourceAttribute) {
            return geometry.FaceCorners.Select(faceCorner => Average(sourceAttribute.Type, geometry.Vertices[faceCorner.Vert].Faces.Select(sourceAttribute.GetValue)));
        }
        
        //!! Face Corner Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable ConvertDomain_FaceCornerToVertex(GeometryData geometry, BaseAttribute sourceAttribute) {
            return geometry.Vertices.Select(vertex => Average(sourceAttribute.Type, vertex.FaceCorners.Select(sourceAttribute.GetValue)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable ConvertDomain_FaceCornerToEdge(GeometryData geometry, BaseAttribute sourceAttribute) {
            return geometry.Edges.Select(edge => {
                // Faces
                edgeIndexList.Clear();
                edgeIndexList.Add(edge.FaceA);
                if (edge.FaceB != -1) edgeIndexList.Add(edge.FaceB);

                return Average(
                    sourceAttribute.Type,
                    edgeIndexList
                    // Get all face corners on faces that neighbour the edge
                    .SelectMany(faceIndex => {
                        var face = geometry.Faces[faceIndex];
                        return new[] { face.FaceCornerA, face.FaceCornerB, face.FaceCornerC };
                    })
                    // Filter out any face corner that isn't on the edge
                    .Where(faceCornerIndex => {
                        var faceCorner = geometry.FaceCorners[faceCornerIndex];
                        return edge.VertA == faceCorner.Vert || edge.VertB == faceCorner.Vert;
                    })
                    // Get attribute value
                    .Select(sourceAttribute.GetValue));
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable ConvertDomain_FaceCornerToFace(GeometryData geometry, BaseAttribute sourceAttribute) {
            return geometry.Faces.Select(face => AverageParams(sourceAttribute.Type, 
                                     sourceAttribute[face.FaceCornerA], sourceAttribute[face.FaceCornerB], sourceAttribute[face.FaceCornerC]));
        }

        private static object AverageParams(AttributeType type, params object[] values) {
            return Average(type, values);
        }
        // Average functions
        private static object Average(AttributeType type, IEnumerable values) {
            return type switch {
                AttributeType.Boolean => Average(values.Convert(o => (bool)o)),
                AttributeType.Integer => Average(values.Convert(o => (int)o)),
                AttributeType.Float => Average(values.Convert(o => (float)o)),
                AttributeType.ClampedFloat => Average(values.Convert(o => (float)o)).Clamped01(),
                AttributeType.Vector2 => Average(values.Convert(o => (float2)o)),
                AttributeType.Vector3 => Average(values.Convert(o => (float3)o)),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        // Note: Maybe there is a better / more correct way to 'average' boolean values but idk
        private static bool Average(IEnumerable<bool> values) {
            var count = 0;
            return values.Count(b => {
                ++count;
                return b;
            }) < 0.5f * count;
        }

        private static int Average(IEnumerable<int> values) => (int) values.Average();
        private static float Average(IEnumerable<float> values) => values.Average();
        private static float2 Average(IEnumerable<float2> values) {
            var count = 1; // 1 because Aggregate already consumes one element before beginning
            return values.Aggregate((a, b) => {
                ++count;
                return a + b;
            }) / count;
        }

        private static float3 Average(IEnumerable<float3> values) {
            var count = 1;
            return values.Aggregate((a, b) => {
                ++count;
                return a + b;
            }) / count;
        }
    }

    // Misc
    internal static partial class AttributeConvert {
        internal static AttributeType GetType(object value) {
            var valueType = value.GetType();
            var type = typeConversionDictionary.ContainsKey(valueType) ? typeConversionDictionary[valueType] : AttributeType.Invalid;
            return type;
        }
        
        internal static bool TryGetType(object value, out AttributeType type) {
            type = GetType(value);
            return type != AttributeType.Invalid;
        }
    }
    
    // Type conversion
    internal static partial class AttributeConvert {
        // oof sorry 
        internal static T ConvertType<T>(object value, AttributeType from, AttributeType to) {
            return from switch {
                AttributeType.Boolean => to switch {
                    AttributeType.Boolean => (T)value,
                    AttributeType.Integer => (T)ConvertType_BoolInt((bool)value),
                    AttributeType.Float => (T)ConvertType_BoolFloat((bool)value),
                    AttributeType.ClampedFloat => (T)ConvertType_BoolClampedFloat((bool)value),
                    AttributeType.Vector2 => (T)ConvertType_BoolVec2((bool)value),
                    AttributeType.Vector3 => (T)ConvertType_BoolVec3((bool)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.Integer => to switch {
                    AttributeType.Boolean => (T)ConvertType_IntBool((int)value),
                    AttributeType.Integer => (T)value,
                    AttributeType.Float => (T)ConvertType_IntFloat((int)value),
                    AttributeType.ClampedFloat => (T)ConvertType_IntClampedFloat((int)value),
                    AttributeType.Vector2 => (T)ConvertType_IntVec2((int)value),
                    AttributeType.Vector3 => (T)ConvertType_IntVec3((int)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.Float => to switch {
                    AttributeType.Boolean => (T)ConvertType_FloatBool((float)value),
                    AttributeType.Integer => (T)ConvertType_FloatInt((float)value),
                    AttributeType.Float => (T)value,
                    AttributeType.ClampedFloat => (T)ConvertType_FloatClampedFloat((float)value),
                    AttributeType.Vector2 => (T)ConvertType_FloatVec2((float)value),
                    AttributeType.Vector3 => (T)ConvertType_FloatVec3((float)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.ClampedFloat => to switch {
                    AttributeType.Boolean => (T)ConvertType_ClampedFloatBool((float)value),
                    AttributeType.Integer => (T)ConvertType_ClampedFloatInt((float)value),
                    AttributeType.Float => (T)ConvertType_ClampedFloatFloat((float)value),
                    AttributeType.ClampedFloat => (T)value,
                    AttributeType.Vector2 => (T)ConvertType_ClampedFloatVec2((float)value),
                    AttributeType.Vector3 => (T)ConvertType_ClampedFloatVec3((float)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.Vector2 => to switch {
                    AttributeType.Boolean => (T)ConvertType_Vec2Bool((float2)value),
                    AttributeType.Integer => (T)ConvertType_Vec2Int((float2)value),
                    AttributeType.Float => (T)ConvertType_Vec2Float((float2)value),
                    AttributeType.ClampedFloat => (T)ConvertType_Vec2ClampedFloat((float2)value),
                    AttributeType.Vector2 => (T)value,
                    AttributeType.Vector3 => (T)ConvertType_Vec2Vec3((float2)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.Vector3 => to switch {
                    AttributeType.Boolean => (T)ConvertType_Vec3Bool((float3)value),
                    AttributeType.Integer => (T)ConvertType_Vec3Int((float3)value),
                    AttributeType.Float => (T)ConvertType_Vec3Float((float3)value),
                    AttributeType.ClampedFloat => (T)ConvertType_Vec3ClampedFloat((float3)value),
                    AttributeType.Vector2 => (T)ConvertType_Vec3Vec2((float3)value),
                    AttributeType.Vector3 => (T)value,
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
            };
        }

        // Boolean
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolInt(bool a) => a ? 1 : 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolFloat(bool a) => a ? 1.0f : 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolClampedFloat(bool a) => a ? 1.0f : 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolVec2(bool a) => a ? float2_util.one : float2.zero;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolVec3(bool a) => a ? float3_util.one : float3.zero;

        // Integer
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntBool(int a) => a != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntFloat(int a) => (float)a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntClampedFloat(int a) => (float)a.Clamped01();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntVec2(int a) => float2_util.one * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntVec3(int a) => float3_util.one * a;

        // Float
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatBool(float a) => a != 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatInt(float a) => (int)a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatClampedFloat(float a) => a.Clamped01();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatVec2(float a) => float2_util.one * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatVec3(float a) => float3_util.one * a;

        // Clamped Float
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatBool(float a) => a != 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatInt(float a) => Mathf.Approximately(a, 1.0f) ? 1 : 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatFloat(float a) => a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatVec2(float a) => float2_util.one * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatVec3(float a) => float3_util.one * a;

        // Vector2
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2Bool(float2 a) => a.x != 0.0f && a.y != 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2Int(float2 a) => (int)a.x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2Float(float2 a) => a.x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2ClampedFloat(float2 a) => a.x.Clamped01();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2Vec3(float2 a) => new float3(a, 0.0f);

        // Vector3
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3Bool(float3 a) => a.x != 0.0f && a.y != 0.0f && a.z != 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3Int(float3 a) => (int)a.x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3Float(float3 a) => a.x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3ClampedFloat(float3 a) => a.x.Clamped01();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3Vec2(float3 a) => a.xy;


        private static readonly Dictionary<Type, AttributeType> typeConversionDictionary = new Dictionary<Type, AttributeType> {
            { typeof(bool), AttributeType.Boolean },
            { typeof(int), AttributeType.Integer },
            { typeof(float), AttributeType.Float },
            { typeof(float2), AttributeType.Vector2 },
            { typeof(float3), AttributeType.Vector3 },
        };
    }
}