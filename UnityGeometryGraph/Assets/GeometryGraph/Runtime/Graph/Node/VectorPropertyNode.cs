﻿using Newtonsoft.Json;
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
        
        public override void RebindPorts() {
            Port = Ports[0];
        }

        public override object GetValueForPort(RuntimePort port) {
            if (Property?.Value == null) return float3.zero;
            return (float3)Property.Value;
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