using System;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public class Node {
        public NodeType Type;
        public string Guid;
    }

    public enum NodeType {
        Property
    }
}