using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using GGraph = GeometryGraph.Runtime.GeometryGraph;
using Object = UnityEngine.Object;

namespace GeometryGraph.Editor {
    [CustomEditor(typeof(GGraph))]
    public class GeometryGraphInspector : UnityEditor.Editor {
        private GGraph targetGraph;
        
        private VisualElement root;
        private Foldout propertiesContainer;

        private void OnEnable() {
            targetGraph = (GGraph)target;

            if (targetGraph.Graph == null) return;
            if (!targetGraph.GraphGuid.Equals(targetGraph.Graph.RuntimeData.Guid, StringComparison.InvariantCulture)) {
                targetGraph.GraphGuid = targetGraph.Graph.RuntimeData.Guid;
                OnGraphChanged();
            }
            
            if (targetGraph.SceneData.PropertyHashCode != targetGraph.Graph.RuntimeData.PropertyHashCode) {
                targetGraph.OnPropertiesChanged(targetGraph.Graph.RuntimeData.PropertyHashCode);
            }
            
            foreach (Property property in targetGraph.Graph.RuntimeData.Properties) {
                targetGraph.SceneData.PropertyData[property.Guid].UpdateDefaultValue(property.DefaultValue);
            }
        }

       public override bool UseDefaultMargins() {
            return false;
        }

        /// <summary>
        ///   <para>Implement this method to make a custom UIElements inspector.</para>
        /// </summary>
        /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=Editor.CreateInspectorGUI">`Editor.CreateInspectorGUI` on docs.unity3d.com</a></footer>
        public override VisualElement CreateInspectorGUI() {
            root = new VisualElement {name = "GeometryGraphInspector"};
            root.AddStyleSheet(EditorGUIUtility.isProSkin ? "Inspector/gg_dark" : "Inspector/gg_light");
            root.AddStyleSheet("Inspector/gg_common");
            

            VisualElement title = new VisualElement();
            title.AddToClassList("title");
            Image icon = new Image();
            icon.AddToClassList("title-icon");
            Label titleLabel = new Label("Geometry Graph");
            titleLabel.AddToClassList("title-label");
            title.Add(icon);
            title.Add(titleLabel);

            VisualElement content = new VisualElement();
            content.AddToClassList("content");

            PropertyField graphPropertyField = new PropertyField(serializedObject.FindProperty("graph")) {
                name = "graph-field"
            };
            graphPropertyField.Bind(serializedObject);
            graphPropertyField.RegisterValueChangeCallback(OnGraphPropertyChanged);
            content.Add(graphPropertyField);
            
            PropertyField exporterPropertyField = new PropertyField(serializedObject.FindProperty("exporter")) {
                name = "exporter-field"
            };
            exporterPropertyField.Bind(serializedObject);
            content.Add(exporterPropertyField);
            
            propertiesContainer = new Foldout {text = "Properties"};
            propertiesContainer.value = targetGraph.SceneData.PropertiesFoldout;
            propertiesContainer.BindProperty(serializedObject.FindProperty("sceneData.PropertiesFoldout"));
            propertiesContainer.AddToClassList("properties-container");
            UpdatePropertiesContainer(targetGraph.Graph);
            content.Add(propertiesContainer);

            root.Add(title);
            root.Add(content);
            
            return root;
        }

        public override void OnInspectorGUI() {
            Debug.Log("OnInspectorGUI");
        }

        /*
        public override void OnInspectorGUI() {
            UnityEditor.SerializedProperty graphProperty = serializedObject.FindProperty("graph");
            EditorGUILayout.PropertyField(graphProperty);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exporter"));
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("curveVisualizer"));
            bool changed = serializedObject.ApplyModifiedProperties();

            if (changed) {
                if (targetGraph.Graph == null) {
                    targetGraph.GraphGuid = string.Empty;
                } else if (!targetGraph.GraphGuid.Equals(targetGraph.Graph.RuntimeData.Guid, StringComparison.InvariantCulture)) {
                    targetGraph.GraphGuid = targetGraph.Graph.RuntimeData.Guid;
                    OnGraphChanged();
                }
            }

            if (targetGraph.Graph == null) return;

            if (targetGraph.Graph.RuntimeData.Properties.Count > 0) {
                DrawProperties();
            }

            if (targetGraph.HasCurveData || true) {
                DrawCurveInspector();
            }

            if (GUILayout.Button("Evaluate")) {
                targetGraph.Evaluate();
            }
        }
        */

        /*private void DrawProperties() {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Properties");
            foreach (Property property in targetGraph.Graph.RuntimeData.Properties) {
                bool isUnityObjectType = Runtime.Graph.PropertyUtils.IsUnityObjectType(property.Type);
                Type backingValueType = Runtime.Graph.PropertyUtils.GetBackingValueType(property.Type);

                if (isUnityObjectType) {
                    EditorGUI.BeginChangeCheck();
                    Object newValue = EditorGUILayout.ObjectField(property.DisplayName, targetGraph.SceneData.PropertyData[property.Guid].ObjectValue, backingValueType, true);
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RegisterCompleteObjectUndo(targetGraph, $"Changed {property.DisplayName} value");
                        targetGraph.SceneData.PropertyData[property.Guid].ObjectValue = newValue;
                        targetGraph.SceneData.PropertyData[property.Guid].HasCustomValue = true;
                    }
                } else {
                    switch (property.Type) {
                        case PropertyType.Integer: {
                            EditorGUI.BeginChangeCheck();
                            int newValue = EditorGUILayout.IntField(property.DisplayName, targetGraph.SceneData.PropertyData[property.Guid].IntValue);
                            if (EditorGUI.EndChangeCheck()) {
                                Undo.RegisterCompleteObjectUndo(targetGraph, $"Changed {property.DisplayName} value");
                                targetGraph.SceneData.PropertyData[property.Guid].IntValue = newValue;
                                targetGraph.SceneData.PropertyData[property.Guid].HasCustomValue = true;
                            }

                            break;
                        }
                        case PropertyType.Float: {
                            EditorGUI.BeginChangeCheck();
                            float newValue = EditorGUILayout.FloatField(property.DisplayName, targetGraph.SceneData.PropertyData[property.Guid].FloatValue);
                            if (EditorGUI.EndChangeCheck()) {
                                Undo.RegisterCompleteObjectUndo(targetGraph, $"Changed {property.DisplayName} value");
                                targetGraph.SceneData.PropertyData[property.Guid].FloatValue = newValue;
                                targetGraph.SceneData.PropertyData[property.Guid].HasCustomValue = true;
                            }

                            break;
                        }
                        case PropertyType.Vector: {
                            EditorGUI.BeginChangeCheck();
                            Vector3 newValue = EditorGUILayout.Vector3Field(property.DisplayName, targetGraph.SceneData.PropertyData[property.Guid].VectorValue);
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
        }

        private void DrawCurveInspector() {
            CurveVisualizerSettings settings = targetGraph.CurveVisualizerSettings;
            GUILayout.BeginVertical();
            GUILayout.Label("Curves", EditorStyles.whiteLargeLabel);
            bool newValue = GUILayout.Toggle(settings.Enabled, "Enable Curve Visualizer");
            if (newValue != settings.Enabled) {
                Undo.RegisterCompleteObjectUndo(targetGraph, "Change Enable Curve Visualizer");
                settings.Enabled = newValue;
            }
            GUI.enabled = settings.Enabled;

            GUILayout.Space(8.0f);
            { // Spline Settings
                settings.SplineSettingsFoldout = EditorGUILayout.Foldout(settings.SplineSettingsFoldout, "Spline Settings", true);
                GUILayout.Label("Spline Settings", EditorStyles.boldLabel);
                bool newShowSpline = GUILayout.Toggle(settings.ShowSpline, "Show Spline");
                if (newShowSpline != settings.ShowSpline) {
                    Undo.RegisterCompleteObjectUndo(targetGraph, "Change Show Spline");
                    settings.ShowSpline = newShowSpline;
                }

                GUI.enabled = settings.Enabled && settings.ShowSpline;
                EditorGUI.BeginChangeCheck();
                Color newSplineColor = EditorGUILayout.ColorField("Spline Color", settings.SplineColor);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RegisterCompleteObjectUndo(targetGraph, "Change Spline Color");
                    settings.SplineColor = newSplineColor;
                }

                EditorGUI.BeginChangeCheck();
                float newSplineWidth = EditorGUILayout.FloatField("Spline Width", settings.SplineWidth);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RegisterCompleteObjectUndo(targetGraph, "Change Spline Width");
                    settings.SplineWidth = newSplineWidth;
                }
            }
            
            GUILayout.Space(8.0f);
            { // Point Settings
                settings.PointSettingsFoldout = EditorGUILayout.Foldout(settings.PointSettingsFoldout, "Point Settings", true);
                GUILayout.Label("Point Settings", EditorStyles.boldLabel);
                bool newShowPoints = GUILayout.Toggle(settings.ShowPoints, "Show Points");
                if (newShowPoints != settings.ShowPoints) {
                    Undo.RegisterCompleteObjectUndo(targetGraph, "Change Show Points");
                    settings.ShowPoints = newShowPoints;
                }
                
                GUI.enabled = settings.Enabled && settings.ShowPoints;
                EditorGUI.BeginChangeCheck();
                Color newPointColor = EditorGUILayout.ColorField("Point Color", settings.PointColor);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RegisterCompleteObjectUndo(targetGraph, "Change Point Color");
                    settings.PointColor = newPointColor;
                }
                EditorGUI.BeginChangeCheck();
                float newPointSize = EditorGUILayout.FloatField("Point Size", settings.PointSize);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RegisterCompleteObjectUndo(targetGraph, "Change Point Size");
                    settings.PointSize = newPointSize;
                }
            }
            
            GUILayout.Space(8.0f);
            { // Direction vector settings
                settings.DirectionVectorSettingsFoldout = EditorGUILayout.Foldout(settings.DirectionVectorSettingsFoldout, "Direction Vector Settings", true);
                GUILayout.Label("Direction Vector Settings", EditorStyles.boldLabel);
                bool newShowDirectionVectors = GUILayout.Toggle(settings.ShowDirectionVectors, "Show Direction Vectors");
                if (newShowDirectionVectors != settings.ShowDirectionVectors) {
                    Undo.RegisterCompleteObjectUndo(targetGraph, "Change Show Direction Vectors");
                    settings.ShowDirectionVectors = newShowDirectionVectors;
                }
                
                GUI.enabled = settings.Enabled && settings.ShowDirectionVectors;
                EditorGUI.BeginChangeCheck();
                float newDirectionVectorLength = EditorGUILayout.FloatField("Direction Vector Length", settings.DirectionVectorLength);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RegisterCompleteObjectUndo(targetGraph, "Change Direction Vector Length");
                    settings.DirectionVectorLength = newDirectionVectorLength;
                }
                EditorGUI.BeginChangeCheck();
                float newDirectionVectorWidth = EditorGUILayout.FloatField("Direction Vector Width", settings.DirectionVectorWidth);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RegisterCompleteObjectUndo(targetGraph, "Change Direction Vector Width");
                    settings.DirectionVectorWidth = newDirectionVectorWidth;
                }
                
                EditorGUI.BeginChangeCheck();
                Color newTangentColor = EditorGUILayout.ColorField("Tangent Color", settings.DirectionTangentColor);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RegisterCompleteObjectUndo(targetGraph, "Change Tangent Color");
                    settings.DirectionTangentColor = newTangentColor;
                }
                EditorGUI.BeginChangeCheck();
                Color newNormalColor = EditorGUILayout.ColorField("Normal Color", settings.DirectionNormalColor);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RegisterCompleteObjectUndo(targetGraph, "Change Normal Color");
                    settings.DirectionNormalColor = newNormalColor;
                }
                EditorGUI.BeginChangeCheck();
                Color newBinormalColor = EditorGUILayout.ColorField("Binormal Color", settings.DirectionBinormalColor);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RegisterCompleteObjectUndo(targetGraph, "Change Binormal Color");
                    settings.DirectionBinormalColor = newBinormalColor;
                }
            }
            GUILayout.EndVertical();
        }*/

        private void BuildProperties(RuntimeGraphObject graphObject) {
            if (graphObject == null) {
                return;
            }
            propertiesContainer.Clear();
            VisualElement contentContainer = new VisualElement();
            contentContainer.AddToClassList("foldout-content");
            propertiesContainer.Add(contentContainer);

            int propIndex = 0;
            Dictionary<string, (Property property, int index)> propertyDictionary = 
                graphObject.RuntimeData.Properties.ToDictionary(property => property.Guid, property => (property, propIndex++));
            
            UnityEditor.SerializedProperty keysProperty = serializedObject.FindProperty("sceneData.propertyData.keys");
            UnityEditor.SerializedProperty valuesProperty = serializedObject.FindProperty("sceneData.propertyData.values");
            IEnumerator keysEnumerator = keysProperty.GetEnumerator();
            IEnumerator valuesEnumerator = valuesProperty.GetEnumerator();
            SortedSet<PropertyField> properties = new(Comparer<PropertyField>.Create((field1, field2) => ((int)field1.userData).CompareTo((int)field2.userData)));
            while (keysEnumerator.MoveNext() && valuesEnumerator.MoveNext()) {
                string propertyGuid = (keysEnumerator.Current as UnityEditor.SerializedProperty)?.stringValue;
                if (!propertyDictionary.ContainsKey(propertyGuid)) {
                    Debug.LogError($"Property with GUID {propertyGuid} not found in graph {graphObject.name}");
                    continue;
                }

                (Property property, int index) = propertyDictionary[propertyGuid];
                string relativePropertyName = property.Type switch {
                    PropertyType.GeometryObject => "ObjectValue",
                    PropertyType.GeometryCollection => "CollectionValue",
                    PropertyType.Integer => "IntValue",
                    PropertyType.Float => "FloatValue",
                    PropertyType.Vector => "VectorValue",
                    _ => throw new ArgumentOutOfRangeException()
                };

                UnityEditor.SerializedProperty valueProperty = valuesEnumerator.Current as UnityEditor.SerializedProperty;
                UnityEditor.SerializedProperty relativeProperty = valueProperty!.FindPropertyRelative(relativePropertyName);
                PropertyField propertyField = new (relativeProperty, property.DisplayName) {
                    userData = index
                };

                propertyField.schedule.Execute(() => {
                    propertyField.RegisterCallback((ContextualMenuPopulateEvent evt) => {
                        evt.menu.AppendAction("Reset to Default", _ => { ResetPropertyToDefault(property, relativePropertyName, relativeProperty, valueProperty); }, DropdownMenuAction.AlwaysEnabled);
                    });
                    
                    if (propertyField.childCount <= 0) return;
                    
                    if (propertyField[0] is ObjectField objectField) {
                        objectField[1][0].AddManipulator(new ContextualMenuManipulator(evt => {
                            evt.menu.AppendAction("Reset to Default", _ => { ResetPropertyToDefault(property, relativePropertyName, relativeProperty, valueProperty); }, DropdownMenuAction.AlwaysEnabled);
                        }));
                    }
                    
                    VisualElement propertyFieldInput = propertyField[0][0];
                    propertyFieldInput.AddManipulator(new ContextualMenuManipulator(evt => {
                        evt.menu.AppendAction("Reset to Default", _ => { ResetPropertyToDefault(property, relativePropertyName, relativeProperty, valueProperty); }, DropdownMenuAction.AlwaysEnabled);
                    }));
                });
                
                propertyField.Bind(serializedObject);
                properties.Add(propertyField);
            }
            
            foreach (PropertyField propertyField in properties) {
                contentContainer.Add(propertyField);
            }
            
            contentContainer.Add(new Button(() => {
                foreach ((string key, PropertyValue value) in targetGraph.SceneData.PropertyData) {
                    Debug.Log($"Key: {key}\nValue: {value}");
                }
            }) {text = "Dump scene data"});
        }

        private void ResetPropertyToDefault(Property property, string targetPropertyName, UnityEditor.SerializedProperty targetProperty, UnityEditor.SerializedProperty valueProperty) {
            serializedObject.Update();

            UnityEditor.SerializedProperty defaultPropertyRelative = valueProperty.FindPropertyRelative($"Default{targetPropertyName}");
            switch (property.Type) {
                case PropertyType.GeometryObject:
                case PropertyType.GeometryCollection: {
                    targetProperty.objectReferenceValue = defaultPropertyRelative.objectReferenceValue;
                    break;
                }
                case PropertyType.Integer: {
                    targetProperty.intValue = defaultPropertyRelative.intValue;
                    break;
                }
                case PropertyType.Float: {
                    targetProperty.floatValue = defaultPropertyRelative.floatValue;
                    break;
                }
                case PropertyType.Vector: {
                    targetProperty.vector3Value = defaultPropertyRelative.vector3Value;
                    break;
                }
                default: throw new ArgumentOutOfRangeException();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnGraphPropertyChanged(SerializedPropertyChangeEvent evt) {
            RuntimeGraphObject graphObject = (RuntimeGraphObject) evt.changedProperty.objectReferenceValue;
            targetGraph.Graph = graphObject;
            
            if (graphObject == null) {
                targetGraph.GraphGuid = string.Empty;
                UpdatePropertiesContainer(graphObject);
            } else if (!targetGraph.GraphGuid.Equals(graphObject.RuntimeData.Guid, StringComparison.InvariantCulture)) {
                targetGraph.GraphGuid = graphObject.RuntimeData.Guid;
                OnGraphChanged();
                UpdatePropertiesContainer(graphObject);
            }
        }

        private void UpdatePropertiesContainer(RuntimeGraphObject graphObject) {
            if (graphObject == null || graphObject.RuntimeData.Properties.Count == 0) {
                propertiesContainer.Clear();
                propertiesContainer.AddToClassList("d-none");
                return;
            }
            
            propertiesContainer.RemoveFromClassList("d-none");
            BuildProperties(graphObject);
        }

        private void OnGraphChanged() {
            targetGraph.SceneData.Reset();
            foreach (Property property in targetGraph.Graph.RuntimeData.Properties) {
                targetGraph.SceneData.PropertyData.Add(property.Guid, new PropertyValue(property));
            }
        }
    }
}