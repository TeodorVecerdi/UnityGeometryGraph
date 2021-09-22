using System;
using System.Linq;
using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Graph;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GeometryGraph.Runtime {
    public class GeometryGraph : MonoBehaviour {
        [SerializeField] private RuntimeGraphObject graph;
        [SerializeField] private GeometryGraphSceneData sceneData = new GeometryGraphSceneData();
        [SerializeField] private GeometryExporter exporter;
        
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
            exporter.Export(graph.Evaluate(sceneData));
        }
        
        public void OnPropertiesChanged(int newPropertyHashCode) {
            sceneData.PropertyHashCode = newPropertyHashCode;
            var toRemove = SceneData.PropertyData.Keys.Where(key => Graph.RuntimeData.Properties.All(property => !string.Equals(property.Guid, key, StringComparison.InvariantCulture))).ToList();
            foreach (var key in toRemove) {
                SceneData.PropertyData.Remove(key);
            }

            foreach (var property in Graph.RuntimeData.Properties) {
                if (!SceneData.PropertyData.ContainsKey(property.Guid)) {
                    SceneData.PropertyData[property.Guid] = new PropertyValue();
                }
            }
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
        [SerializeField] public Object ObjectValue;
        [SerializeField] public int IntValue; 
        [SerializeField] public float FloatValue; 
        [SerializeField] public float3 VectorValue; 
        //!! Add more here as needed

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