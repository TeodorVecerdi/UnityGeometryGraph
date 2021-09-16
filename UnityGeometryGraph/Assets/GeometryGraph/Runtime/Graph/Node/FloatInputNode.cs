namespace GeometryGraph.Runtime.Graph {
    public class FloatInputNode : RuntimeNode {
        private float value;

        public RuntimePort ValuePort { get; }

        public FloatInputNode(string guid) : base(guid) {
            ValuePort = new RuntimePort(PortType.Float, PortDirection.Output, this);
        }

        public void UpdateValue(float newValue) {
            value = newValue;
            NotifyPortValueChanged(ValuePort);
        }

        public override object GetValueForPort(RuntimePort port) {
            if (port == this.ValuePort) return value;
            return null;
        }
    }
}