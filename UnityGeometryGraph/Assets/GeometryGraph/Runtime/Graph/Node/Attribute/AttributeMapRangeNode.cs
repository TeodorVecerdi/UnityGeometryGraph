using System;
using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;
using UnityCommons;
using TargetDomain = GeometryGraph.Runtime.Graph.AttributeMapRangeNode.AttributeMapRangeNode_Domain;
using TargetType = GeometryGraph.Runtime.Graph.AttributeMapRangeNode.AttributeMapRangeNode_Type;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class AttributeMapRangeNode {
        [In] public GeometryData Geometry { get; private set; }
        [In] public string Attribute { get; private set; }
        [In] public string ResultAttribute { get; private set; }
        [In] public int FromMinInt { get; private set; } = 0;
        [In] public int FromMaxInt { get; private set; } = 100;
        [In] public int ToMinInt { get; private set; } = 0;
        [In] public int ToMaxInt { get; private set; } = 100;
        [In] public float FromMinFloat { get; private set; } = 0.0f;
        [In] public float FromMaxFloat { get; private set; } = 1.0f;
        [In] public float ToMinFloat { get; private set; } = 0.0f;
        [In] public float ToMaxFloat { get; private set; } = 1.0f;
        [In] public float3 FromMinVector { get; private set; } = float3.zero;
        [In] public float3 FromMaxVector { get; private set; } = float3_ext.one;
        [In] public float3 ToMinVector { get; private set; } = float3.zero;
        [In] public float3 ToMaxVector { get; private set; } = float3_ext.one;
        [Setting] public TargetDomain Domain { get; private set; } = TargetDomain.Auto;
        [Setting] public TargetType Type { get; private set; } = TargetType.Auto;
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
            if (Geometry == null) {
                Result = GeometryData.Empty;
                return;
            }

            Result = Geometry.Clone();

            if (string.IsNullOrWhiteSpace(Attribute) || string.IsNullOrWhiteSpace(ResultAttribute) || !Geometry.HasAttribute(Attribute)) {
                return;
            }
            
            AttributeDomain targetDomain = Domain switch {
                TargetDomain.Auto => Result.GetAttribute(Attribute)!.Domain,
                TargetDomain.Vertex => AttributeDomain.Vertex,
                TargetDomain.Edge => AttributeDomain.Edge,
                TargetDomain.Face => AttributeDomain.Face,
                TargetDomain.FaceCorner => AttributeDomain.FaceCorner,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            AttributeType targetType = Type switch {
                TargetType.Auto => Result.GetAttribute(Attribute)!.Type,
                TargetType.Float => AttributeType.Float,
                TargetType.Integer => AttributeType.Integer,
                TargetType.Vector => AttributeType.Vector3,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            switch (targetType) {
                case AttributeType.Float: {
                    FloatAttribute attribute = Result.GetAttribute<FloatAttribute>(Attribute, targetDomain);
                    Result.StoreAttribute(attribute!.Yield(value => value.Map(FromMinFloat, FromMaxFloat, ToMinFloat, ToMaxFloat)).Into<FloatAttribute>(ResultAttribute, targetDomain));
                    break;
                }
                case AttributeType.Integer: {
                    IntAttribute attribute = Result.GetAttribute<IntAttribute>(Attribute, targetDomain);
                    Result.StoreAttribute(attribute!.Yield(value => value.Map(FromMinInt, FromMaxInt, ToMinInt, ToMaxInt)).Into<IntAttribute>(ResultAttribute, targetDomain));
                    break;
                }
                case AttributeType.Vector3: {
                    Vector3Attribute attribute = Result.GetAttribute<Vector3Attribute>(Attribute, targetDomain);
                    Result.StoreAttribute(attribute!.Yield(value => math.remap(value, FromMinVector, FromMaxVector, ToMinVector, ToMaxVector)).Into<Vector3Attribute>(ResultAttribute, targetDomain));
                    break;
                }
                case AttributeType.ClampedFloat: {
                    ClampedFloatAttribute attribute = Result.GetAttribute<ClampedFloatAttribute>(Attribute, targetDomain);
                    Result.StoreAttribute(attribute!.Yield(value => value.Map(FromMinFloat, FromMaxFloat, ToMinFloat, ToMaxFloat)).Into<ClampedFloatAttribute>(ResultAttribute, targetDomain));
                    break;
                }
                case AttributeType.Vector2: {
                    float2 fromMin = new float2(FromMinFloat, FromMinFloat);
                    float2 fromMax = new float2(FromMaxFloat, FromMaxFloat);
                    float2 toMin = new float2(ToMinFloat, ToMinFloat);
                    float2 toMax = new float2(ToMaxFloat, ToMaxFloat);
                    
                    Vector2Attribute attribute = Result.GetAttribute<Vector2Attribute>(Attribute, targetDomain);
                    Result.StoreAttribute(attribute!.Yield(value => math.remap(value, fromMin, fromMax, toMin, toMax)).Into<Vector2Attribute>(ResultAttribute, targetDomain));
                    break;
                }
                case AttributeType.Boolean: {
                    // Just copy attribute to target
                    BoolAttribute attribute = Result.GetAttribute<BoolAttribute>(Attribute, targetDomain);
                    Result.StoreAttribute(((IEnumerable<bool>)attribute!).Into<BoolAttribute>(ResultAttribute, targetDomain));
                    break;
                }
                    
                case AttributeType.Invalid: break;
                default: throw new ArgumentOutOfRangeException();
            }

        }

        public enum AttributeMapRangeNode_Type {
            Auto,
            Integer,
            Float,
            Vector
        }

        public enum AttributeMapRangeNode_Domain {
            Auto,
            Vertex,
            Edge,
            Face,
            FaceCorner,
        }
    }
}