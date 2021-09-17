using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class RuntimeGraphObject : ScriptableObject {
        [SerializeField] public RuntimeGraphObjectData RuntimeData = new RuntimeGraphObjectData();
        
        // Graph data
        
        public void BuildGraph() {
            RuntimeData.PropertyDictionary = new PropertyDictionary();
            foreach (var property in RuntimeData.Properties) {
                RuntimeData.PropertyDictionary.Add(property.Guid, property);
            }

            RuntimeData.NodeDictionary = new NodeDictionary();
            foreach (var node in RuntimeData.Nodes) {
                RuntimeData.NodeDictionary.Add(node.Guid, node);
            }
            
            // !! TODO: Implement graph construction? 
        }

        public void Load(RuntimeGraphObjectData runtimeData) {
            RuntimeData.Load(runtimeData);
        }
    }
}