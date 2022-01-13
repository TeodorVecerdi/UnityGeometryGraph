using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Geometry;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class SolidifyCurveNode {
        [In(
            DefaultValue = "CurveData.Empty",
            GetValueCode = "{self} = GetValue(connection, {default})",
            UpdateValueCode = ""
        )]
        public CurveData Curve { get; private set; }

        [In, AdditionalValueChangedCode("{other} = {other}.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        public float Thickness { get; private set; } = 0.1f;
        [In, AdditionalValueChangedCode("{other} = {other}.Clamped(Constants.MIN_CIRCLE_CURVE_RESOLUTION, Constants.MAX_SOLIDIFY_CURVE_RESOLUTION);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        public int Resolution { get; private set; } = 4;

        [Setting] public bool CloseCaps { get; private set; }
        [Setting] public bool SeparateMaterialForCaps { get; private set; }
        [Setting] public bool ShadeSmoothCurve { get; private set; }
        [Setting] public bool ShadeSmoothCaps { get; private set; }
        [Setting] public CurveToGeometrySettings.CapUVType CapUVType { get; private set; } = CurveToGeometrySettings.CapUVType.WorldSpace;
        [Out] public GeometryData Geometry { get; private set; }

        [CalculatesProperty(nameof(Geometry))]
        private void Calculate() {
            if (Curve == null || Curve.Points == 0 || Curve.Type == CurveType.None) {
                Geometry = GeometryData.Empty;
                return;
            }

            CurveData profile = Utils.IfNotSerializing(
                () => CurvePrimitive.Circle(
                    Resolution.Clamped(Constants.MIN_CIRCLE_CURVE_RESOLUTION, Constants.MAX_SOLIDIFY_CURVE_RESOLUTION),
                    Thickness.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS)
                ), "CurvePrimitive.Circle", CurveData.Empty);
            if (profile.Points == 0 || profile.Type == CurveType.None) {
                Geometry = GeometryData.Empty;
                return;
            }

            Geometry = CurveToGeometry.WithProfile(
                Curve, profile, new CurveToGeometrySettings(
                    CloseCaps, SeparateMaterialForCaps, ShadeSmoothCurve,
                    ShadeSmoothCaps, 0.0f, 0.0f, CapUVType
                ));
        }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port == CurvePort) {
                Curve = CurveData.Empty;
                Geometry = GeometryData.Empty;
            }
        }
    }
}