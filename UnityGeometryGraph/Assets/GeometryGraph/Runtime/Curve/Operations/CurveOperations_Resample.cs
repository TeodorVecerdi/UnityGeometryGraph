using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;

namespace GeometryGraph.Runtime.Curve {
    public static partial class CurveOperations {
        public static CurveData ResampleCurveByDistance(CurveData originalCurve, float distance) {
            distance = distance.MinClamped(0.001f);
            float curveLength = CalculateLength(originalCurve);

            if (curveLength <= distance || math.abs(curveLength) < Constants.FLOAT_TOLERANCE) {
                return CurveData.Empty;
            }

            int resamplePoints = (Mathf.FloorToInt(curveLength / distance) - (originalCurve.IsClosed ? 0 : 1)).Clamped(CurveTypeUtilities.MinPoints(originalCurve.Type), Constants.MAX_CURVE_RESOLUTION);
            float resampleDistance = curveLength / (resamplePoints - (originalCurve.IsClosed ? 0 : 1));

            return ResampleCurveCommon(originalCurve, resamplePoints, resampleDistance);
        }

        public static CurveData ResampleCurveByPoints(CurveData originalCurve, int resamplePoints) {
            resamplePoints = resamplePoints.Clamped(CurveTypeUtilities.MinPoints(originalCurve.Type), Constants.MAX_CURVE_RESOLUTION);
            float curveLength = CalculateLength(originalCurve);

            if (math.abs(curveLength) < Constants.FLOAT_TOLERANCE) {
                return CurveData.Empty;
            }

            float resampleDistance = curveLength / (resamplePoints - (originalCurve.IsClosed ? 0 : 1));

            if (curveLength <= resampleDistance) {
                return CurveData.Empty;
            }

            return ResampleCurveCommon(originalCurve, resamplePoints, resampleDistance);
        }

        private static CurveData ResampleCurveCommon(CurveData originalCurve, int resamplePoints, float resampleDistance) {
            float[] cumulativeDistances = CalculateCumulativeDistances(originalCurve);
            float currentDistance = 0.0f;

            List<float3> positions = new(resamplePoints);
            List<float3> tangents = new(resamplePoints);
            List<float3> normals = new(resamplePoints);
            List<float3> binormals = new(resamplePoints);

            IReadOnlyList<float3> originalCurvePosition = originalCurve.Position;
            IReadOnlyList<float3> originalCurveTangent = originalCurve.Tangent;
            IReadOnlyList<float3> originalCurveNormal = originalCurve.Normal;
            IReadOnlyList<float3> originalCurveBinormal = originalCurve.Binormal;

            for (int i = 0; i < resamplePoints; i++) {
                int closestPointIndex = FindIndexOfNearestPoint(cumulativeDistances, currentDistance);
                if (closestPointIndex + 1 >= originalCurve.Points) {
                    float distanceBetweenLast = math.distance(originalCurvePosition[^1], originalCurvePosition[0]);
                    float totalDistanceWithoutLast = cumulativeDistances[^1] - distanceBetweenLast;
                    float newCurrentDistance = currentDistance - totalDistanceWithoutLast;

                    float t = newCurrentDistance / distanceBetweenLast;
                    float3 position = math.lerp(originalCurvePosition[^1], originalCurvePosition[0], t);
                    float3 tangent = math.lerp(originalCurveTangent[^1], originalCurveTangent[0], t);
                    float3 normal = math.lerp(originalCurveNormal[^1], originalCurveNormal[0], t);
                    float3 binormal = math.lerp(originalCurveBinormal[^1], originalCurveBinormal[0], t);

                    positions.Add(position);
                    tangents.Add(tangent);
                    normals.Add(normal);
                    binormals.Add(binormal);
                } else {
                    float3 start = originalCurvePosition[closestPointIndex];
                    float3 end = originalCurvePosition[closestPointIndex + 1];

                    float t = (currentDistance - cumulativeDistances[closestPointIndex]) / (cumulativeDistances[closestPointIndex + 1] - cumulativeDistances[closestPointIndex]);
                    float3 position = math.lerp(start, end, t);
                    float3 tangent = math.lerp(originalCurveTangent[closestPointIndex], originalCurveTangent[closestPointIndex + 1], t);
                    float3 normal = math.lerp(originalCurveNormal[closestPointIndex], originalCurveNormal[closestPointIndex + 1], t);
                    float3 binormal = math.lerp(originalCurveBinormal[closestPointIndex], originalCurveBinormal[closestPointIndex + 1], t);

                    positions.Add(position);
                    tangents.Add(tangent);
                    normals.Add(normal);
                    binormals.Add(binormal);
                }

                currentDistance += resampleDistance;
            }

            return new CurveData(originalCurve.Type, resamplePoints, originalCurve.IsClosed, positions, tangents, normals, binormals);
        }

        private static int FindIndexOfNearestPoint(float[] cumulativeDistances, float distance) {
            bool equalsLast = math.abs(cumulativeDistances[^1] - distance) < Constants.FLOAT_TOLERANCE;

            if (equalsLast) {
                return cumulativeDistances.Length - 2;
            }

            for(int i = 0; i < cumulativeDistances.Length; i++) {
                if (cumulativeDistances[i] > distance) {
                    if (i == 0) {
                        return 0;
                    }

                    return i - 1;
                }
            }

            return cumulativeDistances.Length - 2;
        }
    }
}