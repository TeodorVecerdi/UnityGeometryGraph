using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class RuntimeGraphObject : ScriptableObject {
        [SerializeField] public RuntimeGraphObjectData RuntimeData = new RuntimeGraphObjectData();

        public void Load(RuntimeGraphObjectData runtimeData) {
            RuntimeData.Load(runtimeData);
        }

        public void OnNodeAdded(RuntimeNode node) {
            RuntimeData.Nodes.Add(node);
        }

        public void OnNodeRemoved(RuntimeNode node) {
            RuntimeData.Nodes.RemoveAll(n => n.Guid == node.Guid);
        }

        public void OnConnectionAdded(Connection connection) {
            RuntimeData.Connections.Add(connection);
        }

        public void OnConnectionRemoved(RuntimePort output, RuntimePort input) {
            RuntimeData.Connections.RemoveAll(connection => connection.OutputGuid == output.Guid && connection.InputGuid == input.Guid);
        }
    }
}