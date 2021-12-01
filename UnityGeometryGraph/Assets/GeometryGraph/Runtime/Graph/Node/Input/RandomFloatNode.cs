using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class RandomFloatNode {
        [In] public int Seed { get; private set; }
        [In] public float Min { get; private set; } = 0.0f;
        [In] public float Max { get; private set; } = 1.0f;
        [Out] public float Value { get; private set; }
        
        [GetterMethod(nameof(Value), Inline = true)] 
        private float GetValue() => Rand.RangeSeeded(Min, Max, Seed);

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ValuePort) yield break;
            if (count <= 0) yield break;
            Rand.PushState(Seed);
            for (int i = 0; i < count; i++) {
                yield return Rand.Range(Min, Max);
            }
            Rand.PopState();
        }
    }
}