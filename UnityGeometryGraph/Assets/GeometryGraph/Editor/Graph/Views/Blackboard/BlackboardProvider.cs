using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Graph;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class BlackboardProvider {
        private static readonly Texture2D exposedIcon = Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed");
        
        public Blackboard Blackboard { get; private set; }
        private readonly EditorView editorView;
        private readonly Dictionary<string, BlackboardRow> inputRows;
        private readonly BlackboardSection section;
        private readonly List<Node> selectedNodes = new();

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
            Blackboard.AddStyleSheet("Styles/Blackboard");
            Blackboard.RegisterCallback<KeyDownEvent>(evt => {
                if (evt.keyCode == KeyCode.F2) {
                    foreach (BlackboardRow child in section!.Children().OfType<BlackboardRow>()) {
                        BlackboardField field = child.Q<BlackboardField>();
                        if (field is not { selected: true }) continue;
                        
                        field.OpenTextEditor();
                        break;
                    }
                }
            });

            section = new BlackboardSection {title = "Properties"};
            Blackboard.Add(section);
        }

        private void EditTextRequested(Blackboard blackboard, VisualElement visualElement, string newText) {
            BlackboardField field = (BlackboardField) visualElement;
            AbstractProperty property = (AbstractProperty) field.userData;
            if (string.IsNullOrEmpty(newText) || newText == property.DisplayName) return;
            
            editorView.GraphObject.RegisterCompleteObjectUndo("Edit Property Name");
            property.DisplayName = newText;
            editorView.GraphObject.GraphData.SanitizePropertyName(property);
            field.text = property.DisplayName;
            Type propertyType = PropertyUtils.PropertyTypeToNodeType(property.Type);
            editorView.GraphObject.RuntimeGraph.OnPropertyDisplayNameUpdated(property.GUID, property.DisplayName);
            IEnumerable<AbstractNode> modifiedNodes = editorView.GraphObject.GraphData.Nodes.Where(node => node.Node.GetType() == propertyType).Select(node => node.Node);
            foreach (AbstractNode modifiedNode in modifiedNodes) {
                modifiedNode.OnPropertyUpdated(property);
            }
        }

        private void MoveItemRequested(Blackboard blackboard, int newIndex, VisualElement visualElement) {
            if (visualElement.userData is not AbstractProperty property)
                return;

            editorView.GraphObject.RegisterCompleteObjectUndo("Move Property");
            editorView.GraphObject.GraphData.MoveProperty(property, newIndex);
        }

        private void AddItemRequested(Blackboard blackboard) {
            GenericMenu menu = new();
            menu.AddItem(new GUIContent("Geometry Object"), false, () => AddInputRow(new GeometryObjectProperty(), true));
            menu.AddItem(new GUIContent("Geometry Collection"), false, () => AddInputRow(new GeometryCollectionProperty(), true));
            menu.AddItem(new GUIContent("Integer"), false, () => AddInputRow(new IntegerProperty(), true));
            menu.AddItem(new GUIContent("Float"), false, () => AddInputRow(new FloatProperty(), true));
            menu.AddItem(new GUIContent("Vector"), false, () => AddInputRow(new VectorProperty(), true));
            menu.AddItem(new GUIContent("String"), false, () => AddInputRow(new StringProperty(), true));
            menu.ShowAsContext();
        }

        public void AddInputRow(AbstractProperty property, bool create = false, int index = -1) {
            if (inputRows.ContainsKey(property.GUID))
                return;
            
            if (create) {
                editorView.GraphObject.GraphData.SanitizePropertyName(property);
            }

            string propertyTypeName = property.Type.ToString();
            BlackboardField field = new(exposedIcon, property.DisplayName, propertyTypeName) {userData = property};
            BlackboardRow row = new(field, new BlackboardPropertyView(field, editorView, property)) {userData = property};
            row.AddToClassList($"property-{propertyTypeName}");
            if (index < 0)
                index = inputRows.Count;
            if (index == inputRows.Count)
                section.Add(row);
            else
                section.Insert(index, row);

            Pill pill = row.Q<Pill>();
            pill.RegisterCallback<MouseEnterEvent>(evt => OnMouseHover(evt, property));
            pill.RegisterCallback<MouseLeaveEvent>(evt => OnMouseHover(evt, property));
            pill.RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
            
            field.Q<TextField>().RegisterCallback<BlurEvent>(_ => {
                field.Focus();
            });

            inputRows[property.GUID] = row;

            if (!create)
                return;

            row.expanded = false;
            editorView.GraphObject.RegisterCompleteObjectUndo("Create Property");
            editorView.GraphObject.GraphData.AddProperty(property);

            field.OpenTextEditor();
        }

        private void OnMouseHover(EventBase evt, AbstractProperty input) {
            if (evt.eventTypeId == MouseEnterEvent.TypeId()) {
                foreach (Node node in editorView.GraphView.nodes.ToList()) {
                    if (node.viewDataKey == input.GUID) {
                        selectedNodes.Add(node);
                        node.AddToClassList("hovered");
                    }
                }
            } else if (evt.eventTypeId == MouseLeaveEvent.TypeId() && selectedNodes.Count > 0) {
                foreach (Node node in selectedNodes) {
                    node.RemoveFromClassList("hovered");
                }

                selectedNodes.Clear();
            }
        }

        private void OnDragUpdatedEvent(DragUpdatedEvent evt) {
            if (selectedNodes.Count > 0) {
                foreach (Node node in selectedNodes) {
                    node.RemoveFromClassList("hovered");
                }

                selectedNodes.Clear();
            }
        }

        public void HandleChanges() {
            foreach (AbstractProperty property in editorView.GraphObject.GraphData.RemovedProperties) {
                if (!inputRows.TryGetValue(property.GUID, out BlackboardRow row))
                    continue;

                row.RemoveFromHierarchy();
                inputRows.Remove(property.GUID);

                editorView.GraphObject.RuntimeGraph.OnPropertyRemoved(property.GUID);
            }

            foreach (AbstractProperty property in editorView.GraphObject.GraphData.AddedProperties) {
                AddInputRow(property, index: editorView.GraphObject.GraphData.Properties.IndexOf(property));
                Property runtimeProperty = new() {Guid = property.GUID, DisplayName =  property.DisplayName, ReferenceName = property.ReferenceName, Type = property.Type, DefaultValue = new DefaultPropertyValue(property.Type, property.DefaultValue)};
                editorView.GraphObject.RuntimeGraph.OnPropertyAdded(runtimeProperty);
            }

            if (editorView.GraphObject.GraphData.MovedProperties.Count > 0) {
                foreach (BlackboardRow row in inputRows.Values)
                    row.RemoveFromHierarchy();

                foreach (AbstractProperty property in editorView.GraphObject.GraphData.Properties) {
                    section.Add(inputRows[property.GUID]);
                    // Note: Select the relevant section here
                    // (property.Type == PropertyType.Actor ? actorSection : property.Type == PropertyType.Check ? checkSection : triggerSection).Add(inputRows[property.GUID]);
                }
            }

        }
    }
}