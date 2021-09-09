using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityCommons;
using UnityEngine;
using Debug = UnityEngine.Debug;

[Serializable]
public class GeometryData {
    [SerializeField] public List<Vector3> Vertices;
    [SerializeField] public List<Edge> Edges;
    [SerializeField] public List<Face> Faces;

    public GeometryData(Mesh mesh, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        Vertices = new List<Vector3>();

        mesh.GetVertices(Vertices);

        var triangles = new List<int>();
        mesh.GetTriangles(triangles, 0);
        BuildMetadata(triangles, duplicateDistanceThreshold, duplicateNormalAngleThreshold);
        var elapsed = stopwatch.Elapsed;
        Debug.Log(elapsed.TotalMilliseconds);
        stopwatch.Stop();
    }

    private void BuildMetadata(List<int> triangles, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
        Edges = new List<Edge>(triangles.Count);
        Faces = new List<Face>(triangles.Count / 3);
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
        var duplicateVerticesMap = GetDuplicateVerticesMap(duplicates);
        var reverseDuplicatesMap = RemoveInvalidDuplicates(duplicateVerticesMap);

        RemapDuplicates(duplicates, reverseDuplicatesMap);
        RemoveDuplicates(duplicates, reverseDuplicatesMap);

        CheckForErrors();
    }
    
    private List<(int, int, bool)> GetDuplicateEdges(float sqrDistanceThreshold, float duplicateNormalAngleThreshold) {
        var potentialDuplicates = new List<(int, int, bool)>();
        // Find potential duplicate edges
        for (var i = 0; i < Edges.Count - 1; i++) {
            for (var j = i + 1; j < Edges.Count; j++) {
                var edgeA = Edges[i];
                var edgeB = Edges[j];
                var edgeAVertA = Vertices[edgeA.VertA];
                var edgeAVertB = Vertices[edgeA.VertB];
                var edgeBVertA = Vertices[edgeB.VertA];
                var edgeBVertB = Vertices[edgeB.VertB];

                // var checkA = VectorUtilities.DistanceSqr(edgeAVertA, edgeBVertA) < sqrDistanceThreshold ? 1 : 0;
                // var checkB = VectorUtilities.DistanceSqr(edgeAVertA, edgeBVertB) < sqrDistanceThreshold ? 1 : 0;
                // var checkC = VectorUtilities.DistanceSqr(edgeAVertB, edgeBVertA) < sqrDistanceThreshold ? 1 : 0;
                // var checkD = VectorUtilities.DistanceSqr(edgeAVertB, edgeBVertB) < sqrDistanceThreshold ? 1 : 0;
                // var total = checkA + checkB + checkC + checkD;
                //
                // // Two pairs of vertices are identical
                // if (total < 2) continue;
                //
                // potentialDuplicates.Add((i, j, checkA == 1));
                
                var checkA = edgeAVertA == edgeBVertA && edgeAVertB == edgeBVertB;
                var checkB = edgeAVertA == edgeBVertB && edgeAVertB == edgeBVertA;
                if (!checkA && !checkB) continue;
                
                potentialDuplicates.Add((i, j, checkA));
                break;
            }
        }

        // Find real duplicates based on the face normal angle
        var actualDuplicates = new List<(int, int, bool)>();
        foreach (var potentialDuplicate in potentialDuplicates) {
            var edgeAIndex = potentialDuplicate.Item1;
            var edgeBIndex = potentialDuplicate.Item2;
            var edgeA = Edges[edgeAIndex];
            var edgeB = Edges[edgeBIndex];
            var faceA = Faces[edgeA.FaceA];
            var faceB = Faces[edgeB.FaceA];
            var normalAngle = Vector3.Angle(faceA.FaceNormal, faceB.FaceNormal);

            if (normalAngle > duplicateNormalAngleThreshold) continue;

            actualDuplicates.Add(potentialDuplicate);
            edgeA.FaceB = edgeB.FaceA;
            
            // TODO: Was there a reason I did this in the first place?
            // // Remap face edges
            // if (faceB.EdgeA == edgeBIndex) faceB.EdgeA = edgeAIndex;
            // else if (faceB.EdgeB == edgeBIndex) faceB.EdgeB = edgeAIndex;
            // else if (faceB.EdgeC == edgeBIndex) faceB.EdgeC = edgeAIndex;
        }

        potentialDuplicates.Clear();

        return actualDuplicates;
    }

    private Dictionary<int, List<int>> GetDuplicateVerticesMap(List<(int, int, bool)> duplicates) {
        // Make a dictionary of (UniqueVertex => [Duplicates])
        var duplicateVerticesMap = new Dictionary<int, List<int>>();
        foreach (var duplicate in duplicates) {
            var edgeA = Edges[duplicate.Item1];
            var edgeB = Edges[duplicate.Item2];

            // Check which vertices in each edge are duplicates
            int edgeAVertA = -1, edgeAVertB = -1, edgeBVertA = -1, edgeBVertB = -1;
            if (duplicate.Item3) {
                edgeAVertA = edgeA.VertA;
                edgeBVertA = edgeB.VertA;
                edgeAVertB = edgeA.VertB;
                edgeBVertB = edgeB.VertB;
            } else {
                edgeAVertA = edgeA.VertA;
                edgeBVertA = edgeB.VertB;
                edgeAVertB = edgeA.VertB;
                edgeBVertB = edgeB.VertA;
            }

            // sort as (lower num, bigger num)
            if (edgeAVertA > edgeBVertA) (edgeAVertA, edgeBVertA) = (edgeBVertA, edgeAVertA);
            if (edgeAVertB > edgeBVertB) (edgeAVertB, edgeBVertB) = (edgeBVertB, edgeAVertB);

            if (!duplicateVerticesMap.ContainsKey(edgeAVertA)) {
                duplicateVerticesMap[edgeAVertA] = new List<int>();
            }

            duplicateVerticesMap[edgeAVertA].Add(edgeBVertA);

            if (!duplicateVerticesMap.ContainsKey(edgeAVertB)) {
                duplicateVerticesMap[edgeAVertB] = new List<int>();
            }

            duplicateVerticesMap[edgeAVertB].Add(edgeBVertB);
        }

        return duplicateVerticesMap;
    }

    private Dictionary<int, int> RemoveInvalidDuplicates(Dictionary<int, List<int>> duplicateVerticesMap) {
        // Remove trivial invalid entries
        var removalList = new List<int>();
        foreach (var pair in duplicateVerticesMap) {
            pair.Value.RemoveAll(value => value == pair.Key);
            if (pair.Value.Count == 0) removalList.Add(pair.Key);
        }

        removalList.ForEach(key => duplicateVerticesMap.Remove(key));
        
        // Remove remaining invalid entries
        var sortedKeys = duplicateVerticesMap.Keys.QuickSorted();
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
                actualMap[alreadyExistsKey].AddRange(duplicateVerticesMap[sortedKey]);
                foreach (var duplicate in duplicateVerticesMap[sortedKey]) {
                    reverseDuplicateMap[duplicate] = alreadyExistsKey;
                }
            } else {
                actualMap.Add(sortedKey, new HashSet<int>());
                actualMap[sortedKey].AddRange(duplicateVerticesMap[sortedKey]);
                foreach (var duplicate in duplicateVerticesMap[sortedKey]) {
                    reverseDuplicateMap[duplicate] = sortedKey;
                }
            }
        }

        return reverseDuplicateMap;
    }

    private void RemapDuplicates(List<(int, int, bool)> duplicates, Dictionary<int, int> reverseDuplicatesMap) {
        // Remap the vertex indices for faces and edges
        var edgeReverseMap = new Dictionary<int, int>();
        foreach (var duplicate in duplicates) {
            edgeReverseMap[duplicate.Item2] = duplicate.Item1;
        }

        var allEdgeIndices = Faces.SelectMany(face => new[] { face.EdgeA, face.EdgeB, face.EdgeC }).Distinct().Except(edgeReverseMap.Keys).QuickSorted();
        var edgeRemap = new Dictionary<int, int>();
        var remapIndex = 0;
        foreach (var sortedKey in allEdgeIndices) {
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
   
    private void RemoveDuplicates(List<(int, int, bool)> duplicates, Dictionary<int, int> reverseDuplicatesMap) {
        // Remove duplicate edges
        foreach (var edge in duplicates.Select(tuple => Edges[tuple.Item2]).ToList()) {
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