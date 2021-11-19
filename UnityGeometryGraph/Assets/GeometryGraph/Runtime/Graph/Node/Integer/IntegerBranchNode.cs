using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class IntegerBranchNode {
        [In] public bool Condition { get; private set; }
        [In] public int IfTrue { get; private set; }
        [In] public int IfFalse { get; private set; }
        [Out] public int Result { get; private set; }
        
        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        private int GetResult() => Condition ? IfTrue : IfFalse;
    }
}