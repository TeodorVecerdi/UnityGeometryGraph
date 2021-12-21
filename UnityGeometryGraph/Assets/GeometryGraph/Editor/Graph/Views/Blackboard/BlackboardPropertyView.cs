using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class BlackboardPropertyView : VisualElement {
        private readonly BlackboardField field;
        private readonly EditorView editorView;
        private readonly AbstractProperty property;
        private readonly List<VisualElement> rows;
        
        public BlackboardPropertyView(BlackboardField field, EditorView editorView, AbstractProperty property) {
            this.field = field;
            this.editorView = editorView;
            this.property = property;
            rows = new List<VisualElement>();

            BuildFields(property);
            AddToClassList("blackboardPropertyView");
        }

        private void BuildFields(AbstractProperty property) {
            VisualElement defaultValueField;
            switch (property) {
                case GeometryObjectProperty:
                case GeometryCollectionProperty:
                    return;

                case IntegerProperty integerProperty: {
                    defaultValueField = new IntegerField("Default Value") {isDelayed = true, value = integerProperty.Value};
                    IntegerField intField = (IntegerField) defaultValueField;
                    intField.RegisterValueChangedCallback(evt => {
                        editorView.GraphObject.RegisterCompleteObjectUndo($"Change {property.DisplayName} default value");
                        integerProperty.Value = evt.newValue;
                        editorView.GraphObject.RuntimeGraph.OnPropertyDefaultValueChanged(integerProperty.GUID, evt.newValue);
                    });
                    break;
                }
                case FloatProperty floatProperty: {
                    defaultValueField = new FloatField("Default Value") {isDelayed = true, value = floatProperty.Value};
                    FloatField floatField = (FloatField) defaultValueField;
                    floatField.RegisterValueChangedCallback(evt => {
                        editorView.GraphObject.RegisterCompleteObjectUndo($"Change {property.DisplayName} default value");
                        floatProperty.Value = evt.newValue;
                        editorView.GraphObject.RuntimeGraph.OnPropertyDefaultValueChanged(floatProperty.GUID, evt.newValue);
                    });
                    break;
                }
                case VectorProperty vectorProperty: {
                    defaultValueField = new Vector3Field("Default Value") {value = vectorProperty.Value};
                    Vector3Field vecField = (Vector3Field) defaultValueField;
                    vecField.RegisterValueChangedCallback(evt => {
                        editorView.GraphObject.RegisterCompleteObjectUndo($"Change {property.DisplayName} default value");
                        vectorProperty.Value = evt.newValue;
                        editorView.GraphObject.RuntimeGraph.OnPropertyDefaultValueChanged(vectorProperty.GUID, (float3)evt.newValue);
                    });
                    break;
                }

                case StringProperty stringProperty: {
                    defaultValueField = new TextField("Default Value") {value = stringProperty.Value};
                    TextField textField = (TextField) defaultValueField;
                    textField.RegisterValueChangedCallback(evt => {
                        editorView.GraphObject.RegisterCompleteObjectUndo($"Change {property.DisplayName} default value");
                        stringProperty.Value = evt.newValue;
                        editorView.GraphObject.RuntimeGraph.OnPropertyDefaultValueChanged(stringProperty.GUID, evt.newValue);
                    });
                    break;
                }

                default: throw new ArgumentOutOfRangeException(nameof(property), property, null);
            }

            TextField referenceNameField = new("Reference Name");
            referenceNameField.SetValueWithoutNotify(property.ReferenceName);
            referenceNameField.RegisterValueChangedCallback(evt => {
                string newValue = evt.newValue;
                if (newValue == property.ReferenceName) return;
                
                if (string.IsNullOrEmpty(newValue)) {
                    newValue = property.GetDefaultReferenceName();
                    referenceNameField.SetValueWithoutNotify(newValue);
                }
                
                editorView.GraphObject.RegisterCompleteObjectUndo("Edit Property Reference Name");
                editorView.GraphObject.GraphData.SanitizePropertyReference(property, evt.newValue);
                referenceNameField.SetValueWithoutNotify(newValue);
                editorView.GraphObject.RuntimeGraph.OnPropertyReferenceNameUpdated(property.GUID, property.ReferenceName);
            });
            
            AddRow(referenceNameField);
            AddRow(defaultValueField);
        }

        public void AddRow(VisualElement control) {
            VisualElement rowView = new();
            rowView.AddToClassList("rowView");
            
            control.AddToClassList("rowViewControl");
            rowView.Add(control);

            Add(rowView);
            rows.Add(rowView);
        }

        public void Rebuild() {
            rows.ForEach(Remove);
            BuildFields(property);
        }
    }
}