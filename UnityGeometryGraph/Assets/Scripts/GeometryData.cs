using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityCommons;
using UnityEngine;

[Serializable]
public class GeometryData {
    [SerializeField] public List<Vector3> Vertices;
    [SerializeField] public List<int> Triangles;
    [SerializeField] public GeometryMetadata Metadata;

    public GeometryData(Mesh mesh, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
        Vertices = new List<Vector3>();
        Triangles = new List<int>();
        
        mesh.GetVertices(Vertices);
        mesh.GetTriangles(Triangles, 0);

        BuildMetadata(mesh.normals, duplicateDistanceThreshold, duplicateNormalAngleThreshold);
    }

    private void BuildMetadata(Vector3[] normals, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
        Metadata = new GeometryMetadata(this);
        BuildElements(normals);
        RemoveDuplicates(normals, duplicateDistanceThreshold, duplicateNormalAngleThreshold);
    }

    private void BuildElements(Vector3[] normals) {
        for (var i = 0; i < Triangles.Count; i += 3) {
            var idxA = Triangles[i];
            var idxB = Triangles[i + 1];
            var idxC = Triangles[i + 2];

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
            var faceA = Metadata.Edges[potentialDuplicate.Item1].FaceA;
            var faceB = Metadata.Edges[potentialDuplicate.Item2].FaceA;
            var normalAngle = Vector3.Angle(faceA.FaceNormal, faceB.FaceNormal);
            
            if(normalAngle < duplicateNormalAngleThreshold) 
                actualDuplicates.Add(potentialDuplicate);
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
            pair.Value.Remove(pair.Key);
            if(pair.Value.Count == 0) removalList.Add(pair.Key);
        }
        removalList.ForEach(key => vertexRemap.Remove(key));
        removalList.Clear();

        // Remove remaining invalid entries
        var sortedKeys = vertexRemap.Keys.QuickSorted().ToList();
        var actualMap = new Dictionary<int, HashSet<int>>();
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
            } else {
                actualMap.Add(sortedKey, new HashSet<int>());
                actualMap[sortedKey].AddRange(vertexRemap[sortedKey]);
            }
        }
        
        vertexRemap.Clear();
        sortedKeys.Clear();

        foreach (var pair in actualMap) {
            Debug.Log($"Unique vertex {{{pair.Key}}} has duplicates {pair.Value.ToListString()}");
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


