using System;
using UnityEditor;
using UnityEngine;

namespace GeometryGraph.Editor {
    public class SelectionWindow : EditorWindow {
        private const float k_WindowPadding = 4f;

        private Action<object> onSelect;
        private SelectionTree tree;

        internal static void ShowWindow(Vector2 position, float buttonHeight, SelectionTree tree, Action<object> onSelect) {
            SelectionWindow window = CreateInstance<SelectionWindow>();
            window.onSelect = onSelect;
            window.tree = tree;
            window.wantsMouseEnterLeaveWindow = true;
            float width = tree.GetWidth() + k_WindowPadding * 2.0f;
            float height = tree.GetHeight() + k_WindowPadding * 2.0f;
            window.ShowAsDropDown(new Rect(position, new Vector2(1, buttonHeight)), new Vector2(width, height));
            window.Show();
        }

        private void CreateGUI() {
            if(tree == null) return; 
            
            rootVisualElement.Clear();
            rootVisualElement.AddStyleSheet("Styles/SelectionWindow");
            rootVisualElement.name = "SelectionWindow";
            rootVisualElement.Add(tree.CreateElement(this, onSelect));
        }
    }
}