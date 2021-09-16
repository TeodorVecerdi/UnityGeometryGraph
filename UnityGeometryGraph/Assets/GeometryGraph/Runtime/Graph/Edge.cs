using System;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public class Edge {
        public string FromNode;
        public string FromPort;
        public string ToNode;
        public string ToPort;
    }
}