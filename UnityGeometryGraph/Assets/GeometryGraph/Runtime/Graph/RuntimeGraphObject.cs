using System.Linq;
using GeometryGraph.Runtime.Geometry;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class RuntimeGraphObject : ScriptableObject {
        [SerializeField] public RuntimeGraphObjectData RuntimeData = new RuntimeGraphObjectData();

        public GeometryData Evaluate(GeometryGraphSceneData sceneData) {
            if (RuntimeData.OutputNode == null) {
                Debug.Log("Output node is null");
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
        }

        public void OnPropertyRemoved(string propertyGuid) {
            RuntimeData.Properties.RemoveAll(p => p.Guid == propertyGuid);
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

        public void OnPropertyUpdated(string propertyGuid, string newDisplayName) {
            foreach (var runtimeDataProperty in RuntimeData.Properties) {
                if (runtimeDataProperty.Guid != propertyGuid) continue;
            
                runtimeDataProperty.DisplayName = newDisplayName;
                break;
            }
        }

        public void AssignProperty(RuntimeNode runtimeNode, string propertyGuid) {
            switch (runtimeNode) {
                case GeometryObjectPropertyNode propertyNode: {
                    propertyNode.Property = RuntimeData.Properties.FirstOrGivenDefault(property => property.Guid == propertyGuid, null);
                    break;
                }
                case GeometryCollectionPropertyNode propertyNode: {
                    propertyNode.Property = RuntimeData.Properties.FirstOrGivenDefault(property => property.Guid == propertyGuid, null);
                    break;
                }
            }
        }

        private void LoadScenePropertyValues(PropertyDataDictionary propertyData) {
            foreach (var property in RuntimeData.Properties) {
                property.Value = propertyData[property.Guid].GetValueForPropertyType(property.Type);
            }
        }

        private void CleanupScenePropertyValues() {
            foreach (var property in RuntimeData.Properties) {
                property.Value = null;
            }
        }
    }
}