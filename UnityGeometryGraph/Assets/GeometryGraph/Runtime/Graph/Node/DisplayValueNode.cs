using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class DisplayValueNode : RuntimeNode {
        public RuntimePort Input { get; private set; }

        public DisplayValueNode(string guid) : base(guid) {
            Input = RuntimePort.Create(PortType.Any, PortDirection.Input, this);
        }

        public override object GetValueForPort(RuntimePort port) {
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port != Input) return;
            var value = GetValue(connection, (object)null);
            Debug.Log(value == null ? "Null" : value.ToString());
        }
        
        public override void RebindPorts() {
            Input = Ports[0];
        }
    }
}