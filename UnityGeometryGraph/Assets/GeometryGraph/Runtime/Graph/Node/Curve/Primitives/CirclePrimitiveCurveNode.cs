using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using UnityCommons;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettingsAttribute(CalculateDuringDeserialization = false)]
    public partial class CirclePrimitiveCurveNode {
        [In, AdditionalValueChangedCode("{other} = {other}.Clamped(Constants.MIN_CIRCLE_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)] 
        public int Points { get; private set; } = 32;
        [In, AdditionalValueChangedCode("{other} = {other}.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        public float Radius { get; private set; } = 1.0f;
        [Out] public CurveData Curve { get; private set; }


        [GetterMethod(nameof(Curve), Inline = true)]
        private CurveData GetCurve() {
            if (Curve == null) CalculateResult();
            return Curve ?? CurveData.Empty;
        }

        [CalculatesProperty(nameof(Curve))]
        private void CalculateResult() {
            if (RuntimeGraphObjectData.IsDuringSerialization) {
                Debug.LogWarning("Attempting to generate curve during serialization. Aborting.");
                Curve = CurveData.Empty;
                return;
            }

            Curve = CurvePrimitive.Circle(Points.Clamped(Constants.MIN_CIRCLE_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION), Radius.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS));
        }
    }
}