using System;
using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class AttributeRandomizeNode {
        [In] public GeometryData Geometry { get; private set; }
        [In] public string Attribute { get; private set; }
        [In] public int Seed { get; private set; }
        [In] public int MinInt { get; private set; } = 0;
        [In] public int MaxInt { get; private set; } = 100;
        [In] public float MinFloat { get; private set; } = 0.0f;
        [In] public float MaxFloat { get; private set; } = 1.0f;
        [In] public float3 MinVector { get; private set; } = float3.zero;
        [In] public float3 MaxVector { get; private set; } = float3_ext.one;
        [Setting] public AttributeRandomizeNode_Domain Domain { get; private set; } = AttributeRandomizeNode_Domain.Auto;
        [Setting] public AttributeRandomizeNode_Type Type { get; private set; } = AttributeRandomizeNode_Type.Float;
        [Out] public GeometryData Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true)]
        private GeometryData GetResult() {
            if (Result == null) {
                CalculateResult();
            }
            return Result;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            if (Geometry == null || string.IsNullOrWhiteSpace(Attribute)) {
                Result = GeometryData.Empty;
                return;
            }

            Result = Geometry.Clone();

            AttributeDomain targetDomain = Domain switch {
                AttributeRandomizeNode_Domain.Auto => Result.GetAttribute(Attribute)?.Domain ?? AttributeDomain.Vertex,
                AttributeRandomizeNode_Domain.Vertex => AttributeDomain.Vertex,
                AttributeRandomizeNode_Domain.Edge => AttributeDomain.Edge,
                AttributeRandomizeNode_Domain.Face => AttributeDomain.Face,
                AttributeRandomizeNode_Domain.FaceCorner => AttributeDomain.FaceCorner,
                _ => throw new ArgumentOutOfRangeException()
            };

            Result.RemoveAttribute(Attribute);

            int count = targetDomain switch {
                AttributeDomain.Vertex => Result.Vertices.Count,
                AttributeDomain.Edge => Result.Edges.Count,
                AttributeDomain.Face => Result.Faces.Count,
                AttributeDomain.FaceCorner => Result.FaceCorners.Count,
                _ => throw new ArgumentOutOfRangeException()
            };
            using IDisposable randState = RandPlus.PushState(Seed);
            switch (Type) {
                case AttributeRandomizeNode_Type.Float:
                    List<float> floats = new();
                    for (int i = 0; i < count; i++) {
                        floats.Add(Rand.Range(MinFloat, MaxFloat));
                    }

                    Result.StoreAttribute(floats.Into<FloatAttribute>(Attribute, targetDomain));
                    break;
                case AttributeRandomizeNode_Type.Integer:
                    List<int> integers = new();
                    for (int i = 0; i < count; i++) {
                        integers.Add(Rand.Range(MinInt, MaxInt));
                    }

                    Result.StoreAttribute(integers.Into<IntAttribute>(Attribute, targetDomain));
                    break;
                case AttributeRandomizeNode_Type.Vector:
                    List<float3> vectors = new();
                    for (int i = 0; i < count; i++) {
                        vectors.Add(RandPlus.Range(MinVector, MaxVector));
                    }

                    Result.StoreAttribute(vectors.Into<Vector3Attribute>(Attribute, targetDomain));
                    break;
                case AttributeRandomizeNode_Type.Boolean:
                    List<bool> booleans = new();
                    for (int i = 0; i < count; i++) {
                        booleans.Add(Rand.Bool);
                    }

                    Result.StoreAttribute(booleans.Into<BoolAttribute>(Attribute, targetDomain));
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public enum AttributeRandomizeNode_Domain {
            Auto,
            Vertex,
            Edge,
            Face,
            FaceCorner,
        }

        public enum AttributeRandomizeNode_Type {
            Float,
            Integer,
            Vector,
            Boolean,
        }
    }
}