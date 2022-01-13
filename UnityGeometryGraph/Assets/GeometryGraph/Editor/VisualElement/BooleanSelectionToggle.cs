using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class BooleanSelectionToggle : VisualElement, INotifyValueChanged<bool> {
        private readonly Button trueButton;
        private readonly Button falseButton;
        private bool state;

        public BooleanSelectionToggle(bool initialValue, string trueText = "True", string falseText = "False", string label = null) {
            if (string.IsNullOrWhiteSpace(trueText)) trueText = "True";
            if (string.IsNullOrWhiteSpace(falseText)) falseText = "False";

            if (label != null) {
                Label labelElement = new(label);
                Add(labelElement);
                AddToClassList("with-label");
            }

            AddToClassList("boolean-selection-toggle");

            trueButton = new Button(() => value = true) { text = trueText }.WithClasses("toggle-button", "first");
            falseButton = new Button(() => value = false) { text = falseText }.WithClasses("toggle-button", "last");

            Add(trueButton);
            Add(falseButton);

            SetValueWithoutNotify(initialValue);
        }

        public void SetValueWithoutNotify(bool newState) {
            state = newState;
            if (state) {
                trueButton.AddToClassList("selected");
                falseButton.RemoveFromClassList("selected");
            } else {
                trueButton.RemoveFromClassList("selected");
                falseButton.AddToClassList("selected");
            }
        }

        public bool value {
            get => state;
            set {
                if(state == value) return;

                using ChangeEvent<bool> pooled = ChangeEvent<bool>.GetPooled(state, value);
                pooled.target = this;
                SetValueWithoutNotify(value);
                SendEvent(pooled);
            }
        }
    }
}