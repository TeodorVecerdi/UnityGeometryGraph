using System;
using System.Linq;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public abstract class RuntimeNode {
        public string Guid;

        public RuntimeNode(string guid) {
            Guid = guid;
        }

        public abstract object GetValueForPort(RuntimePort port);
        
        protected virtual void OnPortValueChanged(Connection connection, RuntimePort port) {}
        protected virtual void OnConnectionCreated(Connection connection, RuntimePort port) {}
        protected virtual void OnConnectionRemoved(Connection connection, RuntimePort port) {}

        public void NotifyConnectionCreated(Connection connection, RuntimePort port) {
            if (port.Direction != PortDirection.Input) return;
            OnConnectionCreated(connection, port);
            OnPortValueChanged(connection, port);
        }

        public void NotifyConnectionRemoved(Connection connection, RuntimePort port) {
            OnConnectionRemoved(connection, port);
        }

        public void NotifyPortValueChanged(RuntimePort port) {
            if(port.Direction != PortDirection.Output) return;

            foreach (var connection in port.Connections) {
                connection.Input.Node.OnPortValueChanged(connection, connection.Input);
            }
        }

        protected T GetValue<T>(Connection connection, T defaultValue) {
            var outputPort = connection.Output;
            if (outputPort == null) return defaultValue;

            return (T)outputPort.Node.GetValueForPort(outputPort);
        }

        protected T GetValue<T>(RuntimePort port, T defaultValue) {
            var firstConnection = port.Connections.FirstOrDefault();
            if (firstConnection == null) return defaultValue;

            var outputPort = firstConnection.Output;
            return (T)outputPort.Node.GetValueForPort(outputPort);
        }

        public void OnConnectionCreated(Connection connection) {
            connection.Input.Connections.Add(connection);
            connection.Output.Connections.Add(connection);
            
            NotifyConnectionCreated(connection, connection.Input);
            NotifyConnectionCreated(connection, connection.Output);
        }

        public void OnConnectionRemoved(Connection connection) {
            connection.Input.Connections.Remove(connection);
            connection.Output.Connections.Remove(connection);
            
            NotifyConnectionRemoved(connection, connection.Input);
            NotifyConnectionRemoved(connection, connection.Output);
        }
    }
}