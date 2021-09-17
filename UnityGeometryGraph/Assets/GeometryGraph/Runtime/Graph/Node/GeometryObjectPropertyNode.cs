using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class GeometryObjectPropertyNode : RuntimeNode {
        private static readonly GeometryData defaultValue = GeometryData.Empty;
        
        [SerializeReference] public Property Property;
       
        public RuntimePort Port { get; }
        
        public GeometryObjectPropertyNode(string guid) : base(guid) {
            Port = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public override object GetValueForPort(RuntimePort port) {
            var value = (GeometryObject)Property?.Value;
            return value == null ? defaultValue.Clone() : value.Geometry;
        }
    }
}