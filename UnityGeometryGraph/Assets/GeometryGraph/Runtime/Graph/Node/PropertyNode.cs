namespace GeometryGraph.Runtime.Graph {
    public class PropertyNode : RuntimeNode {
        private Property property;
        private RuntimePort port;
        
        public PropertyNode(string guid) : base(guid) {
        }

        public override object GetValueForPort(RuntimePort port) {
            return null;
        }
    }
}