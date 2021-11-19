using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode(OutputPath = "_Generated")]
    public partial class VectorBranchNode {
        [In] public bool Condition { get; private set; }
        [In(GenerateEquality = false)] public float3 IfTrue { get; private set; }
        [In(GenerateEquality = false)] public float3 IfFalse { get; private set; }
        [Out] public float3 Result { get; private set; }
        
        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        private float3 GetResult() => Condition ? IfTrue : IfFalse;
    }
}