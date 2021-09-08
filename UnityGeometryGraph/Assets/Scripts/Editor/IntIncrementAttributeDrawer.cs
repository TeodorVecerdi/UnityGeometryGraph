using UnityEditor;
using UnityEngine;

namespace GraphFramework.Runtime.Editor {
    [CustomPropertyDrawer(typeof(IntIncrementAttribute))]
    public class IntIncrementAttributeDrawer : PropertyDrawer {
        private const float incrementSize = 16f;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            position.width -= 2.0f * incrementSize;
            EditorGUI.PropertyField(position, property, label);
            var decRect = new Rect(position.xMax, position.y, incrementSize, position.height);
            var incRect = new Rect(position.xMax + incrementSize, position.y, incrementSize, position.height);

            if (GUI.Button(decRect, "-")) {
                property.intValue -= 1;
                property.serializedObject.ApplyModifiedProperties();
            }
            
            if (GUI.Button(incRect, "+")) {
                property.intValue += 1;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}