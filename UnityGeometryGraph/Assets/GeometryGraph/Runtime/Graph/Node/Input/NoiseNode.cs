using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;
using NoiseType = GeometryGraph.Runtime.Graph.NoiseNode.NoiseNode_Type;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class NoiseNode {
        [In] public float3 Position { get; private set; }
        [Setting] public float Scale { get; private set; } = 1.0f;

        [Setting]
        [AdditionalValueChangedCode("{other} = {other}.Clamped(1, Constants.MAX_NOISE_OCTAVES);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        public int Octaves { get; private set; } = 4;

        [Setting]
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(0.001f);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        public float Lacunarity { get; private set; } = 2.0f;

        [Setting]
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(0.001f);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        public float Persistence { get; private set; } = 0.5f;

        [Setting] public NoiseType Type { get; private set; } = NoiseType.Scalar;
        [Out] public float Result { get; private set; }
        [Out] public float3 ResultVector { get; private set; }

        [CalculatesProperty(nameof(Result))] private void MarkFloatResultDirty() => floatResultDirty = true;
        [CalculatesProperty(nameof(ResultVector))] private void MarkVectorResultDirty() => vectorResultDirty = true;

        private readonly List<float3> vectorResults = new();
        private readonly List<float> floatResults = new();
        private bool vectorResultDirty = true;
        private bool floatResultDirty = true;

        [GetterMethod(nameof(Result), Inline = true)]
        private float GetResult() {
            return Utils.IfNotSerializing(() => {
                float3 position = Position;
                return Noise.Simplex3(
                    ref position,
                    Scale,
                    Octaves.Clamped(1, Constants.MAX_NOISE_OCTAVES),
                    Lacunarity.MinClamped(0.001f),
                    Persistence.MinClamped(0.001f)
                );
            }, "Noise.Simplex3");
        }

        [GetterMethod(nameof(ResultVector), Inline = true)]
        private float3 GetResultVector() {
            return Utils.IfNotSerializing(() => {
                float3 position = Position;
                Noise.Simplex3X3(
                    ref position,
                    out float3 noise,
                    Scale,
                    Octaves.Clamped(1, Constants.MAX_NOISE_OCTAVES),
                    Lacunarity.MinClamped(0.001f),
                    Persistence.MinClamped(0.001f)
                );
                return noise;
            }, "Noise.Simplex3");
        }

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (count <= 0) yield break;
            if (Utils.IfNotSerializing(() => false, "Noise.Simplex3(x3)", true)) {
                if (port == ResultPort) {
                    for (int i = 0; i < count; i++) {
                        yield return 0.0f;
                    }
                } else {
                    for (int i = 0; i < count; i++) {
                        yield return float3.zero;
                    }
                }

                yield break;
            }

            if (port == ResultPort) {
                if (!floatResultDirty && floatResults.Count == count) {
                    for (int i = 0; i < floatResults.Count; i++) {
                        yield return floatResults[i];
                    }
                    yield break;
                }

                floatResultDirty = false;
                floatResults.Clear();

                IEnumerable<float3> positions = GetValues(PositionPort, count, Position);

                foreach (float3 position in positions) {
                    float3 p = position;
                    float val = Noise.Simplex3(
                        ref p,
                        Scale,
                        Octaves.Clamped(1, Constants.MAX_NOISE_OCTAVES),
                        Lacunarity.MinClamped(0.001f),
                        Persistence.MinClamped(0.001f)
                    );
                    floatResults.Add(val);
                    yield return val;
                }
            } else if(port == ResultVectorPort) {
                if (!vectorResultDirty && vectorResults.Count == count) {
                    for (int i = 0; i < vectorResults.Count; i++) {
                        yield return vectorResults[i];
                    }
                    yield break;
                }

                vectorResultDirty = false;
                vectorResults.Clear();

                IEnumerable<float3> positions = GetValues(PositionPort, count, Position);

                foreach (float3 position in positions) {
                    float3 p = position;
                    Noise.Simplex3X3(
                        ref p,
                        out float3 val,
                        Scale,
                        Octaves.Clamped(1, Constants.MAX_NOISE_OCTAVES),
                        Lacunarity.MinClamped(0.001f),
                        Persistence.MinClamped(0.001f)
                    );
                    vectorResults.Add(val);
                    yield return val;
                }
            }
        }

        public enum NoiseNode_Type {
            Scalar,
            Vector
        }
    }
}