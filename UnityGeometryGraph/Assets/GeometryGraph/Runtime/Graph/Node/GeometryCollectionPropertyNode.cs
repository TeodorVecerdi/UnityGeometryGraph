using System;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GeometryGraph.Runtime.Graph {
    public class GeometryCollectionPropertyNode : RuntimeNode {
        [SerializeReference] public Property Property;
        public RuntimePort Port { get; private set; }
        public string PropertyGuid { get; private set; }

        public GeometryCollectionPropertyNode(string guid) : base(guid) {
            Port = RuntimePort.Create(PortType.Collection, PortDirection.Output, this);
        }

        public override object GetValueForPort(RuntimePort port) {
            GeometryCollection objectValue = null;
            var value = Property?.Value;
            if ((Object)value != null) {
                objectValue = (GeometryCollection)value;
            }
            return objectValue == null ? Array.Empty<GeometryData>() : objectValue.Collection;
        }
        
        public override void RebindPorts() {
            Port = Ports[0];
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