using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;
using JetBrains.Annotations;

namespace GeometryGraph.Runtime.Graph {
    [AdditionalUsingStatements("UnityCommons")]
    [GenerateRuntimeNode]
    public partial class SubdivideNode {
        [In(DefaultValue = "GeometryData.Empty")]
        [AdditionalValueChangedCode("{other} = {other}.Clone()", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        public GeometryData Input { get; private set; } = GeometryData.Empty;
        
        [AdditionalValueChangedCode("{other} = {other}.Clamped(0, Constants.MAX_SUBDIVISIONS)", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public int Levels { get; private set; } = 1;
        
        [Out] public GeometryData Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        private GeometryData GetResult() {
            if (Result == null) CalculateResult();
            return Result;
        }
        
        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port != InputPort) return;
            Input = GeometryData.Empty;
            Result = GeometryData.Empty;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            DebugUtility.Log("Calculate result");
            Result = SimpleSubdivision.Subdivide(Input, Levels);
        }
    }
}