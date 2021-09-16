using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Geometry {
    
    public class GeometryMerger : MonoBehaviour, IGeometryProvider {
        [SerializeField] private List<GeometryImporter> importers;
        [SerializeField] private GeometryData geometryData;

        [Button]
        private void Merge() {
            geometryData = GeometryData.Empty;

            foreach (var importer in importers) {
                if(importer == null || importer.Geometry == null) continue; 
                GeometryData.Merge(geometryData, importer.Geometry);
            }
        }

        public GeometryData Geometry => geometryData;
        public Matrix4x4 LocalToWorldMatrix => transform.localToWorldMatrix;
    }
}