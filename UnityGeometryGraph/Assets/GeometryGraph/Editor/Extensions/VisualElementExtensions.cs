using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public static class VisualElementExtensions {
        public static void AddStyleSheet(this VisualElement element, string path) {
            StyleSheet stylesheet = Resources.Load<StyleSheet>(path);
            if(stylesheet == null) Debug.LogWarning($"StyleSheet at path \"{path}\" could not be found");
            else element.styleSheets.Add(stylesheet);
        }
    }
}