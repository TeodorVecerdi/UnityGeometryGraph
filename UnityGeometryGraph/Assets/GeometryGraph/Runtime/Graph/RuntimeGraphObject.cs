using System;
using System.Linq;
using GeometryGraph.Runtime.Data;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class RuntimeGraphObject : ScriptableObject {
        [SerializeField] public RuntimeGraphObjectData RuntimeData = new RuntimeGraphObjectData();
        public static bool DebugEnabled = false;

        public GeometryGraphEvaluationResult Evaluate(GeometryGraphSceneData sceneData) {
            if (RuntimeData.OutputNode == null) {
                return GeometryGraphEvaluationResult.Empty;
            }

            LoadScenePropertyValues(sceneData.PropertyData);
            var result = RuntimeData.OutputNode.EvaluateGraph();
            var curve = RuntimeData.OutputNode.GetDisplayCurve();
            CleanupScenePropertyValues();
            
            return new GeometryGraphEvaluationResult(result, curve);
        }

        public void Load(RuntimeGraphObjectData runtimeData) {
            RuntimeData.Load(runtimeData);
        }

        public void OnPropertyAdded(Property property) {
            if(RuntimeData.Properties.Any(p => p.Guid == property.Guid))
                return;
            RuntimeData.Properties.Add(property);

#if UNITY_EDITOR
            GeometryGraph graph;
            if (UnityEditor.Selection.activeGameObject == null || (graph = UnityEditor.Selection.activeGameObject.GetComponent<GeometryGraph>()) == null) return;
            graph.OnPropertiesChanged(RuntimeData.PropertyHashCode);
#endif
        }

        public void OnPropertyRemoved(string propertyGuid) {
            var removed = RuntimeData.Properties.RemoveAll(p => p.Guid == propertyGuid);
            if (removed == 0) return;
            
#if UNITY_EDITOR
            GeometryGraph graph;
            if (UnityEditor.Selection.activeGameObject == null || (graph = UnityEditor.Selection.activeGameObject.GetComponent<GeometryGraph>()) == null) return;
            graph.OnPropertiesChanged(RuntimeData.PropertyHashCode);
#endif
        }

        public void OnPropertyDefaultValueChanged(string propertyGuid, object newValue) {
            var property = RuntimeData.Properties.Find(p => p.Guid == propertyGuid);
            if (property == null) {
                Debug.LogError($"Updated value of non-existent property with guid {propertyGuid}");
                return;
            }

            switch (property.Type) {
                case PropertyType.GeometryObject:
                case PropertyType.GeometryCollection:
                    Debug.LogWarning($"Updating value of property type {property.Type} is not supported");
                    break;
                case PropertyType.Integer:
                case PropertyType.Float:
                case PropertyType.Vector:
                    property.DefaultValue = new DefaultPropertyValue(property.Type, newValue);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(property.Type), property.Type, null);
            }

#if UNITY_EDITOR
            GeometryGraph graph;
            if (UnityEditor.Selection.activeGameObject == null || (graph = UnityEditor.Selection.activeGameObject.GetComponent<GeometryGraph>()) == null) return;
            graph.OnDefaultPropertyValueChanged(property);
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
            connection.Output.Connections.Add(connection);
            connection.Input.Connections.Add(connection);
            connection.Output.Node.NotifyConnectionCreated(connection, connection.Output);
            connection.Input.Node.NotifyConnectionCreated(connection, connection.Input);
        }

        public void OnConnectionRemoved(string outputGuid, string inputGuid) {
            var connection = RuntimeData.Connections.FirstOrDefault(connection => connection.OutputGuid == outputGuid && connection.InputGuid == inputGuid);
            if (connection == null) {
                return;
            }
            
            connection.Output.Node.NotifyConnectionRemoved(connection, connection.Output);
            connection.Input.Node.NotifyConnectionRemoved(connection, connection.Input);
            connection.Output.Connections.Remove(connection);
            connection.Input.Connections.Remove(connection);
            RuntimeData.Connections.Remove(connection);

            var removed = RuntimeData.Connections.RemoveAll(connection => connection.OutputGuid == outputGuid && connection.InputGuid == inputGuid);
            if (removed != 0) Debug.LogWarning("Removed connection when it was supposed to already be removed");
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
            DebugUtility.Log("Loading Scene Property Values");
            foreach (var property in RuntimeData.Properties) {
                property.Value = propertyData[property.Guid].GetValueForPropertyType(property.Type);
                DebugUtility.Log($"Set Property {property.DisplayName}/{property.Type} value to [{property.Value}]");
            }

            DebugUtility.Log("Announcing port value changes on property nodes");
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
                property.Value = property.DefaultValue; 
            }
        }
    }
}