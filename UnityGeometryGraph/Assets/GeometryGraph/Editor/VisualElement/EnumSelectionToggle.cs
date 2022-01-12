using System;
using System.Linq;
using GeometryGraph.Runtime;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class EnumSelectionToggle<T> : VisualElement, INotifyValueChanged<T> where T : Enum {
        private Button[] buttons;
        private int active;
        private T rawValue;

        private readonly T[] values;
        private readonly string[] names;

        public EnumSelectionToggle(T value) {
            values = Enum.GetValues(typeof(T)).Convert(o => (T)o).ToArray();
            names = values.Select(v => RandomUtilities.DisplayNameEnum(v)).ToArray();

            AddToClassList("enum-selection-toggle");
            Build();
            SetValueWithoutNotify(value);
        }

        public void SetValueWithoutNotify(T value) {
            SetValueWithoutNotify(FindIndex(value));
        }

        public void SetValueWithoutNotify(int index) {
            buttons[active].RemoveFromClassList("selected");
            active = index;
            rawValue = values[active];
            buttons[active].AddToClassList("selected");
        }

        private void Build() {
            buttons = new Button[names.Length];
            for (int i = 0; i < names.Length; i++) {
                int index = i;
                buttons[i] = new Button(() => value = values[index]) {text = names[i]};
                buttons[i].AddToClassList("toggle-button");
                Add(buttons[i]);
            }

            buttons[0].AddToClassList("first");
            buttons[^1].AddToClassList("last");
        }

        private int FindIndex(T value) {
            for (int i = 0; i < values.Length; i++) {
                if (value.ToUInt64() == values[i].ToUInt64()) return i;
            }

            throw new ArgumentException($"Could not find index of enum value: {value}");
        }

        void INotifyValueChanged<T>.SetValueWithoutNotify(T newValue) {
            SetValueWithoutNotify(FindIndex(newValue));
        }

        public T value {
            get => rawValue;
            set {
                using ChangeEvent<T> pooled = ChangeEvent<T>.GetPooled(rawValue, value);
                pooled.target = this;
                SetValueWithoutNotify(FindIndex(value));
                SendEvent(pooled);
            }
        }
    }
}