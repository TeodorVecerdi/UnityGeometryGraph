using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Graph {
    public class OutputNode : RuntimeNode {
        public RuntimePort GeometryPort { get; private set; }
        public RuntimePort CurvePort { get; private set; }

        public OutputNode(string guid) : base(guid) {
            GeometryPort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
            CurvePort = RuntimePort.Create(PortType.Curve, PortDirection.Input, this);
        }

        protected override object GetValueForPort(RuntimePort port) {
            return null;
        }

        public override void RebindPorts() {
            GeometryPort = Ports[0];
        }

        public CurveData GetDisplayCurve() {
            DebugUtility.Log("Getting display curve");
            var curve = GetValue(CurvePort, (CurveData) null);
            return curve;
        }

        public GeometryData EvaluateGraph() {
            DebugUtility.Log("Evaluating Graph");
            var value = GetValue(GeometryPort, (GeometryData)null);

            if (value == null) {
                DebugUtility.Log("Return value was null");
                return GeometryData.Empty;
            }

            return value;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            
        }
    }
}