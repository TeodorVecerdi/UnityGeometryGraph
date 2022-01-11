using Unity.Mathematics;

namespace GeometryGraph.Runtime.Curve {
    public static class CurveLength {
        public static float Calculate(CurveData curve) {
            float length = 0.0f;
            for (int i = 0; i < curve.Points - 1; i++) {
                length += math.distance(curve.Position[i], curve.Position[i + 1]);
            }

            if (curve.IsClosed) {
                length += math.distance(curve.Position[curve.Points - 1], curve.Position[0]);
            }

            return length;
        }
    }
}