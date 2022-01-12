using System;

namespace GeometryGraph.Runtime.Curve {
    public enum CurveType {
        None = 0,
        Line,
        Circle,
        QuadraticBezier,
        CubicBezier,
        Helix,
        Unknown,
    }

    public static class CurveTypeUtilities {
        public static int MinPoints(CurveType curveType) {
            return curveType switch {
                CurveType.None => 0,
                CurveType.Line => 2,
                CurveType.Circle => 3,
                CurveType.QuadraticBezier => 2,
                CurveType.CubicBezier => 2,
                CurveType.Helix => 2,
                CurveType.Unknown => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(curveType), curveType, null)
            };
        }
    }
}