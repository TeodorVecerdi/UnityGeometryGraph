using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class GeometryObjectPropertyNode : RuntimeNode {
        private static readonly GeometryData defaultValue = GeometryData.Empty;
        
        [SerializeReference] public Property Property;
       
        public RuntimePort Port { get; private set; }
        public string PropertyGuid { get; private set; }
        
        public GeometryObjectPropertyNode(string guid) : base(guid) {
            Port = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }
        
        protected override object GetValueForPort(RuntimePort port) {
            GeometryObject objectValue = null;
            object value = Property?.Value;
            if ((Object)value != null) {
                objectValue = (GeometryObject)value;
            }
            
            return objectValue == null ? defaultValue.Clone() : objectValue.Geometry.Clone();
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