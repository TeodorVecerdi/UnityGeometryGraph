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

namespace GeometryGraph.Editor {
    [CustomEditor(typeof(GGraph))]
    public class GeometryGraphInspector : UnityEditor.Editor {
        private GGraph targetGraph;
        
        private VisualElement root;
        
        private TabContainer tabContainer;
        private VisualElement missingGraphNotice;
        private VisualElement propertiesTab;
        private VisualElement noPropertiesNotice;
        
        private int activeTab;

        private void OnEnable() {
            targetGraph = (GGraph)target;

            activeTab = EditorPrefs.GetInt($"{targetGraph.GetInstanceID()}_activeTab", 0);

            if (targetGraph.Graph == null) return;
            if (!targetGraph.GraphGuid.Equals(targetGraph.Graph.RuntimeData.Guid, StringComparison.InvariantCulture)) {
                targetGraph.GraphGuid = targetGraph.Graph.RuntimeData.Guid;
                OnGraphChanged(targetGraph.Graph);
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

            PropertyField graphPropertyField = new(serializedObject.FindProperty("graph")) {
                name = "graph-field"
            };
            graphPropertyField.Bind(serializedObject);
            graphPropertyField.RegisterValueChangeCallback(OnGraphChanged);
            content.Add(graphPropertyField);
            
            PropertyField exporterPropertyField = new(serializedObject.FindProperty("exporter")) {
                name = "exporter-field"
            };
            exporterPropertyField.Bind(serializedObject);
            content.Add(exporterPropertyField);
            

            BuildMissingGraphNotice();
            content.Add(missingGraphNotice);

            tabContainer = new TabContainer();
            BuildTabs();

            content.Add(tabContainer);
            
            content.Add(new Button(() => {
                foreach ((string key, PropertyValue value) in targetGraph.SceneData.PropertyData) {
                    Debug.Log($"Key: {key}\nValue: {value}");
                }
            }) {text = "Dump scene data"});

            root.Add(title);
            root.Add(content);
            
            return root;
        }

        private void BuildMissingGraphNotice() {
            missingGraphNotice = new VisualElement { name = "MissingGraphNotice" };
            missingGraphNotice.AddToClassList("d-none");
            
            Label missingGraphNoticeLabel = new Label("No graph selected");
            missingGraphNoticeLabel.AddToClassList("missing-graph-notice-main");
            missingGraphNotice.Add(missingGraphNoticeLabel);
            
            Label missingGraphNoticeSubLabel = new Label("Assign one or create a new graph");
            missingGraphNoticeSubLabel.AddToClassList("missing-graph-notice-sub");
            missingGraphNotice.Add(missingGraphNoticeSubLabel);
            
            Button createNewGraphButton = new Button(CreateAndAssignGraph) {text = "Create new graph"};
            createNewGraphButton.AddToClassList("missing-graph-notice-button");
            missingGraphNotice.Add(createNewGraphButton);
        }

        private void CreateAndAssignGraph() {
            // throw new NotImplementedException();
        }

        private void BuildTabs() {
            propertiesTab = tabContainer.CreateTab("Properties");
            propertiesTab.AddToClassList("properties-container");
            
            noPropertiesNotice = new Label("No properties");
            noPropertiesNotice.AddToClassList("d-none");
            propertiesTab.Add(noPropertiesNotice);
            
            UpdateTabs(targetGraph.Graph);

            VisualElement curveVisualizerTab = tabContainer.CreateTab("Curve Visualizer");
            curveVisualizerTab.Add(new Label("Test"));
            tabContainer.SetActive(activeTab);
        }

        private void UpdateTabs(RuntimeGraphObject graphObject) {
            if (graphObject == null) {
                tabContainer.AddToClassList("d-none");
                missingGraphNotice.RemoveFromClassList("d-none");
                return;
            }

            if (graphObject.RuntimeData.Properties.Count == 0) {
                propertiesTab.AddToClassList("d-none");
                noPropertiesNotice.RemoveFromClassList("d-none");
            } else {
                propertiesTab.RemoveFromClassList("d-none");
                noPropertiesNotice.AddToClassList("d-none");
            }
            
            tabContainer.RemoveFromClassList("d-none");
            missingGraphNotice.AddToClassList("d-none");
            
            BuildProperties(graphObject);
        }

        private void BuildProperties(RuntimeGraphObject graphObject) {
            if (graphObject == null) {
                return;
            }

            VisualElement contentContainer = propertiesTab.Q<VisualElement>("PropertiesContent");
            if (contentContainer == null) {
                contentContainer = new VisualElement {name = "PropertiesContent"};
                propertiesTab.Add(contentContainer);
            }
            contentContainer.Clear();

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

        private void OnGraphChanged(SerializedPropertyChangeEvent evt) {
            RuntimeGraphObject graphObject = (RuntimeGraphObject) evt.changedProperty.objectReferenceValue;

            if (graphObject == null) {
                targetGraph.GraphGuid = string.Empty;
            } else if (!targetGraph.GraphGuid.Equals(graphObject.RuntimeData.Guid, StringComparison.InvariantCulture)) {
                targetGraph.GraphGuid = graphObject.RuntimeData.Guid;
                OnGraphChanged(graphObject);
            }
            UpdateTabs(graphObject);
        }

        private void OnGraphChanged(RuntimeGraphObject graph) {
            targetGraph.SceneData.Reset();
            foreach (Property property in graph.RuntimeData.Properties) {
                targetGraph.SceneData.PropertyData.Add(property.Guid, new PropertyValue(property));
            }
            
            serializedObject.Update();
        }
    }
}