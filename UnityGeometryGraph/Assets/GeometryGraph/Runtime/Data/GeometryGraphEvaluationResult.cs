using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Data {
    public struct GeometryGraphEvaluationResult {
        private readonly GeometryData geometryData;
        private readonly CurveData curveData;

        public GeometryData GeometryData => geometryData;
        public CurveData CurveData => curveData;

        public GeometryGraphEvaluationResult(GeometryData geometryData, CurveData curveData) {
            this.geometryData = geometryData;
            this.curveData = curveData;
        }

        public static GeometryGraphEvaluationResult Empty = new ();
    }
}