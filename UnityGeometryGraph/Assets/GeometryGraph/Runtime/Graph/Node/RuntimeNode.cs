using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public abstract class RuntimeNode {
        public string Guid;
        public List<RuntimePort> Ports;

        public RuntimeNode(string guid) {
            Guid = guid;
            Ports = new List<RuntimePort>();
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

        public void OnNodeRemoved() {
            foreach (var port in Ports) {
                foreach (var connection in port.Connections) {
                    var otherPort = port.Direction == PortDirection.Input ? connection.Output : connection.Input;
                    otherPort.Node.NotifyConnectionRemoved(connection, port);
                    otherPort.Connections.Remove(connection);
                }
            }
            // Debug.Log($"Node removed {GetType()}");
        }

        public void OnConnectionCreated(RuntimePort output, RuntimePort input) {
            var selfPort = output.Node == this ? output : input;
            var connection = new Connection {Input = input, Output = output};
            selfPort.Connections.Add(connection);
            NotifyConnectionCreated(connection, selfPort);

            // Debug.Log($"Connection created {GetType()}");
        }

        public void OnConnectionRemoved(RuntimePort output, RuntimePort input) {
            var selfPort = output.Node == this ? output : input;
            var index = selfPort.Connections.FindIndex(connection => connection.Output == output && connection.Input == input);
            if (index != -1) {
                var connection = selfPort.Connections[index];
                selfPort.Connections.RemoveAt(index);
                NotifyConnectionRemoved(connection, selfPort);
                // Debug.Log($"Connection Removed {GetType()}");
            }
        }
    }
}