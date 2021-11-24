using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GeometryGraph.Runtime {
    public class GeometryGraph : MonoBehaviour {
        [SerializeField] private RuntimeGraphObject graph;
        [SerializeField] private GeometryExporter exporter;
        
        [SerializeField] private GeometryGraphSceneData sceneData = new GeometryGraphSceneData();
        [SerializeField] private CurveVisualizerSettings curveVisualizerSettings;

        // Evaluation data
        [SerializeField] private CurveData curveData;
        [SerializeField] private GeometryData geometryData;
        
        [SerializeField] private string graphGuid;

        public RuntimeGraphObject Graph {
            get => graph;
            set => graph = value;
        }
        public GeometryExporter Exporter {
            get => exporter;
            set => exporter = value;
        }
        public GeometryGraphSceneData SceneData => sceneData;
        public string GraphGuid {
            get => graphGuid;
            set => graphGuid = value;
        }
        
        public bool HasCurveData => curveData is { };
        public CurveVisualizerSettings CurveVisualizerSettings => curveVisualizerSettings;

        public void Evaluate() {
            GeometryGraphEvaluationResult evaluationResult = graph.Evaluate(sceneData);
            curveData = evaluationResult.CurveData;
            geometryData = evaluationResult.GeometryData;
            
            if (exporter != null) {
                exporter.Export(geometryData ?? GeometryData.Empty);
            }
        }

        public void OnPropertiesChanged(int newPropertyHashCode) {
            sceneData.PropertyHashCode = newPropertyHashCode;
            List<string> toRemove = SceneData.PropertyData.Keys.Where(key => Graph.RuntimeData.Properties.All(property => !string.Equals(property.Guid, key, StringComparison.InvariantCulture))).ToList();
            foreach (string key in toRemove) {
                SceneData.PropertyData.Remove(key);
            }

            foreach (Property property in Graph.RuntimeData.Properties) {
                if (!SceneData.PropertyData.ContainsKey(property.Guid)) {
                    SceneData.PropertyData[property.Guid] = new PropertyValue(property);
                }
            }
        }

        public void OnDefaultPropertyValueChanged(Property property) {
            sceneData.PropertyData[property.Guid].UpdateDefaultValue(property.Type, property.DefaultValue);
        }

    }

    [Serializable]
    public class CurveVisualizerSettings {
        // Enable/disable different visualizations
        public bool Enabled = true;
        public bool ShowSpline = true;
        public bool ShowPoints = true;
        public bool ShowDirectionVectors = true;
        
        // Spline settings
        public float SplineWidth = 2.0f;
        public Color SplineColor = Color.white;
        
        // Point settings
        public float PointSize = 0.01f;
        public Color PointColor = Color.white;
        
        // Direction vector settings
        public float DirectionVectorLength = 0.1f;
        public float DirectionVectorWidth = 2.0f;
        public Color DirectionTangentColor = Color.blue;
        public Color DirectionNormalColor = Color.red;
        public Color DirectionBinormalColor = Color.green;
        
#if UNITY_EDITOR
        public bool SplineSettingsFoldout = true;
        public bool PointSettingsFoldout = true;
        public bool DirectionVectorSettingsFoldout = true;
#endif
    }

    [Serializable] 
    public class GeometryGraphSceneData {
        [SerializeField] private PropertyDataDictionary propertyData = new PropertyDataDictionary();
        [SerializeField] private int propertyHashCode;
        
#if UNITY_EDITOR
        public bool PropertiesFoldout = true;
#endif

        public PropertyDataDictionary PropertyData => propertyData;

        public int PropertyHashCode {
            get => propertyHashCode;
            set => propertyHashCode = value;
        }

        public void Reset() {
            Debug.Log("Reset");
            propertyData.Clear();
            propertyHashCode = 0;
        }
    }

    [Serializable] public class PropertyDataDictionary : SerializedDictionary<string, PropertyValue> {}

    [Serializable] public class PropertyValue {
        [SerializeField] public bool HasCustomValue;
        [SerializeField] public GeometryObject ObjectValue;
        [SerializeField] public GeometryCollection CollectionValue;
        [SerializeField] public int IntValue;
        [SerializeField] public float FloatValue;
        [SerializeField] public Vector3 VectorValue;
        //!! Add more here as needed

        public PropertyValue(Property property) {
            HasCustomValue = false;
            UpdateDefaultValue(property.Type, property.DefaultValue);
        }

        public void UpdateDefaultValue(PropertyType type, DefaultPropertyValue defaultPropertyValue) {
            if (HasCustomValue) return;
            switch(type) {
                case PropertyType.GeometryObject:
                    ObjectValue = null;
                    break;
                case PropertyType.GeometryCollection:
                    CollectionValue = null;
                    break;
                case PropertyType.Integer:
                    IntValue = defaultPropertyValue.IntValue;
                    break;
                case PropertyType.Float:
                    FloatValue = defaultPropertyValue.FloatValue;
                    break;
                case PropertyType.Vector:
                    VectorValue = defaultPropertyValue.VectorValue;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public object GetValueForPropertyType(PropertyType type) {
            switch (type) {
                case PropertyType.GeometryObject: return ObjectValue;
                case PropertyType.GeometryCollection: return CollectionValue;
                case PropertyType.Integer: return IntValue;
                case PropertyType.Float: return FloatValue;
                case PropertyType.Vector: return (float3)VectorValue;

                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}