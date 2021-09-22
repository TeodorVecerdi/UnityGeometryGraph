using GeometryGraph.Runtime.Geometry;
using UnityEngine;

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
            var value = GetValue(Input, (GeometryData)null);
            if (value == null) {
                Debug.Log("Output value is null");
                return GeometryData.Empty;
            }

            return value;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port != Input) return;
            var value = GetValue(connection, (object)null);
        }
    }
}