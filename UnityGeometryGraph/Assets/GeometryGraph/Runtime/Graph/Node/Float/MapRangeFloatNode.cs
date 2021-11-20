using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class MapRangeFloatNode {
        [Setting] public bool Clamp { get; private set; }
        [In] public float Value { get; private set; }
        [In] public float FromMin { get; private set; } = 0.0f;
        [In] public float FromMax { get; private set; } = 1.0f;
        [In] public float ToMin { get; private set; } = 0.0f;
        [In] public float ToMax { get; private set; } = 1.0f;
        [Out] public float Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        private float GetResult() {
            float value = Value.Map(FromMin, FromMax, ToMin, ToMax);
            return Clamp ? value.Clamped(ToMin, ToMax) : value;
        }

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ResultPort) yield break;
            if (count <= 0) yield break;

            IEnumerable<float> values;
            if (ValuePort.IsConnected) values = GetValues(ValuePort, count, Value);
            else values = Enumerable.Repeat(Value, count);
            
            foreach (float value in values) {
                float mapped = value.Map(FromMin, FromMax, ToMin, ToMax);
                yield return Clamp ? mapped.Clamped(ToMin, ToMax) : mapped;
            }
        }
    }
}