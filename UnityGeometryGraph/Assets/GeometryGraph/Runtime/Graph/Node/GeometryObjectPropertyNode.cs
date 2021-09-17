using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class GeometryObjectPropertyNode : RuntimeNode {
        [SerializeReference] public Property Property;
        public RuntimePort Port { get; }
        
        public GeometryObjectPropertyNode(string guid) : base(guid) {
            Port = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public override object GetValueForPort(RuntimePort port) {
            return null; //((GeometryObject)Property.Value).Geometry;
        }
    }
}