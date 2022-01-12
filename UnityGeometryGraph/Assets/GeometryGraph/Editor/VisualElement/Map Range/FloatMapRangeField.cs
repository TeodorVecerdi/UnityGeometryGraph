using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class FloatMapRangeField : VisualElement {
        private readonly FloatField fromMinField;
        private readonly FloatField fromMaxField;
        private readonly FloatField toMinField;
        private readonly FloatField toMaxField;

        public FloatMapRangeField(string label, float fromMin, float fromMax, float toMin, float toMax) : this(label) {
            fromMinField.SetValueWithoutNotify(fromMin);
            fromMaxField.SetValueWithoutNotify(fromMax);
            toMinField.SetValueWithoutNotify(toMin);
            toMaxField.SetValueWithoutNotify(toMax);
        }

        public FloatMapRangeField(string label = null) {
            if (!string.IsNullOrEmpty(label)) {
                Label labelElement = new Label(label).WithClasses("map-range-field-label");
                Add(labelElement);
            }
            
            VisualElement fromContainer = new VisualElement().WithClasses("container", "from-container");
            fromMinField = new FloatField("min");
            fromMaxField = new FloatField("max");
            Label fromLabel = new Label("From").WithClasses("from-label");
            fromContainer.Add(fromLabel);
            fromContainer.Add(fromMinField);
            fromContainer.Add(fromMaxField);
            
            VisualElement toContainer = new VisualElement().WithClasses("container", "to-container");
            toMinField = new FloatField("min");
            toMaxField = new FloatField("max");
            Label toLabel = new Label("To").WithClasses("to-label");
            toContainer.Add(toLabel);
            toContainer.Add(toMinField);
            toContainer.Add(toMaxField);
            
            Add(fromContainer);
            Add(toContainer);
            AddToClassList("map-range-field");
            AddToClassList("map-range-field__float");
            
            fromMinField.SetValueWithoutNotify(0.0f);
            fromMaxField.SetValueWithoutNotify(1.0f);
            toMinField.SetValueWithoutNotify(0.0f);
            toMaxField.SetValueWithoutNotify(1.0f);
        }

        public void RegisterFromMinValueChanged(EventCallback<ChangeEvent<float>> callback) {
            fromMinField.RegisterValueChangedCallback(callback);
        }

        public void RegisterFromMaxValueChanged(EventCallback<ChangeEvent<float>> callback) {
            fromMaxField.RegisterValueChangedCallback(callback);
        }

        public void RegisterToMinValueChanged(EventCallback<ChangeEvent<float>> callback) {
            toMinField.RegisterValueChangedCallback(callback);
        }

        public void RegisterToMaxValueChanged(EventCallback<ChangeEvent<float>> callback) {
            toMaxField.RegisterValueChangedCallback(callback);
        }
    }
}