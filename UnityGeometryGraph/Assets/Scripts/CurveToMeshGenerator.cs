using GeometryGraph.Runtime.Geometry;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Curve.TEMP {
    public class CurveToMeshGenerator : SerializedMonoBehaviour {
        [Required] public ICurveProvider Curve;
        [Required] public ICurveProvider Profile;
        [Required] public GeometryExporter Exporter;
        public GeometryData Geometry;

        [Button]
        public void Generate() {
            Geometry = CurveToGeometry.WithProfile(Curve.Curve, Profile.Curve, true, 0.0f);
            Exporter.Export(Geometry);
        }
    }
}