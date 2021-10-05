using GeometryGraph.Runtime.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Geometry {
    public class TestScript : SerializedMonoBehaviour, IGeometryProvider {
        public IGeometryProvider GeometryProvider;
        public GeometryExporter Exporter;
        public int Levels = 1;
        public GeometryData Subdivided;

        [Button]
        private void Subdivide() {
            if (GeometryProvider == null) return;
            Subdivided = SimpleSubdivision.Subdivide(GeometryProvider.Geometry, Levels);
            Exporter.Export(Subdivided);
        }

        public GeometryData Geometry => Subdivided;
        public Matrix4x4 LocalToWorldMatrix => transform.localToWorldMatrix;
    }
}