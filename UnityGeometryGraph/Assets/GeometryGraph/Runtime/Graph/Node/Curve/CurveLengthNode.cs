using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class CurveLengthNode {
        [In] public CurveData Curve { get; private set; }
        [Out] public float Length { get; private set; }

        [CalculatesProperty(nameof(Length))]
        private void Calculate() {
            if (Curve == null || Curve.Points == 0) {
                Length = 0;
                return;
            }
            
            float length = 0f;
            for (int i = 0; i < Curve.Points - 1; i++) {
                length += math.distance(Curve.Position[i], Curve.Position[i + 1]);
            }
            Length = length;
        }
    }
}