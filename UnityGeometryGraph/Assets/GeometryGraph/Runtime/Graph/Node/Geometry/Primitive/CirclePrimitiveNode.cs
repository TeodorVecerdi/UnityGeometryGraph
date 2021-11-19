using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Graph {
    [AdditionalUsingStatements("UnityCommons")]
    [GenerateRuntimeNode]
    [GeneratorSettings(OutputPath = "_Generated")]
    public partial class CirclePrimitiveNode {
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public float Radius { get; private set; } = 1.0f;
        
        [AdditionalValueChangedCode("{other} = {other}.Clamped(Constants.MIN_CIRCULAR_GEOMETRY_POINTS, Constants.MAX_CIRCULAR_GEOMETRY_POINTS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public int Points { get; private set; } = 8;
        
        [Out] public GeometryData Result { get; private set; }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            DebugUtility.Log("Calculated result");
            Result = GeometryPrimitive.Circle(Radius, Points);
        }
    }
}