using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class DoubleQuadMeshGenerator : MonoBehaviour {
        [SerializeField] private MeshFilter target;
        [SerializeField] private bool doubleSided;

        [Button]
        private void Generate() {
            var v0 = Vector3.zero;
            var v1 = Vector3.up;
            var v2 = Vector3.right;
            var v3 = Vector3.up + Vector3.right;

            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            // Front
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            triangles.Add(0);
            triangles.Add(2);
            triangles.Add(1);
            triangles.Add(2);
            triangles.Add(3);
            triangles.Add(1);

            if (doubleSided) {
                // Back
                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);
                vertices.Add(v3);
                triangles.Add(0 + 4);
                triangles.Add(1 + 4);
                triangles.Add(2 + 4);
                triangles.Add(2 + 4);
                triangles.Add(1 + 4);
                triangles.Add(3 + 4);
            }

            var mesh = new Mesh { name = "$$_Double Quad" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            if (target.sharedMesh != null && target.sharedMesh.name.StartsWith("$$_", StringComparison.InvariantCulture)) {
                DestroyImmediate(target.sharedMesh);
            }

            target.sharedMesh = mesh;
        }

        [Button]
        private void GenerateSubmeshes() {
            var v0 = Vector3.zero;
            var v1 = Vector3.up;
            var v2 = Vector3.right;
            var v3 = Vector3.up + Vector3.right;

            var vertices = new List<Vector3> { v0, v1, v2, v3 };
            var t0 = new List<int> { 0, 2, 1 };
            var t1 = new List<int> { 2, 3, 1 };
            List<int> t2 = null;
            List<int> t3 = null;

            if (doubleSided) {
                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);
                vertices.Add(v3);
                t2 = new List<int> { 0 + 4, 1 + 4, 2 + 4 };
                t3 = new List<int> { 2 + 4, 1 + 4, 3 + 4 };
            }

            var mesh = new Mesh { name = "$$_Double Quad" };
            mesh.subMeshCount = doubleSided ? 4 : 2;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(t0, 0);
            mesh.SetTriangles(t1, 1);
            if (doubleSided) {
                mesh.SetTriangles(t2, 2);
                mesh.SetTriangles(t3, 3);
            }

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            if (target.sharedMesh.name.StartsWith("$$_", StringComparison.InvariantCulture)) {
                DestroyImmediate(target.sharedMesh);
            }

            target.sharedMesh = mesh;
        }
    }
}