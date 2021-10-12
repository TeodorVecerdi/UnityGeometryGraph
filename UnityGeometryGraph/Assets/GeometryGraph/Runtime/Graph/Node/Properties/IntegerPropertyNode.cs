using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class IntegerPropertyNode : RuntimeNode {
        [SerializeReference] public Property Property;
       
        public RuntimePort Port { get; private set; }
        public string PropertyGuid { get; private set; }
        
        public IntegerPropertyNode(string guid) : base(guid) {
            Port = RuntimePort.Create(PortType.Integer, PortDirection.Output, this);
        }
        
        public override void RebindPorts() {
            Port = Ports[0];
        }

        public override object GetValueForPort(RuntimePort port) {
            DebugUtility.Log($"Returning property value {Property} => [{Property?.Value}]");
            if (Property?.Value == null) return 0;
            return (int)Property.Value;
        }

        public override string GetCustomData() {
            return new JObject {
                ["p"] = Property?.Guid
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            var jsonObject = JObject.Parse(json);
            PropertyGuid = jsonObject.Value<string>("p");
        }
    }
}