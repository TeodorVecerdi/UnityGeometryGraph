using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Data;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;
using Gradient = GeometryGraph.Runtime.Data.Gradient;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [AdditionalUsingStatements("GeometryGraph.Runtime.Serialization")]
    public partial class IntegerGradientMapNode {
        [In] public int Value { get; private set; }
        [In] public int Min { get; private set; } = 0;
        [In] public int Max { get; private set; } = 100;

        [CustomSerialization("Serializer.Gradient({self})", "{self} = Deserializer.Gradient({storage}[{index}] as JObject)")]
        [Setting(GenerateEquality = false)]
        public Gradient Gradient { get; private set; } = (Gradient)Default;
        
        [Out] public float3 ResultRGB { get; private set; }
        [Out] public float ResultAlpha { get; private set; }

        private RGBAlphaPair result;

        [GetterMethod(nameof(ResultRGB), Inline = true)]
        private float3 GetResultRGB() => result.RGB;

        [GetterMethod(nameof(ResultAlpha), Inline = true)]
        private float GetResultAlpha() => result.Alpha;

        [CalculatesAllProperties]
        private void Calculate() {
            if (Min > Max) (Min, Max) = (Max, Min);
            result = Gradient.Evaluate(((float)Value).Map(Min, Max, 0.0f, 1.0f));
        }
        
        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (count <= 0) yield break;
            IEnumerable<int> values = GetValues(ValuePort, count, Value);
            foreach (int value in values) {
                RGBAlphaPair evaluated = Gradient.Evaluate(((float)value).Map(Min, Max, 0.0f, 1.0f));
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