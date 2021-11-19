using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettings(OutputPath = "_Generated")]
    public partial class CubePrimitiveNode {
        [In] public float3 Size { get; private set; } = float3_ext.one;
        [Out] public GeometryData Result { get; private set; }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            Result = GeometryPrimitive.Cube(Size);
        }
    }
}