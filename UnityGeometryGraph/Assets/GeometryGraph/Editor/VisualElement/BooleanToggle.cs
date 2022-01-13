using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class BooleanToggle : VisualElement, INotifyValueChanged<bool> {
        private readonly Button button;
        private readonly string trueText;
        private readonly string falseText;
        private bool state;

        public BooleanToggle(bool initialValue, string trueText, string falseText) {
            if (string.IsNullOrEmpty(trueText)) trueText = "Bool (true)";
            if (string.IsNullOrEmpty(falseText)) falseText = "Bool (false)";
            this.trueText = trueText;
            this.falseText = falseText;

            AddToClassList("boolean-toggle");

            button = new Button(() => value = !state) { text = "" }.WithClasses("toggle-button");

            Add(button);
            SetValueWithoutNotify(initialValue);
        }

        public BooleanToggle(bool initialValue, string label) : this(initialValue, label, label) {}

        public void SetValueWithoutNotify(bool newState) {
            state = newState;
            if (state) {
                button.AddToClassList("selected");
                button.text = trueText;
            } else {
                button.RemoveFromClassList("selected");
                button.text = falseText;
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