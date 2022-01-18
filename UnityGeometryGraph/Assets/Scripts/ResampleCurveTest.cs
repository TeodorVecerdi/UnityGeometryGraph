using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Curve;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class ResampleCurveTest : MonoBehaviour {
        [Title("Curve Settings")]
        [OnValueChanged(nameof(GenerateCurve)), SerializeField, MinValue(Constants.MIN_CIRCLE_CURVE_RESOLUTION), MaxValue(Constants.MAX_CURVE_RESOLUTION)]
        private int points = 32;
        [Space]
        [SerializeField, OnValueChanged(nameof(GenerateCurve))]
        private bool generateLine = false;
        [OnValueChanged(nameof(GenerateCurve)), SerializeField, MinValue(0.01f), HideIf(nameof(generateLine))]
        private float radius = 1.0f;
        [OnValueChanged(nameof(GenerateCurve)), SerializeField, HideIf(nameof(generateLine))]
        private bool closeCircle = true;
        [SerializeField, ShowIf(nameof(generateLine)), OnValueChanged(nameof(GenerateCurve))]
        private float3 start;
        [SerializeField, ShowIf(nameof(generateLine)), OnValueChanged(nameof(GenerateCurve))]
        private float3 end;
        [Space]
        [OnValueChanged(nameof(GenerateCurve)), SerializeField] private float noiseScale = 0.25f;
        [OnValueChanged(nameof(GenerateCurve)), SerializeField] private float3 noiseOffset = float3.zero;
        [OnValueChanged(nameof(GenerateCurve)), SerializeField] private float3 noiseMultiplier = float3_ext.one;
        [OnValueChanged(nameof(GenerateCurve)), SerializeField] private float noiseFrequency = 1.0f;
        [OnValueChanged(nameof(GenerateCurve)), SerializeField] private float noisePersistence = 0.25f;
        [OnValueChanged(nameof(GenerateCurve)), SerializeField, MinValue(1), MaxValue(Constants.MAX_NOISE_OCTAVES)] private int noiseOctaves = 4;
        [OnValueChanged(nameof(GenerateCurve)), SerializeField] private float noiseAmount = 0.5f;

        [Title("Resample Settings")]
        [SerializeField, OnValueChanged(nameof(Resample))] private bool autoResample = true;
        [SerializeField, OnValueChanged(nameof(Resample))] private bool resampleUsingPointCount = false;

        // Resample by Point Count
        [SerializeField, ShowIf(nameof(resampleUsingPointCount)), OnValueChanged(nameof(Resample)), MinValue(Constants.MIN_CIRCLE_CURVE_RESOLUTION), MaxValue(Constants.MAX_CURVE_RESOLUTION)]
        private int resamplePointCount = 32;

        // Resample by Distance
        [SerializeField, HideIf(nameof(resampleUsingPointCount)), OnValueChanged(nameof(Resample))] private float resampleDistance = 0.1f;

        [Title("References")]
        [SerializeField, InlineButton(nameof(ToggleVisualizer), "Toggle")] private CurveVisualizer visualizer;
        [SerializeField, InlineButton(nameof(ToggleResampledVisualizer), "Toggle")] private CurveVisualizer resampledVisualizer;

        [Title("Stats", bold: false)]
        [SerializeField, DisplayAsString] private float curveLength;
        [ShowInInspector] private int ResampledPointsByDistance => Mathf.FloorToInt(curveLength / resampleDistance) - (curve.IsClosed ? 0 : 1);
        [ShowInInspector] private float ActualResampleDistance => curveLength / ResampledPointsByDistance;
        [ShowInInspector] private float ResampleDistanceForPoints => curveLength / (resamplePointCount - (curve.IsClosed ? 0 : 1));

        [SerializeField, HideInInspector] private CurveData curve;
        [SerializeField, HideInInspector] private CurveData resampled;

        [Button]
        private void GenerateCurve() {
            CurveData tempCurve = generateLine ? CurvePrimitive.Line(points - 1, start, end) : CurvePrimitive.Circle(points, radius);
            List<float3> positions = new();
            for (int i = 0; i < tempCurve.Points; i++) {
                float3 position = tempCurve.Position[i];
                float3 samplePosition = position + noiseOffset;
                Noise.Simplex3X3(ref samplePosition, out float3 noise, noiseScale, noiseOctaves, noiseFrequency, noisePersistence);
                position += noiseMultiplier * (noise * noiseAmount);
                positions.Add(position);
            }

            curve = new CurveData(tempCurve.Type, tempCurve.Points, closeCircle, positions, tempCurve.Tangent.ToList(), tempCurve.Normal.ToList(), tempCurve.Binormal.ToList());
            curveLength = CurveOperations.CalculateLength(curve);

            if (autoResample) {
                Resample();
            }

            if (visualizer != null) {
                visualizer.Load(curve);
            }
        }

        [Button]
        private void Resample() {
            if (resampleUsingPointCount) {
                resampled = CurveOperations.ResampleCurveByPoints(curve, resamplePointCount);
            } else {
                resampled = CurveOperations.ResampleCurveByDistance(curve, resampleDistance);
            }

            if (resampled != null && resampledVisualizer != null) {
                resampledVisualizer.Load(resampled);
            }
        }

        [Button]
        private void CheckDistance() {
            if (resampled == null) {
                Debug.Log("Resampled curve is null");
                return;
            }

            List<float3> positions = resampled.Position.ToList();
            int mismatchCount = 0;
            double deltaSum = 0.0;
            float targetDistance = resampleUsingPointCount ? ResampleDistanceForPoints : ActualResampleDistance;

            for (int i = 0; i < positions.Count - 1; i++) {
                float3 a = positions[i];
                float3 b = positions[i + 1];
                double distance = math.distance((double3)a, b);

                double delta = math.abs(distance - targetDistance);
                if (delta > Constants.FLOAT_TOLERANCE) {
                    mismatchCount++;
                    deltaSum += delta;
                }
            }

            if (resampled.IsClosed) {
                double distance = math.distance((double3)positions[0], positions[^1]);
                double delta = math.abs(distance - targetDistance);
                if (delta > Constants.FLOAT_TOLERANCE) {
                    mismatchCount++;
                    deltaSum += delta;
                }
            }

            double averageDelta = mismatchCount == 0 ? 0.0 : deltaSum / mismatchCount;
            Debug.Log($"Distance mismatch count: {mismatchCount}/{positions.Count} ({(double)mismatchCount / positions.Count:P2}) // Average delta: {averageDelta:F8}");
        }

        private void ResampleUsingPoints() {
            List<float> distances = new() { 0 };

            float d = 0.0f;
            for (int i = 1; i < curve.Points; i++) {
                d += math.distance(curve.Position[i - 1], curve.Position[i]);
                distances.Add(d);
            }

            if (curve.IsClosed) {
                d += math.distance(curve.Position[^1], curve.Position[0]);
                distances.Add(d);
            }

            float resampleDistance = ResampleDistanceForPoints;
            float currentDistance = 0.0f;

            List<float3> positions = new();
            List<float3> tangents = new();
            List<float3> normals = new();
            List<float3> binormals = new();

            for (int i = 0; i < resamplePointCount; i++) {
                int closestPointIndex = ClosestIndexByDistance(currentDistance, distances);
                if (i == resamplePointCount - 1) {
                    Debug.Log($"Last index: Closest: {closestPointIndex}, current distance: {currentDistance}");
                }

                if (closestPointIndex + 1 >= curve.Points) {
                    float distance = math.distance(curve.Position[^1], curve.Position[0]);
                    float totalDistanceWithoutLast = distances[^1] - distance;
                    float newCurrentDistance = currentDistance - totalDistanceWithoutLast;

                    float t = newCurrentDistance / distance;
                    float3 position = math.lerp(curve.Position[^1], curve.Position[0], t);
                    float3 tangent = math.lerp(curve.Tangent[^1], curve.Tangent[0], t);
                    float3 normal = math.lerp(curve.Normal[^1], curve.Normal[0], t);
                    float3 binormal = math.lerp(curve.Binormal[^1], curve.Binormal[0], t);

                    positions.Add(position);
                    tangents.Add(tangent);
                    normals.Add(normal);
                    binormals.Add(binormal);
                } else {
                    float3 start = curve.Position[closestPointIndex];
                    float3 end = curve.Position[closestPointIndex + 1];

                    float t = (currentDistance - distances[closestPointIndex]) / (distances[closestPointIndex + 1] - distances[closestPointIndex]);
                    float3 position = math.lerp(start, end, t);
                    float3 tangent = math.lerp(curve.Tangent[closestPointIndex], curve.Tangent[closestPointIndex + 1], t);
                    float3 normal = math.lerp(curve.Normal[closestPointIndex], curve.Normal[closestPointIndex + 1], t);
                    float3 binormal = math.lerp(curve.Binormal[closestPointIndex], curve.Binormal[closestPointIndex + 1], t);

                    positions.Add(position);
                    tangents.Add(tangent);
                    normals.Add(normal);
                    binormals.Add(binormal);
                }
                currentDistance += resampleDistance;
            }

            resampled = new CurveData(curve.Type, positions.Count, curve.IsClosed, positions, tangents, normals, binormals);
        }

        private void ResampleUsingDistance() {
            List<float> distances = new() { 0 };

            float d = 0.0f;
            for (int i = 1; i < curve.Points; i++) {
                d += math.distance(curve.Position[i - 1], curve.Position[i]);
                distances.Add(d);
            }

            if (curve.IsClosed) {
                d += math.distance(curve.Position[^1], curve.Position[0]);
                distances.Add(d);
            }

            float resampleDistance = ActualResampleDistance;
            float currentDistance = 0.0f;
            int resamplePoints = ResampledPointsByDistance;

            List<float3> positions = new();
            List<float3> tangents = new();
            List<float3> normals = new();
            List<float3> binormals = new();

            for (int i = 0; i < resamplePoints; i++) {
                int closestPointIndex = ClosestIndexByDistance(currentDistance, distances);
                if (closestPointIndex + 1 >= curve.Points) {
                    float distance = math.distance(curve.Position[^1], curve.Position[0]);
                    float totalDistanceWithoutLast = distances[^1] - distance;
                    float newCurrentDistance = currentDistance - totalDistanceWithoutLast;

                    float t = newCurrentDistance / distance;
                    float3 position = math.lerp(curve.Position[^1], curve.Position[0], t);
                    float3 tangent = math.lerp(curve.Tangent[^1], curve.Tangent[0], t);
                    float3 normal = math.lerp(curve.Normal[^1], curve.Normal[0], t);
                    float3 binormal = math.lerp(curve.Binormal[^1], curve.Binormal[0], t);

                    positions.Add(position);
                    tangents.Add(tangent);
                    normals.Add(normal);
                    binormals.Add(binormal);
                } else {
                    float3 start = curve.Position[closestPointIndex];
                    float3 end = curve.Position[closestPointIndex + 1];

                    float t = (currentDistance - distances[closestPointIndex]) / (distances[closestPointIndex + 1] - distances[closestPointIndex]);
                    float3 position = math.lerp(start, end, t);
                    float3 tangent = math.lerp(curve.Tangent[closestPointIndex], curve.Tangent[closestPointIndex + 1], t);
                    float3 normal = math.lerp(curve.Normal[closestPointIndex], curve.Normal[closestPointIndex + 1], t);
                    float3 binormal = math.lerp(curve.Binormal[closestPointIndex], curve.Binormal[closestPointIndex + 1], t);

                    positions.Add(position);
                    tangents.Add(tangent);
                    normals.Add(normal);
                    binormals.Add(binormal);
                }
                currentDistance += resampleDistance;
            }

            resampled = new CurveData(curve.Type, positions.Count, curve.IsClosed, positions, tangents, normals, binormals);
        }

        private int ClosestIndexByDistance(float distance, List<float> sortedDistances) {
            int index = 0;

            for (int i = 0; i < sortedDistances.Count; i++) {
                if (sortedDistances[i] > distance) {
                    break;
                }

                index = i;
            }

            if (index == sortedDistances.Count - 1 && Math.Abs(sortedDistances[^1] - distance) < Constants.FLOAT_TOLERANCE) {
                return index - 1;
            }

            return index;
        }

        private void ToggleVisualizer() => visualizer.Enabled = !visualizer.Enabled;
        private void ToggleResampledVisualizer() => resampledVisualizer.Enabled = !resampledVisualizer.Enabled;
    }
}