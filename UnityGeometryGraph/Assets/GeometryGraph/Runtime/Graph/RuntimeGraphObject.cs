using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class RuntimeGraphObject : ScriptableObject {
        [SerializeField] public RuntimeGraphObjectData RuntimeData = new RuntimeGraphObjectData();
        public static bool DebugEnabled = false;

        internal GeometryGraphEvaluationResult Evaluate(GeometryGraphSceneData sceneData) {
            if (RuntimeData.OutputNode == null) {
                return GeometryGraphEvaluationResult.Empty;
            }

            LoadScenePropertyValues(sceneData.PropertyData);
            GeometryData geometry = RuntimeData.OutputNode.GetGeometryData();
            CurveData curve = RuntimeData.OutputNode.GetCurveData();
            InstancedGeometryData instancedGeometry = RuntimeData.OutputNode.GetInstancedGeometryData();
            CleanupScenePropertyValues();
            
            return new GeometryGraphEvaluationResult(geometry, curve, instancedGeometry);
        }

        internal void Load(RuntimeGraphObjectData runtimeData) {
            RuntimeData.Load(runtimeData);
        }

        internal void OnPropertyAdded(Property property) {
            if (RuntimeData.Properties.Any(p => p.Guid == property.Guid)) {
                Debug.Log($"Property already exists: `{property.Guid}`");
                return;
            }

            Debug.Log($"Adding property: `{property.Guid}`");
            RuntimeData.Properties.Add(property);

#if UNITY_EDITOR
            GeometryGraph graph;
            if (UnityEditor.Selection.activeGameObject == null || (graph = UnityEditor.Selection.activeGameObject.GetComponent<GeometryGraph>()) == null) return;
            graph.OnPropertiesChanged(RuntimeData.PropertyHashCode);
#endif
            // RuntimeData.OnBeforeSerialize();
        }

        internal void OnPropertyRemoved(string propertyGuid) {
            int removed = RuntimeData.Properties.RemoveAll(p => p.Guid == propertyGuid);
            if (removed == 0) {
                Debug.Log($"Property not removed: `{propertyGuid}`");
                return;
            }
            Debug.Log($"Removed property: `{propertyGuid}`");
#if UNITY_EDITOR
            GeometryGraph graph;
            if (UnityEditor.Selection.activeGameObject == null || (graph = UnityEditor.Selection.activeGameObject.GetComponent<GeometryGraph>()) == null) return;
            graph.OnPropertiesChanged(RuntimeData.PropertyHashCode);
#endif
            // RuntimeData.OnBeforeSerialize();
        }

        internal void OnPropertyDefaultValueChanged(string propertyGuid, object newValue) {
            Property property = RuntimeData.Properties.Find(p => p.Guid == propertyGuid);
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
                case PropertyType.String:
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

        internal void OnNodeAdded(RuntimeNode node) {
            Debug.Log($"Adding node: `{node.Guid}`");
            RuntimeData.Nodes.Add(node);
            if (node is OutputNode outputNode) RuntimeData.OutputNode = outputNode;
            // RuntimeData.OnBeforeSerialize();
        }

        internal void OnNodeRemoved(RuntimeNode node) {
            Debug.Log($"Removing node: `{node.Guid}`");
            RuntimeData.Nodes.RemoveAll(n => n.Guid == node.Guid);
            if (node is OutputNode) RuntimeData.OutputNode = null;
            // RuntimeData.OnBeforeSerialize();
        }

        internal void OnConnectionAdded(Connection connection) {
            RuntimeData.Connections.Add(connection);
            connection.Output.Connections.Add(connection);
            connection.Input.Connections.Add(connection);
            connection.Output.Node.NotifyConnectionCreated(connection, connection.Output);
            connection.Input.Node.NotifyConnectionCreated(connection, connection.Input);
            // RuntimeData.OnBeforeSerialize();
        }

        internal void OnConnectionRemoved(string outputGuid, string inputGuid) {
            Connection connection = RuntimeData.Connections.FirstOrDefault(connection => connection.OutputGuid == outputGuid && connection.InputGuid == inputGuid);
            if (connection == null) {
                return;
            }
            
            connection.Output.Node.NotifyConnectionRemoved(connection, connection.Output);
            connection.Input.Node.NotifyConnectionRemoved(connection, connection.Input);
            connection.Output.Connections.Remove(connection);
            connection.Input.Connections.Remove(connection);
            RuntimeData.Connections.Remove(connection);

            int removed = RuntimeData.Connections.RemoveAll(connection => connection.OutputGuid == outputGuid && connection.InputGuid == inputGuid);
            if (removed != 0) Debug.LogWarning("Removed connection when it was supposed to already be removed");
            // RuntimeData.OnBeforeSerialize();
        }

        internal void OnPropertyDisplayNameUpdated(string propertyGuid, string newDisplayName) {
            foreach (Property runtimeDataProperty in RuntimeData.Properties) {
                if (runtimeDataProperty.Guid != propertyGuid) continue;

                runtimeDataProperty.DisplayName = newDisplayName;
                break;
            }
            // RuntimeData.OnBeforeSerialize();
        }
        
        internal void OnPropertyReferenceNameUpdated(string propertyGuid, string newReferenceName) {
            foreach (Property runtimeDataProperty in RuntimeData.Properties) {
                if (runtimeDataProperty.Guid != propertyGuid) continue;

                runtimeDataProperty.ReferenceName = newReferenceName;
                break;
            }
            // RuntimeData.OnBeforeSerialize();
        }

        internal void AssignProperty(RuntimeNode runtimeNode, string propertyGuid) {
            switch (runtimeNode) {
                case GeometryObjectPropertyNode propertyNode: propertyNode.Property = RuntimeData.Properties.FirstOrGivenDefault(property => property.Guid == propertyGuid, null); break;
                case GeometryCollectionPropertyNode propertyNode: propertyNode.Property = RuntimeData.Properties.FirstOrGivenDefault(property => property.Guid == propertyGuid, null); break;
                case IntegerPropertyNode propertyNode: propertyNode.Property = RuntimeData.Properties.FirstOrGivenDefault(property => property.Guid == propertyGuid, null); break;
                case FloatPropertyNode propertyNode: propertyNode.Property = RuntimeData.Properties.FirstOrGivenDefault(property => property.Guid == propertyGuid, null); break;
                case VectorPropertyNode propertyNode: propertyNode.Property = RuntimeData.Properties.FirstOrGivenDefault(property => property.Guid == propertyGuid, null); break;
            }
        }

        private void LoadScenePropertyValues(IReadOnlyDictionary<string, PropertyValue> propertyData) {
            DebugUtility.Log("Loading Scene Property Values");
            foreach (Property property in RuntimeData.Properties) {
                property.Value = propertyData[property.Guid].GetValue();
                DebugUtility.Log($"Set Property {property.DisplayName}/{property.Type} value to [{property.Value}]");
            }

            DebugUtility.Log("Announcing port value changes on property nodes");
            // Call NotifyPortValueChanged on all property nodes
            foreach (RuntimeNode runtimeNode in RuntimeData.Nodes) {
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
            foreach (Property property in RuntimeData.Properties) {
                property.Value = property.DefaultValue; 
            }
        }
    }
}