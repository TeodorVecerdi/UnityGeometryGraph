using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class BlackboardPropertyView : VisualElement {
        private readonly BlackboardField field;
        private readonly EditorView editorView;
        private AbstractProperty property;
        private VisualElement defaultValueField;

        private List<VisualElement> rows;
        public List<VisualElement> Rows => rows;

        private int undoGroup = -1;
        public int UndoGroup => undoGroup;

        private static readonly Type contextualMenuManipulator = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).FirstOrDefault(t => t.FullName == "UnityEngine.UIElements.ContextualMenuManipulator");
        private IManipulator resetReferenceMenu;

        private readonly EventCallback<KeyDownEvent> keyDownCallback;
        public EventCallback<KeyDownEvent> KeyDownCallback => keyDownCallback;
        private readonly EventCallback<FocusOutEvent> focusOutCallback;
        public EventCallback<FocusOutEvent> FocusOutCallback => focusOutCallback;

        public BlackboardPropertyView(BlackboardField field, EditorView editorView, AbstractProperty property) {
            this.AddStyleSheet("Styles/PropertyView/Blackboard");
            this.field = field;
            this.editorView = editorView;
            this.property = property;
            rows = new List<VisualElement>();

            keyDownCallback = evt => {
                // Record Undo for input field edit
                if (undoGroup == -1) {
                    undoGroup = Undo.GetCurrentGroup();
                    editorView.GraphObject.RegisterCompleteObjectUndo("Change property value");
                }

                // Handle escaping input field edit
                if (evt.keyCode == KeyCode.Escape && undoGroup > -1) {
                    Undo.RevertAllDownToGroup(undoGroup);
                    undoGroup = -1;
                    evt.StopPropagation();
                }

                // Don't record Undo again until input field is unfocused
                undoGroup++;
                MarkDirtyRepaint();
            };

            focusOutCallback = _ => undoGroup = -1;

            BuildFields(property);
            AddToClassList("blackboardPropertyView");
        }

        private void BuildFields(AbstractProperty property) {
            switch (property) {
                case GeometryObjectProperty:
                case GeometryCollectionProperty:
                    return;

                case IntegerProperty integerProperty: {
                    defaultValueField = new IntegerField() {isDelayed = true, value = integerProperty.Value};
                    var intField = (IntegerField) defaultValueField;
                    intField.RegisterValueChangedCallback(evt => {
                        editorView.GraphObject.RegisterCompleteObjectUndo($"Change {property.DisplayName} default value");
                        integerProperty.Value = evt.newValue;
                        editorView.GraphObject.RuntimeGraph.OnPropertyDefaultValueChanged(integerProperty.GUID, evt.newValue);
                    });
                    break;
                }
                case FloatProperty floatProperty: {
                    defaultValueField = new FloatField() {isDelayed = true, value = floatProperty.Value};
                    var floatField = (FloatField) defaultValueField;
                    floatField.RegisterValueChangedCallback(evt => {
                        editorView.GraphObject.RegisterCompleteObjectUndo($"Change {property.DisplayName} default value");
                        floatProperty.Value = evt.newValue;
                        editorView.GraphObject.RuntimeGraph.OnPropertyDefaultValueChanged(floatProperty.GUID, evt.newValue);
                    });
                    break;
                }
                case VectorProperty vectorProperty: {
                    defaultValueField = new Vector3Field() {value = vectorProperty.Value};
                    var vecField = (Vector3Field) defaultValueField;
                    vecField.RegisterValueChangedCallback(evt => {
                        editorView.GraphObject.RegisterCompleteObjectUndo($"Change {property.DisplayName} default value");
                        vectorProperty.Value = evt.newValue;
                        editorView.GraphObject.RuntimeGraph.OnPropertyDefaultValueChanged(vectorProperty.GUID, (float3)evt.newValue);
                    });
                    break;
                }

                default: throw new ArgumentOutOfRangeException(nameof(property), property, null);
            }
            AddRow("Default Value", defaultValueField);
        }

        private void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
        }

        public VisualElement AddRow(string labelText, VisualElement control, bool enabled = true) {
            var rowView = CreateRow(labelText, control, enabled);
            Add(rowView);
            rows.Add(rowView);
            return rowView;
        }

        public void Rebuild() {
            rows.Where(t => t.parent == this).ToList().ForEach(Remove);
            BuildFields(property);
        }

        private VisualElement CreateRow(string labelText, VisualElement control, bool enabled) {
            var rowView = new VisualElement();
            rowView.AddToClassList("rowView");
            if (!string.IsNullOrEmpty(labelText)) {
                var label = new Label(labelText);
                label.SetEnabled(enabled);
                label.AddToClassList("rowViewLabel");
                rowView.Add(label);
            }

            control.AddToClassList("rowViewControl");
            control.SetEnabled(enabled);

            rowView.Add(control);
            return rowView;
        }
    }
}