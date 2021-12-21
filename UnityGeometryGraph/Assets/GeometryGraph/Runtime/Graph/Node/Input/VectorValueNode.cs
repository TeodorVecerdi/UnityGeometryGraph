using GeometryGraph.Runtime.Attributes;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class VectorValueNode {
        [Setting] public float3 Value { get; private set; }
        [Out(PortName = "ValuePort")] public float3 Result { get; private set; }
        [GetterMethod(nameof(Result), Inline = true)] private float3 GetResult() => Value;
    }
}