using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class FloatPropertyNode : RuntimeNode {
        [SerializeReference] public Property Property;
       
        public RuntimePort Port { get; private set; }
        public string PropertyGuid { get; private set; }
        
        public FloatPropertyNode(string guid) : base(guid) {
            Port = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
        }
        
        public override void RebindPorts() {
            Port = Ports[0];
        }

        public override object GetValueForPort(RuntimePort port) {
            if (Property == null) return 0.0f;
            return (float)Property.Value;
        }

        public override string GetCustomData() {
            return new JObject {
                ["p"] = Property.Guid
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            var jsonObject = JObject.Parse(json);
            PropertyGuid = jsonObject.Value<string>("p");
        }
    }
}