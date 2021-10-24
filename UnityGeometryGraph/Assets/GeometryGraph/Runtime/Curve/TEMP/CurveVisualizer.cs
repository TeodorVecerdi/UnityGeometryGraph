using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GeometryGraph.Runtime.Curve.TEMP {
    public class CurveVisualizer : MonoBehaviour {
        /*
         * TODO: Move into GeometryGraph MonoBehaviour and integrate it into the custom editor.  
         */
        public bool ShowSpline = true;
        [ShowIf(nameof(ShowSpline))] public float SplineWidth = 2.0f;
        [ShowIf(nameof(ShowSpline))] public Color SplineColor = Color.white;
        [Space] public bool ShowPoints = true;
        [ShowIf(nameof(ShowPoints))] public float PointSize = 0.01f;
        [ShowIf(nameof(ShowPoints))] public Color PointColor = Color.white;

        [Space] public bool ShowDirectionVectors = true;
        [ShowIf(nameof(ShowDirectionVectors))] public float DirectionVectorLength = 0.1f;
        [ShowIf(nameof(ShowDirectionVectors))] public float DirectionVectorWidth = 2.0f;
        [ShowIf(nameof(ShowDirectionVectors))] public Color DirectionTangentColor = Color.blue;
        [ShowIf(nameof(ShowDirectionVectors))] public Color DirectionNormalColor = Color.red;
        [ShowIf(nameof(ShowDirectionVectors))] public Color DirectionBinormalColor = Color.green;

        public bool Enabled;

        [ShowInInspector] private CurveData curveData;

        public void Load(CurveData curveData) {
            DebugUtility.Log("Loaded curve into visualizer");
            this.curveData = curveData;
        }

        private void OnDrawGizmos() {
            if (!Enabled || curveData == null) return;

            Handles.matrix = Gizmos.matrix = transform.localToWorldMatrix;

            if (ShowPoints || ShowSpline) {
                var points = curveData.Position.Select(float3 => (Vector3)float3).ToArray();
                Handles.color = SplineColor;
                if (ShowSpline) {
                    Handles.DrawAAPolyLine(SplineWidth, points);
                    if (curveData.IsClosed) Handles.DrawAAPolyLine(SplineWidth, points[0], points[^1]);
                }

                if (ShowPoints) {
                    Gizmos.color = PointColor;
                    foreach (var p in points) {
                        Gizmos.DrawSphere(p, PointSize);
                    }
                }
            }

            if (ShowDirectionVectors) {
                for (var i = 0; i < curveData.Position.Count; i++) {
                    var p = curveData.Position[i];
                    var t = curveData.Tangent[i];
                    var n = curveData.Normal[i];
                    var b = curveData.Binormal[i];

                    Handles.color = DirectionTangentColor;
                    Handles.DrawAAPolyLine(DirectionVectorWidth, p, p + t * DirectionVectorLength);
                    Handles.color = DirectionNormalColor;
                    Handles.DrawAAPolyLine(DirectionVectorWidth, p, p + n * DirectionVectorLength);
                    Handles.color = DirectionBinormalColor;
                    Handles.DrawAAPolyLine(DirectionVectorWidth, p, p + b * DirectionVectorLength);
                }
            }
        }
    }
}