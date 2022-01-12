using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
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

        private readonly List<float> results = new();
        private bool resultsDirty = true;

        [GetterMethod(nameof(Result), Inline = true)]
        private float GetResult() {
            return CalculateResult(Value, FromMin, FromMax, ToMin, ToMax, Clamp);
        }

        [CalculatesAllProperties] private void MarkResultsDirty() => resultsDirty = true;

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ResultPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }

                yield break;
            }

            List<float> values = GetValues(ValuePort, count, Value).ToList();
            List<float> fromMin = GetValues(FromMinPort, count, FromMin).ToList();
            List<float> fromMax = GetValues(FromMaxPort, count, FromMax).ToList();
            List<float> toMin = GetValues(ToMinPort, count, ToMin).ToList();
            List<float> toMax = GetValues(ToMaxPort, count, ToMax).ToList();
            results.Clear();

            for (int i = 0; i < count; i++) {
                float result = CalculateResult(values[i], fromMin[i], fromMax[i], toMin[i], toMax[i], Clamp);
                results.Add(result);
                yield return result;
            }
            
            resultsDirty = false;
        }

        private float CalculateResult(float value, float fromMin, float fromMax, float toMin, float toMax, bool clamp) {
            float calculated = value.Map(fromMin, fromMax, toMin, toMax);
            return clamp ? calculated.Clamped(toMin, toMax) : calculated;
        }
    }
}