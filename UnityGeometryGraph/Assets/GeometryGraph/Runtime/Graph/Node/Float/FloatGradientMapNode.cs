using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Data;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [AdditionalUsingStatements("GeometryGraph.Runtime.Serialization")]
    public partial class FloatGradientMapNode {
        [In] public float Value { get; private set; }

        [CustomSerialization("Serializer.Gradient({self})", "{self} = Deserializer.Gradient({storage}[{index}] as JObject)")]
        [Setting(GenerateEquality = false)]
        public Data.Gradient Gradient { get; private set; } = (Data.Gradient)Default;

        [Out] public float3 ResultRGB { get; private set; }
        [Out] public float ResultAlpha { get; private set; }

        private RGBAlphaPair result;
        
        [GetterMethod(nameof(ResultRGB), Inline = true)]
        private float3 GetResultRGB() => result.RGB;

        [GetterMethod(nameof(ResultAlpha), Inline = true)]
        private float GetResultAlpha() => result.Alpha;

        [CalculatesAllProperties]
        private void Calculate() {
            result = Gradient.Evaluate(Value);
        }

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (count <= 0) yield break;
            IEnumerable<float> values = GetValues(ValuePort, count, Value);
            foreach (float value in values) {
                RGBAlphaPair evaluated = Gradient.Evaluate(value);
                if (port == ResultRGBPort) {
                    yield return evaluated.RGB;
                } else if (port == ResultAlphaPort) {
                    yield return evaluated.Alpha;
                }
            }
        }

        public static UnityEngine.Gradient Default => new() {
            colorKeys = new[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            alphaKeys = new[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        };
    }
}