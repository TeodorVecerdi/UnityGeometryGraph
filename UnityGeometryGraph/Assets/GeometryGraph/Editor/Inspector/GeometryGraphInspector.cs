using System;
using System.Linq;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using UnityEditor;
using UnityEngine;
using GGraph = GeometryGraph.Runtime.GeometryGraph;
using Object = UnityEngine.Object;

namespace GeometryGraph.Editor {
    [CustomEditor(typeof(GGraph))]
    public class GeometryGraphInspector : UnityEditor.Editor {
        private GGraph targetGraph;

        private void OnEnable() {
            targetGraph = (GGraph)target;

            if(targetGraph.Graph == null) return;
            if (!targetGraph.GraphGuid.Equals(targetGraph.Graph.RuntimeData.Guid, StringComparison.InvariantCulture)) {
                targetGraph.GraphGuid = targetGraph.Graph.RuntimeData.Guid;
                OnGraphChanged();
            }

            if (targetGraph.SceneData.PropertyHashCode != targetGraph.Graph.RuntimeData.PropertyHashCode) {
                targetGraph.OnPropertiesChanged(targetGraph.Graph.RuntimeData.PropertyHashCode);
            }

            foreach (var property in targetGraph.Graph.RuntimeData.Properties) {
                if (targetGraph.SceneData.PropertyData[property.Guid].HasCustomValue) continue;
                targetGraph.SceneData.PropertyData[property.Guid].UpdateDefaultValue(property.Type, property.DefaultValue);
            }
        }

        public override void OnInspectorGUI() {
            var graphProperty = serializedObject.FindProperty("graph");
            EditorGUILayout.PropertyField(graphProperty);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exporter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("curveVisualizer"));
            var changed = serializedObject.ApplyModifiedProperties();

            if (changed) {
                if(targetGraph.Graph == null) {
                    targetGraph.GraphGuid = string.Empty;
                } else if (!targetGraph.GraphGuid.Equals(targetGraph.Graph.RuntimeData.Guid, StringComparison.InvariantCulture)) {
                    targetGraph.GraphGuid = targetGraph.Graph.RuntimeData.Guid;
                    OnGraphChanged();
                }
            }

            if (targetGraph.Graph == null) return;

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Properties");
            foreach (var property in targetGraph.Graph.RuntimeData.Properties) {
                var isUnityObjectType = Runtime.Graph.PropertyUtils.IsUnityObjectType(property.Type);
                var backingValueType = Runtime.Graph.PropertyUtils.GetBackingValueType(property.Type);

                if (isUnityObjectType) {
                    EditorGUI.BeginChangeCheck();
                    var newValue = EditorGUILayout.ObjectField(property.DisplayName, targetGraph.SceneData.PropertyData[property.Guid].ObjectValue, backingValueType, true);
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RegisterCompleteObjectUndo(targetGraph, $"Changed {property.DisplayName} value");
                        targetGraph.SceneData.PropertyData[property.Guid].ObjectValue = newValue;
                        targetGraph.SceneData.PropertyData[property.Guid].HasCustomValue = true;
                    }
                } else {
                    switch (property.Type) {
                        case PropertyType.Integer: {
                            EditorGUI.BeginChangeCheck();
                            var newValue = EditorGUILayout.IntField(property.DisplayName, targetGraph.SceneData.PropertyData[property.Guid].IntValue);
                            if (EditorGUI.EndChangeCheck()) {
                                Undo.RegisterCompleteObjectUndo(targetGraph, $"Changed {property.DisplayName} value");
                                targetGraph.SceneData.PropertyData[property.Guid].IntValue = newValue;
                                targetGraph.SceneData.PropertyData[property.Guid].HasCustomValue = true;
                            }
                            break;
                        }
                        case PropertyType.Float: {
                            EditorGUI.BeginChangeCheck();
                            var newValue = EditorGUILayout.FloatField(property.DisplayName, targetGraph.SceneData.PropertyData[property.Guid].FloatValue);
                            if (EditorGUI.EndChangeCheck()) {
                                Undo.RegisterCompleteObjectUndo(targetGraph, $"Changed {property.DisplayName} value");
                                targetGraph.SceneData.PropertyData[property.Guid].FloatValue = newValue;
                                targetGraph.SceneData.PropertyData[property.Guid].HasCustomValue = true;
                            }
                            break;
                        }
                        case PropertyType.Vector: {
                            EditorGUI.BeginChangeCheck();
                            var newValue = EditorGUILayout.Vector3Field(property.DisplayName, targetGraph.SceneData.PropertyData[property.Guid].VectorValue);
                            if (EditorGUI.EndChangeCheck()) {
                                Undo.RegisterCompleteObjectUndo(targetGraph, $"Changed {property.DisplayName} value");
                                targetGraph.SceneData.PropertyData[property.Guid].VectorValue = newValue;
                                targetGraph.SceneData.PropertyData[property.Guid].HasCustomValue = true;
                            }
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            GUILayout.EndVertical();

            if (GUILayout.Button("Evaluate")) {
                targetGraph.Evaluate();
            }
        }

        private void OnGraphChanged() {
            targetGraph.SceneData.Reset();
            foreach (var property in targetGraph.Graph.RuntimeData.Properties) {
                targetGraph.SceneData.PropertyData.Add(property.Guid, new PropertyValue(property));
            }
        }
    }
}