using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class VectorBranchNode {
        [In] public bool Condition { get; private set; }
        [In(GenerateEquality = false)] public float3 IfTrue { get; private set; }
        [In(GenerateEquality = false)] public float3 IfFalse { get; private set; }
        [Out] public float3 Result { get; private set; }

        private readonly List<float3> results = new();
        private bool resultsDirty = true;

        [CalculatesAllProperties] private void MarkResultsDirty() => resultsDirty = true;

        [GetterMethod(nameof(Result), Inline = true)]
        private float3 GetResult() => Condition ? IfTrue : IfFalse;

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if(port != ResultPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }

                yield break;
            }

            List<bool> conditions = GetValues(ConditionPort, count, Condition).ToList();
            List<float3> ifTrue = GetValues(IfTruePort, count, IfTrue).ToList();
            List<float3> ifFalse = GetValues(IfFalsePort, count, IfFalse).ToList();
            results.Clear();
            resultsDirty = false;

            for (int i = 0; i < count; i++) {
                float3 result = conditions[i] ? ifTrue[i] : ifFalse[i];
                results.Add(result);
                yield return result;
            }
        }
    }
}