using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettings(GenerateSerialization = false)]
    public partial class BoundingBoxNode {
        [In] public GeometryData Input { get; private set; } = GeometryData.Empty;
        [Out] public GeometryData BoundingBox { get; private set; }
        [Out] public float3 Min { get; private set; }
        [Out] public float3 Max { get; private set; }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port != InputPort) return;
            Input = GeometryData.Empty;
            Min = float3.zero;
            Max = float3.zero;
            BoundingBox = GeometryData.Empty;
        }

        [CalculatesAllProperties]
        private void CalculateResult() {
            Input ??= GeometryData.Empty;
            (Min, Max, BoundingBox) = Geometry.Geometry.BoundingBox(Input);
        }
    }
}