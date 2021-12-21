using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class StringPropertyNode : RuntimeNode {
        [SerializeReference] public Property Property;
       
        public RuntimePort Port { get; private set; }
        public string PropertyGuid { get; private set; }
        
        public StringPropertyNode(string guid) : base(guid) {
            Port = RuntimePort.Create(PortType.String, PortDirection.Output, this);
        }
        
        protected override object GetValueForPort(RuntimePort port) {
            return Property.GetValueOrDefault<string>(Property, "");
        }

        public override string Serialize() {
            return new JObject {
                ["p"] = Property?.Guid
            }.ToString(Formatting.None);
        }

        public override void Deserialize(string json) {
            JObject jsonObject = JObject.Parse(json);
            PropertyGuid = jsonObject.Value<string>("p");
        }
    }
}