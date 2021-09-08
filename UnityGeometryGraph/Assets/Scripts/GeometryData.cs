﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityCommons;
using UnityEngine;

[Serializable]
public class GeometryData {
    [SerializeField] public List<Vector3> Vertices;
    [SerializeField] public List<Edge> Edges;
    [SerializeField] public List<Face> Faces;

    public GeometryData(Mesh mesh, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
        Vertices = new List<Vector3>();

        mesh.GetVertices(Vertices);

        var triangles = new List<int>();
        mesh.GetTriangles(triangles, 0);
        BuildMetadata(triangles, duplicateDistanceThreshold, duplicateNormalAngleThreshold);
    }

    private void BuildMetadata(List<int> triangles, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
        Edges = new List<Edge>();
        Faces = new List<Face>();
        BuildElements(triangles);
        RemoveDuplicates(duplicateDistanceThreshold, duplicateNormalAngleThreshold);
    }

    private void BuildElements(List<int> triangles) {
        for (var i = 0; i < triangles.Count; i += 3) {
            var idxA = triangles[i];
            var idxB = triangles[i + 1];
            var idxC = triangles[i + 2];

            var vertA = Vertices[idxA];
            var vertB = Vertices[idxB];
            var vertC = Vertices[idxC];

            var AB = vertB - vertA;
            var AC = vertC - vertA;
            var faceNormal = Vector3.Cross(AB, AC).normalized;

            var face = new Face(idxA, idxB, idxC, faceNormal);
            var edgeA = new Edge(idxA, idxB) { FaceA = Faces.Count };
            var edgeB = new Edge(idxB, idxC) { FaceA = Faces.Count };
            var edgeC = new Edge(idxC, idxA) { FaceA = Faces.Count };
            face.EdgeA = Edges.Count;
            face.EdgeB = Edges.Count + 1;
            face.EdgeC = Edges.Count + 2;

            Faces.Add(face);
            Edges.Add(edgeA);
            Edges.Add(edgeB);
            Edges.Add(edgeC);
        }
    }

    private void RemoveDuplicates(float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
        var duplicates = GetDuplicateEdges(duplicateDistanceThreshold * duplicateDistanceThreshold, duplicateNormalAngleThreshold);
        var vertexDuplicatesMap = GetVertexDuplicatesMap(duplicates);
        var reverseDuplicatesMap = RemoveInvalidDuplicates(vertexDuplicatesMap);

        RemapDuplicates(duplicates, reverseDuplicatesMap);
        RemoveDuplicates(duplicates, reverseDuplicatesMap);

        CheckForErrors();
    }
    
    private List<(int, int, BitArray)> GetDuplicateEdges(float sqrDistanceThreshold, float duplicateNormalAngleThreshold) {
        var potentialDuplicates = new List<(int, int, BitArray)>();
        
        // Find potential duplicate edges
        for (var i = 0; i < Edges.Count - 1; i++) {
            for (var j = i + 1; j < Edges.Count; j++) {
                var edgeA = Edges[i];
                var edgeB = Edges[j];
                
                var checkA = (Vertices[edgeA.VertA] - Vertices[edgeB.VertA]).sqrMagnitude < sqrDistanceThreshold ? 1 : 0;
                var checkB = (Vertices[edgeA.VertA] - Vertices[edgeB.VertB]).sqrMagnitude < sqrDistanceThreshold ? 1 : 0;
                var checkC = (Vertices[edgeA.VertB] - Vertices[edgeB.VertA]).sqrMagnitude < sqrDistanceThreshold ? 1 : 0;
                var checkD = (Vertices[edgeA.VertB] - Vertices[edgeB.VertB]).sqrMagnitude < sqrDistanceThreshold ? 1 : 0;
                var total = checkA + checkB + checkC + checkD;

                // Two pairs of vertices are identical
                if (total < 2) continue;
                
                // Used to remap vertices later without checking again for which vertices are identical
                var matchBitArray = new BitArray(4) { [0] = checkA != 0, [1] = checkB != 0, [2] = checkC != 0, [3] = checkD != 0 };
                potentialDuplicates.Add((i, j, matchBitArray));
            }
        }

        // Debug.Log($"Found {potentialDuplicates.Count} potential duplicates");

        // Find real duplicates based on the face normal angle
        var actualDuplicates = new List<(int, int, BitArray)>();
        foreach (var potentialDuplicate in potentialDuplicates) {
            var edgeA = Edges[potentialDuplicate.Item1];
            var edgeB = Edges[potentialDuplicate.Item2];
            var faceA = Faces[edgeA.FaceA];
            var faceB = Faces[edgeB.FaceA];
            var normalAngle = Vector3.Angle(faceA.FaceNormal, faceB.FaceNormal);

            if (normalAngle > duplicateNormalAngleThreshold) continue;

            actualDuplicates.Add(potentialDuplicate);
            edgeA.FaceB = edgeB.FaceA;

            // Remap face edges
            if (faceB.EdgeA == potentialDuplicate.Item2) faceB.EdgeA = potentialDuplicate.Item1;
            else if (faceB.EdgeB == potentialDuplicate.Item2) faceB.EdgeB = potentialDuplicate.Item1;
            else if (faceB.EdgeC == potentialDuplicate.Item2) faceB.EdgeC = potentialDuplicate.Item1;
        }

        potentialDuplicates.Clear();


        return actualDuplicates;
    }

    private Dictionary<int, List<int>> GetVertexDuplicatesMap(List<(int, int, BitArray)> actualDuplicates) {
        // Make a dictionary of (UniqueVertex => [Duplicates])
        var vertexRemap = new Dictionary<int, List<int>>();
        foreach (var actualDuplicate in actualDuplicates) {
            var edgeA = Edges[actualDuplicate.Item1];
            var edgeB = Edges[actualDuplicate.Item2];
            var bitset = actualDuplicate.Item3;

            // Check which vertices in each edge are duplicates
            int edgeAVertA = -1, edgeAVertB = -1, edgeBVertA = -1, edgeBVertB = -1;
            if (bitset[0]) {
                edgeAVertA = edgeA.VertA;
                edgeBVertA = edgeB.VertA;
            }

            if (bitset[1]) {
                edgeAVertA = edgeA.VertA;
                edgeBVertA = edgeB.VertB;
            }

            if (bitset[2]) {
                edgeAVertB = edgeA.VertB;
                edgeBVertB = edgeB.VertA;
            }

            if (bitset[3]) {
                edgeAVertB = edgeA.VertB;
                edgeBVertB = edgeB.VertB;
            }

            // Sanity check that nothing went wrong
            if (edgeAVertA < 0 || edgeAVertB < 0 || edgeBVertA < 0 || edgeBVertB < 0) {
                throw new Exception("Duplicate edge vertices were somehow uninitialized.");
            }

            // sort as (lower num, bigger num)
            if (edgeAVertA > edgeBVertA) (edgeAVertA, edgeBVertA) = (edgeBVertA, edgeAVertA);
            if (edgeAVertB > edgeBVertB) (edgeAVertB, edgeBVertB) = (edgeBVertB, edgeAVertB);

            if (!vertexRemap.ContainsKey(edgeAVertA)) {
                vertexRemap[edgeAVertA] = new List<int>();
            }

            vertexRemap[edgeAVertA].Add(edgeBVertA);

            if (!vertexRemap.ContainsKey(edgeAVertB)) {
                vertexRemap[edgeAVertB] = new List<int>();
            }

            vertexRemap[edgeAVertB].Add(edgeBVertB);
        }

        return vertexRemap;
    }

    private Dictionary<int, int> RemoveInvalidDuplicates(Dictionary<int, List<int>> vertexDuplicatesMap) {
        // Remove trivial invalid entries
        var removalList = new List<int>();
        foreach (var pair in vertexDuplicatesMap) {
            pair.Value.RemoveAll(value => value == pair.Key);
            if (pair.Value.Count == 0) removalList.Add(pair.Key);
        }

        removalList.ForEach(key => vertexDuplicatesMap.Remove(key));
        
        // Remove remaining invalid entries
        var sortedKeys = vertexDuplicatesMap.Keys.QuickSorted().ToList();
        var actualMap = new Dictionary<int, HashSet<int>>();
        var reverseDuplicateMap = new Dictionary<int, int>();
        
        foreach (var sortedKey in sortedKeys) {
            var alreadyExistsKey = -1;
            
            foreach (var pair in actualMap) {
                if (!pair.Value.Contains(sortedKey)) continue;
                
                alreadyExistsKey = pair.Key;
                break;
            }

            if (alreadyExistsKey != -1) {
                actualMap[alreadyExistsKey].AddRange(vertexDuplicatesMap[sortedKey]);
                foreach (var duplicate in vertexDuplicatesMap[sortedKey]) {
                    reverseDuplicateMap[duplicate] = alreadyExistsKey;
                }
            } else {
                actualMap.Add(sortedKey, new HashSet<int>());
                actualMap[sortedKey].AddRange(vertexDuplicatesMap[sortedKey]);
                foreach (var duplicate in vertexDuplicatesMap[sortedKey]) {
                    reverseDuplicateMap[duplicate] = sortedKey;
                }
            }
        }

        return reverseDuplicateMap;
    }

    private void RemapDuplicates(List<(int, int, BitArray)> actualDuplicates, Dictionary<int, int> reverseDuplicatesMap) {
        // Remap the vertex indices for faces and edges
        var edgeReverseMap = new Dictionary<int, int>();
        foreach (var duplicate in actualDuplicates) {
            edgeReverseMap[duplicate.Item2] = duplicate.Item1;
        }

        var sortedEdgeRemapKeys = actualDuplicates.Select(tuple => tuple.Item1).QuickSorted().ToList();
        var edgeRemap = new Dictionary<int, int>();
        var remapIndex = 0;
        foreach (var sortedKey in sortedEdgeRemapKeys) {
            edgeRemap[sortedKey] = remapIndex++;
        }

        foreach (var face in Faces) {
            RemapEdge(face.EdgeA, reverseDuplicatesMap);
            RemapEdge(face.EdgeB, reverseDuplicatesMap);
            RemapEdge(face.EdgeC, reverseDuplicatesMap);

            RemapFace(face, reverseDuplicatesMap, edgeReverseMap, edgeRemap);
        }
    }

    private void RemapFace(Face face, Dictionary<int, int> reverseDuplicateMap, Dictionary<int, int> edgeReverseMap, Dictionary<int, int> edgeIndexRemap) {
        if (reverseDuplicateMap.ContainsKey(face.VertA)) {
            face.VertA = reverseDuplicateMap[face.VertA];
        }

        if (reverseDuplicateMap.ContainsKey(face.VertB)) {
            face.VertB = reverseDuplicateMap[face.VertB];
        }

        if (reverseDuplicateMap.ContainsKey(face.VertC)) {
            face.VertC = reverseDuplicateMap[face.VertC];
        }

        if (edgeReverseMap.ContainsKey(face.EdgeA)) {
            face.EdgeA = edgeIndexRemap[edgeReverseMap[face.EdgeA]];
        } else if (edgeIndexRemap.ContainsKey(face.EdgeA)) {
            face.EdgeA = edgeIndexRemap[face.EdgeA];
        }

        if (edgeReverseMap.ContainsKey(face.EdgeB)) {
            face.EdgeB = edgeIndexRemap[edgeReverseMap[face.EdgeB]];
        } else if (edgeIndexRemap.ContainsKey(face.EdgeB)) {
            face.EdgeB = edgeIndexRemap[face.EdgeB];
        }

        if (edgeReverseMap.ContainsKey(face.EdgeC)) {
            face.EdgeC = edgeIndexRemap[edgeReverseMap[face.EdgeC]];
        } else if (edgeIndexRemap.ContainsKey(face.EdgeC)) {
            face.EdgeC = edgeIndexRemap[face.EdgeC];
        }
    }

    private void RemapEdge(int edgeIndex, Dictionary<int, int> reverseDuplicateMap) {
        var edge = Edges[edgeIndex];
        if (reverseDuplicateMap.ContainsKey(edge.VertA)) {
            edge.VertA = reverseDuplicateMap[edge.VertA];
        }

        if (reverseDuplicateMap.ContainsKey(edge.VertB)) {
            edge.VertB = reverseDuplicateMap[edge.VertB];
        }
    }
   
    private void RemoveDuplicates(List<(int, int, BitArray)> actualDuplicates, Dictionary<int, int> reverseDuplicatesMap) {
        // Remove duplicate edges
        foreach (var edge in actualDuplicates.Select(tuple => Edges[tuple.Item2]).ToList()) {
            Edges.Remove(edge);
        }

        // Remove duplicate vertices
        var sortedDuplicateVertices = reverseDuplicatesMap.Keys.QuickSorted().Reverse().ToList();
        foreach (var vertexIndex in sortedDuplicateVertices) {
            Vertices.RemoveAt(vertexIndex);
        }
    }
    
    private void CheckForErrors() {
        // Check if there are any invalid edges
        for (var i = 0; i < Edges.Count; i++) {
            var edge = Edges[i];
            if (edge.VertA >= Vertices.Count || edge.VertB >= Vertices.Count) {
                Debug.LogError($"Edge at index {i} contains invalid vertices");
            }
        }

        // Check if there are any invalid faces
        for (var i = 0; i < Faces.Count; i++) {
            var face = Faces[i];
            if (face.EdgeA >= Edges.Count || face.EdgeB >= Edges.Count || face.EdgeC >= Edges.Count) {
                Debug.LogError($"Face at index {i} contains invalid edges");
            }

            if (face.VertA >= Vertices.Count || face.VertB >= Vertices.Count || face.VertC >= Vertices.Count) {
                Debug.LogError($"Face at index {i} contains invalid vertices");
            }
        }
    }
    
    [Serializable]
    public class Edge {
        public int VertA;
        public int VertB;
        public int FaceA;
        public int FaceB;

        public Edge(int vertA, int vertB) {
            VertA = vertA;
            VertB = vertB;
        }
    }

    [Serializable]
    public class Face {
        public int VertA;
        public int VertB;
        public int VertC;
        public Vector3 FaceNormal;

        public int EdgeA;
        public int EdgeB;
        public int EdgeC;

        public Face(int vertA, int vertB, int vertC, Vector3 faceNormal) {
            VertA = vertA;
            VertB = vertB;
            VertC = vertC;
            FaceNormal = faceNormal;
        }
    }
}