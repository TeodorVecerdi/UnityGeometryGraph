using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Curve.TEMP;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class ResampleCurveTest : MonoBehaviour {
        [Title("Curve Settings")]
        [OnValueChanged(nameof(GenerateCurve)), SerializeField, MinValue(Constants.MIN_CIRCLE_CURVE_RESOLUTION), MaxValue(Constants.MAX_CURVE_RESOLUTION)]
        private int points = 32;
        [OnValueChanged(nameof(GenerateCurve)), SerializeField, MinValue(0.01f)] private float radius = 1.0f;
        [Space]
        [SerializeField] private bool generateLine = false;
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
        [SerializeField, Button] private bool autoResample = true;
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
        [ShowInInspector] private int ResampledPointsByDistance => Mathf.FloorToInt(curveLength / resampleDistance);
        [ShowInInspector] private float ActualResampleDistance => curveLength / ResampledPointsByDistance;

        [SerializeField, HideInInspector] private CurveData curve;
        [SerializeField, HideInInspector] private CurveData resampled;

        [Button]
        private void GenerateCurve() {
            CurveData tempCurve = generateLine ? CurvePrimitive.Line(points, start, end) : CurvePrimitive.Circle(points, radius);
            List<float3> positions = new();
            for (int i = 0; i < tempCurve.Points; i++) {
                float3 position = tempCurve.Position[i];
                float3 samplePosition = position + noiseOffset;
                Noise.Simplex3X3(ref samplePosition, out float3 noise, noiseScale, noiseOctaves, noiseFrequency, noisePersistence);
                position += noiseMultiplier * (noise * noiseAmount);
                positions.Add(position);
            }

            curve = new CurveData(tempCurve.Type, tempCurve.Points, tempCurve.IsClosed, positions, tempCurve.Tangent.ToList(), tempCurve.Normal.ToList(), tempCurve.Binormal.ToList());
            curveLength = CurveLength.Calculate(curve);

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
                ResampleUsingPoints();
            } else {
                ResampleUsingDistance();
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
            float firstDistance = math.distance(positions[0], positions[1]);
            for (int i = 1; i < positions.Count - 1; i++) {
                float distance = math.distance(positions[i], positions[i + 1]);

                if (Math.Abs(firstDistance - distance) > Constants.FLOAT_TOLERANCE) {
                    Debug.Log($"Distance mismatch at index {i} [{firstDistance} (ref) != {distance}]");
                }
            }

            if (resampled.IsClosed) {
                float distance = math.distance(positions[0], positions[^1]);

                if (Math.Abs(firstDistance - distance) > Constants.FLOAT_TOLERANCE) {
                    Debug.Log($"Distance mismatch at index {positions.Count - 1} [{firstDistance} (ref) != {distance}]");
                }
            }

        }

        private void ResampleUsingPoints() {
            // not implemented yet
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

                    float3 position = curve.Position[^1] + (newCurrentDistance / distance) * (curve.Position[0] - curve.Position[^1]);
                    float t = math_ext.inverse_lerp_clamped(curve.Position[^1], curve.Position[0], position);

                    float3 tangent = math.lerp(curve.Tangent[^1], curve.Tangent[0], t);
                    float3 normal = math.lerp(curve.Normal[^1], curve.Normal[0], t);
                    float3 binormal = math.cross(tangent, normal);

                    positions.Add(position);
                    tangents.Add(tangent);
                    normals.Add(normal);
                    binormals.Add(binormal);
                } else {
                    float3 start = curve.Position[closestPointIndex];
                    float3 end = curve.Position[closestPointIndex + 1];

                    float3 position = start + (currentDistance - distances[closestPointIndex]) / (distances[closestPointIndex + 1] - distances[closestPointIndex]) * (end - start);
                    float t = math_ext.inverse_lerp_clamped(start, end, position);

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
            float closestDistance = sortedDistances[0];

            for (int i = 0; i < sortedDistances.Count; i++) {
                if (sortedDistances[i] > distance) {
                    break;
                }

                closestDistance = sortedDistances[i];
                index = i;
            }

            return index;
        }

        private void ToggleVisualizer() => visualizer.Enabled = !visualizer.Enabled;
        private void ToggleResampledVisualizer() => resampledVisualizer.Enabled = !resampledVisualizer.Enabled;
    }
}