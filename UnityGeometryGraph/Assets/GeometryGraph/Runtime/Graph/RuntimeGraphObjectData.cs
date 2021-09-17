using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GeometryGraph.Runtime.Graph;
using UnityEngine;

namespace GeometryGraph.Runtime {
    [Serializable]
    public class RuntimeGraphObjectData : ISerializationCallbackReceiver {
        [NonSerialized] public List<RuntimeNode> Nodes = new List<RuntimeNode>();
        [SerializeField] public List<Connection> Connections = new List<Connection>();
        [SerializeField] public List<Property> Properties = new List<Property>();
        [SerializeField] private List<SerializedRuntimeNode> serializedRuntimeNodes = new List<SerializedRuntimeNode>();

        public void Load(RuntimeGraphObjectData runtimeData) {
            Nodes.Clear();
            Nodes.AddRange(runtimeData.Nodes);
            Connections.Clear();
            Connections.AddRange(runtimeData.Connections);
            Properties.Clear();
            Properties.AddRange(runtimeData.Properties);

            OnBeforeSerialize();
        }

        [OnSerializing]
        internal void JsonNet_OnBeforeSerialize(StreamingContext context) {
            OnBeforeSerialize();
        }

        [OnDeserialized]
        internal void JsonNet_OnAfterDeserialize(StreamingContext context) {
            OnAfterDeserialize();
        }

        /// <summary>
        ///   <para>Implement this method to receive a callback before Unity serializes your object.</para>
        /// </summary>
        /// <footer><a href="file:///C:/Program%20Files/Unity/Hub/Editor/2021.1.19f1/Editor/Data/Documentation/en/ScriptReference/ISerializationCallbackReceiver.OnBeforeSerialize.html">External documentation for `ISerializationCallbackReceiver.OnBeforeSerialize`</a></footer>
        public void OnBeforeSerialize() {
            if (Nodes == null) return;

            serializedRuntimeNodes ??= new List<SerializedRuntimeNode>();
            serializedRuntimeNodes.Clear();
            foreach (var runtimeNode in Nodes) {
                serializedRuntimeNodes.Add(SerializedRuntimeNode.FromRuntimeNode(runtimeNode));
            }
        }

        /// <summary>
        ///   <para>Implement this method to receive a callback after Unity deserializes your object.</para>
        /// </summary>
        /// <footer><a href="file:///C:/Program%20Files/Unity/Hub/Editor/2021.1.19f1/Editor/Data/Documentation/en/ScriptReference/ISerializationCallbackReceiver.OnAfterDeserialize.html">External documentation for `ISerializationCallbackReceiver.OnAfterDeserialize`</a></footer>
        public void OnAfterDeserialize() {
            if (serializedRuntimeNodes == null) return;

            Nodes ??= new List<RuntimeNode>();
            Nodes.Clear();
            foreach (var serializedRuntimeNode in serializedRuntimeNodes) {
                Nodes.Add(SerializedRuntimeNode.FromSerializedNode(serializedRuntimeNode));
            }

            var allPorts = Nodes.SelectMany(node => node.Ports).ToList();
            foreach (var connection in Connections) {
                connection.Output = allPorts.Find(port => port.Guid == connection.OutputGuid);
                connection.Input = allPorts.Find(port => port.Guid == connection.InputGuid);
            }

            foreach (var node in Nodes) {
                foreach (var port in node.Ports) {
                    port.Node = node;
                    port.Connections = new List<Connection>(Connections.Where(connection => connection.Output == port || connection.Input == port));
                }
            }
        }
    }
}