using System;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public class RuntimePort {
        public string Guid;
        public PortType Type;
        public PortDirection Direction;
        [SerializeReference] public RuntimeNode Node;
        public List<Connection> Connections;
    }
}