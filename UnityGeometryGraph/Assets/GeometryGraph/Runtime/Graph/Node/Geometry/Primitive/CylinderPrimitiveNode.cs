using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Graph {
    [AdditionalUsingStatements("UnityCommons")]
    [GenerateRuntimeNode]
    public partial class CylinderPrimitiveNode {
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public float BottomRadius { get; private set; } = 1.0f;
        
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public float TopRadius { get; private set; } = 1.0f;
        
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(Constants.MIN_GEOMETRY_HEIGHT);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public float Height { get; private set; } = 2.0f;
        
        [AdditionalValueChangedCode("{other} = {other}.Clamped(Constants.MIN_CIRCULAR_GEOMETRY_POINTS, Constants.MAX_CIRCULAR_GEOMETRY_POINTS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public int Points { get; private set; } = 8;
        
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(1);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public int HorizontalUVRepetitions { get; private set; } = 2;
        
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(1);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public int VerticalUVRepetitions { get; private set; } = 1;
        
        [Out] public GeometryData Result { get; private set; }
        
        [GetterMethod(nameof(Result), Inline = true)]
        private GeometryData GetResult() {
            if (Result == null) CalculateResult();
            return Result;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            Result = GeometryPrimitive.Cylinder(BottomRadius, TopRadius, Height, Points, new CylinderUVSettings(HorizontalUVRepetitions, VerticalUVRepetitions));
        }
    }
}