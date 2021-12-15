using System;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class AttributeRemoveNode {
        [In] public GeometryData Geometry { get; private set; }
        [In] public string Attribute { get; private set; }
        [Setting] public AttributeRemoveNode_Domain Domain { get; private set; } = AttributeRemoveNode_Domain.All;
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

            AttributeDomain concreteDomain = Domain switch {
                AttributeRemoveNode_Domain.All => AttributeDomain.Vertex,
                AttributeRemoveNode_Domain.Vertex => AttributeDomain.Vertex,
                AttributeRemoveNode_Domain.Edge => AttributeDomain.Edge,
                AttributeRemoveNode_Domain.Face => AttributeDomain.Face,
                AttributeRemoveNode_Domain.FaceCorner => AttributeDomain.FaceCorner,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (string.IsNullOrWhiteSpace(Attribute)
             || AttributeId.BuiltinIds.Contains(Attribute)
             || Domain is not AttributeRemoveNode_Domain.All && !Result.HasAttribute(Attribute, concreteDomain)
             || Domain is AttributeRemoveNode_Domain.All && !Result.HasAttribute(Attribute)
            ) {
                return;
            }
            if (Domain is AttributeRemoveNode_Domain.All) {
                Result.RemoveAttribute(Attribute);
            } else {
                Result.RemoveAttribute(Attribute, concreteDomain);
            }
        }

        public enum AttributeRemoveNode_Domain {
            All,
            Vertex,
            Edge,
            Face,
            FaceCorner,
        }
    }
}