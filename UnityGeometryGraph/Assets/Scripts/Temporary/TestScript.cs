using GeometryGraph.Runtime.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Geometry {
    public class TestScript : SerializedMonoBehaviour, IGeometryProvider {
        public GeometryExporter Exporter;
        [MinValue(0.0f)] public float Radius = 0.5f;
        [MinValue(0)] public int Subdivisions = 3;
        public GeometryData Icosphere;

        [Button]
        private void MakeIcosphere() {
            Icosphere = Primitive.Icosphere(Radius, Subdivisions);
            // Icosphere = Primitive.IcosahedronHardcoded();
            Exporter.Export(Icosphere);
        }

        [Button]
        private void MakeIcosahedron() {
            Icosphere = Primitive.Icosahedron();
            Exporter.Export(Icosphere);
        }

        public GeometryData Geometry => Icosphere;
        public Matrix4x4 LocalToWorldMatrix => transform.localToWorldMatrix;
    }
}