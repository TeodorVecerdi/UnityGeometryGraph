using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettings(OutputPath = "_Generated")]
    public partial class RandomFloatNode {
        [In] public int Seed { get; private set; }
        [Out] public float Value { get; private set; }
        
        [GetterMethod(nameof(Value), Inline = true), UsedImplicitly] 
        private float GetValue() => Rand.FloatSeeded(Seed);

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ValuePort) yield break;
            if (count < 0) yield break;
            Rand.PushState(Seed);
            for (var i = 0; i < count; i++) {
                yield return Rand.Float;
            }
            Rand.PopState();
        }
    }
}