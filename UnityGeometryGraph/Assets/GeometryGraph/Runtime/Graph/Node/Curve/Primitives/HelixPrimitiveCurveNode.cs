using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using UnityCommons;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettings(CalculateDuringDeserialization = false)]
    public partial class HelixPrimitiveCurveNode {
        [AdditionalValueChangedCode("{other} = {other}.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1)", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public int Points { get; private set; } = 64;
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public float TopRadius { get; private set; } = 1.0f;
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public float BottomRadius { get; private set; } = 1.0f;
        [In] public float Rotations { get; private set; } = 2.0f;
        [In] public float Pitch { get; private set; } = 1.0f;
        [Out] public CurveData Curve { get; private set; }

        [GetterMethod(nameof(Curve), Inline = true)]
        private CurveData GetCurve() {
            if (Curve == null) CalculateResult();
            return Curve ?? CurveData.Empty;
        }

        [CalculatesProperty(nameof(Curve))]
        private void CalculateResult() {
            Curve = Utils.IfNotSerializing(
                () => CurvePrimitive.Helix(
                    Points.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1) - 1,
                    Rotations, Pitch,
                    TopRadius.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS),
                    BottomRadius.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS)),
                "CurvePrimitive.Helix", CurveData.Empty);
        }
    }
}