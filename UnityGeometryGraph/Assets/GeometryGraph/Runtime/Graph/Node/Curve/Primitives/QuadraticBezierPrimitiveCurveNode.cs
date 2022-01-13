using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettings(CalculateDuringDeserialization = false)]
    public partial class QuadraticBezierPrimitiveCurveNode {
        [AdditionalValueChangedCode("{other} = {other}.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1)", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public int Points { get; private set; } = 32;
        [In] public bool IsClosed { get; private set; }
        [In] public float3 Start { get; private set; } = float3.zero;
        [In] public float3 Control { get; private set; } = float3_ext.right;
        [In] public float3 End { get; private set; } = float3_ext.forward;
        [Out] public CurveData Curve { get; private set; }

        [GetterMethod(nameof(Curve), Inline = true)]
        private CurveData GetCurve() {
            if (Curve == null) CalculateResult();
            return Curve ?? CurveData.Empty;
        }

        [CalculatesProperty(nameof(Curve))]
        private void CalculateResult() {
            Curve = Utils.IfNotSerializing(
                () => CurvePrimitive.QuadraticBezier(
                    Points.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1) - 1,
                    IsClosed, Start, Control, End),
                "CurvePrimitive.QuadraticBezier", CurveData.Empty);
        }
    }
}