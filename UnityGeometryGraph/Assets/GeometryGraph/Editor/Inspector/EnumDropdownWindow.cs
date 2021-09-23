using System;
using System.Collections.Generic;
using System.Text;
using GeometryGraph.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class EnumDropdownWindow : EditorWindow {
        private const int MAX_ITEMS_PER_ROW = 6;
        private const float ROW_HEIGHT = 24.0f;
        private const float COLUMN_WIDTH = 128.0f;

        private List<Enum> values;
        private Action<Enum> onSelect;
        
        public static void ShowWindow<T>(Vector2 position, Action<T> onSelect) where T : Enum {
            var allValues = new List<Enum>(Enum.GetValues(typeof(T)).Convert(o => (Enum)o));
            var columns = Mathf.CeilToInt(allValues.Count / (float)MAX_ITEMS_PER_ROW);
            var window = CreateInstance<EnumDropdownWindow>();
            window.values = new List<Enum>(allValues);
            window.onSelect = val => onSelect((T)val);
            window.wantsMouseEnterLeaveWindow = true;
            window.ShowAsDropDown(new Rect(position, Vector2.one), new Vector2(columns * COLUMN_WIDTH, Mathf.Min(allValues.Count, MAX_ITEMS_PER_ROW) * ROW_HEIGHT + 16.0f));
        }

        private void CreateGUI() {
            rootVisualElement.Clear();
            rootVisualElement.AddStyleSheet("Styles/EnumDropdownWindow");
            rootVisualElement.name = "EnumDropdownWindow";
            rootVisualElement.RegisterCallback(new EventCallback<MouseLeaveWindowEvent>(evt => {
                Close();
            }));

            var columns = Mathf.CeilToInt(values.Count / (float)MAX_ITEMS_PER_ROW);
            for (var c = 0; c < columns; c++) {
                var col = new VisualElement();
                col.AddToClassList("column");
                var startIndex = c * MAX_ITEMS_PER_ROW;
                var endIndex = Mathf.Min(startIndex + MAX_ITEMS_PER_ROW, values.Count);
                for (var i = startIndex; i < endIndex; i++) {
                    var i1 = i;
                    var button = new Button(() => {
                        onSelect(values[i1]);
                        Close();
                    }) { text = RandomUtilities.WordBreakString(values[i].ToString()) };
                    col.Add(button);
                }

                if (c != 0) {
                    var separator = new VisualElement();
                    separator.AddToClassList("column-separator");
                    rootVisualElement.Add(separator);
                }
                rootVisualElement.Add(col);
            }
        }
    }
}