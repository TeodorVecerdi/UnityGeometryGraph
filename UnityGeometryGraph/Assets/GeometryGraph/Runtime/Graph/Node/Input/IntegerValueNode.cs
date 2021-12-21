using GeometryGraph.Runtime.Attributes;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class IntegerValueNode {
        [Setting] public int Value { get; private set; }
        [Out(PortName = "ValuePort")] public int Result { get; private set; }
        [GetterMethod(nameof(Result), Inline = true)] private int GetResult() => Value;
    }
}