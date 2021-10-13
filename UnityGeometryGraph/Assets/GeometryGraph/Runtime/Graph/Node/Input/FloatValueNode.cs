using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.Graph {
    public class FloatValueNode : RuntimeNode {
        private float value;

        public RuntimePort ValuePort { get; private set; }

        public FloatValueNode(string guid) : base(guid) {
            ValuePort = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
        }

        public void UpdateValue(float newValue) {
            value = newValue;
            NotifyPortValueChanged(ValuePort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            return port == ValuePort ? value : 0.0f;
        }

        public override void RebindPorts() {
            ValuePort = Ports[0];
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["v"] = value
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            var data = JObject.Parse(json);
            value = data.Value<float>("v");
            NotifyPortValueChanged(ValuePort);
        }
    }
}