using GeometryGraph.Runtime.Attributes;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class StringValueNode {
        [Setting] public string Value { get; private set; }
        [Out] public string Result { get; private set; }
        [GetterMethod(nameof(Result), Inline = true)] private string GetResult() => Value;
    }
}