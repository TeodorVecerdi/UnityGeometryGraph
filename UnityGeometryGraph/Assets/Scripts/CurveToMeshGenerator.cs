using GeometryGraph.Runtime.Geometry;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Curve.TEMP {
    public class CurveToMeshGenerator : SerializedMonoBehaviour {
        [Required] public ICurveProvider Curve;
        [Required] public ICurveProvider Profile;
        [Required] public GeometryExporter Exporter;
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
            Geometry = CurveToGeometry.WithProfile(Curve.Curve, Profile.Curve, new CurveToGeometrySettings(CloseCaps, SeparateMaterialForCaps, ShadeSmoothCurve, ShadeSmoothCaps, RotationOffset, IncrementalRotationOffset, CapUVType));
            Exporter.Export(Geometry);
        }
    }
}