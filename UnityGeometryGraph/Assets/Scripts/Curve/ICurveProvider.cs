using GeometryGraph.Runtime.Curve;

namespace GeometryGraph.Runtime.Testing {
    public interface ICurveProvider {
        CurveData Curve { get; }
    }
}