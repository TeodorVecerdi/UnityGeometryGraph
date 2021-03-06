using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class SearchWindowProvider : ScriptableObject {
        private GraphFrameworkEditorWindow editorWindow;
        private EditorView editorView;
        private Texture2D icon;
        public List<NodeEntry> CurrentNodeEntries;
        public GraphFrameworkPort ConnectedPort;
        public bool RegenerateEntries { get; set; }

        private static readonly Type outputNodeType = typeof(OutputNode);

        public void Initialize(GraphFrameworkEditorWindow editorWindow, EditorView editorView) {
            this.editorWindow = editorWindow;
            this.editorView = editorView;

            GenerateNodeEntries();
            icon = new Texture2D(1, 1);
            icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            icon.Apply();
        }

        private void OnDestroy() {
            if (icon != null) {
                DestroyImmediate(icon);
                icon = null;
            }
        }

        public void GenerateNodeEntries() {
            // First build up temporary data structure containing group & title as an array of strings (the last one is the actual title) and associated node type.
            List<NodeEntry> nodeEntries = new();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<AbstractNode>()) {
                if (!type.IsClass || type.IsAbstract)
                    continue;

                if(type == outputNodeType && editorView.GraphView.GraphOutputNode != null) continue;

                if (type.GetCustomAttributes(typeof(TitleAttribute), false) is TitleAttribute[] attrs && attrs.Length > 0) {
                    foreach (TitleAttribute attr in attrs) {
                        AddEntries(type, attr.Title, nodeEntries);
                    }
                }
            }

            foreach (AbstractProperty property in editorView.GraphObject.GraphData.Properties) {
                SerializedNode node = new(PropertyUtils.PropertyTypeToNodeType(property.Type), new Rect(Vector2.zero, EditorView.DefaultNodeSize));
                node.BuildNode(editorView, editorView.EdgeConnectorListener, false);
                AbstractNode propertyNode = node.Node;
                propertyNode.PropertyGuid = property.GUID;
                node.BuildPortData();
                AddEntries(node, new[] {"Properties", $"{property.Type}: {property.DisplayName}"}, nodeEntries);
            }

            nodeEntries.Sort((entry1, entry2) => {
                for (int i = 0; i < entry1.Title.Length; i++) {
                    if (i >= entry2.Title.Length)
                        return 1;
                    int value = string.Compare(entry1.Title[i], entry2.Title[i], StringComparison.Ordinal);
                    if (value == 0)
                        continue;

                    // Make sure that leaves go before nodes
                    if (entry1.Title.Length != entry2.Title.Length && (i == entry1.Title.Length - 1 || i == entry2.Title.Length - 1)) {
                        //once nodes are sorted, sort slot entries by slot order instead of alphebetically
                        int alphaOrder = entry1.Title.Length < entry2.Title.Length ? -1 : 1;
                        int slotOrder = entry1.CompatiblePortIndex.CompareTo(entry2.CompatiblePortIndex);
                        return alphaOrder.CompareTo(slotOrder);
                    }

                    return value;
                }

                return 0;
            });

            CurrentNodeEntries = nodeEntries;
        }

        private void AddEntries(SerializedNode node, string[] title, List<NodeEntry> nodeEntries) {
            if (ConnectedPort == null) {
                nodeEntries.Add(new NodeEntry(node, title, -1, null));
                return;
            }

            List<int> portIndices = new();
            for (int i = 0; i < node.Node.Ports.Count; i++) {
                if (ConnectedPort.IsCompatibleWith(node.Node.Ports[i]) && ConnectedPort.direction != node.Node.Ports[i].direction && node.Node.Ports[i].PortVisible) {
                    portIndices.Add(i);
                }
            }

            foreach (int portIndex in portIndices) {
                string[] newTitle = new string[title.Length];
                for (int i = 0; i < title.Length - 1; i++)
                    newTitle[i] = title[i];

                newTitle[title.Length - 1] = title[title.Length - 1];
                if (!string.IsNullOrEmpty(node.Node.Ports[portIndex].OriginalLabel))
                    newTitle[title.Length - 1] += $" ({node.Node.Ports[portIndex].OriginalLabel})";

                nodeEntries.Add(new NodeEntry(node, newTitle, portIndex, node.Node.Ports[portIndex].capacity));
            }
        }

        private void AddEntries(Type nodeType, string[] title, List<NodeEntry> nodeEntries) {
            if (ConnectedPort == null) {
                nodeEntries.Add(new NodeEntry(nodeType, title, -1, null));
                return;
            }

            AbstractNode node = (AbstractNode) Activator.CreateInstance(nodeType);
            node.InitializeNode(null);
            List<int> portIndices = new();
            for (int i = 0; i < node.Ports.Count; i++) {
                if (ConnectedPort.IsCompatibleWith(node.Ports[i]) && ConnectedPort.direction != node.Ports[i].direction && node.Ports[i].PortVisible) {
                    portIndices.Add(i);
                }
            }

            foreach (int portIndex in portIndices) {
                string[] newTitle = new string[title.Length];
                for (int i = 0; i < title.Length - 1; i++)
                    newTitle[i] = title[i];
                newTitle[title.Length - 1] = title[title.Length - 1] + $" ({node.Ports[portIndex].OriginalLabel})";

                nodeEntries.Add(new NodeEntry(nodeType, newTitle, portIndex, node.Ports[portIndex].capacity));
            }
        }

        public Searcher LoadSearchWindow() {
            if (RegenerateEntries) {
                GenerateNodeEntries();
                RegenerateEntries = false;
            }

            //create empty root for searcher tree
            List<SearcherItem> root = new();
            NodeEntry dummyEntry = new();

            foreach (NodeEntry nodeEntry in CurrentNodeEntries) {
                SearcherItem parent = null;
                for (int i = 0; i < nodeEntry.Title.Length; i++) {
                    string pathEntry = nodeEntry.Title[i];
                    List<SearcherItem> children = parent != null ? parent.Children : root;
                    SearcherItem item = children.Find(x => x.Name == pathEntry);

                    if (item == null) {
                        //if we don't have slot entries and are at a leaf, add userdata to the entry
                        if (i == nodeEntry.Title.Length - 1)
                            item = new SearchNodeItem(pathEntry, nodeEntry);

                        //if we aren't a leaf, don't add user data
                        else
                            item = new SearchNodeItem(pathEntry, dummyEntry);

                        if (parent != null) {
                            parent.AddChild(item);
                        } else {
                            children.Add(item);
                        }
                    }

                    parent = item;

                    if (parent.Depth == 0 && !root.Contains(parent))
                        root.Add(parent);
                }
            }

            SearcherDatabase nodeDatabase = SearcherDatabase.Create(root, string.Empty, false);
            Searcher searcher = new(nodeDatabase, new SearchWindowAdapter("Create Node"));
            return searcher;
        }

        public bool OnSelectEntry(SearcherItem selectedEntry, Vector2 mousePosition) {
            SearchNodeItem searchNodeItem = selectedEntry as SearchNodeItem;

            if (searchNodeItem == null || searchNodeItem.NodeEntry is { Type: null, Node: null }) {
                return false;
            }

            NodeEntry nodeEntry = searchNodeItem.NodeEntry;
            Vector2 windowMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, mousePosition);
            Vector2 graphMousePosition = editorView.GraphView.contentViewContainer.WorldToLocal(windowMousePosition);

            SerializedNode node;
            if (nodeEntry.Node != null) {
                node = nodeEntry.Node;
                node.DrawState.Position.position = graphMousePosition;
            } else {
                Type nodeType = nodeEntry.Type;
                node = new SerializedNode(nodeType, new Rect(graphMousePosition, EditorView.DefaultNodeSize));
            }

            editorView.GraphObject.RegisterCompleteObjectUndo("Add " + node.Type);
            editorView.GraphObject.GraphData.AddNode(node);

            if (ConnectedPort != null) {
                if (nodeEntry.Node == null)
                    node.BuildNode(editorView, null);
                SerializedEdge edge = new() {
                    Output = ConnectedPort.node.viewDataKey,
                    Input = node.GUID,
                    OutputPort = ConnectedPort.viewDataKey,
                    InputPort = node.PortData[nodeEntry.CompatiblePortIndex],
                    OutputCapacity =  ConnectedPort.capacity,
                    InputCapacity = nodeEntry.Capacity.Value
                };
                editorView.GraphObject.GraphData.AddEdge(edge);
            }

            return true;
        }

        public struct NodeEntry : IEquatable<NodeEntry> {
            public readonly Type Type;
            public readonly string[] Title;
            public readonly int CompatiblePortIndex;
            public readonly SerializedNode Node;
            public Port.Capacity? Capacity;

            public NodeEntry(Type type, string[] title, int compatiblePortIndex, Port.Capacity? capacity) {
                Type = type;
                Title = title;
                CompatiblePortIndex = compatiblePortIndex;
                Capacity = capacity;
                Node = null;
            }

            public NodeEntry(SerializedNode node, string[] title, int compatiblePortIndex, Port.Capacity? capacity) {
                Node = node;
                Title = title;
                CompatiblePortIndex = compatiblePortIndex;
                Type = Type.GetType(node.Type);
                Capacity = capacity;
            }

            public bool Equals(NodeEntry other) {
                return Equals(Title, other.Title) && Type == other.Type;
            }

            public override bool Equals(object obj) {
                return obj is NodeEntry other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    return ((Title != null ? Title.GetHashCode() : 0) * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                }
            }
        }
    }
}