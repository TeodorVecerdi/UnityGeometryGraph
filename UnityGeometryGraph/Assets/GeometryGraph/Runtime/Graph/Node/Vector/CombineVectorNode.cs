using GeometryGraph.Runtime.Attributes;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class CombineVectorNode {
        [In] public float X { get; private set; }
        [In] public float Y { get; private set; }
        [In] public float Z { get; private set; }
        [Out] public float3 Vector { get; private set; }

        [CalculatesProperty(nameof(Vector))]
        private void CalculateVector() {
            Vector = new float3(X, Y, Z);
        }
    }
}