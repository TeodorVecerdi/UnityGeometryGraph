using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GeometryGraph.Editor {
    [Serializable]
    public class GraphFrameworkData : ISerializationCallbackReceiver {
        public GraphFrameworkObject Owner { get; set; }
        [SerializeField] public string AssetGuid;
        [SerializeField] public bool IsBlackboardVisible;
        [SerializeField] public string GraphVersion;
        [SerializeReference] public RuntimeGraphObjectData RuntimeGraphData;

        [NonSerialized] private Dictionary<string, SerializedNode> nodeDictionary = new Dictionary<string, SerializedNode>();
        [SerializeField] private List<SerializedNode> nodes = new List<SerializedNode>();
        [NonSerialized] private List<SerializedNode> addedNodes = new List<SerializedNode>();
        [NonSerialized] private List<SerializedNode> removedNodes = new List<SerializedNode>();
        [NonSerialized] private List<SerializedNode> pastedNodes = new List<SerializedNode>();
        public List<SerializedNode> Nodes => nodes;
        public List<SerializedNode> AddedNodes => addedNodes;
        public List<SerializedNode> RemovedNodes => removedNodes;
        public List<SerializedNode> PastedNodes => pastedNodes;

        [SerializeField] private List<SerializedEdge> edges = new List<SerializedEdge>();
        [NonSerialized] private List<SerializedEdge> addedEdges = new List<SerializedEdge>();
        [NonSerialized] private List<SerializedEdge> removedEdges = new List<SerializedEdge>();
        public List<SerializedEdge> Edges => edges;
        public List<SerializedEdge> AddedEdges => addedEdges;
        public List<SerializedEdge> RemovedEdges => removedEdges;

        [NonSerialized] private List<AbstractProperty> properties = new List<AbstractProperty>();
        [NonSerialized] private List<AbstractProperty> addedProperties = new List<AbstractProperty>();
        [NonSerialized] private List<AbstractProperty> removedProperties = new List<AbstractProperty>();
        [NonSerialized] private List<AbstractProperty> movedProperties = new List<AbstractProperty>();
        [SerializeField] private List<SerializedProperty> serializedProperties = new List<SerializedProperty>();
        public List<AbstractProperty> Properties => properties;
        public List<AbstractProperty> AddedProperties => addedProperties;
        public List<AbstractProperty> RemovedProperties => removedProperties;
        public List<AbstractProperty> MovedProperties => movedProperties;

        [NonSerialized] private List<SerializedNode> nodeSelectionQueue = new List<SerializedNode>();
        [NonSerialized] private List<SerializedEdge> edgeSelectionQueue = new List<SerializedEdge>();
        public List<SerializedNode> NodeSelectionQueue => nodeSelectionQueue;
        public List<SerializedEdge> EdgeSelectionQueue => edgeSelectionQueue;

        [JsonConstructor]
        private GraphFrameworkData() {
            
        }
        
        public GraphFrameworkData(RuntimeGraphObject runtimeGraph) {
            RuntimeGraphData = runtimeGraph.RuntimeData;
        }

        public void OnBeforeSerialize() {
            if (Owner != null) {
                IsBlackboardVisible = Owner.IsBlackboardVisible; 
            }
            
            serializedProperties.Clear();
            foreach (AbstractProperty property in properties) {
                serializedProperties.Add(new SerializedProperty(property));
            }
        }

        public void OnAfterDeserialize() {
            nodes.ForEach(node => nodeDictionary.Add(node.GUID, node));
            serializedProperties.ForEach(prop => AddProperty(prop.Deserialize()));
        }

        public void ClearChanges() {
            addedNodes.Clear();
            removedNodes.Clear();
            addedEdges.Clear();
            removedEdges.Clear();
            addedProperties.Clear();
            removedProperties.Clear();
            movedProperties.Clear();
            nodeSelectionQueue.Clear();
            edgeSelectionQueue.Clear();
        }

        public void ReplaceWith(GraphFrameworkData otherGraphData) {
            // Remove everything 
            List<string> removedNodesGuid = new List<string>();
            removedNodesGuid.AddRange(nodes.Select(node => node.GUID));
            foreach (string node in removedNodesGuid) {
                RemoveNode(nodeDictionary[node]);
            }

            List<AbstractProperty> removedProperties = new List<AbstractProperty>(properties);
            foreach (AbstractProperty prop in removedProperties)
                RemoveProperty(prop);

            // Add back everything
            foreach (SerializedNode node in otherGraphData.nodes) {
                AddNode(node);
            }

            foreach (SerializedEdge edge in otherGraphData.edges) {
                AddEdge(edge);
            }

            foreach (AbstractProperty property in otherGraphData.properties) {
                AddProperty(property);
            }
            
            RuntimeGraphData.Load(otherGraphData.RuntimeGraphData);
        }

        public void AddNode(SerializedNode node) {
            nodeDictionary.Add(node.GUID, node);
            nodes.Add(node);
            addedNodes.Add(node);
        }

        public void RemoveNode(SerializedNode node) {
            if (!nodeDictionary.ContainsKey(node.GUID))
                throw new InvalidOperationException($"Cannot remove node ({node.GUID}) because it doesn't exist.");

            nodes.Remove(node);
            nodeDictionary.Remove(node.GUID);
            removedNodes.Add(node);

            edges.Where(edge => edge.Input == node.GUID || edge.Output == node.GUID).ToList().ForEach(RemoveEdge);
        }

        public bool HasEdge(Edge edge) {
            SerializedEdge serializedEdge = new SerializedEdge {
                Input = edge.input.node.viewDataKey,
                Output = edge.output.node.viewDataKey,
                InputPort = edge.input.viewDataKey,
                OutputPort = edge.output.viewDataKey
            };
            return Edges.Any(edge1 => edge1.Input == serializedEdge.Input && edge1.Output == serializedEdge.Output && edge1.InputPort == serializedEdge.InputPort && edge1.OutputPort == serializedEdge.OutputPort);
        }

        public void AddEdge(Edge edge) {
            SerializedEdge serializedEdge = new SerializedEdge {
                Input = edge.input.node.viewDataKey,
                Output = edge.output.node.viewDataKey,
                InputPort = edge.input.viewDataKey,
                OutputPort = edge.output.viewDataKey,
                InputCapacity  = edge.input.capacity,
                OutputCapacity = edge.output.capacity
            };
            AddEdge(serializedEdge);
        }

        public void AddEdge(SerializedEdge edge) {
            if (edge.InputCapacity == Port.Capacity.Single) {
                // Remove all edges with the same port
                List<SerializedEdge> temp = new List<SerializedEdge>();
                temp.AddRange(edges.Where(edge1 => edge1.InputPort == edge.InputPort));
                temp.ForEach(RemoveEdge);
            }

            if (edge.OutputCapacity == Port.Capacity.Single) {
                // Remove all edges with the same port
                List<SerializedEdge> temp = new List<SerializedEdge>();
                temp.AddRange(edges.Where(edge1 => edge1.OutputPort == edge.OutputPort));
                temp.ForEach(RemoveEdge);
            }
            
            edges.Add(edge);
            addedEdges.Add(edge);
        }

        public void RemoveEdge(SerializedEdge edge) {
            edges.Remove(edge);
            removedEdges.Add(edge);
        }

        public void AddProperty(AbstractProperty property) {
            if (property == null) return;
            if (properties.Contains(property)) return;
            properties.Add(property);
            addedProperties.Add(property);
        }

        public void RemoveProperty(AbstractProperty property) {
            List<SerializedNode> propertyNodes = nodes.FindAll(node => node.Node.IsProperty &&  node.Node.PropertyGuid == property.GUID);
            foreach (SerializedNode node in propertyNodes)
                RemoveNode(node);

            if (properties.Remove(property)) {
                removedProperties.Add(property);
                addedProperties.Remove(property);
                movedProperties.Remove(property);
            }
        }

        public void MoveProperty(AbstractProperty property, int newIndex) {
            if (newIndex > properties.Count || newIndex < 0)
                throw new ArgumentException("New index is not within properties list.");
            int currentIndex = properties.IndexOf(property);
            if (currentIndex == -1)
                throw new ArgumentException("Property is not in graph.");
            if (newIndex == currentIndex) {
                return;
            }
            properties.RemoveAt(currentIndex);
            Property runtimeProperty = RuntimeGraphData.Properties[currentIndex];
            RuntimeGraphData.Properties.RemoveAt(currentIndex);
            if (newIndex > currentIndex)
                newIndex--;
            bool isLast = newIndex == properties.Count;
            if (isLast) {
                properties.Add(property);
                RuntimeGraphData.Properties.Add(runtimeProperty);
            } else {
                properties.Insert(newIndex, property);
                RuntimeGraphData.Properties.Insert(newIndex, runtimeProperty);
            }
            if (!movedProperties.Contains(property)) {}
                movedProperties.Add(property);
        }

        public void RemoveElements(List<SerializedNode> nodes, List<SerializedEdge> edges) {
            foreach (SerializedEdge edge in edges) {
                RemoveEdge(edge);
            }

            foreach (SerializedNode node in nodes) {
                RemoveNode(node);
            }
        }

        public void QueueSelection(List<SerializedNode> nodes, List<SerializedEdge> edges) {
            nodeSelectionQueue.AddRange(nodes);
            edgeSelectionQueue.AddRange(edges);
        }

        public void SanitizePropertyName(AbstractProperty property) {
            property.DisplayName = property.DisplayName.Trim();
            property.DisplayName = GraphFrameworkUtility.SanitizeName(properties.Where(prop => prop.GUID != property.GUID).Select(prop => prop.DisplayName), "{0} ({1})", property.DisplayName);
        }

        public void SanitizePropertyReference(AbstractProperty property, string newReferenceName) {
            if (string.IsNullOrEmpty(newReferenceName))
                return;

            string name = newReferenceName.Trim();
            if (string.IsNullOrEmpty(name))
                return;

            property.OverrideReferenceName = GraphFrameworkUtility.SanitizeName(properties.Where(prop => prop.GUID != property.GUID).Select(prop => prop.ReferenceName), "{0} ({1})", name);
        }

        public void Paste(CopyPasteData copyPasteData, List<SerializedNode> remappedNodes, List<SerializedEdge> remappedEdges) {
            Dictionary<string, string> nodeGuidMap = new Dictionary<string, string>();
            Dictionary<string, string> portGuidMap = new Dictionary<string, string>();
            foreach (SerializedNode node in copyPasteData.Nodes) {
                string oldGuid = node.GUID;
                string newGuid = Guid.NewGuid().ToString();
                node.GUID = newGuid;
                nodeGuidMap[oldGuid] = newGuid;
                for (int i = 0; i < node.PortData.Count; i++) {
                    string newPortGuid = Guid.NewGuid().ToString();
                    string oldPortGuid = node.PortData[i];
                    portGuidMap[oldPortGuid] = newPortGuid;

                    node.PortData[i] = newPortGuid;
                }
                
                // offset the pasted node slightly so it's not on top of the original one
                NodeDrawState drawState = node.DrawState;
                Rect position = drawState.Position;
                position.x += 30;
                position.y += 30;
                drawState.Position = position;
                node.DrawState = drawState;
                remappedNodes.Add(node);
                AddNode(node);

                // add the node to the pasted node list
                pastedNodes.Add(node);
            }

            foreach (SerializedEdge edge in copyPasteData.Edges) {
                if ((nodeGuidMap.ContainsKey(edge.Input) && nodeGuidMap.ContainsKey(edge.Output)) && (portGuidMap.ContainsKey(edge.InputPort) && portGuidMap.ContainsKey(edge.OutputPort))) {
                    string remappedOutputGuid = nodeGuidMap.ContainsKey(edge.Output) ? nodeGuidMap[edge.Output] : edge.Output;
                    string remappedInputGuid = nodeGuidMap.ContainsKey(edge.Input) ? nodeGuidMap[edge.Input] : edge.Input;
                    string remappedOutputPortGuid = portGuidMap.ContainsKey(edge.OutputPort) ? portGuidMap[edge.OutputPort] : edge.OutputPort;
                    string remappedInputPortGuid = portGuidMap.ContainsKey(edge.InputPort) ? portGuidMap[edge.InputPort] : edge.InputPort;
                    SerializedEdge remappedEdge = new SerializedEdge {
                        Input = remappedInputGuid,
                        Output = remappedOutputGuid,
                        InputPort = remappedInputPortGuid,
                        OutputPort = remappedOutputPortGuid,
                        InputCapacity = edge.InputCapacity,
                        OutputCapacity = edge.OutputCapacity
                    };
                    remappedEdges.Add(remappedEdge);
                    AddEdge(remappedEdge);
                }
            }
        }

        public void Load(RuntimeGraphObject runtimeGraph) {
            if(RuntimeGraphData != null) runtimeGraph.Load(RuntimeGraphData); 
            RuntimeGraphData = runtimeGraph.RuntimeData;
        }
    }
}