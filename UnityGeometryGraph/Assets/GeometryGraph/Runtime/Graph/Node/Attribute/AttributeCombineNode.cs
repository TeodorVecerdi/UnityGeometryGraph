using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class AttributeCombineNode {
        [In] public GeometryData Geometry { get; private set; }
        [In] public float XFloat { get; private set; }
        [In] public float YFloat { get; private set; }
        [In] public float ZFloat { get; private set; }
        [In] public string XAttribute { get; private set; }
        [In] public string YAttribute { get; private set; }
        [In] public string ZAttribute { get; private set; }
        [In] public string ResultAttribute { get; private set; }
        [Out] public GeometryData Result { get; private set; }

        [Setting] public AttributeDomain TargetDomain { get; private set; } = AttributeDomain.Vertex;
        [Setting] public AttributeCombineNode_ComponentType XType { get; private set; } = AttributeCombineNode_ComponentType.Attribute;
        [Setting] public AttributeCombineNode_ComponentType YType { get; private set; } = AttributeCombineNode_ComponentType.Attribute;
        [Setting] public AttributeCombineNode_ComponentType ZType { get; private set; } = AttributeCombineNode_ComponentType.Attribute;

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            if (Geometry == null || string.IsNullOrWhiteSpace(ResultAttribute)) {
                Result = GeometryData.Empty;
                return;
            }
            
            IEnumerable<float> xValues = null;
            IEnumerable<float> yValues = null;
            IEnumerable<float> zValues = null;
            AttributeDomain? domain = null;
            
            if (XType is AttributeCombineNode_ComponentType.Attribute && !string.IsNullOrWhiteSpace(XAttribute)) {
                FloatAttribute xAttr = Geometry.GetAttribute<FloatAttribute>(XAttribute);
                if (xAttr != null) {
                    domain = xAttr.Domain;
                    xValues = xAttr;
                }
            }
            
            if (YType is AttributeCombineNode_ComponentType.Attribute && !string.IsNullOrWhiteSpace(YAttribute)) {
                FloatAttribute yAttr = Geometry.GetAttribute<FloatAttribute>(YAttribute);
                if (yAttr != null) {
                    domain = yAttr.Domain;
                    yValues = yAttr;
                }
            }
            
            if (ZType is AttributeCombineNode_ComponentType.Attribute && !string.IsNullOrWhiteSpace(ZAttribute)) {
                FloatAttribute zAttr = Geometry.GetAttribute<FloatAttribute>(ZAttribute);
                if (zAttr != null) {
                    domain = zAttr.Domain;
                    zValues = zAttr;
                }
            }
            
            domain ??= TargetDomain;
            
            int count = domain switch {
                AttributeDomain.Vertex => Geometry.Vertices.Count,
                AttributeDomain.Edge => Geometry.Edges.Count,
                AttributeDomain.Face => Geometry.Faces.Count,
                AttributeDomain.FaceCorner => Geometry.FaceCorners.Count,
                _ => throw new ArgumentOutOfRangeException()
            };

            xValues ??= XType is AttributeCombineNode_ComponentType.Float ? GetValues(XFloatPort, count, XFloat) : Enumerable.Repeat(0.0f, count);
            yValues ??= YType is AttributeCombineNode_ComponentType.Float ? GetValues(YFloatPort, count, YFloat) : Enumerable.Repeat(0.0f, count);
            zValues ??= ZType is AttributeCombineNode_ComponentType.Float ? GetValues(ZFloatPort, count, ZFloat) : Enumerable.Repeat(0.0f, count);

            Vector3Attribute result = xValues.Zip(yValues, (x, y) => (x, y)).Zip(zValues, (xy, z) => new float3(xy.x, xy.y, z)).Into<Vector3Attribute>(ResultAttribute, domain.Value);
            Result = Geometry.Clone();
            Result.StoreAttribute(result, TargetDomain);
        }

        public enum AttributeCombineNode_ComponentType {
            Float,
            Attribute
        }
    }
}