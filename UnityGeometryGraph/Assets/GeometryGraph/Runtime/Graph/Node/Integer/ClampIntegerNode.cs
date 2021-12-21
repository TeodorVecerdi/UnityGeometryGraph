using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class ClampIntegerNode {
        [In] public int Input {get; private set; }
        [In] public int Min {get; private set; } = 0;
        [In] public int Max {get; private set; } = 1;
        [Out] public int Result {get; private set; }
        
        private readonly List<int> results = new();
        private bool resultsDirty = true;

        [GetterMethod(nameof(Result), Inline = true)]
        private int GetResult() => Input.Clamped(Min, Max);
        
        [CalculatesAllProperties] private void MarkResultsDirty() => resultsDirty = true;

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ResultPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }
                
                yield break;
            }

            List<int> inputs = GetValues(InputPort, count, Input).ToList();
            List<int> mins = GetValues(MinPort, count, Min).ToList();
            List<int> maxs = GetValues(MaxPort, count, Max).ToList();
            results.Clear();
            
            for (int i = 0; i < count; i++) {
                int result = inputs[i].Clamped(mins[i], maxs[i]);
                results.Add(result);
                yield return result;
            }
            
            resultsDirty = false;
        }
    }
}