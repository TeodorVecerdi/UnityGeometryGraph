namespace GeometryGraph.Runtime.Graph {
    public class PropertyNode : RuntimeNode {
        private Property property;
        public RuntimePort Port { get; }
        
        public PropertyNode(string guid) : base(guid) {
            Port = RuntimePort.Create(PortType.Any, PortDirection.Output, this);
        }

        public override object GetValueForPort(RuntimePort port) {
            return null;
        }
    }
}