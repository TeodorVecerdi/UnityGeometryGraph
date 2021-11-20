using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class TransformGeometryNode {
        [In(
            DefaultValue = "GeometryData.Empty",
            GetValueCode = "{self} = GetValue(connection, {default})",
            UpdateValueCode = ""
        )]
        public GeometryData Input { get; private set; }
        [In] public float3 Translation { get; private set; } = float3.zero;
        [In] public float3 Rotation { get; private set; } = float3.zero;
        [In] public float3 Scale { get; private set; } = float3_ext.one;
        [Out] public GeometryData Result { get; private set; }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port != InputPort) return;
            Input = GeometryData.Empty;
            Result = GeometryData.Empty;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            var translation = GetValue(TranslationPort, Translation);
            var rotation = GetValue(RotationPort, Rotation);
            var scale = GetValue(ScalePort, Scale);
            var rotQuaternion = quaternion.Euler(math.radians(rotation));
            var trs = float4x4.TRS(translation, rotQuaternion, scale);
            var trsNormal = float4x4.TRS(float3.zero, rotQuaternion, scale);
            Result = Input.Clone();

            var positionAttribute = Result.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            positionAttribute.Yield(pos => math.mul(trs, new float4(pos, 1.0f)).xyz).Into(positionAttribute);
            var normalAttribute = Result.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face);
            normalAttribute.Yield(normal => math.normalize(math.mul(trsNormal, new float4(normal, 1.0f)).xyz)).Into(normalAttribute);

            Result.StoreAttribute(positionAttribute, AttributeDomain.Vertex);
            Result.StoreAttribute(normalAttribute, AttributeDomain.Face);
        }
    }
}