using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GeometryGraph.Runtime {
    [Serializable]
    public class BakedInstancedGeometry : ISerializationCallbackReceiver {
        [SerializeField] private List<Mesh> meshes = new();
        [NonSerialized] private List<Matrix4x4[][]> matrices = new();

        [SerializeField] private List<Matrix4x4> serializedMatrices = new();
        [SerializeField] private List<int> drawCallCounts = new();
        [SerializeField] private int drawCalls;

        public List<Mesh> Meshes => meshes;
        public List<Matrix4x4[][]> Matrices => matrices;

        public BakedInstancedGeometry() {
            meshes = new List<Mesh>();
            matrices = new List<Matrix4x4[][]>();
        }

        /// <summary>
        ///   <para>Implement this method to receive a callback before Unity serializes your object.</para>
        /// </summary>
        public void OnBeforeSerialize() {
            serializedMatrices.Clear();
            drawCallCounts.Clear();

            if (matrices.Count == 0) return;

            drawCalls = matrices.Count;

            foreach (Matrix4x4[][] instanceCalls in matrices) {
                int drawCallCount = 0;
                for (int j = 0; j < instanceCalls.Length; j++) {
                    drawCallCount += instanceCalls[j].Length;
                }
                drawCallCounts.Add(drawCallCount);

                foreach (Matrix4x4[] call in instanceCalls) {
                    serializedMatrices.AddRange(call);
                }
            }
        }

        /// <summary>
        ///   <para>Implement this method to receive a callback after Unity deserializes your object.</para>
        /// </summary>
        public void OnAfterDeserialize() {
            matrices.Clear();

            if (drawCalls == 0 || serializedMatrices.Count == 0) return;

            int currentIndex = 0;
            for (int i = 0; i < drawCalls; i++) {
                int drawCallCount = drawCallCounts[i];
                Matrix4x4[][] parsedMatrices = ParseDrawCalls(serializedMatrices, drawCallCount, currentIndex);
                matrices.Add(parsedMatrices);
                currentIndex += drawCallCount;
            }
        }

        private Matrix4x4[][] ParseDrawCalls(List<Matrix4x4> flattened, int count, int startIndex) {
            IEnumerable<Matrix4x4> drawCalls = flattened.GetRange(startIndex, count);

            if (count <= 1023) {
                return new[] {drawCalls.ToArray()};
            }

            int drawCallCount = count / 1023;
            if (count % 1023 != 0) {
                drawCallCount++;
            }

            Matrix4x4[][] result = new Matrix4x4[drawCallCount][];

            for (int i = 0; i < drawCallCount; i++) {
                int start = i * 1023;
                int end = Math.Min(start + 1023, count);
                result[i] = drawCalls.Skip(start).Take(end - start).ToArray();
            }

            return result;
        }
    }
}