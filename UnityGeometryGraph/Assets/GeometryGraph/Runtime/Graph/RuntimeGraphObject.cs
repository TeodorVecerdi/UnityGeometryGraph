using System;
using System.Linq;
using GeometryGraph.Runtime.Geometry;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class RuntimeGraphObject : ScriptableObject {
        [SerializeField] public RuntimeGraphObjectData RuntimeData = new RuntimeGraphObjectData();

        public GeometryData Evaluate(GeometryGraphSceneData sceneData) {
            if (RuntimeData.OutputNode == null) {
                return GeometryData.Empty;
            }

            LoadScenePropertyValues(sceneData.PropertyData);
            var result = RuntimeData.OutputNode.EvaluateGraph();
            CleanupScenePropertyValues();
            return result;
        }

        public void Load(RuntimeGraphObjectData runtimeData) {
            RuntimeData.Load(runtimeData);
        }

        public void OnPropertyAdded(Property property) {
            if(RuntimeData.Properties.Any(p => p.Guid == property.Guid)) 
                return;
            RuntimeData.Properties.Add(property);
            RuntimeData.UpdatePropertyHashCode();

#if UNITY_EDITOR
            GeometryGraph graph;
            if (UnityEditor.Selection.activeGameObject == null || (graph = UnityEditor.Selection.activeGameObject.GetComponent<GeometryGraph>()) == null) return;
            graph.OnPropertiesChanged(RuntimeData.PropertyHashCode);
#endif
        }

        public void OnPropertyRemoved(string propertyGuid) {
            var removed = RuntimeData.Properties.RemoveAll(p => p.Guid == propertyGuid);
            if (removed != 0) RuntimeData.UpdatePropertyHashCode();
            
#if UNITY_EDITOR
            GeometryGraph graph;
            if (UnityEditor.Selection.activeGameObject == null || (graph = UnityEditor.Selection.activeGameObject.GetComponent<GeometryGraph>()) == null) return;
            graph.OnPropertiesChanged(RuntimeData.PropertyHashCode);
#endif
        }

        public void OnNodeAdded(RuntimeNode node) {
            RuntimeData.Nodes.Add(node);
            if (node is OutputNode outputNode) RuntimeData.OutputNode = outputNode;
        }

        public void OnNodeRemoved(RuntimeNode node) {
            RuntimeData.Nodes.RemoveAll(n => n.Guid == node.Guid);
            if (node is OutputNode) RuntimeData.OutputNode = null;
        }

        public void OnConnectionAdded(Connection connection) {
            RuntimeData.Connections.Add(connection);
            var ports = RuntimeData.Nodes.SelectMany(node => node.Ports).Where(port => port.Guid == connection.OutputGuid || port.Guid == connection.InputGuid);
            foreach (var runtimePort in ports) {
                runtimePort.Connections.Add(connection);
            }
        }

        public void OnConnectionRemoved(string outputGuid, string inputGuid) {
            foreach (var connection in RuntimeData.Connections.Where(connection => connection.OutputGuid == outputGuid && connection.InputGuid == inputGuid)) {
                foreach (var runtimePort in connection.Output.Node.Ports) {
                    if (string.Equals(runtimePort.Guid, outputGuid, StringComparison.InvariantCulture)) {
                        runtimePort.Connections.Remove(connection);
                        break;
                    }
                }
                
                foreach (var runtimePort in connection.Input.Node.Ports) {
                    if (string.Equals(runtimePort.Guid, inputGuid, StringComparison.InvariantCulture)) {
                        runtimePort.Connections.Remove(connection);
                        break;
                    }
                }
            } 

            RuntimeData.Connections.RemoveAll(connection => connection.OutputGuid == outputGuid && connection.InputGuid == inputGuid);
        }

        public void OnPropertyUpdated(string propertyGuid, string newDisplayName) {
            foreach (var runtimeDataProperty in RuntimeData.Properties) {
                if (runtimeDataProperty.Guid != propertyGuid) continue;
            
                runtimeDataProperty.DisplayName = newDisplayName;
                break;
            }
        }

        public void AssignProperty(RuntimeNode runtimeNode, string propertyGuid) {
            switch (runtimeNode) {
                case GeometryObjectPropertyNode propertyNode: propertyNode.Property = RuntimeData.Properties.FirstOrGivenDefault(property => property.Guid == propertyGuid, null); break;
                case GeometryCollectionPropertyNode propertyNode: propertyNode.Property = RuntimeData.Properties.FirstOrGivenDefault(property => property.Guid == propertyGuid, null); break;
                case IntegerPropertyNode propertyNode: propertyNode.Property = RuntimeData.Properties.FirstOrGivenDefault(property => property.Guid == propertyGuid, null); break;
                case FloatPropertyNode propertyNode: propertyNode.Property = RuntimeData.Properties.FirstOrGivenDefault(property => property.Guid == propertyGuid, null); break;
                case VectorPropertyNode propertyNode: propertyNode.Property = RuntimeData.Properties.FirstOrGivenDefault(property => property.Guid == propertyGuid, null); break;
            }
        }

        private void LoadScenePropertyValues(PropertyDataDictionary propertyData) {
            foreach (var property in RuntimeData.Properties) {
                property.Value = propertyData[property.Guid].GetValueForPropertyType(property.Type);
            }
            
            // Call NotifyPortValueChanged on all property nodes
            foreach (var runtimeNode in RuntimeData.Nodes) {
                switch (runtimeNode) {
                    case GeometryObjectPropertyNode propertyNode: propertyNode.NotifyPortValueChanged(propertyNode.Port); break;
                    case GeometryCollectionPropertyNode propertyNode: propertyNode.NotifyPortValueChanged(propertyNode.Port); break;
                    case IntegerPropertyNode propertyNode: propertyNode.NotifyPortValueChanged(propertyNode.Port); break;
                    case FloatPropertyNode propertyNode: propertyNode.NotifyPortValueChanged(propertyNode.Port); break;
                    case VectorPropertyNode propertyNode: propertyNode.NotifyPortValueChanged(propertyNode.Port); break;
                    default: continue;
                }
            }
        }

        private void CleanupScenePropertyValues() {
            foreach (var property in RuntimeData.Properties) {
                property.Value = null;
            }
        }
    }
}