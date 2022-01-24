using GeometryGraph.Runtime.Curve;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class DemoVideoCurves : MonoBehaviour {
        public CurveVisualizer CurveVisualizer;
        public bool DrawCubicBezierGizmos = true;
        public bool DrawBezierGizmos = true;
        public float BezierGizmoThickness = 4.0f;
        public float BezierGizmoSize = 0.2f;
        public Vector3 BezierStart;
        public Vector3 BezierEnd;
        public Vector3 BezierControlPointA;
        public Vector3 BezierControlPointB;

        [Button]
        public void GenerateLine(Vector3 start, Vector3 end, int resolution) {
            DrawCubicBezierGizmos = false;
            DrawBezierGizmos = false;
            CurveData line = CurvePrimitive.Line(resolution, start, end);
            CurveVisualizer.Load(line);
        }

        [Button]
        public void GenerateCircle(float radius, int resolution) {
            DrawCubicBezierGizmos = false;
            DrawBezierGizmos = false;
            CurveData circle = CurvePrimitive.Circle(resolution, radius);
            CurveVisualizer.Load(circle);
        }

        [Button]
        public void GenerateQuadraticBezier(Vector3 start, Vector3 controlPoint, Vector3 end, int resolution) {
            DrawCubicBezierGizmos = false;
            DrawBezierGizmos = true;
            BezierStart = start;
            BezierControlPointA = controlPoint;
            BezierEnd = end;
            CurveData bezier = CurvePrimitive.QuadraticBezier(resolution, false, start, controlPoint, end);
            CurveVisualizer.Load(bezier);
        }

        [Button]
        public void GenerateCubicBezier(Vector3 start, Vector3 controlPointA, Vector3 controlPointB, Vector3 end, int resolution) {
            DrawCubicBezierGizmos = true;
            DrawBezierGizmos = false;

            BezierStart = start;
            BezierControlPointA = controlPointA;
            BezierControlPointB = controlPointB;
            BezierEnd = end;

            CurveData bezier = CurvePrimitive.CubicBezier(resolution, false, start, controlPointA, controlPointB, end);
            CurveVisualizer.Load(bezier);
        }

        [Button]
        public void GenerateHelix(float radius, float pitch, float rotations, int resolution) {
            DrawCubicBezierGizmos = false;
            DrawBezierGizmos = false;
            CurveData helix = CurvePrimitive.Helix(resolution, rotations, pitch, radius, radius);
            CurveVisualizer.Load(helix);
        }

        public void Clear() {
            DrawCubicBezierGizmos = false;
            DrawBezierGizmos = false;
            CurveVisualizer.Load(null);
        }
        private void OnDrawGizmos() {
            if (DrawBezierGizmos) {
                Handles.color = Color.yellow;
                Handles.matrix = transform.localToWorldMatrix;
                Handles.DrawAAPolyLine(BezierGizmoThickness, BezierStart, BezierControlPointA, BezierEnd);
                Handles.matrix = Matrix4x4.identity;

                Gizmos.color = Color.cyan;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawSphere(BezierStart, BezierGizmoSize);
                Gizmos.DrawSphere(BezierControlPointA, BezierGizmoSize);
                Gizmos.DrawSphere(BezierEnd, BezierGizmoSize);
                Gizmos.matrix = Matrix4x4.identity;
            }

            if (DrawCubicBezierGizmos) {
                Handles.color = Color.yellow;
                Handles.matrix = transform.localToWorldMatrix;
                Handles.DrawAAPolyLine(BezierGizmoThickness, BezierStart, BezierControlPointA, BezierControlPointB, BezierEnd);
                Handles.matrix = Matrix4x4.identity;

                Gizmos.color = Color.cyan;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawSphere(BezierStart, BezierGizmoSize);
                Gizmos.DrawSphere(BezierControlPointA, BezierGizmoSize);
                Gizmos.DrawSphere(BezierControlPointB, BezierGizmoSize);
                Gizmos.DrawSphere(BezierEnd, BezierGizmoSize);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }
}