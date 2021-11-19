using System;
using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettings(OutputPath = "_Generated")]
    public partial class CompareIntegerNode {
        [In] public int A {get; private set; }
        [In] public int B {get; private set; }
        [Out] public bool Result {get; private set; }
        [Setting] public CompareIntegerNode_CompareOperation Operation {get; private set; }

        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        private bool GetResult() {
            return Operation switch {
                CompareIntegerNode_CompareOperation.LessThan => A < B,
                CompareIntegerNode_CompareOperation.LessThanOrEqual => A <= B,
                CompareIntegerNode_CompareOperation.GreaterThan => A > B,
                CompareIntegerNode_CompareOperation.GreaterThanOrEqual => A >= B,
                CompareIntegerNode_CompareOperation.Equal => A == B,
                CompareIntegerNode_CompareOperation.NotEqual => A != B,
                _ => throw new ArgumentOutOfRangeException(nameof(Operation), Operation, null)
            };
        }
        
        public enum CompareIntegerNode_CompareOperation {LessThan = 0, LessThanOrEqual = 1, GreaterThan = 2, GreaterThanOrEqual = 3, Equal = 4, NotEqual = 5}
    }
}