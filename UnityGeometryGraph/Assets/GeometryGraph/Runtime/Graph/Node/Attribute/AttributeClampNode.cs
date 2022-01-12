using System;
using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;
using UnityCommons;
using TargetType = GeometryGraph.Runtime.Graph.AttributeClampNode.AttributeClampNode_Type;
using TargetDomain = GeometryGraph.Runtime.Graph.AttributeClampNode.AttributeClampNode_Domain;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class AttributeClampNode {
        [In] public GeometryData Geometry { get; private set; }
        [In] public string Attribute { get; private set; }
        [In] public string ResultAttribute { get; private set; }
        [In] public int MinInt { get; private set; } = 0;
        [In] public int MaxInt { get; private set; } = 100;
        [In] public float MinFloat { get; private set; } = 0.0f;
        [In] public float MaxFloat { get; private set; } = 1.0f;
        [In] public float3 MinVector { get; private set; } = float3.zero;
        [In] public float3 MaxVector { get; private set; } = float3_ext.one;
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
                    Result.StoreAttribute(attribute!.Yield(value => value.Clamped(MinFloat, MaxFloat)).Into<FloatAttribute>(ResultAttribute, targetDomain));
                    break;
                }
                case AttributeType.Integer: {
                    IntAttribute attribute = Result.GetAttribute<IntAttribute>(Attribute, targetDomain);
                    Result.StoreAttribute(attribute!.Yield(value => value.Clamped(MinInt, MaxInt)).Into<IntAttribute>(ResultAttribute, targetDomain));
                    break;
                }
                case AttributeType.Vector3: {
                    Vector3Attribute attribute = Result.GetAttribute<Vector3Attribute>(Attribute, targetDomain);
                    Result.StoreAttribute(attribute!.Yield(value => math.clamp(value, MinVector, MaxVector)).Into<Vector3Attribute>(ResultAttribute, targetDomain));
                    break;
                }
                case AttributeType.ClampedFloat: {
                    float min = math.min(math.max(MinFloat, 0.0f), 1.0f);
                    float max = math.min(math.max(MaxFloat, 0.0f), 1.0f);
                    ClampedFloatAttribute attribute = Result.GetAttribute<ClampedFloatAttribute>(Attribute, targetDomain);
                    Result.StoreAttribute(attribute!.Yield(value => value.Clamped(min, max)).Into<ClampedFloatAttribute>(ResultAttribute, targetDomain));
                    break;
                }
                case AttributeType.Vector2: {
                    float2 min = new(MinFloat, MinFloat);
                    float2 max = new(MaxFloat, MaxFloat);
                    Vector2Attribute attribute = Result.GetAttribute<Vector2Attribute>(Attribute, targetDomain);
                    Result.StoreAttribute(attribute!.Yield(value => math.clamp(value, min, max)).Into<Vector2Attribute>(ResultAttribute, targetDomain));
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

        public enum AttributeClampNode_Type {
            Auto,
            Integer,
            Float,
            Vector
        }

        public enum AttributeClampNode_Domain {
            Auto,
            Vertex,
            Edge,
            Face,
            FaceCorner,
        }
    }
}