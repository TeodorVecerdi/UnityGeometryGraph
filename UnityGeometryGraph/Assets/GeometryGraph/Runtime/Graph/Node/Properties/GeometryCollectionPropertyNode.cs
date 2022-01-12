using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override object GetValueForPort(RuntimePort port) {
            GeometryCollection objectValue = null;
            object value = Property?.Value;
            if (value == null) return Array.Empty<GeometryData>();

            if ((Object)value != null) {
                objectValue = (GeometryCollection)value;
            }
            return objectValue == null ? (IEnumerable<GeometryData>)Array.Empty<GeometryData>() : (IEnumerable<GeometryData>)objectValue.Collection.Select(data => data.Clone());
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