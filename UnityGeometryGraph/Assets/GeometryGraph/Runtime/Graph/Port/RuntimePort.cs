using System;
using System.Collections.Generic;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public class RuntimePort {
        public string Guid;
        public PortType Type;
        public PortDirection Direction;
        public RuntimeNode Node;
        public List<Connection> Connections;
    }
}