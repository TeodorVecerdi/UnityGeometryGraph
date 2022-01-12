using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettings(CalculateDuringDeserialization = false)]
    public partial class CurveToGeometryNode {
        [In(
            UpdateValueCode = "",
            GetValueCode = "{self} = GetValue(connection, CurveData.Empty).Clone()"
        )]
        public CurveData Source { get; private set; }

        [In(
            UpdateValueCode = "",
            GetValueCode = "{self} = GetValue(connection, CurveData.Empty).Clone()"
        )]
        public CurveData Profile { get; private set; }

        [In] public float RotationOffset { get; private set; }
        [In] public float IncrementalRotationOffset { get; private set; }

        [Setting] public bool CloseCaps { get; private set; }
        [Setting] public bool SeparateMaterialForCaps { get; private set; }
        [Setting] public bool ShadeSmoothCurve { get; private set; }
        [Setting] public bool ShadeSmoothCaps { get; private set; }
        [Setting] public CurveToGeometrySettings.CapUVType CapUVType { get; private set; } = CurveToGeometrySettings.CapUVType.WorldSpace;

        [Out] public GeometryData Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true)]
        private GeometryData GetResult() {
            if (Result == null) CalculateResult();
            return Result;
        }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port == SourcePort) {
                Source = null;
                Result = GeometryData.Empty;
            } else if (port == ProfilePort) {
                Profile = null;
            }
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            if (Source is not { Type: not CurveType.None }) {
                DebugUtility.Log("Source curve was null");
                Result = GeometryData.Empty;
                return;
            }

            if (Profile is not { Type: not CurveType.None }) {
                DebugUtility.Log("Profile curve was null");
                Result = CurveToGeometry.WithoutProfile(Source);
                return;
            }

            if (RuntimeGraphObjectData.IsDuringSerialization) {
                DebugUtility.Log("Attempting to generate geometry from curve during serialization. Aborting.");
                Result = null;
                return;
            }

            DebugUtility.Log("Generated mesh with profile");

            Result = CurveToGeometry.WithProfile(
                Source, Profile,
                new CurveToGeometrySettings(CloseCaps, SeparateMaterialForCaps, ShadeSmoothCurve, ShadeSmoothCaps, RotationOffset, IncrementalRotationOffset, CapUVType));
        }
    }
}