using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class GridNode {
        [In] public float Width { get; private set; } = 1.0f;
        [In] public float Height { get; private set; } = 1.0f;
        [In] public int PointsX { get; private set; } = 4;
        [In] public int PointsY { get; private set; } = 4;
        [Out] public GeometryData Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true)]
        private GeometryData GetResult() {
            if (Result == null) CalculateResult();
            return Result;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            Result = GeometryGrid.Make(new float2(Width, Height), PointsX, PointsY);
        }
    }
}