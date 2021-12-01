using GeometryGraph.Runtime.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace GeometryGraph.Runtime.Geometry {
    public class TestScript : SerializedMonoBehaviour, IGeometryProvider {
        public MeshFilter MeshFilter;
        public Mesh Mesh;
        public GeometryExporter Exporter = new GeometryExporter();
        [MinValue(0.0f)] public float Radius = 0.5f;
        [MinValue(0)] public int Subdivisions = 3;
        public GeometryData Icosphere;

        [Button]
        private void MakeIcosphere() {
            if (Mesh == null) InitializeMesh();
            Icosphere = GeometryPrimitive.Icosphere(Radius, Subdivisions);
            Exporter.Export(Icosphere, Mesh);
        }

        [Button]
        private void MakeIcosahedron() {
            if (Mesh == null) InitializeMesh();
            Icosphere = GeometryPrimitive.Icosahedron();
            Exporter.Export(Icosphere, Mesh);
        }

        private void InitializeMesh() {
            Mesh = new Mesh {
                name = "Generated Mesh",
                indexFormat = IndexFormat.UInt32
            };
            if (MeshFilter.sharedMesh != null) DestroyImmediate(MeshFilter.sharedMesh);
            MeshFilter.sharedMesh = Mesh;
        }

        public GeometryData Geometry => Icosphere;
        public Matrix4x4 LocalToWorldMatrix => transform.localToWorldMatrix;
    }
}