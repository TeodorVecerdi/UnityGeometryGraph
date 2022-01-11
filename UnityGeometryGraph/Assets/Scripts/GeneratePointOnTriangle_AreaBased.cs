using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityCommons;
using UnityEditor;
using UnityEngine;

namespace GeometryGraph.Runtime.Curve.TEMP {
    public class GeneratePointOnTriangle_AreaBased : MonoBehaviour {
        [SerializeField, OnValueChanged(nameof(Generate))] private float multiplier = 1.0f;
        [SerializeField, OnValueChanged(nameof(UpdatePoints))] private Vector3 p0;
        [SerializeField, OnValueChanged(nameof(UpdatePoints))] private Vector3 p1;
        [SerializeField, OnValueChanged(nameof(UpdatePoints))] private Vector3 p2;
        [SerializeField, ReadOnly] private float area;

        [SerializeField, ListDrawerSettings(IsReadOnly = true), ReadOnly] private List<Vector3> points = new();

        [Space]
        [SerializeField] private float gizmoSize = 0.1f;

        [Button]
        private void Generate() {
            int pointsCount = Mathf.CeilToInt(area * multiplier);
            points.Clear();

            for (int i = 0; i < pointsCount; i++) {
                points.Add(GeneratePoint());
            }
        }

        private void UpdatePoints() {
            Vector3 v1 = p1 - p0;
            Vector3 v2 = p2 - p0;
            area = 0.5f * Vector3.Cross(v1, v2).magnitude;

            Generate();
        }

        private Vector3 GeneratePoint() {
            float a = Rand.Float;
            float b = Rand.Float;

            if (a + b >= 1) {
                a = 1 - a;
                b = 1 - b;
            }

            return p0 + a * (p1 - p0) + b * (p2 - p0);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Handles.matrix = Gizmos.matrix = transform.localToWorldMatrix;

            Handles.color = Color.yellow;
            Handles.DrawAAConvexPolygon(p0, p1, p2);
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(p0, gizmoSize);
            Gizmos.DrawSphere(p1, gizmoSize);
            Gizmos.DrawSphere(p2, gizmoSize);

            Gizmos.color = Color.red;
            foreach (Vector3 point in points) {
                Gizmos.DrawSphere(point, gizmoSize);
            }
        }
#endif
    }
}