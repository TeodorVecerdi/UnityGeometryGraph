using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode(OutputPath = "_Generated")]
    public partial class ClampIntegerNode {
        [In] public int Input {get; private set; }
        [In] public int Min {get; private set; } = 0;
        [In] public int Max {get; private set; } = 1;
        [Out] public int Result {get; private set; }

        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        private int GetResult() => Input.Clamped(Min, Max);
    }
}