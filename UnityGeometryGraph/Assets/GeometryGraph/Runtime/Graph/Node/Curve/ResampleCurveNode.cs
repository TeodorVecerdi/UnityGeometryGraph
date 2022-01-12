using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using UnityCommons;
using ResampleMode = GeometryGraph.Runtime.Graph.ResampleCurveNode.ResampleCurveNode_Mode;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class ResampleCurveNode {
        [In(
            DefaultValue = "CurveData.Empty",
            GetValueCode = "{self} = GetValue(connection, {default})",
            UpdateValueCode = ""
        )]
        public CurveData Curve { get; private set; }

        [In]
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(0.001f);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        public float Distance { get; private set; } = 0.1f;

        [In]
        [AdditionalValueChangedCode("{other} = {other}.Clamped(GetMinPoints(), Constants.MAX_CURVE_RESOLUTION);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        public int Points { get; private set; } = 32;

        [Setting] public ResampleMode Mode { get; private set; } = ResampleMode.Points;
        [Out] public CurveData Result { get; private set; }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port != CurvePort) return;
            Result = CurveData.Empty;
            Curve = CurveData.Empty;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            if (Curve == null || Curve.Type == CurveType.None || Curve.Points == 0) {
                Result = CurveData.Empty;
                return;
            }

            if (Mode == ResampleCurveNode_Mode.Distance) {
                Result = CurveOperations.ResampleCurveByDistance(Curve, Distance.MinClamped(0.001f));
            } else {
                Result = CurveOperations.ResampleCurveByPoints(Curve, Points.Clamped(GetMinPoints(), Constants.MAX_CURVE_RESOLUTION));
            }
        }

        private int GetMinPoints() => Curve == null ? 0 : CurveTypeUtilities.MinPoints(Curve.Type);

        public enum ResampleCurveNode_Mode {
            Distance,
            Points
        }
    }
}