namespace GeometryGraph.Runtime.Graph {
    public static class NodeUtilities {
        public static bool IsLeafNode(RuntimeNode node) {
            bool isLeaf = true;
            foreach (RuntimePort port in node.Ports) {
                if (port.Direction == PortDirection.Input && port.Connections.Count > 0) {
                    isLeaf = false;
                    break;
                }
            }
            return isLeaf;
        }
    }
}