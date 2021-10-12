using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    public class SplitVectorNode : RuntimeNode {
        private float3 vector;

        public RuntimePort VectorPort { get; private set; }
        public RuntimePort XPort { get; private set; }
        public RuntimePort YPort { get; private set; }
        public RuntimePort ZPort { get; private set; }

        public SplitVectorNode(string guid) : base(guid) {
            VectorPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            XPort = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
            YPort = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
            ZPort = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
        }

        public void UpdateValue(float3 value) {
            vector = value;
            NotifyPortValueChanged(XPort);
            NotifyPortValueChanged(YPort);
            NotifyPortValueChanged(ZPort);
        }

        public override object GetValueForPort(RuntimePort port) {
            if (port == XPort) return vector.x;
            if (port == YPort) return vector.y;
            if (port == ZPort) return vector.z;
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == VectorPort) {
                vector = GetValue(VectorPort, vector);
                NotifyPortValueChanged(XPort);
                NotifyPortValueChanged(YPort);
                NotifyPortValueChanged(ZPort);
            }
        }
        
        public override void RebindPorts() {
            VectorPort = Ports[0];
            XPort = Ports[1];
            YPort = Ports[2];
            ZPort = Ports[3];
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["v"] = JsonConvert.SerializeObject(vector, float3Converter.Converter),
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            vector = JsonConvert.DeserializeObject<float3>(data.Value<string>("v")!, float3Converter.Converter);
            NotifyPortValueChanged(XPort);
            NotifyPortValueChanged(YPort);
            NotifyPortValueChanged(ZPort);
        }
    }
}