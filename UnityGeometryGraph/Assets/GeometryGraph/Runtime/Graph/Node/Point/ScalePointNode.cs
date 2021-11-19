using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettings(OutputPath = "_Generated")]
    public partial class ScalePointNode {
        [In] public GeometryData Input { get; private set; }
        [In] public float3 Vector { get; private set; }
        [In] public float Scalar { get; private set; }
        [In] public string AttributeName { get; private set; }
        [Setting] public ScalePointNode_Mode Mode { get; private set; }
        [Out] public GeometryData Result { get; private set; }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port == InputPort) {
                Input = GeometryData.Empty;
                Result = GeometryData.Empty;
                NotifyPortValueChanged(ResultPort);
            }
        }

        [CalculatesProperty(nameof(Result))]
        private void Calculate() {
            if (Input == null) return;
            Result = Input.Clone();
            var scaleAttr = Result.GetAttributeOrDefault<Vector3Attribute, float3>("scale", AttributeDomain.Vertex, float3_ext.one);
            
            if (Mode is ScalePointNode_Mode.Vector or ScalePointNode_Mode.Float) {
                if (Mode is ScalePointNode_Mode.Float) {
                    var multiplier = GetValues(ScalarPort, Input.Vertices.Count, Scalar).Into<FloatAttribute>("multiplier", AttributeDomain.Vertex);
                    scaleAttr.YieldWithAttribute(multiplier, (scale, scalar) => scale * scalar).Into(scaleAttr);
                    Result.StoreAttribute(scaleAttr);
                } else {
                    var multiplier = GetValues(VectorPort, Input.Vertices.Count, Vector).Into<Vector3Attribute>("multiplier", AttributeDomain.Vertex);
                    scaleAttr.YieldWithAttribute(multiplier, (scale, mul) => scale * mul).Into(scaleAttr);
                    Result.StoreAttribute(scaleAttr);
                }
                
                // var multiplier = mode == ScalePointNode_Mode.Vector ? vector : new float3(scalar);
                // scaleAttr.Yield(scale => scale * multiplier).Into(scaleAttr);
                // result.StoreAttribute(scaleAttr);
            } else {
                if (!Result.HasAttribute(AttributeName)) {
                    Debug.LogWarning($"Couldn't find attribute [{AttributeName}]");
                    return;
                }
                
                var otherAttribute = Result.GetAttribute<Vector3Attribute>(AttributeName, AttributeDomain.Vertex);
                scaleAttr.YieldWithAttribute(otherAttribute, (scale, multiplier) => scale * multiplier).Into(scaleAttr);
                Result.StoreAttribute(scaleAttr);
            }
        }
        
        public enum ScalePointNode_Mode {Vector = 0, Float = 1, Attribute = 2}
    }
}