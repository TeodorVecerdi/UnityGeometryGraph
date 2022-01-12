using GeometryGraph.Runtime.Curve.Primitive;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Curve {
    public static class CurvePrimitive {
        public static CurveData Line(int resolution, float3 start, float3 end) {
            resolution = resolution.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION);

            LineCurve lineCurve = new(resolution, start, end);
            lineCurve.Generate();
            return lineCurve.ToCurveData();
        }

        public static CurveData Circle(int resolution, float radius) {
            resolution = resolution.Clamped(Constants.MIN_CIRCLE_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION);
            radius = radius.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS);

            CircleCurve circleCurve = new(resolution, radius);
            circleCurve.Generate();
            return circleCurve.ToCurveData();
        }

        public static CurveData QuadraticBezier(int resolution, bool isClosed, float3 start, float3 control, float3 end) {
            resolution = resolution.Clamped(Constants.MIN_BEZIER_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION);

            QuadraticBezierCurve quadraticBezierCurve = new(resolution, isClosed, start, control, end);
            quadraticBezierCurve.Generate();
            return quadraticBezierCurve.ToCurveData();
        }

        public static CurveData CubicBezier(int resolution, bool isClosed, float3 start, float3 controlA, float3 controlB, float3 end) {
            resolution = resolution.Clamped(Constants.MIN_BEZIER_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION);

            CubicBezierCurve cubicBezierCurve = new(resolution, isClosed, start, controlA, controlB, end);
            cubicBezierCurve.Generate();
            return cubicBezierCurve.ToCurveData();
        }

        public static CurveData Helix(int resolution, float rotations, float pitch, float topRadius, float bottomRadius) {
            resolution = resolution.Clamped(Constants.MIN_BEZIER_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION);

            HelixCurve helixCurve = new(resolution, rotations, pitch, topRadius, bottomRadius);
            helixCurve.Generate();
            return helixCurve.ToCurveData();
        }
    }
}