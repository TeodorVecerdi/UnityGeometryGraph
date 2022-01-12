using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class MapRangeIntegerNode {
        [Setting] public bool Clamp { get; private set; }
        [In] public int Value { get; private set; }
        [In] public int FromMin { get; private set; } = 0;
        [In] public int FromMax { get; private set; } = 1;
        [In] public int ToMin { get; private set; } = 0;
        [In] public int ToMax { get; private set; } = 1;
        [Out] public int Result { get; private set; }

        private readonly List<int> results = new();
        private bool resultsDirty = true;

        [CalculatesAllProperties] private void MarkResultsDirty() => resultsDirty = true;

        [GetterMethod(nameof(Result), Inline = true)]
        private int GetResult() {
            return CalculateResult(Value, FromMin, FromMax, ToMin, ToMax, Clamp);
        }

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ResultPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }

                yield break;
            }

            List<int> values = GetValues(ValuePort, count, Value).ToList();
            List<int> fromMin = GetValues(FromMinPort, count, FromMin).ToList();
            List<int> fromMax = GetValues(FromMaxPort, count, FromMax).ToList();
            List<int> toMin = GetValues(ToMinPort, count, ToMin).ToList();
            List<int> toMax = GetValues(ToMaxPort, count, ToMax).ToList();
            results.Clear();

            for (int i = 0; i < count; i++) {
                int result = CalculateResult(values[i], fromMin[i], fromMax[i], toMin[i], toMax[i], Clamp);
                results.Add(result);
                yield return result;
            }
            
            resultsDirty = false;
        }


        private int CalculateResult(int value, int fromMin, int fromMax, int toMin, int toMax, bool clamp) {
            int result = value.Map(fromMin, fromMax, toMin, toMax);
            return clamp ? result.Clamped(toMin, toMax) : result;
        }
    }
}