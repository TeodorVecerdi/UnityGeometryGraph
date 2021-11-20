using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class ClampFloatNode {
        [In] public float Input { get; private set; }
        [In] public float Min { get; private set; } = 0.0f;
        [In] public float Max { get; private set; } = 1.0f;
        [Out] public float Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        private float GetResult() => Input.Clamped(Min, Max);
    }
}