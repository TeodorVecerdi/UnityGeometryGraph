using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Graph {
    [AdditionalUsingStatements("UnityCommons")]
    [GenerateRuntimeNode(OutputPath = "_Generated")]
    public partial class ConePrimitiveNode {
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public float Radius { get; private set; } = 1.0f;
        
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(Constants.MIN_GEOMETRY_HEIGHT);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public float Height { get; private set; } = 2.0f;
        
        [AdditionalValueChangedCode("{other} = {other}.Clamped(Constants.MIN_CIRCULAR_GEOMETRY_POINTS, Constants.MAX_CIRCULAR_GEOMETRY_POINTS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public int Points { get; private set; } = 8;
        [Out] public GeometryData Result { get; private set; }
        
        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            Result = GeometryPrimitive.Cone(Radius, Height, Points);
        }
    }
}