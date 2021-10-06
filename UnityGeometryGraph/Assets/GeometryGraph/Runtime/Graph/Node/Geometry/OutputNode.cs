using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Graph {
    public class OutputNode : RuntimeNode {
        public RuntimePort Input { get; private set; }

        public OutputNode(string guid) : base(guid) {
            Input = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
        }

        public override object GetValueForPort(RuntimePort port) {
            return null;
        }

        public override void RebindPorts() {
            Input = Ports[0];
        }

        public GeometryData EvaluateGraph() {
            DebugUtility.Log("Evaluating Graph");
            var value = GetValue(Input, (GeometryData)null);

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