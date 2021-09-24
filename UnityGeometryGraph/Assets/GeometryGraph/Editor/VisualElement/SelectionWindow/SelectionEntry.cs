using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    internal class SelectionEntry {
        private const float k_Height = 16.0f + 4.0f;
        private const float k_SeparatorHeight = 4.0f;

        private readonly string title;
        private readonly string tooltip;
        private readonly int index;
        private readonly bool hasSeparator;

        public SelectionEntry(string title, string tooltip, int index, bool hasSeparator) {
            this.title = title;
            this.tooltip = tooltip;
            this.index = index;
            this.hasSeparator = hasSeparator;
        }

        public SelectionEntry(string tooltip, int index, bool hasSeparator) : this(null, tooltip, index, hasSeparator) { }

        public float GetHeight() {
            return hasSeparator ? k_Height + k_SeparatorHeight : k_Height;
        }

        public VisualElement CreateElement(List<object> valueProvider, Action<object> onSelect, EditorWindow window) {
            var root = new VisualElement();
            root.AddToClassList("entry");
            var button = new Button(() => {
                window.Close();
                onSelect(valueProvider?[index] ?? index);
            }) { 
                text = title ?? RandomUtilities.DisplayNameString(valueProvider[index].ToString()),
                tooltip = tooltip
            };
            button.AddToClassList("entry-button");
            root.Add(button);

            if (hasSeparator) {
                root.AddToClassList("has-separator");
                var separator = new VisualElement();
                separator.AddToClassList("entry-separator");
                root.Add(separator);
            }

            return root;
        }
    }
}