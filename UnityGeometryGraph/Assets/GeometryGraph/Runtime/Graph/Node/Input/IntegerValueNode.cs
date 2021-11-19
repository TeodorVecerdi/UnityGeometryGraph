using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode(OutputPath = "_Generated")]
    public partial class IntegerValueNode {
        [Setting] public int Value { get; private set; }
        [Out(PortName = "ValuePort")] public int Result { get; private set; }
        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly] private int GetResult() => Value;
    }
}