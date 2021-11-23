using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class VectorPropertyNode : RuntimeNode {
        [SerializeReference] public Property Property;

        public RuntimePort Port { get; private set; }
        public string PropertyGuid { get; private set; }

        public VectorPropertyNode(string guid) : base(guid) {
            Port = RuntimePort.Create(PortType.Vector, PortDirection.Output, this);
        }

        protected override object GetValueForPort(RuntimePort port) {
            return Property.GetValueOrDefault<float3>(Property, float3.zero);
        }

        public override string GetCustomData() {
            return new JObject {
                ["p"] = Property?.Guid
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            JObject jsonObject = JObject.Parse(json);
            PropertyGuid = jsonObject.Value<string>("p");
        }
    }
}