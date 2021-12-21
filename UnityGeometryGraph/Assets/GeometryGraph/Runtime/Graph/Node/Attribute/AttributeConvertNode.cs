using System;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;

using TargetDomain = GeometryGraph.Runtime.Graph.AttributeConvertNode.AttributeConvertNode_Domain;
using TargetType = GeometryGraph.Runtime.Graph.AttributeConvertNode.AttributeConvertNode_Type;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class AttributeConvertNode {
        [In] public GeometryData Geometry { get; private set; }
        [In] public string Attribute { get; private set; }
        [In] public string ResultAttribute { get; private set; }
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

            if (string.IsNullOrWhiteSpace(ResultAttribute) || string.IsNullOrWhiteSpace(Attribute)) {
                return;
            }

            AttributeDomain targetDomain;
            if (Domain == TargetDomain.Auto) {
                if (Geometry.GetAttribute(ResultAttribute) is { } resultAttribute) {
                    targetDomain = resultAttribute.Domain;
                } else if (Geometry.GetAttribute(Attribute) is { } attribute) {
                    targetDomain = attribute.Domain;
                } else {
                    targetDomain = AttributeDomain.Vertex;
                }
            } else {
                targetDomain = Domain switch {
                    AttributeConvertNode_Domain.Vertex => AttributeDomain.Vertex,
                    AttributeConvertNode_Domain.Edge => AttributeDomain.Edge,
                    AttributeConvertNode_Domain.Face => AttributeDomain.Face,
                    AttributeConvertNode_Domain.FaceCorner => AttributeDomain.FaceCorner,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            
            AttributeType targetType;
            if (Type == TargetType.Auto) {
                if (Geometry.GetAttribute(ResultAttribute) is { } resultAttribute) {
                    targetType = resultAttribute.Type;
                } else if (Geometry.GetAttribute(Attribute) is { } attribute) {
                    targetType = attribute.Type;
                } else {
                    targetType = AttributeType.Float;
                }
            } else {
                targetType = Type switch {
                    AttributeConvertNode_Type.Float => AttributeType.Float,
                    AttributeConvertNode_Type.Integer => AttributeType.Integer,
                    AttributeConvertNode_Type.Vector => AttributeType.Vector3,
                    AttributeConvertNode_Type.Boolean => AttributeType.Boolean,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            Result.RemoveAttribute(ResultAttribute);
            Result.StoreAttribute(Result.GetAttribute(Attribute).Into(ResultAttribute, targetType, targetDomain), targetDomain);
        }
        
        public enum AttributeConvertNode_Domain {
            Auto,
            Vertex,
            Edge,
            Face,
            FaceCorner
        }
        
        public enum AttributeConvertNode_Type {
            Auto,
            Float,
            Integer,
            Vector,
            Boolean
        }
    }
}