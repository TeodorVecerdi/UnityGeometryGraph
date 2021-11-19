using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [AdditionalUsingStatements("UnityCommons")]
    [GenerateRuntimeNode]
    [GeneratorSettings(OutputPath = "_Generated")]
    public partial class PlanePrimitiveNode {
        [In] public float Width { get; private set; } = 1.0f;
        [In] public float Height { get; private set; } = 1.0f;
        
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(0);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public int Subdivisions { get; private set; }
        
        [Out] public GeometryData Result { get; private set; }
        
        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            Result = GeometryPrimitive.Plane(new float2(Width, Height), Subdivisions);
        }
    }
}