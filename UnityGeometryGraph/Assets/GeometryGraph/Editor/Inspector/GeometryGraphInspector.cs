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

        private VisualElement mainContent;
        private TabContainer tabContainer;
        private VisualElement missingGraphNotice;
        private VisualElement noPropertiesNotice;
        
        private VisualElement propertiesTab;
        private VisualElement curveVisualizerTab;
        private VisualElement instancesTab;
        
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
                targetGraph.SceneData.UpdatePropertyDefaultValue(property.Guid, property.DefaultValue);
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

            PropertyField graphPropertyField = new(serializedObject.FindProperty("graph"), "Graph") {
                name = "graph-field"
            };
            graphPropertyField.Bind(serializedObject);
            graphPropertyField.RegisterValueChangeCallback(OnGraphChanged);
            content.Add(graphPropertyField);
            
            PropertyField meshFilterPropertyField = new(serializedObject.FindProperty("meshFilter"), "Mesh Filter") {
                name = "meshFilter-field"
            };
            meshFilterPropertyField.Bind(serializedObject);
            content.Add(meshFilterPropertyField);
            

            BuildMissingGraphNotice();
            content.Add(missingGraphNotice);
            
            mainContent = new VisualElement();
            mainContent.AddToClassList("main-content");
            
            Button evaluateButton = new Button(() => {
                targetGraph.Evaluate();
            }) {text = "Evaluate Graph"};
            evaluateButton.AddToClassList("evaluate-button");
            mainContent.Add(evaluateButton);

            tabContainer = new TabContainer(tabIndex => {
                EditorPrefs.SetInt($"{targetGraph.GetInstanceID()}_activeTab", tabIndex);
            });
            BuildTabs();

            mainContent.Add(tabContainer);
            content.Add(mainContent);

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
            
            VisualElement cta = new VisualElement();
            cta.AddToClassList("missing-graph-notice-cta");
            missingGraphNotice.Add(cta);
            
            Label missingGraphNoticeCta0 = new Label("Assign one or");
            missingGraphNoticeCta0.AddToClassList("missing-graph-notice-cta-0");
            cta.Add(missingGraphNoticeCta0);
            
            Button missingGraphNoticeCtaButton = new Button(CreateAndAssignGraph) {text = "create new graph"};
            missingGraphNoticeCtaButton.AddToClassList("missing-graph-notice-cta-button");
            cta.Add(missingGraphNoticeCtaButton);
            
            Label missingGraphNoticeCta1 = new Label(".");
            missingGraphNoticeCta1.AddToClassList("missing-graph-notice-cta-1");
            cta.Add(missingGraphNoticeCta1);
        }

        private void CreateAndAssignGraph() {
            string path = EditorUtility.SaveFilePanel("Create a new Geometry Graph", Application.dataPath, "New Geometry Graph", GraphFrameworkImporter.Extension);
            if (string.IsNullOrEmpty(path)) return;

            if (!path.EndsWith($".{GraphFrameworkImporter.Extension}")) path += $".{GraphFrameworkImporter.Extension}";
            path = path.Replace(Application.dataPath, "Assets");
            CreateGraphObject.CreateObject(path);
            RuntimeGraphObject graphObject = AssetDatabase.LoadAssetAtPath<RuntimeGraphObject>(path);
            serializedObject.FindProperty("graph").objectReferenceValue = graphObject;
            serializedObject.ApplyModifiedProperties();
        }

        private void BuildTabs() {
            propertiesTab = tabContainer.CreateTab("Properties");
            propertiesTab.AddToClassList("properties-container");
            
            noPropertiesNotice = new Label("No properties");
            noPropertiesNotice.AddToClassList("no-properties-notice");
            noPropertiesNotice.AddToClassList("d-none");
            propertiesTab.Add(noPropertiesNotice);
            
            instancesTab = tabContainer.CreateTab("Instances");
            instancesTab.AddToClassList("instances-container");

            curveVisualizerTab = tabContainer.CreateTab("Curve Visualizer");
            curveVisualizerTab.AddToClassList("curve-visualizer-container");

            UpdateTabs(targetGraph.Graph);
            tabContainer.SetActive(activeTab);
        }

        private void UpdateTabs(RuntimeGraphObject graphObject) {
            if (graphObject == null) {
                mainContent.AddToClassList("d-none");
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
            
            mainContent.RemoveFromClassList("d-none");
            missingGraphNotice.AddToClassList("d-none");
            
            BuildPropertiesTab(graphObject);
            BuildCurveTab();
            BuildInstancesTab();
        }

        private void BuildPropertiesTab(RuntimeGraphObject graphObject) {
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

        private void BuildCurveTab() {
            UnityEditor.SerializedProperty settingsProperty = serializedObject.FindProperty("curveVisualizerSettings");

            VisualElement mainContainer = curveVisualizerTab.Q<VisualElement>("CurveVisualizerMain");
            if (mainContainer == null) {
                mainContainer = new VisualElement {name = "CurveVisualizerMain"}.WithClasses("curve-visualizer-container");
                curveVisualizerTab.Add(mainContainer);
            }
            mainContainer.Clear();
            
            (PropertyField enabledField, Button enabledButton) = MakeCurveVisualizerToggle("Disable Curve Visualizer", "Enable Curve Visualizer", "Enabled", settingsProperty);
            mainContainer.Add(enabledField);
            mainContainer.Add(enabledButton);
            enabledField.userData = new List<(string, VisualElement)>();
            enabledField.RegisterValueChangeCallback(evt => {
                bool newValue = evt.changedProperty.boolValue;
                List<(string, VisualElement)> fields = enabledField.userData as List<(string, VisualElement)>;
                foreach ((string prop, VisualElement field) in fields) {
                    bool shouldEnable = newValue;
                    if (!string.IsNullOrEmpty(prop)) {
                        shouldEnable = shouldEnable && settingsProperty.FindPropertyRelative(prop).boolValue;
                    }
                    field.SetEnabled(shouldEnable);
                }
            });
            
            VisualElement splineSettingsContainer = new VisualElement().WithClasses("spline-settings-container", "curve-settings-container");
            mainContainer.Add(splineSettingsContainer);
            {
                
                VisualElement header = new VisualElement().WithClasses("curve-settings-category-header");
                Label splineSettingsLabel = new Label("Spline Settings").WithClasses("curve-settings-category-title", "spline-settings-label");
                header.Add(splineSettingsLabel);
                
                (PropertyField showSplineField, Button showSplineButton) = MakeCurveVisualizerToggle("Hide", "Show", "ShowSpline", settingsProperty);
                header.Add(showSplineField);
                header.Add(showSplineButton);
                splineSettingsContainer.Add(header);
                
                PropertyField splineWidthField = new (settingsProperty.FindPropertyRelative("SplineWidth"), "Width");
                splineWidthField.Bind(serializedObject);
                splineSettingsContainer.Add(splineWidthField);
                
                PropertyField splineColorField = new(settingsProperty.FindPropertyRelative("SplineColor"), "Color");
                splineColorField.Bind(serializedObject);
                splineSettingsContainer.Add(splineColorField);
                
                showSplineField.RegisterValueChangeCallback(evt => {
                    bool newValue = evt.changedProperty.boolValue && settingsProperty.FindPropertyRelative("Enabled").boolValue;
                    splineSettingsLabel.SetEnabled(newValue);
                    splineWidthField.SetEnabled(newValue);
                    splineColorField.SetEnabled(newValue);
                });
                List<(string, VisualElement)> fields = enabledField.userData as List<(string, VisualElement)>;
                fields.Add((string.Empty, showSplineButton));
                fields.Add(("ShowSpline", splineSettingsLabel));
                fields.Add(("ShowSpline", splineWidthField));
                fields.Add(("ShowSpline", splineColorField));
            }
            
            VisualElement pointSettingsContainer = new VisualElement().WithClasses("point-settings-container", "curve-settings-container");
            mainContainer.Add(pointSettingsContainer);
            {
                VisualElement header = new VisualElement().WithClasses("curve-settings-category-header");;
                Label pointSettingsLabel = new Label("Point Settings").WithClasses("curve-settings-category-title", "point-settings-label");
                header.Add(pointSettingsLabel);
                
                (PropertyField showPointsField, Button showPointsButton) = MakeCurveVisualizerToggle("Hide", "Show", "ShowPoints", settingsProperty);
                header.Add(showPointsField);
                header.Add(showPointsButton);
                pointSettingsContainer.Add(header);
                
                PropertyField pointSizeField = new(settingsProperty.FindPropertyRelative("PointSize"), "Size");
                pointSizeField.Bind(serializedObject);
                pointSettingsContainer.Add(pointSizeField);
                PropertyField pointColorField = new(settingsProperty.FindPropertyRelative("PointColor"), "Color");
                pointColorField.Bind(serializedObject);
                pointSettingsContainer.Add(pointColorField);
                
                showPointsField.RegisterValueChangeCallback(evt => {
                    bool newValue = evt.changedProperty.boolValue && settingsProperty.FindPropertyRelative("Enabled").boolValue;
                    pointSettingsLabel.SetEnabled(newValue);
                    pointSizeField.SetEnabled(newValue);
                    pointColorField.SetEnabled(newValue);
                });
                
                List<(string, VisualElement)> fields = enabledField.userData as List<(string, VisualElement)>;
                fields.Add((string.Empty, showPointsButton));
                fields.Add(("ShowPoints", pointSettingsLabel));
                fields.Add(("ShowPoints", pointSizeField));
                fields.Add(("ShowPoints", pointColorField));
            }

            VisualElement directionVectorSettingsContainer = new VisualElement().WithClasses("curve-settings-container", "direction-vector-settings-container");
            mainContainer.Add(directionVectorSettingsContainer);
            {
                VisualElement header = new VisualElement().WithClasses("curve-settings-category-header");
                Label directionVectorSettingsLabel = new Label("Direction Vector Settings").WithClasses("curve-settings-category-title", "direction-vector-settings-label");
                header.Add(directionVectorSettingsLabel);
                
                (PropertyField showDirectionVectorsField, Button showDirectionVectorsButton) = MakeCurveVisualizerToggle("Hide", "Show", "ShowDirectionVectors", settingsProperty);
                header.Add(showDirectionVectorsField);
                header.Add(showDirectionVectorsButton);
                directionVectorSettingsContainer.Add(header);
                
                PropertyField directionVectorLengthField = new(settingsProperty.FindPropertyRelative("DirectionVectorLength"), "Length");
                directionVectorLengthField.Bind(serializedObject);
                directionVectorSettingsContainer.Add(directionVectorLengthField);
                
                PropertyField directionVectorWidthField = new(settingsProperty.FindPropertyRelative("DirectionVectorWidth"), "Width");
                directionVectorWidthField.Bind(serializedObject);
                directionVectorSettingsContainer.Add(directionVectorWidthField);
                
                PropertyField directionTangentColorField = new(settingsProperty.FindPropertyRelative("DirectionTangentColor"), "Tangent Color");
                directionTangentColorField.Bind(serializedObject);
                directionVectorSettingsContainer.Add(directionTangentColorField);
                
                PropertyField directionNormalColorField = new(settingsProperty.FindPropertyRelative("DirectionNormalColor"), "Normal Color");
                directionNormalColorField.Bind(serializedObject);
                directionVectorSettingsContainer.Add(directionNormalColorField);
                
                PropertyField directionBinormalColorField = new(settingsProperty.FindPropertyRelative("DirectionBinormalColor"), "Binormal Color");
                directionBinormalColorField.Bind(serializedObject);
                directionVectorSettingsContainer.Add(directionBinormalColorField);
                
                showDirectionVectorsField.RegisterValueChangeCallback(evt => {
                    bool newValue = evt.changedProperty.boolValue && settingsProperty.FindPropertyRelative("Enabled").boolValue;
                    
                    directionVectorSettingsLabel.SetEnabled(newValue);
                    directionVectorLengthField.SetEnabled(newValue);
                    directionVectorWidthField.SetEnabled(newValue);
                    directionTangentColorField.SetEnabled(newValue);
                    directionNormalColorField.SetEnabled(newValue);
                    directionBinormalColorField.SetEnabled(newValue);
                });
                
                List<(string, VisualElement)> fields = enabledField.userData as List<(string, VisualElement)>;
                fields.Add((string.Empty, showDirectionVectorsButton));
                fields.Add(("ShowDirectionVectors", directionVectorSettingsLabel));
                fields.Add(("ShowDirectionVectors", directionVectorLengthField));
                fields.Add(("ShowDirectionVectors", directionVectorWidthField));
                fields.Add(("ShowDirectionVectors", directionTangentColorField));
                fields.Add(("ShowDirectionVectors", directionNormalColorField));
                fields.Add(("ShowDirectionVectors", directionBinormalColorField));
            }
            
            curveVisualizerTab.Add(mainContainer);
        }

        private void BuildInstancesTab() {
            UnityEditor.SerializedProperty settingsProperty = serializedObject.FindProperty("instancedGeometrySettings");
            VisualElement contentContainer = instancesTab.Q<VisualElement>("InstancesContent");
            if (contentContainer == null) {
                contentContainer = new VisualElement {name = "InstancesContent"};
                instancesTab.Add(contentContainer);
            } else {
                contentContainer.Clear();
            }
            
            PropertyField materialsListField = new(settingsProperty.FindPropertyRelative("Materials"), "Materials");
            materialsListField.AddToClassList("materials-list");
            materialsListField.Bind(serializedObject);
            contentContainer.Add(materialsListField);
        }

        private (PropertyField field, Button button) MakeCurveVisualizerToggle(string onLabel, string offLabel, string propertyName, UnityEditor.SerializedProperty settingsProperty) {
            UnityEditor.SerializedProperty property = settingsProperty.FindPropertyRelative(propertyName);
            PropertyField field = new PropertyField(property).WithClasses("d-none");
            field.Bind(serializedObject);

            Button button = new Button { text = property.boolValue ? onLabel : offLabel }.WithClasses("toggle-button");
            button.clicked += () => {
                serializedObject.Update();
                property.boolValue = !property.boolValue;
                if (property.boolValue) {
                    button.AddToClassList("toggle-button__active");
                } else {
                    button.RemoveFromClassList("toggle-button__active");
                }
                button.text = property.boolValue ? onLabel : offLabel;
                serializedObject.ApplyModifiedProperties();
            };
            
            if (property.boolValue) {
                button.AddToClassList("toggle-button__active");
            }
            
            return (field, button);
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
                targetGraph.SceneData.AddProperty(property.Guid, property.ReferenceName, new PropertyValue(property));
            }
            
            serializedObject.Update();
        }
    }
}