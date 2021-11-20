using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;


namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class FloatBranchNode {
        [In] public bool Condition { get; private set; }
        [In] public float IfTrue { get; private set; }
        [In] public float IfFalse { get; private set; }
        [Out] public float Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        private float GetResult() => Condition ? IfTrue : IfFalse;
    }
}