using System;
using System.Linq;
using GeometryGraph.Runtime.Graph;
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

        private readonly GraphFrameworkGraphView graphFrameworkGraphView;
        private readonly GraphFrameworkEditorWindow editorWindow;
        private readonly GraphFrameworkObject graphObject;
        private readonly BlackboardProvider blackboardProvider;
        private readonly EdgeConnectorListener edgeConnectorListener;
        private SearchWindowProvider searchWindowProvider;

        public bool IsBlackboardVisible {
            get => blackboardProvider.Blackboard.style.display == DisplayStyle.Flex;
            set => blackboardProvider.Blackboard.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public GraphFrameworkEditorWindow EditorWindow => editorWindow;
        public GraphFrameworkObject GraphObject => graphObject;
        public GraphFrameworkGraphView GraphFrameworkGraphView => graphFrameworkGraphView;
        public EdgeConnectorListener EdgeConnectorListener => edgeConnectorListener;

        public EditorView(GraphFrameworkEditorWindow editorWindow, GraphFrameworkObject graphObject) {
            this.graphObject = graphObject;
            this.editorWindow = editorWindow;
            this.AddStyleSheet("Styles/Graph");

            var toolbar = new IMGUIContainer(() => {
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

                GUILayout.FlexibleSpace();
                IsBlackboardVisible = GUILayout.Toggle(IsBlackboardVisible, "Blackboard", EditorStyles.toolbarButton);
                graphObject.IsBlackboardVisible = IsBlackboardVisible;

                GUILayout.EndHorizontal();
            });

            Add(toolbar);
            var content = new VisualElement { name = "content" };
            {
                graphFrameworkGraphView = new GraphFrameworkGraphView(this);
                graphFrameworkGraphView.SetupZoom(0.05f, 8f);
                graphFrameworkGraphView.AddManipulator(new ContentDragger());
                graphFrameworkGraphView.AddManipulator(new SelectionDragger());
                graphFrameworkGraphView.AddManipulator(new RectangleSelector());
                graphFrameworkGraphView.AddManipulator(new ClickSelector());
                graphFrameworkGraphView.RegisterCallback<KeyDownEvent>(OnKeyDown);
                content.Add(graphFrameworkGraphView);

                var grid = new GridBackground();
                graphFrameworkGraphView.Insert(0, grid);
                grid.StretchToParentSize();

                blackboardProvider = new BlackboardProvider(this);
                graphFrameworkGraphView.Add(blackboardProvider.Blackboard);

                graphFrameworkGraphView.graphViewChanged += OnGraphViewChanged;
            }

            searchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            searchWindowProvider.Initialize(this.editorWindow, this);

            graphFrameworkGraphView.nodeCreationRequest = ctx => {
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
                foreach (var node in graphViewChange.movedElements.OfType<AbstractNode>()) {
                    var rect = node.parent.ChangeCoordinatesTo(graphFrameworkGraphView.contentViewContainer, node.GetPosition());
                    node.Owner.DrawState.Position = rect;
                }
            }

            if (graphViewChange.edgesToCreate != null) {
                editorWindow.GraphObject.RegisterCompleteObjectUndo("Created edges");
                foreach (var edge in graphViewChange.edgesToCreate) {
                    graphObject.GraphData.AddEdge(edge);
                }

                graphViewChange.edgesToCreate.Clear();
            }

            if (graphViewChange.elementsToRemove != null) {
                editorWindow.GraphObject.RegisterCompleteObjectUndo("Removed elements");
                foreach (var node in graphViewChange.elementsToRemove.OfType<AbstractNode>()) {
                    graphObject.GraphData.RemoveNode(node.Owner);
                }

                foreach (var edge in graphViewChange.elementsToRemove.OfType<Edge>()) {
                    graphObject.GraphData.RemoveEdge((SerializedEdge)edge.userData);
                }

                foreach (var property in graphViewChange.elementsToRemove.OfType<BlackboardField>()) {
                    GraphObject.GraphData.RemoveProperty(property.userData as AbstractProperty);
                }
            }

            return graphViewChange;
        }

        private void OnKeyDown(KeyDownEvent evt) {
            if (evt.actionKey && evt.keyCode == KeyCode.G) {
                if (graphFrameworkGraphView.selection.OfType<GraphElement>().Any()) {
                    // TODO: GROUP
                }
            }

            if (evt.actionKey && evt.keyCode == KeyCode.U) {
                if (graphFrameworkGraphView.selection.OfType<GraphElement>().Any()) {
                    // TODO: UNGROUP
                }
            }
        }

        public void BuildGraph() {
            Debug.Log("Build graph");
            // Remove existing elements
            graphFrameworkGraphView.graphElements.ToList().OfType<Node>().ToList().ForEach(graphFrameworkGraphView.RemoveElement);
            graphFrameworkGraphView.graphElements.ToList().OfType<Edge>().ToList().ForEach(graphFrameworkGraphView.RemoveElement);
            graphFrameworkGraphView.graphElements.ToList().OfType<Group>().ToList().ForEach(graphFrameworkGraphView.RemoveElement);
            graphFrameworkGraphView.graphElements.ToList().OfType<StickyNote>().ToList().ForEach(graphFrameworkGraphView.RemoveElement);
            graphFrameworkGraphView.graphElements.ToList().OfType<BlackboardRow>().ToList().ForEach(graphFrameworkGraphView.RemoveElement);

            // Create & add graph elements 
            graphObject.GraphData.Nodes.ForEach(node => {
                AddNode(node);
                if (node.Node is OutputNode outputNode) {
                    graphFrameworkGraphView.GraphOutputNode = outputNode;
                }
            });
            graphObject.GraphData.Edges.ForEach(edge => {
                var outputNode = GraphFrameworkGraphView.nodes.First(node => node.viewDataKey == edge.Output) as AbstractNode;
                var inputNode = GraphFrameworkGraphView.nodes.First(node => node.viewDataKey == edge.Input) as AbstractNode;
                var outputPort = (GraphFrameworkPort)outputNode.Owner.GuidPortDictionary[edge.OutputPort];
                var inputPort = (GraphFrameworkPort)inputNode.Owner.GuidPortDictionary[edge.InputPort];

                if (inputPort != null && outputPort != null) {
                    var runtimeOutput = outputPort.node.RuntimePortDictionary[outputPort];
                    var runtimeInput = inputPort.node.RuntimePortDictionary[inputPort];
                    var connection = new Connection { Output = runtimeOutput, Input = runtimeInput };
                    runtimeOutput.Node.OnConnectionCreated(connection);
                    runtimeInput.Node.OnConnectionCreated(connection);
                } else {
                    Debug.Log("Edge ports were null");
                }

                AddEdge(edge);
            });
            graphObject.GraphData.Properties.ForEach(AddProperty);
        }

        public void HandleChanges() {
            if (graphObject.GraphData.AddedProperties.Any() || graphObject.GraphData.RemovedProperties.Any())
                searchWindowProvider.RegenerateEntries = true;
            blackboardProvider.HandleChanges();

            foreach (var removedNode in graphObject.GraphData.RemovedNodes) {
                removedNode.Node.NotifyRuntimeNodeRemoved();
                if (removedNode.Node is OutputNode) {
                    GraphFrameworkGraphView.GraphOutputNode = null;
                    searchWindowProvider.RegenerateEntries = true;
                }
                RemoveNode(removedNode);
                graphObject.RuntimeGraph.OnNodeRemoved(removedNode.Node.Runtime);
            }

            foreach (var removedEdge in graphObject.GraphData.RemovedEdges) {
                var inputPort = (GraphFrameworkPort)removedEdge.Edge?.input;
                var outputPort = (GraphFrameworkPort)removedEdge.Edge?.output;
                if (inputPort == null || outputPort == null) {
                } else {
                    var runtimeOutput = outputPort.node.RuntimePortDictionary[outputPort];
                    var runtimeInput = inputPort.node.RuntimePortDictionary[inputPort];
                    runtimeInput.Node.OnConnectionRemoved(runtimeOutput, runtimeInput);
                    runtimeOutput.Node.OnConnectionRemoved(runtimeOutput, runtimeInput);
                    graphObject.RuntimeGraph.OnConnectionRemoved(runtimeOutput, runtimeInput);
                }

                RemoveEdge(removedEdge);
            }

            foreach (var addedNode in graphObject.GraphData.AddedNodes) {
                AddNode(addedNode);
                if (addedNode.Node is OutputNode outputNode) {
                    graphFrameworkGraphView.GraphOutputNode = outputNode;
                    searchWindowProvider.RegenerateEntries = true;
                }
                graphObject.RuntimeGraph.OnNodeAdded(addedNode.Node.Runtime);
                addedNode.Node.OnPropertyUpdated(null);
            }

            foreach (var addedEdge in graphObject.GraphData.AddedEdges) {
                AddEdge(addedEdge);
                var outputPort = (GraphFrameworkPort)addedEdge.Edge?.output;
                var inputPort = (GraphFrameworkPort)addedEdge.Edge?.input;

                if (inputPort != null && outputPort != null) {
                    var runtimeOutput = outputPort.node.RuntimePortDictionary[outputPort];
                    var runtimeInput = inputPort.node.RuntimePortDictionary[inputPort];
                    var connection = new Connection { Output = runtimeOutput, Input = runtimeInput };
                    runtimeOutput.Node.OnConnectionCreated(connection);
                    runtimeInput.Node.OnConnectionCreated(connection);
                    graphObject.RuntimeGraph.OnConnectionAdded(connection);
                }
            }

            foreach (var queuedNode in graphObject.GraphData.NodeSelectionQueue) {
                graphFrameworkGraphView.AddToSelection(queuedNode.Node);
            }

            foreach (var queuedEdge in graphObject.GraphData.EdgeSelectionQueue) {
                graphFrameworkGraphView.AddToSelection(queuedEdge.Edge);
            }
        }

        public void AddNode(SerializedNode nodeToAdd) {
            nodeToAdd.BuildNode(this, edgeConnectorListener);
            graphFrameworkGraphView.AddElement(nodeToAdd.Node);
        }

        public void RemoveNode(SerializedNode nodeToRemove) {
            if (nodeToRemove.Node != null)
                graphFrameworkGraphView.RemoveElement(nodeToRemove.Node);
            else {
                var view = graphFrameworkGraphView.GetNodeByGuid(nodeToRemove.GUID);
                if (view != null)
                    graphFrameworkGraphView.RemoveElement(view);
            }
        }

        public void AddEdge(SerializedEdge edgeToAdd) {
            edgeToAdd.BuildEdge(this);
            graphFrameworkGraphView.AddElement(edgeToAdd.Edge);
        }

        public void RemoveEdge(SerializedEdge edgeToRemove) {
            if (edgeToRemove.Edge != null) {
                edgeToRemove.Edge.input?.Disconnect(edgeToRemove.Edge);
                edgeToRemove.Edge.output?.Disconnect(edgeToRemove.Edge);
                graphFrameworkGraphView.RemoveElement(edgeToRemove.Edge);
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