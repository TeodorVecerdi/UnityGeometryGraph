using System;
using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettings(OutputPath = "_Generated")]
    public partial class BooleanMathNode {
        [In] public bool A { get; private set; }
        [In] public bool B { get; private set; }
        [Setting] public BooleanMathNode_Operation Operation { get; private set; }
        [Out] public bool Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        private bool GetResult() {
            return Operation switch {
                BooleanMathNode_Operation.AND => A && B,
                BooleanMathNode_Operation.OR => A || B,
                BooleanMathNode_Operation.XOR => A ^ B,
                BooleanMathNode_Operation.NOT => !A,
                _ => throw new ArgumentOutOfRangeException(nameof(Operation), Operation, null)
            };
        }
        
        public enum BooleanMathNode_Operation {AND = 0, OR = 1, XOR = 2, NOT = 3}
    }
}