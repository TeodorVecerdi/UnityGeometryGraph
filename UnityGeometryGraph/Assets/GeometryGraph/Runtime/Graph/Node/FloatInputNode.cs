namespace GeometryGraph.Runtime.Graph {
    public class FloatInputNode : RuntimeNode {
        private float value;

        public RuntimePort ValuePort { get; private set; }

        public FloatInputNode(string guid) : base(guid) {
            ValuePort = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
        }

        public void UpdateValue(float newValue) {
            value = newValue;
            NotifyPortValueChanged(ValuePort);
        }

        public override object GetValueForPort(RuntimePort port) {
            if (port == ValuePort) return value;
            return null;
        }
        
        public override void RebindPorts() {
            ValuePort = Ports[0];
        }
    }
}