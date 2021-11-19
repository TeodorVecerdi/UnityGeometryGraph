using System;
using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettings(OutputPath = "_Generated")]
    public partial class CompareFloatNode {
        [In] public float Tolerance { get; private set; }
        [In] public float A { get; private set; }
        [In] public float B { get; private set; }
        [Setting] public CompareFloatNode_CompareOperation Operation { get; private set; }
        [Out] public bool Result { get; private set; }
        
        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        public bool GetResult() {
            return Operation switch {
                CompareFloatNode_CompareOperation.LessThan => A < B,
                CompareFloatNode_CompareOperation.LessThanOrEqual => A <= B,
                CompareFloatNode_CompareOperation.GreaterThan => A > B,
                CompareFloatNode_CompareOperation.GreaterThanOrEqual => A >= B,
                CompareFloatNode_CompareOperation.Equal => MathF.Abs(A - B) < Tolerance,
                CompareFloatNode_CompareOperation.NotEqual => MathF.Abs(A - B) > Tolerance,
                _ => throw new ArgumentOutOfRangeException(nameof(Operation), Operation, null)
            };
        }

        public enum CompareFloatNode_CompareOperation {LessThan = 0, LessThanOrEqual = 1, GreaterThan = 2, GreaterThanOrEqual = 3, Equal = 4, NotEqual = 5}
    }
}