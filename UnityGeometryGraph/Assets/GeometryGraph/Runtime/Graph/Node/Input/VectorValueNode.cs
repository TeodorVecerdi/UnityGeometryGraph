using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode(OutputPath = "_Generated")]
    public partial class VectorValueNode {
        [Setting] public float3 Value { get; private set; }
        [Out(PortName = "ValuePort")] public float3 Result { get; private set; }
        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly] private float3 GetResult() => Value;
    }
}