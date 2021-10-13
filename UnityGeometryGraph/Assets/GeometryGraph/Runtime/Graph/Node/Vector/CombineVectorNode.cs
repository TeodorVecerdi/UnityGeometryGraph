using System;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    public class CombineVectorNode : RuntimeNode {
        private float3 vector;

        public RuntimePort XPort { get; private set; }
        public RuntimePort YPort { get; private set; }
        public RuntimePort ZPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public CombineVectorNode(string guid) : base(guid) {
            XPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            YPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ZPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Vector, PortDirection.Output, this);
        }

        public void UpdateValue(float value, CombineVectorNode_Which which) {
            switch (which) {
                case CombineVectorNode_Which.X: vector.x = value; break;
                case CombineVectorNode_Which.Y: vector.y = value; break;
                case CombineVectorNode_Which.Z: vector.z = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            return vector;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if(port == ResultPort) return;
            if (port == XPort) {
                var newValue = GetValue(XPort, vector.x);
                if (Math.Abs(newValue - vector.x) > 0.000001f) {
                    vector.x = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            } else if (port == YPort) {
                var newValue = GetValue(YPort, vector.y);
                if (Math.Abs(newValue - vector.y) > 0.000001f) {
                    vector.y = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            } else if (port == ZPort) {
                var newValue = GetValue(ZPort, vector.z);
                if (Math.Abs(newValue - vector.z) > 0.000001f) {
                    vector.z = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            }
        }
        
        public override void RebindPorts() {
            XPort = Ports[0];
            YPort = Ports[1];
            ZPort = Ports[2];
            ResultPort = Ports[3];
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
            vector = JsonConvert.DeserializeObject<float3>(data.Value<string>("v"), float3Converter.Converter);
            NotifyPortValueChanged(ResultPort);
        }

        public enum CombineVectorNode_Which {X = 0, Y = 1, Z = 2}
    }
}