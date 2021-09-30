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
        public abstract void RebindPorts();
        
        protected virtual void OnPortValueChanged(Connection connection, RuntimePort port) {}
        protected virtual void OnConnectionCreated(Connection connection, RuntimePort port) {}
        protected virtual void OnConnectionRemoved(Connection connection, RuntimePort port) {}

        public virtual string GetCustomData() {
            return string.Empty;
        }
        
        public virtual void SetCustomData(string json) {}

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

            var value = outputPort.Node.GetValueForPort(outputPort);
            var tValue = (T)value;
            if (tValue != null) return tValue;
            return (T)PortValueConverter.Convert(value, outputPort.Type, connection.Input.Type);
        }

        protected IEnumerable<T> GetValues<T>(RuntimePort port, T defaultValue) {
            if (port.Connections.Count == 0) yield return defaultValue;
            foreach (var connection in port.Connections) {
                var value = connection.Output.Node.GetValueForPort(connection.Output);
                var tValue = (T)value;
                if (tValue != null) yield return tValue;
                else yield return (T)PortValueConverter.Convert(value, connection.Output.Type, connection.Input.Type);
            }
        }

        protected T GetValue<T>(RuntimePort port, T defaultValue) {
            var firstConnection = port.Connections.FirstOrDefault();
            if (firstConnection == null) {
                return defaultValue;
            }

            return GetValue(firstConnection, defaultValue);
        }

        public void OnNodeRemoved() {
            foreach (var port in Ports) {
                foreach (var connection in port.Connections) {
                    var otherPort = port.Direction == PortDirection.Input ? connection.Output : connection.Input;
                    otherPort.Node.NotifyConnectionRemoved(connection, port);
                }
            }
        }

        public void OnConnectionCreated(Connection connection) {
            var selfPort = connection.Output.Node == this ? connection.Output : connection.Input;
            NotifyConnectionCreated(connection, selfPort);
        }

        public void OnConnectionRemoved(RuntimePort output, RuntimePort input) {
            var selfPort = output.Node == this ? output : input;
            var index = selfPort.Connections.FindIndex(connection => connection.Output == output && connection.Input == input);
            if (index != -1) {
                var connection = selfPort.Connections[index];
                NotifyConnectionRemoved(connection, selfPort);
            }
        }
    }
}