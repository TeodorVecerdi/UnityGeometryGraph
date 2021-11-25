using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using UnityEditor;

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

        public RuntimeGraphObject Graph => graph;
        public GeometryGraphSceneData SceneData => sceneData;
        
        public string GraphGuid {
            get => graphGuid;
            set => graphGuid = value;
        }
        
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
            sceneData.PropertyData[property.Guid].UpdateDefaultValue(property.DefaultValue);
        }
        
        private void OnDrawGizmos() {
            if (!curveVisualizerSettings.Enabled || curveData == null) return;

            Handles.matrix = Gizmos.matrix = transform.localToWorldMatrix;

            if (curveVisualizerSettings.ShowPoints || curveVisualizerSettings.ShowSpline) {
                Vector3[] points = curveData.Position.Select(float3 => (Vector3)float3).ToArray();
                Handles.color = curveVisualizerSettings.SplineColor;
                if (curveVisualizerSettings.ShowSpline) {
                    Handles.DrawAAPolyLine(curveVisualizerSettings.SplineWidth, points);
                    if (curveData.IsClosed) Handles.DrawAAPolyLine(curveVisualizerSettings.SplineWidth, points[0], points[^1]);
                }

                if (curveVisualizerSettings.ShowPoints) {
                    Gizmos.color = curveVisualizerSettings.PointColor;
                    foreach (Vector3 p in points) {
                        Gizmos.DrawSphere(p, curveVisualizerSettings.PointSize);
                    }
                }
            }

            if (curveVisualizerSettings.ShowDirectionVectors) {
                for (int i = 0; i < curveData.Position.Count; i++) {
                    float3 p = curveData.Position[i];
                    float3 t = curveData.Tangent[i];
                    float3 n = curveData.Normal[i];
                    float3 b = curveData.Binormal[i];

                    Handles.color = curveVisualizerSettings.DirectionTangentColor;
                    Handles.DrawAAPolyLine(curveVisualizerSettings.DirectionVectorWidth, p, p + t * curveVisualizerSettings.DirectionVectorLength);
                    Handles.color = curveVisualizerSettings.DirectionNormalColor;
                    Handles.DrawAAPolyLine(curveVisualizerSettings.DirectionVectorWidth, p, p + n * curveVisualizerSettings.DirectionVectorLength);
                    Handles.color = curveVisualizerSettings.DirectionBinormalColor;
                    Handles.DrawAAPolyLine(curveVisualizerSettings.DirectionVectorWidth, p, p + b * curveVisualizerSettings.DirectionVectorLength);
                }
            }
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
        [SerializeField] public GeometryObject ObjectValue;
        [SerializeField] public GeometryCollection CollectionValue;
        [SerializeField] public int IntValue;
        [SerializeField] public float FloatValue;
        [SerializeField] public Vector3 VectorValue; 
        
        [SerializeField] public GeometryObject DefaultObjectValue;
        [SerializeField] public GeometryCollection DefaultCollectionValue;
        [SerializeField] public int DefaultIntValue;
        [SerializeField] public float DefaultFloatValue;
        [SerializeField] public Vector3 DefaultVectorValue;
        //!! Add more here as needed

        [SerializeField] public PropertyType PropertyType;

        public PropertyValue(Property property) {
            PropertyType = property.Type;
            UpdateDefaultValue(property.DefaultValue, true);
        }

        public bool HasDefaultValue() {
            return PropertyType switch {
                PropertyType.GeometryObject => ObjectValue != DefaultObjectValue,
                PropertyType.GeometryCollection => CollectionValue != DefaultCollectionValue,
                PropertyType.Integer => IntValue != DefaultIntValue,
                PropertyType.Float => Math.Abs(FloatValue - DefaultFloatValue) > Constants.FLOAT_TOLERANCE,
                PropertyType.Vector => (VectorValue - DefaultVectorValue).sqrMagnitude > Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void UpdateDefaultValue(DefaultPropertyValue defaultPropertyValue, bool forceUpdate = false) {
            switch(PropertyType) {
                case PropertyType.GeometryObject:
                    if (forceUpdate || !HasDefaultValue()) ObjectValue = null;
                    DefaultObjectValue = null;
                    break;
                case PropertyType.GeometryCollection:
                    if (forceUpdate || !HasDefaultValue()) CollectionValue = null;
                    DefaultCollectionValue = null;
                    break;
                case PropertyType.Integer:
                    if (forceUpdate || !HasDefaultValue()) IntValue = defaultPropertyValue.IntValue;
                    DefaultIntValue = defaultPropertyValue.IntValue;
                    break;
                case PropertyType.Float:
                    if (forceUpdate || !HasDefaultValue()) FloatValue = defaultPropertyValue.FloatValue;
                    DefaultFloatValue = defaultPropertyValue.FloatValue;
                    break;
                case PropertyType.Vector:
                    if (forceUpdate || !HasDefaultValue()) VectorValue = defaultPropertyValue.VectorValue;
                    DefaultVectorValue = defaultPropertyValue.VectorValue;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(PropertyType), PropertyType, null);
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

        public override string ToString() {
            string value = PropertyType switch {
                PropertyType.GeometryObject => ObjectValue != null ? ObjectValue.name : "null",
                PropertyType.GeometryCollection => CollectionValue != null ? CollectionValue.name : "null",
                PropertyType.Integer => $"{IntValue}",
                PropertyType.Float => $"{FloatValue}",
                PropertyType.Vector => $"{VectorValue}",
                _ => throw new ArgumentOutOfRangeException()
            };
            
            string defaultValue = PropertyType switch {
                PropertyType.GeometryObject => DefaultObjectValue != null ? DefaultObjectValue.name : "null",
                PropertyType.GeometryCollection => DefaultCollectionValue != null ? DefaultCollectionValue.name : "null",
                PropertyType.Integer => $"{DefaultIntValue}",
                PropertyType.Float => $"{DefaultFloatValue}",
                PropertyType.Vector => $"{DefaultVectorValue}",
                _ => throw new ArgumentOutOfRangeException()
            };
            
            return $"{value} (default: {defaultValue})";
        }
    }
}