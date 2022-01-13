using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class RecalculateCurveDirectionsNode {
        [In(
            DefaultValue = "CurveData.Empty",
            GetValueCode = "{self} = GetValue(connection, {default})",
            UpdateValueCode = ""
        )]
        public CurveData Curve { get; private set; }

        [Out] public CurveData Result { get; private set; }

        [Setting] public bool FlipTangents { get; private set; }
        [Setting] public bool FlipNormals { get; private set; }
        [Setting] public bool FlipBinormals { get; private set; }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port == CurvePort) {
                Curve = CurveData.Empty;
                Result = CurveData.Empty;
            }
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            if (Curve == null) {
                Result = CurveData.Empty;
                return;
            }

            Result = CurveOperations.RecalculateDirectionVectors(Curve, new RecalculateCurveDirectionsSettings(FlipTangents, FlipNormals, FlipBinormals));
        }
    }
}