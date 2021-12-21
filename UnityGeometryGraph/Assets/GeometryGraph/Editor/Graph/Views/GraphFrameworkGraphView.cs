using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    internal static class GraphViewReflectionHelper {
        private static readonly MethodInfo requestNodeCreationMethodInfo = typeof(GraphView).GetMethod("RequestNodeCreation", BindingFlags.Instance | BindingFlags.NonPublic);
        public static Action<VisualElement, int, Vector2> RequestNodeCreation { get; private set; }
        
        public static void CreateDelegate(GraphView instance) {
            RequestNodeCreation = (Action<VisualElement, int, Vector2>)requestNodeCreationMethodInfo.CreateDelegate(typeof(Action<VisualElement, int, Vector2>), instance);
        }
        
    }
    
    public class GraphFrameworkGraphView : GraphView {
        private EditorView editorView;
        public EditorView EditorView => editorView;
        public GraphFrameworkData GraphData => editorView.GraphObject.GraphData;

        public OutputNode GraphOutputNode;

        protected override bool canCutSelection {
            get {
                bool onlyOutputNode = selection.OfType<AbstractNode>().SequenceEqual(new[] { GraphOutputNode });
                return !onlyOutputNode && base.canCutSelection;
            }
        }

        protected override bool canDuplicateSelection {
            get {
                bool onlyOutputNode = selection.OfType<AbstractNode>().SequenceEqual(new[] { GraphOutputNode });
                return !onlyOutputNode && base.canDuplicateSelection;
            }
        }

        protected override bool canCopySelection {
            get {
                bool onlyOutputNode = selection.OfType<AbstractNode>().SequenceEqual(new[] { GraphOutputNode });
                if (onlyOutputNode) return false;

                return selection.OfType<AbstractNode>().Any() || selection.OfType<Group>().Any() || selection.OfType<BlackboardField>().Any();
            }
        }

        public GraphFrameworkGraphView(EditorView editorView) {
            this.editorView = editorView;
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerformed);
            serializeGraphElements = SerializeGraphElementsImpl;
            unserializeAndPaste = UnserializeAndPasteImpl;
            deleteSelection = DeleteSelectionImpl;
            
            GraphViewReflectionHelper.CreateDelegate(this);
        }

        private void UnserializeAndPasteImpl(string operation, string data) {
            editorView.GraphObject.RegisterCompleteObjectUndo(operation);
            CopyPasteData copyPasteData = CopyPasteData.FromJson(data);
            this.InsertCopyPasteData(copyPasteData);
        }

        private string SerializeGraphElementsImpl(IEnumerable<GraphElement> elements) {
            List<GraphElement> elementsList = elements.ToList();
            IEnumerable<SerializedNode> nodes =
                elementsList
                    .OfType<AbstractNode>()
                    .Where(node => !(node is OutputNode))
                    .Select(x => x.Owner);
            IEnumerable<SerializedEdge> edges =
                elementsList
                    .OfType<Edge>()
                    .Where(edge => !(edge.output.node is OutputNode || edge.input.node is OutputNode))
                    .Select(x => x.userData).OfType<SerializedEdge>();
            IEnumerable<AbstractProperty> properties = selection.OfType<BlackboardField>().Select(x => x.userData as AbstractProperty);

            // Collect the property nodes and get the corresponding properties
            IEnumerable<string> propertyNodeGuids = this.nodes.Where(node => ((AbstractNode)node).IsProperty).Select(node => ((AbstractNode)node).PropertyGuid);
            IEnumerable<AbstractProperty> metaProperties = editorView.GraphObject.GraphData.Properties.Where(x => propertyNodeGuids.Contains(x.GUID));

            CopyPasteData copyPasteData = new CopyPasteData(editorView, nodes, edges, properties, metaProperties);
            return JsonUtility.ToJson(copyPasteData);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            if (evt.target is GraphView && nodeCreationRequest != null) {
                evt.menu.AppendAction("Create Node _Space", new Action<DropdownMenuAction>(this.OnContextMenuNodeCreate), DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendSeparator();
            }

            if (evt.target is GraphView or Node or Group or BlackboardField)
                evt.menu.AppendAction("Cut %x", _ => CutSelectionCallback(),
                                      _ => canCutSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            if (evt.target is GraphView or Node or Group or BlackboardField)
                evt.menu.AppendAction("Copy %c", _ => CopySelectionCallback(),
                                      _ => canCopySelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            if (evt.target is GraphView)
                evt.menu.AppendAction("Paste %v", _ => PasteCallback(),
                                      _ =>
                                          canPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            if (evt.target is GraphView or Node or Group or Edge or BlackboardField) {
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Delete _Delete", _ => DeleteSelectionCallback(AskUser.DontAskUser),
                                      _ => canDeleteSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }

            if (evt.target is not GraphView && evt.target is not Node && evt.target is not Group && evt.target is not BlackboardField)
                return;
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Duplicate %d", _ => DuplicateSelectionCallback(),
                                  _ => canDuplicateSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
        }

        private void OnContextMenuNodeCreate(DropdownMenuAction a) => GraphViewReflectionHelper.RequestNodeCreation(null, -1, a.eventInfo.mousePosition);
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(port => {
                if (startPort != port /*&& startPort.node != port.node*/ && port.direction != startPort.direction &&
                    (startPort as GraphFrameworkPort).IsCompatibleWith(port as GraphFrameworkPort)) {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        private void DeleteSelectionImpl(string operation, AskUser askUser) {
            IEnumerable<SerializedNode> nodesToDelete = selection.OfType<AbstractNode>().Select(node => node.Owner);
            editorView.GraphObject.RegisterCompleteObjectUndo(operation);
            editorView.GraphObject.GraphData.RemoveElements(nodesToDelete.ToList(),
                                                            selection.OfType<Edge>().Select(e => e.userData).OfType<SerializedEdge>().ToList());

            foreach (ISelectable selectable in selection) {
                if (!(selectable is BlackboardField field) || field.userData == null) continue;
                AbstractProperty property = (AbstractProperty)field.userData;
                editorView.GraphObject.GraphData.RemoveProperty(property);
            }

            selection.Clear();
        }

        #region Drag & Drop

        private void OnDragUpdated(DragUpdatedEvent evt) {
            List<ISelectable> selection = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            bool dragging = false;
            if (selection != null)
                if (selection.OfType<BlackboardField>().Any())
                    dragging = true;

            if (dragging) {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }
        }

        private void OnDragPerformed(DragPerformEvent evt) {
            Vector2 localPos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);

            List<ISelectable> selection = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            if (selection != null) {
                // Blackboard
                if (selection.OfType<BlackboardField>().Any()) {
                    IEnumerable<BlackboardField> fields = selection.OfType<BlackboardField>();
                    foreach (BlackboardField field in fields) {
                        CreateNode(field, localPos);
                    }
                }
            }
        }

        private void CreateNode(object obj, Vector2 nodePosition) {
            if (obj is BlackboardField blackboardField) {
                editorView.GraphObject.RegisterCompleteObjectUndo("Drag Blackboard Field");
                AbstractProperty property = blackboardField.userData as AbstractProperty;
                SerializedNode node = new SerializedNode(PropertyUtils.PropertyTypeToNodeType(property.Type), new Rect(nodePosition, EditorView.DefaultNodeSize));
                editorView.GraphObject.GraphData.AddNode(node);
                node.BuildNode(editorView, editorView.EdgeConnectorListener, false);

                AbstractNode propertyNode = node.Node;
                propertyNode.PropertyGuid = property.GUID;
                propertyNode.Property = property;
                node.BuildPortData();
            }
        }

        #endregion
    }
}