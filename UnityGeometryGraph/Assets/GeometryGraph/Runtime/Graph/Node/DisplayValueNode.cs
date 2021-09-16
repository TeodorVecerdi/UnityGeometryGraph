using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class DisplayValueNode : RuntimeNode {
        public RuntimePort Input { get; }

        public DisplayValueNode(string guid) : base(guid) {
            Input = new RuntimePort(PortType.Any, PortDirection.Input, this);
        }

        public override object GetValueForPort(RuntimePort port) {
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port != Input) return;

            Debug.Log(GetValue(connection, (object)null).ToString());
        }
    }
}