using System.Collections.Generic;
using System.Linq;
using GraphFramework.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Node = UnityEditor.Experimental.GraphView.Node;

namespace GraphFramework.Editor {
    public class BlackboardProvider {
        private static readonly Texture2D exposedIcon = Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed");
        public Blackboard Blackboard { get; private set; }
        private readonly EditorView editorView;
        private readonly Dictionary<string, BlackboardRow> inputRows;
        
        private readonly BlackboardSection section;
        // private readonly BlackboardSection checkSection;
        // private readonly BlackboardSection triggerSection;
        // private readonly BlackboardSection actorSection;
        
        private readonly List<Node> selectedNodes = new List<Node>();
        private readonly Dictionary<string, bool> expandedInputs = new Dictionary<string, bool>();

        public Dictionary<string, bool> ExpandedInputs => expandedInputs;
        
        public string AssetName {
            get => Blackboard.title;
            set => Blackboard.title = value;
        }

        public BlackboardProvider(EditorView editorView) {
            this.editorView = editorView;
            inputRows = new Dictionary<string, BlackboardRow>();
            Blackboard = new Blackboard {
                scrollable = true,
                title = "Properties",
                subTitle = "Graph",
                editTextRequested = EditTextRequested,
                addItemRequested = AddItemRequested,
                moveItemRequested = MoveItemRequested
            };

            section = new BlackboardSection {title = "Properties"};
            Blackboard.Add(section);
            // checkSection = new BlackboardSection {title = "Checks"};
            // Blackboard.Add(checkSection);
            // triggerSection = new BlackboardSection {title = "Triggers"};
            // Blackboard.Add(triggerSection);
            // actorSection = new BlackboardSection {title = "Actors"};
            // Blackboard.Add(actorSection);
        }

        private void EditTextRequested(Blackboard blackboard, VisualElement visualElement, string newText) {
            var field = (BlackboardField) visualElement;
            var property = (AbstractProperty) field.userData;
            if (!string.IsNullOrEmpty(newText) && newText != property.DisplayName) {
                editorView.GraphObject.RegisterCompleteObjectUndo("Edit Property Name");
                property.DisplayName = newText;
                editorView.GraphObject.GraphData.SanitizePropertyName(property);
                field.text = property.DisplayName;
                var modifiedNodes = editorView.GraphObject.GraphData.Nodes.Where(node => node.Node is PropertyNode propertyNode && propertyNode.PropertyGuid == property.GUID).Select(node => node.Node as PropertyNode);
                foreach (var modifiedNode in modifiedNodes) {
                    modifiedNode?.Update(property);
                }
            }
        }

        private void MoveItemRequested(Blackboard blackboard, int newIndex, VisualElement visualElement) {
            if (!(visualElement.userData is AbstractProperty property))
                return;

            editorView.GraphObject.RegisterCompleteObjectUndo("Move Property");
            editorView.GraphObject.GraphData.MoveProperty(property, newIndex);
        }

        private void AddItemRequested(Blackboard blackboard) {
            var menu = new GenericMenu();
            // Note: Left as reference
            // menu.AddItem(new GUIContent("Check"), false, () => AddInputRow(new CheckProperty(), true));
            // menu.AddItem(new GUIContent("Trigger"), false, () => AddInputRow(new TriggerProperty(), true));
            // menu.AddItem(new GUIContent("Actor"), false, () => AddInputRow(new ActorProperty(), true));
            menu.ShowAsContext();
        }

        public void AddInputRow(AbstractProperty property, bool create = false, int index = -1) {
            if (inputRows.ContainsKey(property.GUID))
                return;

            // Note: Select the relevant section here
            // var section = property.Type == PropertyType.Actor ? actorSection : property.Type == PropertyType.Check ? checkSection : triggerSection;

            if (create) {
                editorView.GraphObject.GraphData.SanitizePropertyName(property);
            }

            var field = new BlackboardField(exposedIcon, property.DisplayName, property.Type.ToString()) {userData = property};
            var row = new BlackboardRow(field, new BlackboardPropertyView(field, editorView, property)) {userData = property};
            if (index < 0)
                index = inputRows.Count;
            if (index == inputRows.Count)
                section.Add(row);
            else
                section.Insert(index, row);

            var pill = row.Q<Pill>();
            pill.RegisterCallback<MouseEnterEvent>(evt => OnMouseHover(evt, property));
            pill.RegisterCallback<MouseLeaveEvent>(evt => OnMouseHover(evt, property));
            pill.RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);

            var expandButton = row.Q<Button>("expandButton");
            expandButton.RegisterCallback<MouseDownEvent>(evt => OnExpanded(evt, property), TrickleDown.TrickleDown);
            inputRows[property.GUID] = row;
            
            if (!create)
                return;
            
            row.expanded = true;
            expandedInputs[property.GUID] = true;
            editorView.GraphObject.RegisterCompleteObjectUndo("Create Property");
            editorView.GraphObject.GraphData.AddProperty(property);
            field.OpenTextEditor();
        }

        private void OnExpanded(MouseDownEvent evt, AbstractProperty input) {
            expandedInputs[input.GUID] = !inputRows[input.GUID].expanded;
        }

        private void OnMouseHover(EventBase evt, AbstractProperty input) {
            if (evt.eventTypeId == MouseEnterEvent.TypeId()) {
                foreach (var node in editorView.GraphFrameworkGraphView.nodes.ToList()) {
                    if (node.viewDataKey == input.GUID) {
                        selectedNodes.Add(node);
                        node.AddToClassList("hovered");
                    }
                }
            } else if (evt.eventTypeId == MouseLeaveEvent.TypeId() && selectedNodes.Any()) {
                foreach (var node in selectedNodes) {
                    node.RemoveFromClassList("hovered");
                }

                selectedNodes.Clear();
            }
        }

        private void OnDragUpdatedEvent(DragUpdatedEvent evt) {
            if (selectedNodes.Any()) {
                foreach (var node in selectedNodes) {
                    node.RemoveFromClassList("hovered");
                }

                selectedNodes.Clear();
            }
        }

        public void HandleChanges() {
            foreach (var inputGuid in editorView.GraphObject.GraphData.RemovedProperties) {
                if (!inputRows.TryGetValue(inputGuid.GUID, out var row))
                    continue;

                row.RemoveFromHierarchy();
                inputRows.Remove(inputGuid.GUID);
            }

            foreach (var input in editorView.GraphObject.GraphData.AddedProperties)
                AddInputRow(input, index: editorView.GraphObject.GraphData.Properties.IndexOf(input));

            if (editorView.GraphObject.GraphData.MovedProperties.Any()) {
                foreach (var row in inputRows.Values)
                    row.RemoveFromHierarchy();

                foreach (var property in editorView.GraphObject.GraphData.Properties) {
                    section.Add(inputRows[property.GUID]);
                    // Note: Select the relevant section here 
                    // (property.Type == PropertyType.Actor ? actorSection : property.Type == PropertyType.Check ? checkSection : triggerSection).Add(inputRows[property.GUID]);
                }
            }

            expandedInputs.Clear();
        }
    }
}