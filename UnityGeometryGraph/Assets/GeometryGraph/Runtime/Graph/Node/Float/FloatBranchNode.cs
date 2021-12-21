using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class FloatBranchNode {
        [In] public bool Condition { get; private set; }
        [In] public float IfTrue { get; private set; }
        [In] public float IfFalse { get; private set; }
        [Out] public float Result { get; private set; }
        
        private readonly List<float> results = new();
        private bool resultsDirty = true;

        [GetterMethod(nameof(Result), Inline = true)]
        private float GetResult() => Condition ? IfTrue : IfFalse;
        
        [CalculatesProperty(nameof(Result))]
        private void MarkResultsDirty() => resultsDirty = true;

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ResultPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }
             
                yield break;
            }

            List<bool> conditions = GetValues(ConditionPort, count, Condition).ToList();
            List<float> ifTrue = GetValues(IfTruePort, count, IfTrue).ToList();
            List<float> ifFalse = GetValues(IfFalsePort, count, IfFalse).ToList();
            results.Clear();
            
            for (int i = 0; i < count; i++) {
                float result = conditions[i] ? ifTrue[i] : ifFalse[i];
                results.Add(result);
                yield return result;
            }
            
            resultsDirty = false;
        }
    }
}