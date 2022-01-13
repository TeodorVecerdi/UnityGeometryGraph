using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;

namespace GeometryGraph.Runtime.Graph {
    public partial class RecalculateCurveDirectionsNode {
        [In(
            DefaultValue = "(CurveData)null",
            GetValueCode = "{self} = GetValue(connection, {default})",
            UpdateValueCode = ""
        )]
        public CurveData Curve { get; private set; }
        [Out] public CurveData Result { get; private set; }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            // TODO: Implement
        }
    }
}