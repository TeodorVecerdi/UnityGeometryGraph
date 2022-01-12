using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Data;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [AdditionalUsingStatements("GeometryGraph.Runtime.Serialization")]
    public partial class FloatCurveMapNode {
        [In] public float Value { get; private set; }

        [CustomSerialization("Serializer.AnimationCurve({self})", "{self} = Deserializer.AnimationCurve({storage}[{index}] as JObject)")]
        [Setting(GenerateEquality = false)]
        public AnimationCurve Curve { get; private set; } = (AnimationCurve) UnityEngine.AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

        [Out] public float Result { get; private set; }

        private readonly List<float> results = new();
        private bool resultsDirty = true;

        [CalculatesProperty(nameof(Result))]
        private void Calculate() => Result = Curve.Evaluate(Value);

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

            List<float> values = GetValues(ValuePort, count, Value).ToList();
            results.Clear();

            for (int i = 0; i < count; i++) {
                float result = Curve.Evaluate(values[i]);
                results.Add(result);
                yield return result;
            }

            resultsDirty = false;
        }
    }
}