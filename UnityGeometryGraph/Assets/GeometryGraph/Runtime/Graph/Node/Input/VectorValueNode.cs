using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    public class VectorValueNode : RuntimeNode {
        private float3 value;

        public RuntimePort ValuePort { get; private set; }

        public VectorValueNode(string guid) : base(guid) {
            ValuePort = RuntimePort.Create(PortType.Vector, PortDirection.Output, this);
        }

        public void UpdateValue(float3 newValue) {
            value = newValue;
            NotifyPortValueChanged(ValuePort);
        }

        public override object GetValueForPort(RuntimePort port) {
            return port == ValuePort ? value : float3.zero;
        }
        
        public override void RebindPorts() {
            ValuePort = Ports[0];
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["v"] = JsonConvert.SerializeObject(value, float3Converter.Converter)
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            var data = JObject.Parse(json);
            value = JsonConvert.DeserializeObject<float3>(data.Value<string>("v")!, float3Converter.Converter);
            NotifyPortValueChanged(ValuePort);
        }
    }
}