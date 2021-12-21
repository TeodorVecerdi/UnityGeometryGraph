using System;
using System.Linq;
using GeometryGraph.Runtime.Graph;
using UnityCommons;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace GeometryGraph.Editor {
    public class EditorView : VisualElement, IDisposable {
        public static readonly Vector2 DefaultNodeSize = new Vector2(200, 150);
        public static readonly Rect DefaultNodePosition = new Rect(Vector2.zero, DefaultNodeSize);

        private readonly GraphFrameworkGraphView graphView;
        private readonly GraphFrameworkEditorWindow editorWindow;
        private readonly BlackboardProvider blackboardProvider;
        private readonly EdgeConnectorListener edgeConnectorListener;
        private SearchWindowProvider searchWindowProvider;
        private bool areCategoriesEnabled;

        public bool IsBlackboardVisible {
            get => blackboardProvider.Blackboard.style.display == DisplayStyle.Flex;
            set => blackboardProvider.Blackboard.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public bool AreCategoriesEnabled {
            get => areCategoriesEnabled;
            set {
                areCategoriesEnabled = value;
                if (areCategoriesEnabled) {
                    graphView.AddToClassList("categorized");
                } else {
                    graphView.RemoveFromClassList("categorized");
                }
            }
        }

        public Vector3 GraphPosition {
            get => graphView.contentViewContainer.transform.position;
            set => graphView.contentViewContainer.transform.position = value;
        }

        public GraphFrameworkEditorWindow EditorWindow => editorWindow;
        public GraphFrameworkObject GraphObject => EditorWindow.GraphObject;
        public GraphFrameworkGraphView GraphView => graphView;
        public EdgeConnectorListener EdgeConnectorListener => edgeConnectorListener;

        public EditorView(GraphFrameworkEditorWindow editorWindow) {
            this.editorWindow = editorWindow;
            this.AddStyleSheet("Styles/Graph");

            IMGUIContainer toolbar = new IMGUIContainer(() => {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button("Save Graph", EditorStyles.toolbarButton)) {
                    EditorWindow.Events.SaveRequested?.Invoke();
                }

                GUILayout.Space(6);
                if (GUILayout.Button("Save As...", EditorStyles.toolbarButton)) {
                    EditorWindow.Events.SaveAsRequested?.Invoke();
                }

                GUILayout.Space(6);
                if (GUILayout.Button("Show In Project", EditorStyles.toolbarButton)) {
                    EditorWindow.Events.ShowInProjectRequested?.Invoke();
                }
                
                GUILayout.Space(6);
                if (GUILayout.Button("Debug", EditorStyles.toolbarButton)) {
                    string current = JsonUtility.ToJson(GraphObject.GraphData);
                    string saved = GraphFrameworkUtility.ReadCompressed(AssetDatabase.GUIDToAssetPath(GraphObject.AssetGuid));
                    Debug.Log($"{current}\n\n{saved}");
                }
                if (GUILayout.Button("Is Same Asset", EditorStyles.toolbarButton)) {
                    string path = AssetDatabase.GUIDToAssetPath(GraphObject.AssetGuid);
                    Object[] allAssetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
                    GraphFrameworkObject gfo = null;
                    RuntimeGraphObject rgo = null;
                    foreach (Object o in allAssetsAtPath) {
                        if (o is GraphFrameworkObject gfo2) {
                            gfo = gfo2;
                        } else if (o is RuntimeGraphObject rgo2) {
                            rgo = rgo2;
                        }
                    }

                    Debug.Log($"GO: {GraphObject == gfo} ;; {GraphObject.GetInstanceID()} - {(gfo != null ? gfo.GetInstanceID() : -1)}");
                    Debug.Log($"RGO: {GraphObject.RuntimeGraph == rgo} ;; {GraphObject.RuntimeGraph.GetInstanceID()} - {(rgo != null ? rgo.GetInstanceID() : -1)}");
                }

                if (GUILayout.Button("Print Position")) {
                    Debug.Log($"ContentViewContainer.worldBound: {graphView.contentViewContainer.worldBound}\n.layout: {graphView.contentViewContainer.layout}\n.transform.position: {graphView.contentViewContainer.transform.position}");
                }

                if (GUILayout.Button("Set Position to 0")) {
                    graphView.contentViewContainer.transform.position = Vector3.zero;
                }

                GUILayout.FlexibleSpace();
                AreCategoriesEnabled = GUILayout.Toggle(AreCategoriesEnabled, "Category Colors", EditorStyles.toolbarButton);
                IsBlackboardVisible = GUILayout.Toggle(IsBlackboardVisible, "Blackboard", EditorStyles.toolbarButton);
                GraphObject.AreCategoriesEnabled = AreCategoriesEnabled;
                GraphObject.IsBlackboardVisible = IsBlackboardVisible;

                GUILayout.EndHorizontal();
            });

            Add(toolbar);
            VisualElement content = new VisualElement { name = "content" };
            {
                graphView = new GraphFrameworkGraphView(this);
                graphView.SetupZoom(0.05f, 8f);
                graphView.AddManipulator(new ContentDragger());
                graphView.AddManipulator(new SelectionDragger());
                graphView.AddManipulator(new RectangleSelector());
                graphView.AddManipulator(new ClickSelector());
                graphView.RegisterCallback<KeyDownEvent>(OnKeyDown);
                content.Add(graphView);

                GridBackground grid = new GridBackground();
                graphView.Insert(0, grid);
                grid.StretchToParentSize();

                blackboardProvider = new BlackboardProvider(this);
                graphView.Add(blackboardProvider.Blackboard);

                graphView.graphViewChanged += OnGraphViewChanged;

                graphView.contentViewContainer.transform.position = GraphObject.GraphPosition;
            }

            searchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            searchWindowProvider.Initialize(this.editorWindow, this);

            graphView.nodeCreationRequest = ctx => {
                searchWindowProvider.ConnectedPort = null;
                SearcherWindow.Show(editorWindow, searchWindowProvider.LoadSearchWindow(),
                                    item => searchWindowProvider.OnSelectEntry(item, ctx.screenMousePosition - editorWindow.position.position),
                                    ctx.screenMousePosition - editorWindow.position.position, null);
            };
            edgeConnectorListener = new EdgeConnectorListener(this, searchWindowProvider);

            Add(content);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            if (graphViewChange.movedElements != null) {
                editorWindow.GraphObject.RegisterCompleteObjectUndo("Moved elements");
                foreach (AbstractNode node in graphViewChange.movedElements.OfType<AbstractNode>()) {
                    Rect rect = node.parent.ChangeCoordinatesTo(graphView.contentViewContainer, node.GetPosition());
                    node.Owner.DrawState.Position = rect;
                }
            }

            if (graphViewChange.edgesToCreate != null) {
                editorWindow.GraphObject.RegisterCompleteObjectUndo("Created edges");
                foreach (Edge edge in graphViewChange.edgesToCreate) {
                    GraphObject.GraphData.AddEdge(edge);
                }

                graphViewChange.edgesToCreate.Clear();
            }

            if (graphViewChange.elementsToRemove != null) {
                editorWindow.GraphObject.RegisterCompleteObjectUndo("Removed elements");
                foreach (AbstractNode node in graphViewChange.elementsToRemove.OfType<AbstractNode>()) {
                    GraphObject.GraphData.RemoveNode(node.Owner);
                }

                foreach (Edge edge in graphViewChange.elementsToRemove.OfType<Edge>()) {
                    GraphObject.GraphData.RemoveEdge((SerializedEdge)edge.userData);
                }

                foreach (BlackboardField property in graphViewChange.elementsToRemove.OfType<BlackboardField>()) {
                    GraphObject.GraphData.RemoveProperty(property.userData as AbstractProperty);
                }
            }

            return graphViewChange;
        }

        private void OnKeyDown(KeyDownEvent evt) {
            if (evt.actionKey && evt.keyCode == KeyCode.G) {
                if (graphView.selection.OfType<GraphElement>().Any()) {
                    // TODO/NOTE: GROUP
                }
            }

            if (evt.actionKey && evt.keyCode == KeyCode.U) {
                if (graphView.selection.OfType<GraphElement>().Any()) {
                    // TODO/NOTE: UNGROUP
                }
            }
        }

        public void BuildGraph() {
            // Remove existing elements
            graphView.graphElements.Where(el => el is Node or Edge or Group or StickyNote or BlackboardRow).ToList().ForEach(graphView.RemoveElement);
            // graphView.graphElements.ToList().OfType<Node>().ToList().ForEach(graphView.RemoveElement);
            // graphView.graphElements.ToList().OfType<Edge>().ToList().ForEach(graphView.RemoveElement);
            // graphView.graphElements.ToList().OfType<Group>().ToList().ForEach(graphView.RemoveElement);
            // graphView.graphElements.ToList().OfType<StickyNote>().ToList().ForEach(graphView.RemoveElement);
            // graphView.graphElements.ToList().OfType<BlackboardRow>().ToList().ForEach(graphView.RemoveElement);

            // Create & add graph elements 
            GraphObject.GraphData.Nodes.ForEach(node => {
                AddNode(node);
                
                node.Node.SetExpandedWithoutNotify(true);
                node.Node.RefreshPorts();
                
                if (node.Node is OutputNode outputNode) {
                    graphView.GraphOutputNode = outputNode;
                }
            });
            GraphObject.GraphData.Edges.ForEach(edge => {
                AbstractNode outputNode = GraphView.nodes.First(node => node.viewDataKey == edge.Output) as AbstractNode;
                AbstractNode inputNode = GraphView.nodes.First(node => node.viewDataKey == edge.Input) as AbstractNode;
                GraphFrameworkPort outputPort = (GraphFrameworkPort)outputNode.Owner.GuidPortDictionary[edge.OutputPort];
                GraphFrameworkPort inputPort = (GraphFrameworkPort)inputNode.Owner.GuidPortDictionary[edge.InputPort];

                if (inputPort != null && outputPort != null) {
                    RuntimePort runtimeOutput = outputPort.node.RuntimePortDictionary[outputPort];
                    RuntimePort runtimeInput = inputPort.node.RuntimePortDictionary[inputPort];
                    Connection connection = new Connection { Output = runtimeOutput, Input = runtimeInput };
                    runtimeOutput.Node.NotifyConnectionCreated(connection, runtimeOutput);
                    runtimeInput.Node.NotifyConnectionCreated(connection, runtimeInput);
                } else {
                    Debug.Log("Edge ports were null");
                }

                AddEdge(edge);
            });
            
            // Refresh expanded state
            GraphObject.GraphData.Nodes.ForEach(node => {
                node.Node.SetExpandedWithoutNotify(node.DrawState.Expanded);
                node.Node.RefreshPorts();
            });

            GraphObject.GraphData.Properties.ForEach(AddProperty);
        }

        public void HandleChanges() {
            if (GraphObject.GraphData.AddedProperties.Any() || GraphObject.GraphData.RemovedProperties.Any())
                searchWindowProvider.RegenerateEntries = true;
            blackboardProvider.HandleChanges();

            foreach (SerializedNode removedNode in GraphObject.GraphData.RemovedNodes) {
                removedNode.Node.NotifyRuntimeNodeRemoved();
                if (removedNode.Node is OutputNode) {
                    GraphView.GraphOutputNode = null;
                    searchWindowProvider.RegenerateEntries = true;
                }
                RemoveNode(removedNode);
                GraphObject.RuntimeGraph.OnNodeRemoved(removedNode.Node.Runtime);
            }

            foreach (SerializedEdge removedEdge in GraphObject.GraphData.RemovedEdges) {
                GraphObject.RuntimeGraph.OnConnectionRemoved(removedEdge.OutputPort, removedEdge.InputPort);

                RemoveEdge(removedEdge);
            }

            bool anyCollapsedNode = false;
            foreach (SerializedNode addedNode in GraphObject.GraphData.AddedNodes) {
                AddNode(addedNode);
                if (addedNode.DrawState.Expanded == false) {
                    anyCollapsedNode = true;
                    addedNode.Node.SetExpandedWithoutNotify(true);
                    addedNode.Node.RefreshPorts();
                }
                if (addedNode.Node is OutputNode outputNode) {
                    graphView.GraphOutputNode = outputNode;
                    searchWindowProvider.RegenerateEntries = true;
                }
                GraphObject.RuntimeGraph.OnNodeAdded(addedNode.Node.Runtime);
                addedNode.Node.OnPropertyUpdated(null);
            }

            foreach (SerializedEdge addedEdge in GraphObject.GraphData.AddedEdges) {
                AddEdge(addedEdge);
                GraphFrameworkPort outputPort = (GraphFrameworkPort)addedEdge.Edge?.output;
                GraphFrameworkPort inputPort = (GraphFrameworkPort)addedEdge.Edge?.input;

                if (inputPort != null && outputPort != null) {
                    RuntimePort runtimeOutput = outputPort.node.RuntimePortDictionary[outputPort];
                    RuntimePort runtimeInput = inputPort.node.RuntimePortDictionary[inputPort];
                    Connection connection = new Connection { Output = runtimeOutput, OutputGuid = runtimeOutput.Guid, Input = runtimeInput, InputGuid = runtimeInput.Guid};
                    GraphObject.RuntimeGraph.OnConnectionAdded(connection);
                }
            }

            if (anyCollapsedNode) {
                foreach (SerializedNode addedNode in GraphObject.GraphData.AddedNodes) {
                    addedNode.Node.SetExpandedWithoutNotify(addedNode.DrawState.Expanded);
                    addedNode.Node.RefreshPorts();
                }
            }

            foreach (SerializedNode queuedNode in GraphObject.GraphData.NodeSelectionQueue) {
                graphView.AddToSelection(queuedNode.Node);
            }

            foreach (SerializedEdge queuedEdge in GraphObject.GraphData.EdgeSelectionQueue) {
                graphView.AddToSelection(queuedEdge.Edge);
            }
        }

        public void AddNode(SerializedNode nodeToAdd) {
            nodeToAdd.BuildNode(this, edgeConnectorListener);
            graphView.AddElement(nodeToAdd.Node);
        }

        public void RemoveNode(SerializedNode nodeToRemove) {
            if (nodeToRemove.Node != null)
                graphView.RemoveElement(nodeToRemove.Node);
            else {
                Node view = graphView.GetNodeByGuid(nodeToRemove.GUID);
                if (view != null)
                    graphView.RemoveElement(view);
            }
        }

        public void AddEdge(SerializedEdge edgeToAdd) {
            edgeToAdd.BuildEdge(this);
            graphView.AddElement(edgeToAdd.Edge);
        }

        public void RemoveEdge(SerializedEdge edgeToRemove) {
            if (edgeToRemove.Edge != null) {
                edgeToRemove.Edge.input?.Disconnect(edgeToRemove.Edge);
                edgeToRemove.Edge.output?.Disconnect(edgeToRemove.Edge);
                graphView.RemoveElement(edgeToRemove.Edge);
            }
        }

        public void AddProperty(AbstractProperty property) {
            blackboardProvider.AddInputRow(property);
        }

        public void Dispose() {
            if (searchWindowProvider != null) {
                Object.DestroyImmediate(searchWindowProvider);
                searchWindowProvider = null;
            }
        }
    }
}