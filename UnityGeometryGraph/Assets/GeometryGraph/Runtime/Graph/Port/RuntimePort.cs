using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public class RuntimePort {
        public string Guid;
        public PortType Type;
        public PortDirection Direction;
        [NonSerialized] public RuntimeNode Node;
        [NonSerialized, ShowInInspector] public List<Connection> Connections;

        public bool IsConnected => Connections?.Count > 0;

        private RuntimePort() {
        }

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