using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Curve.TEMP;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class RecalculateCurveDirsTest : MonoBehaviour {
        [Title("Base Settings")]
        [SerializeField, OnValueChanged(nameof(GenerateCurve)), MinValue(Constants.MIN_LINE_CURVE_RESOLUTION), MaxValue(Constants.MAX_CURVE_RESOLUTION)]
        private int resolution = 32;
        [SerializeField, OnValueChanged(nameof(GenerateCurve))]
        private float3 startPoint = float3.zero;
        [SerializeField, OnValueChanged(nameof(GenerateCurve))]
        private float3 endPoint = float3_ext.right;

        [Space]
        [SerializeField, OnValueChanged(nameof(GenerateCurve))]
        private Transformation transformation = Transformation.None;
        [SerializeField, OnValueChanged(nameof(GenerateCurve)), ShowIf("@transformation == Transformation.Circle")]
        private bool closeCircle = true;
        [SerializeField, OnValueChanged(nameof(GenerateCurve)), ShowIf("@transformation == Transformation.Circle")]
        private float circleRadius = 1.0f;
        [SerializeField, OnValueChanged(nameof(GenerateCurve)), ShowIf("@transformation == Transformation.CompoundQuadKnot")]
        private float compoundQuadMultiplier = 1.0f;

        [Title("Recalculate Settings")]
        [SerializeField, OnValueChanged(nameof(Recalculate))]
        private bool autoRecalculate = true;

        [Title("References")]
        [LabelText("@GetVisualizerLabel()"), SerializeField,
         InlineButton(nameof(ToggleVisualizer), "Toggle"),
         InlineButton(nameof(ToggleVSpline), "@ToggleVSplineLabel()"),
         InlineButton(nameof(ToggleVPoints), "@ToggleVPointsLabel()"),
         InlineButton(nameof(ToggleVDirs), "@ToggleVDirsLabel()"),
        ]
        private CurveVisualizer visualizer;

        [LabelText("@GetRecalculatedVisualizerLabel()"), SerializeField,
         InlineButton(nameof(ToggleRecalculatedVisualizer), "Toggle"),
         InlineButton(nameof(ToggleRSpline), "@ToggleRSplineLabel()"),
         InlineButton(nameof(ToggleRPoints), "@ToggleRPointsLabel()"),
         InlineButton(nameof(ToggleRDirs), "@ToggleRDirsLabel()"),
        ]
        private CurveVisualizer recalculatedVisualizer;

        [SerializeField] private CurveData curve;
        [SerializeField] private CurveData recalculatedCurve;

        [Button(ButtonSizes.Large)]
        private void GenerateCurve() {
            CurveData tempCurve = CurvePrimitive.Line(resolution, startPoint, endPoint);
            curve = TransformCurve(tempCurve);

            if (visualizer != null) {
                visualizer.Load(curve);
#if UNITY_EDITOR
                UnityEditor.SceneView.RepaintAll();
#endif
            }

            if (autoRecalculate) {
                Recalculate();
            }
        }

        [Button(ButtonSizes.Large)]
        private void Recalculate() {
            if(curve == null) return;

            List<float3> position = curve.Position.ToList();
            List<float3> tangent = curve.Tangent.ToList();
            List<float3> normal = curve.Normal.ToList();
            List<float3> binormal = curve.Binormal.ToList();

            // Tangents
            for (int i = 0; i < curve.Points; i++) {
                if (i == curve.Points - 1) {
                    if (curve.IsClosed) {
                        tangent[i] = math.normalize(position[(i + 1) % curve.Points] - position[i]);
                    } else {
                        tangent[i] = tangent[i - 1];
                    }
                } else {
                    tangent[i] = math.normalize(position[i + 1] - position[i]);
                }
            }

            // Normals
            for (int i = 0; i < curve.Points; i++) {
                normal[i] = math.normalize(math.cross(binormal[i], tangent[i]));
            }

            // Binormals
            for (int i = 0; i < curve.Points; i++) {
                binormal[i] = math.normalize(math.cross(tangent[i], normal[i]));
            }

            recalculatedCurve = new CurveData(curve.Type, curve.Points, curve.IsClosed, position, tangent, normal, binormal);

            if (recalculatedVisualizer != null) {
                recalculatedVisualizer.Load(recalculatedCurve);
            }
        }

        private CurveData TransformCurve(CurveData curve) {
            switch (transformation) {
                case Transformation.None:
                    return curve.Clone();
                case Transformation.Circle: {
                    List<float3> position = curve.Position.ToList();

                    for (int i = 0; i < curve.Points; i++) {
                        float theta = i * 2.0f * math.PI / curve.Points;
                        position[i] = new float3(circleRadius * math.cos(theta), 0.0f, circleRadius * math.sin(theta));
                    }

                    List<float3> tangent = curve.Tangent.ToList();
                    List<float3> normal = curve.Normal.ToList();
                    List<float3> binormal = curve.Binormal.ToList();
                    return new CurveData(curve.Type, position.Count, closeCircle, position, tangent, normal, binormal);
                }
                case Transformation.TrefoilKnot: {
                    List<float3> position = curve.Position.ToList();

                    for (int i = 0; i < curve.Points; i++) {
                        float theta = i * 2.0f * math.PI / curve.Points;
                        float x = math.sin(theta) + 2.0f * math.sin(2.0f * theta);
                        float y = -(math.cos(theta) - 2.0f * math.cos(2.0f * theta));
                        float z = math.sin(3.0f * theta);

                        position[i] = new float3(x, y, z);
                    }

                    List<float3> tangent = curve.Tangent.ToList();
                    List<float3> normal = curve.Normal.ToList();
                    List<float3> binormal = curve.Binormal.ToList();
                    return new CurveData(curve.Type, position.Count, closeCircle, position, tangent, normal, binormal);
                }
                case Transformation.TorusKnot: {
                    List<float3> position = curve.Position.ToList();

                    for (int i = 0; i < curve.Points; i++) {
                        float theta = i * 2.0f * math.PI / curve.Points;
                        float x = math.cos(3.0f * theta) * (3.0f + math.cos(4.0f * theta));
                        float y = math.sin(3.0f * theta) * (3.0f + math.cos(4.0f * theta));
                        float z = math.sin(4.0f * theta);

                        position[i] = new float3(x, y, z);
                    }

                    List<float3> tangent = curve.Tangent.ToList();
                    List<float3> normal = curve.Normal.ToList();
                    List<float3> binormal = curve.Binormal.ToList();
                    return new CurveData(curve.Type, position.Count, closeCircle, position, tangent, normal, binormal);
                }
                case Transformation.CinquefoilKnot: {
                    List<float3> position = curve.Position.ToList();

                    for (int i = 0; i < curve.Points; i++) {
                        float theta = i * 2.0f * math.PI / curve.Points;
                        float x = math.cos(2.0f * theta) * (3.0f + math.cos(5.0f * theta));
                        float y = math.sin(2.0f * theta) * (3.0f + math.cos(5.0f * theta));
                        float z = math.sin(5.0f * theta);

                        position[i] = new float3(x, y, z);
                    }

                    List<float3> tangent = curve.Tangent.ToList();
                    List<float3> normal = curve.Normal.ToList();
                    List<float3> binormal = curve.Binormal.ToList();
                    return new CurveData(curve.Type, position.Count, closeCircle, position, tangent, normal, binormal);
                }
                case Transformation.CompoundQuadKnot: {
                    List<float3> position = curve.Position.ToList();

                    for (int i = 0; i < curve.Points; i++) {
                        float theta = i * 2.0f * math.PI / curve.Points;
                        float x = compoundQuadMultiplier * 0.6f * math.cos(theta) + compoundQuadMultiplier * 0.25f * math.cos(-3.0f * theta) - compoundQuadMultiplier * 0.26f * math.cos(9.0f * theta);
                        float y = compoundQuadMultiplier * 0.6f * math.sin(theta) + compoundQuadMultiplier * 0.25f * math.sin(-3.0f * theta) - compoundQuadMultiplier * 0.26f * math.sin(9.0f * theta);
                        float z = compoundQuadMultiplier * 0.12f * math.sin(16.0f * theta) - compoundQuadMultiplier * 0.06f * math.sin(4.0f * theta);

                        position[i] = new float3(x, y, z);
                    }

                    List<float3> tangent = curve.Tangent.ToList();
                    List<float3> normal = curve.Normal.ToList();
                    List<float3> binormal = curve.Binormal.ToList();
                    return new CurveData(curve.Type, position.Count, closeCircle, position, tangent, normal, binormal);
                }
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void ToggleVisualizer() {
            if (visualizer != null) visualizer.Enabled = !visualizer.Enabled;
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private void ToggleRecalculatedVisualizer() {
            if (recalculatedVisualizer != null) recalculatedVisualizer.Enabled = !recalculatedVisualizer.Enabled;
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private string GetVisualizerLabel() {
            return $"Visualizer ({(visualizer.Enabled ? "On" : "Off")})";
        }

        private string GetRecalculatedVisualizerLabel() {
            return $"RecalculatedVisualizer ({(recalculatedVisualizer.Enabled ? "On" : "Off")})";
        }

        private void ToggleVSpline() {
            visualizer.ShowSpline = !visualizer.ShowSpline;
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private void ToggleVPoints() {
            visualizer.ShowPoints = !visualizer.ShowPoints;
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private void ToggleVDirs() {
            visualizer.ShowDirectionVectors = !visualizer.ShowDirectionVectors;
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private void ToggleRSpline() {
            recalculatedVisualizer.ShowSpline = !recalculatedVisualizer.ShowSpline;
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private void ToggleRPoints() {
            recalculatedVisualizer.ShowPoints = !recalculatedVisualizer.ShowPoints;
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private void ToggleRDirs() {
            recalculatedVisualizer.ShowDirectionVectors = !recalculatedVisualizer.ShowDirectionVectors;
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private string ToggleVSplineLabel() {
            return $"S ({(visualizer.ShowSpline ? "+" : "-")})";
        }
        private string ToggleRSplineLabel() {
            return $"S ({(recalculatedVisualizer.ShowSpline ? "+" : "-")})";
        }
        private string ToggleVPointsLabel() {
            return $"P ({(visualizer.ShowPoints ? "+" : "-")})";
        }
        private string ToggleRPointsLabel() {
            return $"P ({(recalculatedVisualizer.ShowPoints ? "+" : "-")})";
        }
        private string ToggleVDirsLabel() {
            return $"D ({(visualizer.ShowDirectionVectors ? "+" : "-")})";
        }
        private string ToggleRDirsLabel() {
            return $"D ({(recalculatedVisualizer.ShowDirectionVectors ? "+" : "-")})";
        }

        public enum Transformation {
            None,
            Circle,
            TrefoilKnot,
            TorusKnot,
            CinquefoilKnot,
            CompoundQuadKnot,
        }
    }
}