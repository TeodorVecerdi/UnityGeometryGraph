using System;

namespace GeometryGraph.Runtime.Graph {
    public class DebugNode : RuntimeNode {
        private object value;
        public RuntimePort Port { get; private set; }

        public event Action<object> ValueChanged;

        public DebugNode(string guid) : base(guid) {
            Port = RuntimePort.Create(PortType.Any, PortDirection.Input, this);
        }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            value = null;
            ValueChanged?.Invoke(value);
        }

        protected override object GetValueForPort(RuntimePort port) {
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == Port) {
                value = GetValue(connection, value);
                ValueChanged?.Invoke(value);
            }
        }

        public void SetOnValueChanged(Action<object> onValueChanged) {
            ValueChanged = onValueChanged;
        }
    }
}