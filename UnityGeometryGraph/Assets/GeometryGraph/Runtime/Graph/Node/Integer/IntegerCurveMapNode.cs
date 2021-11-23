using System.Collections.Generic;
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

        [CalculatesProperty(nameof(Result))]
        private void Calculate() {
            Result = Mathf.FloorToInt(
                Curve.Evaluate(
                    ((float)Value).Map(Min, Max, 0.0f, 1.0f)
                ).Map(0.0f, 1.0f, Min, Max)
            );
        }

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ResultPort && count <= 0) yield break;
            IEnumerable<int> values = GetValues(ValuePort, count, Value);
            foreach (int value in values) {
                yield return Curve.Evaluate(((float)value).Map(Min, Max, 0.0f, 1.0f));
            }
        }
    }
}