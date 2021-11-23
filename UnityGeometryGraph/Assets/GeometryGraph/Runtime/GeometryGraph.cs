using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Curve.TEMP;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Graph;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GeometryGraph.Runtime {
    public class GeometryGraph : MonoBehaviour {
        [SerializeField] private RuntimeGraphObject graph;
        [SerializeField] private GeometryGraphSceneData sceneData = new GeometryGraphSceneData();
        [Space]
        [SerializeField] private GeometryExporter exporter;
        [SerializeField] private CurveVisualizer curveVisualizer;

        [SerializeField] private string graphGuid;

        public RuntimeGraphObject Graph {
            get => graph;
            set => graph = value;
        }
        public GeometryGraphSceneData SceneData => sceneData;
        public string GraphGuid {
            get => graphGuid;
            set => graphGuid = value;
        }

        public void Evaluate() {
            if(exporter == null) return;
            GeometryGraphEvaluationResult evaluationResult = graph.Evaluate(sceneData);
            if (exporter != null) {
                exporter.Export(evaluationResult.GeometryData ?? GeometryData.Empty);
            }
            
            if (curveVisualizer != null) {
                curveVisualizer.Load(evaluationResult.CurveData);
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

    [Serializable] public class GeometryGraphSceneData {
        [SerializeField] private PropertyDataDictionary propertyData = new PropertyDataDictionary();
        [SerializeField] private int propertyHashCode;

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
        [SerializeField] public Object ObjectValue;
        [SerializeField] public int IntValue;
        [SerializeField] public float FloatValue;
        [SerializeField] public float3 VectorValue;
        //!! Add more here as needed

        public PropertyValue(Property property) {
            HasCustomValue = false;
            UpdateDefaultValue(property.Type, property.DefaultValue);
        }

        public void UpdateDefaultValue(PropertyType type, DefaultPropertyValue defaultPropertyValue) {
            if (HasCustomValue) return;
            switch(type) {
                case PropertyType.GeometryObject:
                case PropertyType.GeometryCollection:
                    ObjectValue = null;
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
                case PropertyType.GeometryObject:
                case PropertyType.GeometryCollection:
                    return ObjectValue;

                case PropertyType.Integer: return IntValue;
                case PropertyType.Float:   return FloatValue;
                case PropertyType.Vector:  return VectorValue;

                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}