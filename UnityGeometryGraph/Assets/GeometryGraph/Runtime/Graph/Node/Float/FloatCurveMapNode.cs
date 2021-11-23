using System.Collections.Generic;
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

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ResultPort || count <= 0) yield break;
            IEnumerable<float> values = GetValues(ValuePort, count, Value);
            foreach (float value in values) {
                yield return Curve.Evaluate(value);
            }
        }

        [CalculatesProperty(nameof(Result))]
        private void Calculate() {
            Result = Curve.Evaluate(Value);
        }
    }
}