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

        private RuntimePort() {}
        private RuntimePort(PortType type, PortDirection direction, RuntimeNode owner) {
            Type = type;
            Direction = direction;
            Node = owner;
            Connections = new List<Connection>();
        }

        public static RuntimePort Create(PortType type, PortDirection direction, RuntimeNode owner) {
            var port = new RuntimePort(type, direction, owner);
            owner.Ports.Add(port);
            return port;
        }
    }
}