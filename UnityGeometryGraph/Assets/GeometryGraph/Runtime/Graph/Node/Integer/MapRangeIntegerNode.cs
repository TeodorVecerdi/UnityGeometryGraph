using GeometryGraph.Runtime.Attributes;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettings(OutputPath = "_Generated")]
    public partial class MapRangeIntegerNode {
        [Setting] public bool Clamp { get; private set; }
        [In] public int Value { get; private set; }
        [In] public int FromMin { get; private set; } = 0;
        [In] public int FromMax { get; private set; } = 1;
        [In] public int ToMin { get; private set; } = 0;
        [In] public int ToMax { get; private set; } = 1;
        [Out] public int Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true)]
        private int GetResult() {
            int value = Value.Map(FromMin, FromMax, ToMin, ToMax);
            return Clamp ? value.Clamped(ToMin, ToMax) : value;
        }
    }
}