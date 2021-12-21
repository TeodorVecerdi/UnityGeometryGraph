using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class ClampFloatNode {
        [In] public float Input { get; private set; }
        [In] public float Min { get; private set; } = 0.0f;
        [In] public float Max { get; private set; } = 1.0f;
        [Out] public float Result { get; private set; }
        
        private readonly List<float> results = new List<float>();
        private bool resultsDirty = true;

        [GetterMethod(nameof(Result), Inline = true)]
        private float GetResult() => Input.Clamped(Min, Max);
        
        [CalculatesProperty(nameof(Result))]
        private void MarkResultsDirty() => resultsDirty = true;

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ResultPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                foreach (float result in results) {
                    yield return result;
                }
                
                yield break;
            }
            
            List<float> inputs = GetValues(InputPort, count, Input).ToList();
            List<float> mins = GetValues(MinPort, count, Min).ToList();
            List<float> maxs = GetValues(MaxPort, count, Max).ToList();
            results.Clear();
            
            for (int i = 0; i < count; i++) {
                float result = inputs[i].Clamped(mins[i], maxs[i]);
                results.Add(result);
                yield return result;
            }
            
            resultsDirty = false;
        }
    }
}