using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class DoubleQuadMeshGenerator : MonoBehaviour {
    [SerializeField] private MeshFilter target;

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
        
        // Back
        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);
        triangles.Add(2);
        triangles.Add(1);
        triangles.Add(3);

        var mesh = new Mesh {name = "$$_Double Quad"};
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        if (target.sharedMesh.name.StartsWith("$$_", StringComparison.InvariantCulture)) {
            DestroyImmediate(target.sharedMesh);
        }
        target.sharedMesh = mesh;
    }
}