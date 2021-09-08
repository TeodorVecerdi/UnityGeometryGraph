using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityCommons;
using UnityEngine;

[Serializable]
public class GeometryData {
    [SerializeField] public List<Vector3> Vertices;
    [SerializeField] public GeometryMetadata Metadata;

    public GeometryData(Mesh mesh, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
        Vertices = new List<Vector3>();
        
        mesh.GetVertices(Vertices);

        var triangles = new List<int>();
        mesh.GetTriangles(triangles, 0);
        BuildMetadata(mesh.normals, triangles, duplicateDistanceThreshold, duplicateNormalAngleThreshold);
    }

    private void BuildMetadata(Vector3[] normals, List<int> triangles, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
        Metadata = new GeometryMetadata(this);
        BuildElements(normals, triangles);
        RemoveDuplicates(normals, duplicateDistanceThreshold, duplicateNormalAngleThreshold);
    }

    private void BuildElements(Vector3[] normals, List<int> triangles) {
        for (var i = 0; i < triangles.Count; i += 3) {
            var idxA = triangles[i];
            var idxB = triangles[i + 1];
            var idxC = triangles[i + 2];

            var faceNormal = normals[idxA] + normals[idxB] + normals[idxC];

            var face = new Face(idxA, idxB, idxC, faceNormal / 3.0f);
            var edgeA = face.EdgeA = new Edge(idxA, idxB) {FaceA = face};
            var edgeB = face.EdgeB = new Edge(idxB, idxC) {FaceA = face};
            var edgeC = face.EdgeC = new Edge(idxC, idxA) {FaceA = face};

            Metadata.Faces.Add(face);
            Metadata.Edges.Add(edgeA);
            Metadata.Edges.Add(edgeB);
            Metadata.Edges.Add(edgeC);
        }
    }

    private void RemoveDuplicates(Vector3[] normals, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
        
        var potentialDuplicates = new List<(int, int, BitArray)>();
        var thresholdSqr = duplicateDistanceThreshold * duplicateDistanceThreshold;
        
        // Find duplicate edges
        for (var i = 0; i < Metadata.Edges.Count - 1; i++) {
            for (var j = i + 1; j < Metadata.Edges.Count; j++) {
                var edgeA = Metadata.Edges[i];
                var edgeB = Metadata.Edges[j];
                var checkA = (Vertices[edgeA.VertA] - Vertices[edgeB.VertA]).sqrMagnitude < thresholdSqr ? 1 : 0;
                var checkB = (Vertices[edgeA.VertA] - Vertices[edgeB.VertB]).sqrMagnitude < thresholdSqr ? 1 : 0;
                var checkC = (Vertices[edgeA.VertB] - Vertices[edgeB.VertA]).sqrMagnitude < thresholdSqr ? 1 : 0;
                var checkD = (Vertices[edgeA.VertB] - Vertices[edgeB.VertB]).sqrMagnitude < thresholdSqr ? 1 : 0;
                var total = checkA + checkB + checkC + checkD;

                // Two pairs of vertices are identical
                if (total >= 2) {
                    // Used to remap vertices later without checking again for which vertices are identical
                    var matchBitArray = new BitArray(4) { [0] = checkA != 0, [1] = checkB != 0, [2] = checkC != 0, [3] = checkD != 0 };
                    potentialDuplicates.Add((i, j, matchBitArray));
                }
            }
        }

        Debug.Log($"Found {potentialDuplicates.Count} potential duplicates");
        
        // Find real duplicates based on the face normal angle
        var actualDuplicates = new List<(int, int, BitArray)>();
        foreach (var potentialDuplicate in potentialDuplicates) {
            var edgeA = Metadata.Edges[potentialDuplicate.Item1];
            var edgeB = Metadata.Edges[potentialDuplicate.Item2];
            var faceA = edgeA.FaceA;
            var faceB = edgeB.FaceA;
            var normalAngle = Vector3.Angle(faceA.FaceNormal, faceB.FaceNormal);

            if (normalAngle > duplicateNormalAngleThreshold) continue;
            
            actualDuplicates.Add(potentialDuplicate);
            edgeA.FaceB = faceB;
            
            // Remap face edges
            if (faceB.EdgeA == edgeB) faceB.EdgeA = edgeA;
            else if (faceB.EdgeB == edgeB) faceB.EdgeB = edgeA;
            else if (faceB.EdgeC == edgeB) faceB.EdgeC = edgeA;
        }
        potentialDuplicates.Clear();
        Debug.Log($"Of which {actualDuplicates.Count} are actual duplicates");

        // Make a dictionary of (UniqueVertex => [Duplicates])
        var vertexRemap = new Dictionary<int, List<int>>();
        foreach (var actualDuplicate in actualDuplicates) {
            var edgeA = Metadata.Edges[actualDuplicate.Item1];
            var edgeB = Metadata.Edges[actualDuplicate.Item2];
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
                Debug.LogError("Duplicate edge vertices were somehow uninitialized.");
                return;
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

        // Remove trivial invalid entries (key==contents)
        var removalList = new List<int>();
        foreach (var pair in vertexRemap) {
            pair.Value.RemoveAll(value => value == pair.Key);
            if(pair.Value.Count == 0) removalList.Add(pair.Key);
        }
        removalList.ForEach(key => vertexRemap.Remove(key));
        removalList.Clear();

        foreach (var keyValuePair in vertexRemap) {
            Debug.LogWarning($"Vertex {{{keyValuePair.Key}}} has duplicates {keyValuePair.Value.ToListString()}");
        }

        // Remove remaining invalid entries
        var sortedKeys = vertexRemap.Keys.QuickSorted().ToList();
        var actualMap = new Dictionary<int, HashSet<int>>();
        var reverseDuplicateMap = new Dictionary<int, int>();
        foreach (var sortedKey in sortedKeys) {
            var alreadyExistsKey = -1;
            foreach (var pair in actualMap) {
                if (pair.Value.Contains(sortedKey)) {
                    alreadyExistsKey = pair.Key;
                    break;
                }
            }

            if (alreadyExistsKey != -1) {
                actualMap[alreadyExistsKey].AddRange(vertexRemap[sortedKey]);
                foreach (var duplicate in vertexRemap[sortedKey]) {
                    reverseDuplicateMap[duplicate] = alreadyExistsKey;
                }
            } else {
                actualMap.Add(sortedKey, new HashSet<int>());
                actualMap[sortedKey].AddRange(vertexRemap[sortedKey]);
                foreach (var duplicate in vertexRemap[sortedKey]) {
                    reverseDuplicateMap[duplicate] = sortedKey;
                }
            }
        }
        
        vertexRemap.Clear();
        sortedKeys.Clear();

        foreach (var pair in actualMap) {
            Debug.Log($"Unique vertex {{{pair.Key}}} has duplicates {pair.Value.ToListString()}");
        }
        Debug.Log($"Reverse duplicate map:\n{reverseDuplicateMap.ToListString()}");
        
        // Remap the vertex indices for faces and edges
        foreach (var face in Metadata.Faces) {
            RemapFace(face, reverseDuplicateMap);
        }

        // Remove duplicate edges
        foreach (var edge in actualDuplicates.Select(tuple => Metadata.Edges[tuple.Item2]).ToList()) {
            Metadata.Edges.Remove(edge);
        }

        // Remove duplicate vertices
        var sortedDuplicateVertices = reverseDuplicateMap.Keys.QuickSorted().Reverse().ToList();
        foreach (var vertexIndex in sortedDuplicateVertices) {
            Vertices.RemoveAt(vertexIndex);
        }

        // Check if there are any invalid faces
        for (var i = 0; i < Metadata.Faces.Count; i++) {
            var face = Metadata.Faces[i];
            if (!Metadata.Edges.Contains(face.EdgeA) || !Metadata.Edges.Contains(face.EdgeB) || !Metadata.Edges.Contains(face.EdgeC)) {
                Debug.LogError($"Face at index {i} contains invalid edges");
            }
        }
        
        for (var i = 0; i < Metadata.Edges.Count; i++) {
            var edge = Metadata.Edges[i];
            if (edge.FaceA == null || edge.FaceB == null) {
                Debug.LogError($"Edge at index {i} doesn't have all faces assigned");
            }
        }
    }

    private void RemapFace(Face face, Dictionary<int, int> reverseDuplicateMap) {
        if (reverseDuplicateMap.ContainsKey(face.VertA)) {
            face.VertA = reverseDuplicateMap[face.VertA];
        }
        if (reverseDuplicateMap.ContainsKey(face.VertB)) {
            face.VertB = reverseDuplicateMap[face.VertB];
        }
        if (reverseDuplicateMap.ContainsKey(face.VertC)) {
            face.VertC = reverseDuplicateMap[face.VertC];
        }
        
        RemapEdge(face.EdgeA, reverseDuplicateMap);
        RemapEdge(face.EdgeB, reverseDuplicateMap);
        RemapEdge(face.EdgeC, reverseDuplicateMap);
    }

    private void RemapEdge(Edge edge, Dictionary<int, int> reverseDuplicateMap) {
        if (reverseDuplicateMap.ContainsKey(edge.VertA)) {
            edge.VertA = reverseDuplicateMap[edge.VertA];
        }
        if (reverseDuplicateMap.ContainsKey(edge.VertB)) {
            edge.VertB = reverseDuplicateMap[edge.VertB];
        }
    } 

    [Serializable]
    public class GeometryMetadata {
        [SerializeReference, HideInInspector] private GeometryData owner;

        public List<Edge> Edges;
        public List<Face> Faces;

        public GeometryMetadata(GeometryData owner) {
            this.owner = owner;
            Edges = new List<Edge>();
            Faces = new List<Face>();
        }
    }
    
    [Serializable]
    public class Edge {
        public int VertA;
        public int VertB;
        [SerializeReference, HideInInspector] public Face FaceA;
        [SerializeReference, HideInInspector] public Face FaceB;

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

        [SerializeReference, HideInInspector] public Edge EdgeA;
        [SerializeReference, HideInInspector] public Edge EdgeB;
        [SerializeReference, HideInInspector] public Edge EdgeC;

        public Face(int vertA, int vertB, int vertC, Vector3 faceNormal) {
            VertA = vertA;
            VertB = vertB;
            VertC = vertC;
            FaceNormal = faceNormal;
        }
    }
}


