using System;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public class Connection {
        public RuntimePort Output;
        public RuntimePort Input;
    }
}