using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class BooleanMathNode {
        [In] public bool A { get; private set; }
        [In] public bool B { get; private set; }
        [Setting] public BooleanMathNode_Operation Operation { get; private set; }
        [Out] public bool Result { get; private set; }
        
        private readonly List<bool> results = new();
        private bool resultsDirty = true;
        
        [CalculatesAllProperties] private void MarkResultsDirty() => resultsDirty = true;

        [GetterMethod(nameof(Result), Inline = true)]
        private bool GetResult() {
            return CalculateResult(A, B);
        }

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ResultPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }
                yield break;
            }
            
            resultsDirty = false;
            results.Clear();
            List<bool> a = GetValues(APort, count, A).ToList();
            List<bool> b = GetValues(BPort, count, B).ToList();
            for (int i = 0; i < count; i++) {
                bool result = CalculateResult(a[i], b[i]);
                results.Add(result);
                yield return result;
            }
        }

        private bool CalculateResult(bool a, bool b) {
            return Operation switch {
                BooleanMathNode_Operation.AND => a && b,
                BooleanMathNode_Operation.OR => a || b,
                BooleanMathNode_Operation.XOR => a ^ b,
                BooleanMathNode_Operation.NOT => !a,
                _ => throw new ArgumentOutOfRangeException(nameof(Operation), Operation, null)
            };
        }
        
        public enum BooleanMathNode_Operation {AND = 0, OR = 1, XOR = 2, NOT = 3}
    }
}