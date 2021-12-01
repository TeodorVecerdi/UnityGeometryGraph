using System;
using System.Collections.Generic;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;
using UnityCommons;
using UnityEngine;

namespace GeometryGraph.Runtime {
    public partial class GeometryGraph {
        private void Start() {
            Evaluate();
        }

        private void Update() {
            Render();

            if (!realtimeEvaluation && !realtimeEvaluationAsync) {
                return;
            }
            
            if (realtimeEvaluationAsync) {
                if (isAsyncEvaluationComplete) {
                    StartCoroutine(EvaluateAsync(true, null));
                }
            } else {
                Evaluate();
            }
        }

        private void OnDestroy() {
            meshPool.Cleanup();
        }

        private void Render() {
            if (instancedGeometryData is not { GeometryCount: not 0 }) {
                return;
            }
            
            int geometryCount = instancedGeometryData.GeometryCount;
            for(int i = 0; i < geometryCount; i++) {
                GeometryData geometry = instancedGeometryData.Geometry(i);
                Mesh mesh = bakedInstancedGeometry.Meshes[i];
                Matrix4x4[] matrices = bakedInstancedGeometry.Matrices[i];
                for (int submeshIndex = 0; submeshIndex < geometry.SubmeshCount; submeshIndex++) {
                    Material material = instancedGeometrySettings.Materials[submeshIndex.MinClamped(instancedGeometrySettings.Materials.Count - 1)];
                    Graphics.DrawMeshInstanced(mesh, submeshIndex, material, matrices, instancedGeometryData.TransformCount(i));
                }
            }
        }
    }
}