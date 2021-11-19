using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode(OutputPath = "_Generated")]
    public partial class FloatValueNode{
        [Setting] public float Value { get; private set; }
        [Out(PortName = "ValuePort")] public float Result { get; private set; }
        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly] private float GetResult() => Value;
    }
}