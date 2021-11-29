using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class IntegerBranchNode {
        [In] public bool Condition { get; private set; }
        [In] public int IfTrue { get; private set; }
        [In] public int IfFalse { get; private set; }
        [Out] public int Result { get; private set; }
        
        private readonly List<int> results = new List<int>();
        private bool resultsDirty = true;

        [GetterMethod(nameof(Result), Inline = true), ]
        private int GetResult() => Condition ? IfTrue : IfFalse;
        
        [CalculatesAllProperties] private void MarkResultsDirty() => resultsDirty = true;
        
        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ResultPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }
             
                yield break;
            }

            List<bool> conditions = GetValues(ConditionPort, count, Condition).ToList();
            List<int> ifTrue = GetValues(IfTruePort, count, IfTrue).ToList();
            List<int> ifFalse = GetValues(IfFalsePort, count, IfFalse).ToList();
            results.Clear();
            
            for (int i = 0; i < count; i++) {
                int result = conditions[i] ? ifTrue[i] : ifFalse[i];
                results.Add(result);
                yield return result;
            }
            
            resultsDirty = false;
        }

    }
}