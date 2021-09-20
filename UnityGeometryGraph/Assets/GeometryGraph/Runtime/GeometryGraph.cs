using System;
using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Graph;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GeometryGraph.Runtime {
    public class GeometryGraph : MonoBehaviour {
        [SerializeField] private RuntimeGraphObject graph;
        [SerializeField] private GeometryGraphSceneData sceneData = new GeometryGraphSceneData();
        [SerializeField] private GeometryExporter exporter;
        
        [SerializeField] private string graphGuid;

        public RuntimeGraphObject Graph => graph;
        public GeometryGraphSceneData SceneData => sceneData;
        public string GraphGuid {
            get => graphGuid;
            set => graphGuid = value;
        }
        
        public void Evaluate() {
            if(exporter == null) return;

            Debug.Log(graph.RuntimeData.Connections.Count);
            Debug.Log(graph.RuntimeData.Nodes.Count);
            Debug.Log(graph.RuntimeData.Properties.Count);
            exporter.Export(graph.Evaluate(sceneData));
        }
    }

    [Serializable] public class GeometryGraphSceneData {
        [SerializeField] private PropertyDataDictionary propertyData = new PropertyDataDictionary();
        public PropertyDataDictionary PropertyData => propertyData;
        public void Reset() {
            Debug.Log("Reset");
            propertyData.Clear();
        }
    }
    
    [Serializable] public class PropertyDataDictionary : SerializedDictionary<string, PropertyValue> {}

    [Serializable] public class PropertyValue {
        [SerializeField] public Object ObjectValue;
        //!! Add more here as needed

        public object GetValueForPropertyType(PropertyType type) {
            switch (type) {
                case PropertyType.GeometryObject:
                case PropertyType.GeometryCollection:
                    return ObjectValue;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}