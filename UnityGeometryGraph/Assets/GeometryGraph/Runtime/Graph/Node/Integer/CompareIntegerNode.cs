using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class CompareIntegerNode {
        [In] public int A {get; private set; }
        [In] public int B {get; private set; }
        [Out] public bool Result {get; private set; }
        [Setting] public CompareIntegerNode_CompareOperation Operation {get; private set; }
        
        private readonly List<bool> results = new List<bool>();
        private bool resultsDirty = true;

        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        private bool GetResult() {
            return CalculateResult(A, B);
        }
        
        [CalculatesAllProperties] private void MarkResultsDirty() => resultsDirty = true;

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if(port != ResultPort || count <= 0) yield break;
            if(!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }
                
                yield break;
            }

            List<int> aValues = GetValues(APort, count, A).ToList();
            List<int> bValues = GetValues(BPort, count, B).ToList();
            results.Clear();
            
            for (int i = 0; i < count; i++) {
                bool result = CalculateResult(aValues[i], bValues[i]);
                results.Add(result);
                yield return result;
            }
            
            resultsDirty = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CalculateResult(int a, int b) {
            return Operation switch {
                CompareIntegerNode_CompareOperation.LessThan => a < b,
                CompareIntegerNode_CompareOperation.LessThanOrEqual => a <= b,
                CompareIntegerNode_CompareOperation.GreaterThan => a > b,
                CompareIntegerNode_CompareOperation.GreaterThanOrEqual => a >= b,
                CompareIntegerNode_CompareOperation.Equal => a == b,
                CompareIntegerNode_CompareOperation.NotEqual => a != b,
                _ => throw new ArgumentOutOfRangeException(nameof(Operation), Operation, null)
            };
        }

        public enum CompareIntegerNode_CompareOperation {LessThan = 0, LessThanOrEqual = 1, GreaterThan = 2, GreaterThanOrEqual = 3, Equal = 4, NotEqual = 5}
    }
}