using System.Collections.Generic;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Curve {
    public static partial class CurveOperations {
        #region API

        /// <summary>
        /// Calculates the (discrete/evaluated) length of the <paramref name="curve"/>
        /// </summary>
        public static float CalculateLength(CurveData curve) => CalculateLengthImpl(curve);

        /// <summary>
        /// Calculates the cumulative distance of each point on the <paramref name="curve"/>
        /// </summary>
        public static float[] CalculateCumulativeDistances(CurveData curve) => CalculateCumulativeDistancesImpl(curve);

        #endregion

        private static float CalculateLengthImpl(CurveData curveData) {
            float length = 0.0f;
            IReadOnlyList<float3> positions = curveData.Position;

            for (int i = 0; i < curveData.Points - 1; i++) {
                length += math.distance(positions[i], positions[i + 1]);
            }

            if (curveData.IsClosed) {
                length += math.distance(positions[^1], positions[0]);
            }

            return length;
        }

        private static float[] CalculateCumulativeDistancesImpl(CurveData curveData) {
            float[] distances = new float[curveData.Points + (curveData.IsClosed ? 1 : 0)];
            IReadOnlyList<float3> positions = curveData.Position;

            distances[0] = 0.0f;
            float currentDistance = 0.0f;

            for (int i = 1; i < curveData.Points; i++) {
                currentDistance += math.distance(positions[i - 1], positions[i]);
                distances[i] = currentDistance;
            }

            if (curveData.IsClosed) {
                currentDistance += math.distance(positions[^1], positions[0]);
                distances[^1] = currentDistance;
            }

            return distances;
        }
    }
}