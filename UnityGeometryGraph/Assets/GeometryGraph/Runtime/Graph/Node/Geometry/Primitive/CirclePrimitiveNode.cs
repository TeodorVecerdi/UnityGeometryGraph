using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [AdditionalUsingStatements("UnityCommons")]
    [GenerateRuntimeNode]
    public partial class CirclePrimitiveNode {
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public float Radius { get; private set; } = 1.0f;

        [AdditionalValueChangedCode("{other} = {other}.Clamped(Constants.MIN_CIRCULAR_GEOMETRY_POINTS, Constants.MAX_CIRCULAR_GEOMETRY_POINTS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public int Points { get; private set; } = 8;

        [Out] public GeometryData Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true)]
        private GeometryData GetResult() {
            if (Result == null) CalculateResult();
            return Result;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            Result = GeometryPrimitive.Circle(Radius.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS), Points.Clamped(Constants.MIN_CIRCULAR_GEOMETRY_POINTS, Constants.MAX_CIRCULAR_GEOMETRY_POINTS));
        }
    }
}