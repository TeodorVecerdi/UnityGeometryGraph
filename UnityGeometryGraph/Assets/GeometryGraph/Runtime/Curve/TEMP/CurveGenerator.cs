using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Curve.TEMP {
    public class CurveGenerator : MonoBehaviour {
        public CurveType Type;
        [MinValue(nameof(__MinResolution))] public int Resolution;
        [ShowIf("@Type == CurveType.CubicBezier || Type == CurveType.QuadraticBezier || Type == CurveType.Line")] public Vector3 Start;
        [ShowIf("@Type == CurveType.CubicBezier || Type == CurveType.QuadraticBezier || Type == CurveType.Line")] public Vector3 End;
        [ShowIf("@Type == CurveType.CubicBezier || Type == CurveType.QuadraticBezier")] public Vector3 ControlA;
        [ShowIf("@Type == CurveType.CubicBezier")] public Vector3 ControlB;
        [ShowIf("@Type == CurveType.Helix || Type == CurveType.Circle")] public float Radius;
        [ShowIf("@Type == CurveType.Helix")] public float TopRadius;
        [ShowIf("@Type == CurveType.Helix")] public float Rotations;
        [ShowIf("@Type == CurveType.Helix")] public float Pitch;

        public CurveData CurveData;


        private int __MinResolution() {
            return Type switch {
                CurveType.Line => Constants.MIN_LINE_CURVE_RESOLUTION,
                CurveType.Circle => Constants.MIN_CIRCLE_CURVE_RESOLUTION,
                CurveType.QuadraticBezier => Constants.MIN_BEZIER_CURVE_RESOLUTION,
                CurveType.CubicBezier => Constants.MIN_BEZIER_CURVE_RESOLUTION,
                CurveType.Helix => Constants.MIN_HELIX_CURVE_RESOLUTION,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        [Button]
        public void Generate() {
            CurveData = Type switch {
                CurveType.Line => CurvePrimitive.Line(Resolution, Start, End),
                CurveType.Circle => CurvePrimitive.Circle(Resolution, Radius),
                CurveType.QuadraticBezier => CurvePrimitive.QuadraticBezier(Resolution, false, Start, ControlA, End),
                CurveType.CubicBezier => CurvePrimitive.CubicBezier(Resolution, false, Start, ControlA, ControlB, End),
                CurveType.Helix => CurvePrimitive.Helix(Resolution, Rotations, Pitch, TopRadius, Radius),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}