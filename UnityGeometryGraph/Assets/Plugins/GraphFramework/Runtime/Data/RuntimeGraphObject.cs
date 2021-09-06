using UnityEngine;
using System.Collections.Generic;

namespace GraphFramework.Runtime {
    public class RuntimeGraphObject : ScriptableObject {
        // Graph data
        [SerializeField] public List<Node> Nodes;
        [SerializeField] public List<Edge> Edges;
        [SerializeField] public List<Property> Properties;

        public NodeDictionary NodeDictionary;
        public PropertyDictionary PropertyDictionary;

        public void BuildGraph() {
            PropertyDictionary = new PropertyDictionary();
            foreach (var property in Properties) {
                PropertyDictionary.Add(property.Guid, property);
            }

            NodeDictionary = new NodeDictionary();
            foreach (var node in Nodes) {
                NodeDictionary.Add(node.Guid, node);
            }
            
            // !! TODO: Implement graph construction? 
        }
    }
}