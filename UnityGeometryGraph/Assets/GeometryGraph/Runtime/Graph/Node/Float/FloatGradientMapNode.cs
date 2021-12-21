using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Data;
using Unity.Mathematics;
using UnityEngine;
using Gradient = GeometryGraph.Runtime.Data.Gradient;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [AdditionalUsingStatements("GeometryGraph.Runtime.Serialization")]
    public partial class FloatGradientMapNode {
        [In] public float Value { get; private set; }

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
        private void Calculate() => result = Gradient.Evaluate(Value);

        [CalculatesAllProperties]
        private void MarkResultsDirty() => resultsDirty = true;

        private readonly List<RGBAlphaPair> results = new();
        private bool resultsDirty = true;

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    if (port == ResultRGBPort) {
                        yield return results[i].RGB;
                    } else if (port == ResultAlphaPort) {
                        yield return results[i].Alpha;
                    }
                }
                
                yield break;
            }
            
            IEnumerable<float> values = GetValues(ValuePort, count, Value);
            results.Clear();
            
            foreach (float value in values) {
                RGBAlphaPair evaluated = Gradient.Evaluate(value);
                results.Add(evaluated);
                if (port == ResultRGBPort) {
                    yield return evaluated.RGB;
                } else if (port == ResultAlphaPort) {
                    yield return evaluated.Alpha;
                }
            }
            
            resultsDirty = false;
        }

        public static UnityEngine.Gradient Default => new() {
            colorKeys = new[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            alphaKeys = new[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        };
    }
}