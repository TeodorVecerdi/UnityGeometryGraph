using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using UnityCommons;
using UnityEngine;
using AnimationCurve = GeometryGraph.Runtime.Data.AnimationCurve;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [AdditionalUsingStatements("GeometryGraph.Runtime.Serialization")]
    public partial class IntegerCurveMapNode {
        [In] public int Value { get; private set; }
        [In] public int Min { get; private set; } = 0;
        [In] public int Max { get; private set; } = 100;

        [CustomSerialization("Serializer.AnimationCurve({self})", "{self} = Deserializer.AnimationCurve({storage}[{index}] as JObject)")]
        [Setting(GenerateEquality = false)]
        public AnimationCurve Curve { get; private set; } = (AnimationCurve)UnityEngine.AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

        [Out] public int Result { get; private set; }


        private readonly List<int> results = new();
        private bool resultsDirty = true;

        [CalculatesAllProperties] private void MarkResultsDirty() => resultsDirty = true;

        [CalculatesProperty(nameof(Result))]
        private void Calculate() {
            Result = Calculate(Value, Min, Max);
        }

        private int Calculate(int value, int min, int max) {
            return Mathf.FloorToInt(
                Curve.Evaluate(
                    ((float)value).Map(min, max, 0.0f, 1.0f)
                ).Map(0.0f, 1.0f, min, max)
            );
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
            List<int> mins = GetValues(MinPort, count, Min).ToList();
            List<int> maxs = GetValues(MaxPort, count, Max).ToList();
            results.Clear();

            for (int i = 0; i < count; i++) {
                int result = Calculate(values[i], mins[i], maxs[i]);
                results.Add(result);
                yield return result;
            }

            resultsDirty = false;
        }
    }
}