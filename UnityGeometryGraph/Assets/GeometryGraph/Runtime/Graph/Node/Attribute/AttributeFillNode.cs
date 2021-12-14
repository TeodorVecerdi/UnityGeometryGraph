using System;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class AttributeFillNode {
        [In] public GeometryData Geometry { get; private set; }
        [In] public string Attribute { get; private set; }
        [In] public float Float { get; private set; }
        [In] public int Integer { get; private set; }
        [In] public float3 Vector { get; private set; }
        [In] public bool Boolean { get; private set; }
        [Out] public GeometryData Result { get; private set; }
        [Setting] public AttributeFillNode_Domain Domain { get; private set; } = AttributeFillNode_Domain.Auto;
        [Setting] public AttributeFillNode_Type Type { get; private set; } = AttributeFillNode_Type.Float;

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
                AttributeFillNode_Domain.Auto => Result.GetAttribute(Attribute)?.Domain ?? AttributeDomain.Vertex,
                AttributeFillNode_Domain.Vertex => AttributeDomain.Vertex,
                AttributeFillNode_Domain.Edge => AttributeDomain.Edge,
                AttributeFillNode_Domain.Face => AttributeDomain.Face,
                AttributeFillNode_Domain.FaceCorner => AttributeDomain.FaceCorner,
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

            switch (Type) {
                case AttributeFillNode_Type.Float: Result.StoreAttribute(GetValues(FloatPort, count, Float).Into<FloatAttribute>(Attribute, targetDomain), targetDomain); break;
                case AttributeFillNode_Type.Integer: Result.StoreAttribute(GetValues(IntegerPort, count, Integer).Into<IntAttribute>(Attribute, targetDomain), targetDomain); break;
                case AttributeFillNode_Type.Vector: Result.StoreAttribute(GetValues(VectorPort, count, Vector).Into<Vector3Attribute>(Attribute, targetDomain), targetDomain); break;
                case AttributeFillNode_Type.Boolean: Result.StoreAttribute(GetValues(BooleanPort, count, Boolean).Into<BoolAttribute>(Attribute, targetDomain), targetDomain); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public enum AttributeFillNode_Type {
            Float,
            Integer,
            Vector,
            Boolean,
        }

        public enum AttributeFillNode_Domain {
            Auto,
            Vertex,
            Edge,
            Face,
            FaceCorner
        }
    }
}