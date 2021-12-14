using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Data {
    public struct GeometryGraphEvaluationResult {
        private readonly GeometryData geometryData;
        private readonly CurveData curveData;
        private readonly InstancedGeometryData instancedGeometryData;

        public GeometryData GeometryData => geometryData;
        public CurveData CurveData => curveData;
        public InstancedGeometryData InstancedGeometryData => instancedGeometryData;

        public GeometryGraphEvaluationResult(GeometryData geometryData, CurveData curveData, InstancedGeometryData instancedGeometryData) {
            this.geometryData = geometryData;
            this.curveData = curveData;
            this.instancedGeometryData = instancedGeometryData;
        }

        public static GeometryGraphEvaluationResult Empty = new ();
    }
}