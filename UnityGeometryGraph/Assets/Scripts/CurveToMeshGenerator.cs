using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Geometry;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace GeometryGraph.Runtime.Testing {
    public class CurveToMeshGenerator : SerializedMonoBehaviour {
        [Required] public ICurveProvider Curve;
        [Required] public ICurveProvider Profile;
        [Required] public MeshFilter MeshFilter;
        public Mesh Mesh;
        public GeometryExporter Exporter = new GeometryExporter();
        [Space]
        public bool CloseCaps;
        public bool SeparateMaterialForCaps;
        public bool ShadeSmoothCurve;
        public bool ShadeSmoothCaps;
        public float RotationOffset;
        public float IncrementalRotationOffset;
        [EnumToggleButtons] public CurveToGeometrySettings.CapUVType CapUVType = CurveToGeometrySettings.CapUVType.WorldSpace;

        public GeometryData Geometry;

        [Button]
        public void Generate() {
            if (Mesh == null) InitializeMesh();
            Geometry = CurveToGeometry.WithProfile(Curve.Curve, Profile.Curve, new CurveToGeometrySettings(CloseCaps, SeparateMaterialForCaps, ShadeSmoothCurve, ShadeSmoothCaps, RotationOffset, IncrementalRotationOffset, CapUVType));
            Exporter.Export(Geometry, Mesh);
        }

        private void InitializeMesh() {
            Mesh = new Mesh {
                name = "Generated Mesh",
                indexFormat = IndexFormat.UInt32
            };
            if (MeshFilter.sharedMesh != null) DestroyImmediate(MeshFilter.sharedMesh);
            MeshFilter.sharedMesh = Mesh;
        }
    }
}