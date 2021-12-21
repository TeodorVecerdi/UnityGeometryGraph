using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class CompareFloatNode {
        [In] public float Tolerance { get; private set; }
        [In] public float A { get; private set; }
        [In] public float B { get; private set; }
        [Setting] public CompareFloatNode_CompareOperation Operation { get; private set; }
        [Out] public bool Result { get; private set; }

        private readonly List<bool> results = new();
        private bool resultsDirty = true;
        
        [GetterMethod(nameof(Result), Inline = true)]
        public bool GetResult() {
            return CalculateResult(A, B, Tolerance);
        }

        [CalculatesProperty(nameof(Result))]
        private void MarkResultsDirty() => resultsDirty = true;

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if(port != ResultPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }
                
                yield break;
            }

            List<float> tolerances = GetValues(TolerancePort, count, Tolerance).ToList();
            List<float> aValues = GetValues(APort, count, A).ToList();
            List<float> bValues = GetValues(BPort, count, B).ToList();
            results.Clear();
            
            for (int i = 0; i < count; i++) {
                bool result = CalculateResult(aValues[i], bValues[i], tolerances[i]);
                results.Add(result);
                yield return result;
            }
            
            resultsDirty = false;
        }

        private bool CalculateResult(float a, float b, float tolerance) {
            return Operation switch {
                CompareFloatNode_CompareOperation.LessThan => a < b,
                CompareFloatNode_CompareOperation.LessThanOrEqual => a <= b,
                CompareFloatNode_CompareOperation.GreaterThan => a > b,
                CompareFloatNode_CompareOperation.GreaterThanOrEqual => a >= b,
                CompareFloatNode_CompareOperation.Equal => MathF.Abs(a - b) < tolerance,
                CompareFloatNode_CompareOperation.NotEqual => MathF.Abs(a - b) > tolerance,
                _ => throw new ArgumentOutOfRangeException(nameof(Operation), Operation, null)
            };
        }

        public enum CompareFloatNode_CompareOperation {LessThan = 0, LessThanOrEqual = 1, GreaterThan = 2, GreaterThanOrEqual = 3, Equal = 4, NotEqual = 5}
    }
}