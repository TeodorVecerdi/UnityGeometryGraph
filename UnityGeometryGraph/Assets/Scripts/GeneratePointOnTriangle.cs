using Sirenix.OdinInspector;
using UnityCommons;
using UnityEditor;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class GeneratePointOnTriangle : MonoBehaviour {
        [SerializeField] private bool manual;
        [SerializeField, OnValueChanged(nameof(CalculatePoint)), EnableIf(nameof(manual))] private float a;
        [SerializeField, OnValueChanged(nameof(CalculatePoint)), EnableIf(nameof(manual))] private float b;

        [SerializeField, OnValueChanged(nameof(UpdateTriangleArea))] private Vector3 p0;
        [SerializeField, OnValueChanged(nameof(UpdateTriangleArea))] private Vector3 p1;
        [SerializeField, OnValueChanged(nameof(UpdateTriangleArea))] private Vector3 p2;
        [SerializeField, ReadOnly] private Vector3 p;
        [SerializeField, ReadOnly] private float area;

        [Space]
        [SerializeField] private float gizmoSize = 0.1f;

        [Button]
        private void Generate() {
            a = Rand.Float;
            b = Rand.Float;

            if (a + b >= 1) {
                a = 1 - a;
                b = 1 - b;
            }

            CalculatePoint();
        }

        private void CalculatePoint() {
            Debug.Log("Calculated point");

            p = p0 + a * (p1 - p0) + b * (p2 - p0);
        }

        private void UpdateTriangleArea() {
            Vector3 v1 = p1 - p0;
            Vector3 v2 = p2 - p0;
            area = 0.5f * Vector3.Cross(v1, v2).magnitude;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Handles.matrix = Gizmos.matrix = transform.localToWorldMatrix;

            Handles.color = Color.yellow;
            Handles.DrawAAConvexPolygon(p0, p1, p2);

            float gizmoSize = HandleUtility.GetHandleSize(transform.position) * this.gizmoSize;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(p0, gizmoSize);
            Gizmos.DrawSphere(p1, gizmoSize);
            Gizmos.DrawSphere(p2, gizmoSize);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(p, gizmoSize);
        }
#endif
    }
}