using System;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class GeometryCollectionPropertyNode : RuntimeNode {
        [SerializeReference] public Property Property;
        public RuntimePort Port { get; private set; }
        
        public GeometryCollectionPropertyNode(string guid) : base(guid) {
            Port = RuntimePort.Create(PortType.Collection, PortDirection.Output, this);
        }

        public override object GetValueForPort(RuntimePort port) {
            var value = (GeometryCollection)Property?.Value;
            return value == null ? Array.Empty<GeometryData>().Clone() : value.Collection;
        }
        
        public override void RebindPorts() {
            Port = Ports[0];
        }

    }
}