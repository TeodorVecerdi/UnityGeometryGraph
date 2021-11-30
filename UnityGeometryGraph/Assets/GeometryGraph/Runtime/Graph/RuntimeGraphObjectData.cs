using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GeometryGraph.Runtime.Graph;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime {
    [Serializable]
    public class RuntimeGraphObjectData : ISerializationCallbackReceiver {
        public static bool DeserializingFromJson;
        public static bool IsDuringSerialization;
        
        [SerializeField] public string Guid;
        
        [NonSerialized, ShowInInspector] public List<RuntimeNode> Nodes = new List<RuntimeNode>();
        [NonSerialized] public OutputNode OutputNode;
        [SerializeField] public List<Connection> Connections = new List<Connection>();
        [SerializeField] public List<Property> Properties = new List<Property>();
        [SerializeField] private List<SerializedRuntimeNode> serializedRuntimeNodes = new List<SerializedRuntimeNode>();

        public int PropertyHashCode {
            get {
                unchecked {
                    int hashCode = 13;
                    foreach (Property property in Properties) {
                        hashCode = HashHelpers.Combine(hashCode, HashCode.Combine(property.Guid, property.ReferenceName));
                    }

                    return hashCode;
                }
            }
        }

        public RuntimeGraphObjectData() {
            Guid = System.Guid.NewGuid().ToString();
        }

        public void Load(RuntimeGraphObjectData runtimeData) {
            Guid = runtimeData.Guid;

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

        public void OnBeforeSerialize() {
            if (Nodes == null) return;

            IsDuringSerialization = true;

            serializedRuntimeNodes ??= new List<SerializedRuntimeNode>();
            serializedRuntimeNodes.Clear();
            foreach (RuntimeNode runtimeNode in Nodes) {
                serializedRuntimeNodes.Add(SerializedRuntimeNode.FromRuntimeNode(runtimeNode));
            }

            IsDuringSerialization = false;
        }

        public void OnAfterDeserialize() {
            if (serializedRuntimeNodes == null) return;

            IsDuringSerialization = true;
            
            Nodes ??= new List<RuntimeNode>();
            Nodes.Clear();
            OutputNode = null;

            foreach (SerializedRuntimeNode serializedRuntimeNode in serializedRuntimeNodes) {
                RuntimeNode node = SerializedRuntimeNode.FromSerializedNode(serializedRuntimeNode);
                if (node is OutputNode outputNode) {
                    OutputNode = outputNode;
                }
                Nodes.Add(node);
            }

            List<RuntimePort> allPorts = Nodes.SelectMany(node => node.Ports).ToList();
            foreach (Connection connection in Connections) {
                RuntimePort outputPort = allPorts.Find(port => port.Guid == connection.OutputGuid);
                RuntimePort inputPort = allPorts.Find(port => port.Guid == connection.InputGuid);
                connection.Output = outputPort;
                connection.Input = inputPort;
                
                if (outputPort == null || inputPort == null) return;

                outputPort.Connections.Add(connection);
                inputPort.Connections.Add(connection);
            }

            for (int i = 0; i < Nodes.Count; i++) {
                RuntimeNode node = Nodes[i];
                if (!DeserializingFromJson) {
                    node.SetCustomData(serializedRuntimeNodes[i].CustomData);
                }
                
                foreach (RuntimePort port in node.Ports) {
                    DebugUtility.Log($"Port on {node.GetType().Name} has {port.Connections.Count} connections");
                }

                // DebugUtility.Log($"Rebinding ports on {node.GetType().Name}");
                // node.RebindPorts();

                switch (node) {
                    case GeometryObjectPropertyNode propertyNode: propertyNode.Property = Properties.Find(property => property.Guid == propertyNode.PropertyGuid); break;
                    case GeometryCollectionPropertyNode propertyNode: propertyNode.Property = Properties.Find(property => property.Guid == propertyNode.PropertyGuid); break;
                    case IntegerPropertyNode propertyNode: propertyNode.Property = Properties.Find(property => property.Guid == propertyNode.PropertyGuid); break;
                    case FloatPropertyNode propertyNode: propertyNode.Property = Properties.Find(property => property.Guid == propertyNode.PropertyGuid); break;
                    case VectorPropertyNode propertyNode: propertyNode.Property = Properties.Find(property => property.Guid == propertyNode.PropertyGuid); break;
                }
            }

            IsDuringSerialization = false;
        }
    }
}