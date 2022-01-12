using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class CubePrimitiveNode {
        [In] public float3 Size { get; private set; } = float3_ext.one;
        [Out] public GeometryData Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true)]
        private GeometryData GetResult() {
            if (Result == null) CalculateResult();
            return Result;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            Result = GeometryPrimitive.Cube(Size);
        }
    }
}