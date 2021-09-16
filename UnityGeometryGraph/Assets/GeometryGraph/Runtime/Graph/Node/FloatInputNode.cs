namespace GeometryGraph.Runtime.Graph {
    public class FloatInputNode : RuntimeNode {
        private float value;
        private RuntimePort port;
        
        public FloatInputNode(string guid, float value) : base(guid) {
            this.value = value;
        }

        public void UpdateValue(float newValue) {
            value = newValue;
            NotifyPortValueChanged(port);
        }

        public override object GetValueForPort(RuntimePort port) {
            if (port == this.port) return value;
            return null;
        }
    }
}