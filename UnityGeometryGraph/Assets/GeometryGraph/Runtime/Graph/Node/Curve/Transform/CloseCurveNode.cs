using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class CloseCurveNode {
        [In(
            DefaultValue = "CurveData.Empty",
            GetValueCode = "{self} = GetValue(connection, {default})",
            UpdateValueCode = ""
        )]
        public CurveData Input { get; private set; }

        [AdditionalValueChangedCode(
            @"if (Result != null && Result.Points != 0 && Result.Type != CurveType.None) {
{indent}    Result.IsClosed = Close;
{indent}} else {
{indent}    CalculateResult();
{indent}}",
            Where = AdditionalValueChangedCodeAttribute.Location.AfterUpdate
        )]
        [In(CallCalculateMethodsIfChanged = false, UpdatedFromEditorNode = false, PortName = "Fake{Self}Port")]
        public bool Close { get; private set; }

        [Out] public CurveData Result { get; private set; }

        public void UpdateClose(bool newValue) {
            if (Close == newValue) return;
            Close = newValue;
            if (Result != null && Result.Points != 0 && Result.Type != CurveType.None) {
                Result.IsClosed = Close;
            } else {
                CalculateResult();
            }
            NotifyPortValueChanged(ResultPort);
        }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port != InputPort) return;
            Result = CurveData.Empty;
        }

        [GetterMethod(nameof(Result), Inline = true)]
        private CurveData GetResult() => Result ?? CurveData.Empty;

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            if (Input == null || Input.Points == 0 || Input.Type == CurveType.None) {
                Result = CurveData.Empty;
                return;
            }

            Result = Input.Clone();
            Result.IsClosed = Close;
        }
    }
}