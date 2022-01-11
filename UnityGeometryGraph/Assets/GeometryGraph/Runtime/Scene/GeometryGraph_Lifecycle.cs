using System.Linq;
using GeometryGraph.Runtime.Geometry;
using UnityCommons;
using UnityEngine;

namespace GeometryGraph.Runtime {
    public partial class GeometryGraph {
        private void Start() {
            Evaluate();
        }

        private void Update() {
            if (realtimeEvaluation) {
                if (realtimeEvaluationAsync) {
                    if (isAsyncEvaluationComplete) {
                        StartCoroutine(EvaluateAsync(true, null));
                    }
                } else {
                    Evaluate();
                }
            }
            
            RenderInstances();
        }

        private void OnDestroy() {
            meshPool?.Cleanup();
        }

        internal void RenderInstances() {
            if (instancedGeometryData is not { GeometryCount: not 0 }) {
                return;
            }

            if (instancedGeometrySettings.Materials.Any(material => !material.enableInstancing)) {
                foreach (Material material in instancedGeometrySettings.Materials) {
                    material.enableInstancing = true;
                }
            }
            
            int geometryCount = instancedGeometryData.GeometryCount;
            for(int i = 0; i < geometryCount; i++) {
                GeometryData geometry = instancedGeometryData.Geometry(i);
                Mesh mesh = bakedInstancedGeometry.Meshes[i];
                Matrix4x4[][] matrices = bakedInstancedGeometry.Matrices[i];
                for (int submeshIndex = 0; submeshIndex < geometry.SubmeshCount; submeshIndex++) {
                    Material material = instancedGeometrySettings.Materials[submeshIndex.MinClamped(instancedGeometrySettings.Materials.Count - 1)];
                    foreach (Matrix4x4[] drawCallMatrices in matrices) {
                        Graphics.DrawMeshInstanced(mesh, submeshIndex, material, drawCallMatrices, drawCallMatrices.Length);
                    }
                }
            }
        }
    }
}