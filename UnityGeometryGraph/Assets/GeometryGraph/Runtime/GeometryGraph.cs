using System;
using System.Collections.Generic;
using GeometryGraph.Runtime.Graph;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GeometryGraph.Runtime {
    public class GeometryGraph : MonoBehaviour {
        [SerializeField] private RuntimeGraphObject graph;
        [SerializeField] private GeometryGraphSceneData sceneData = new GeometryGraphSceneData();
        [SerializeField] private string graphGuid;

        public RuntimeGraphObject Graph => graph;
        public GeometryGraphSceneData SceneData => sceneData;
        public string GraphGuid {
            get => graphGuid;
            set => graphGuid = value;
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
    }
}