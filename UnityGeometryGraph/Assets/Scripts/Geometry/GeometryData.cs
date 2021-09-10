using System;
using System.Collections.Generic;
using System.Linq;
using Attribute;
using Geometry;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;
using Debug = UnityEngine.Debug;

[Serializable]
public class GeometryData {
    [SerializeField] private List<Vertex> vertices;
    [SerializeField] private List<Edge> edges;
    [SerializeField] private List<Face> faces;
    [SerializeField] private List<FaceCorner> faceCorners;
    [SerializeField] private AttributeManager attributeManager;

    public IReadOnlyList<Vertex> Vertices => vertices.AsReadOnly();
    public IReadOnlyList<Edge> Edges => edges.AsReadOnly();
    public IReadOnlyList<Face> Faces => faces.AsReadOnly();
    public IReadOnlyList<FaceCorner> FaceCorners => faceCorners.AsReadOnly();

    public GeometryData(Mesh mesh, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
        var vertices = new List<float3>(mesh.vertices.Select(vertex => new float3(vertex.x, vertex.y, vertex.z)));
        var triangles = new List<int>();
        var meshUvs = new List<float2>(mesh.uv.Select(uv => new float2(uv.x, uv.y)));
        var uvs = new List<float2>();
        mesh.GetTriangles(triangles, 0);

        var faceNormals = BuildMetadata(vertices, triangles, meshUvs, uvs, duplicateDistanceThreshold, duplicateNormalAngleThreshold);

        attributeManager = new AttributeManager();
        FillBuiltinAttributes(vertices, faceNormals, uvs);
    }

    public TAttribute GetAttribute<TAttribute>(string name) where TAttribute : BaseAttribute {
        return (TAttribute)attributeManager.Request(name);
    }

    public TAttribute GetAttribute<TAttribute>(string name, AttributeDomain domain) where TAttribute : BaseAttribute {
        return (TAttribute)attributeManager.Request(name, domain);
    }

    private void FillBuiltinAttributes(IEnumerable<float3> vertices, IEnumerable<float3> faceNormals, List<float2> uvs) {
        attributeManager.Store(vertices.Into<Vector3Attribute>("position", AttributeDomain.Vertex));

        attributeManager.Store(faceNormals.Into<Vector3Attribute>("normal", AttributeDomain.Face));
        attributeManager.Store(new int[faces.Count].Into<IntAttribute>("material_index", AttributeDomain.Face));
        attributeManager.Store(new bool[faces.Count].Into<BoolAttribute>("shade_smooth", AttributeDomain.Face));

        attributeManager.Store(new float[edges.Count].Into<ClampedFloatAttribute>("crease", AttributeDomain.Edge));

        if (uvs.Count > 0) attributeManager.Store(uvs.Into<Vector2Attribute>("uv", AttributeDomain.FaceCorner));
    }

    private IEnumerable<float3> BuildMetadata(List<float3> vertices, List<int> triangles, List<float2> uvs, List<float2> correctUvs, float duplicateDistanceThreshold,
                                              float duplicateNormalAngleThreshold) {
        edges = new List<Edge>(triangles.Count);
        faces = new List<Face>(triangles.Count / 3);
        faceCorners = new List<FaceCorner>(triangles.Count);
        var faceNormals = BuildElements(vertices, triangles, uvs, correctUvs).ToList();
        RemoveDuplicates(vertices, faceNormals, duplicateDistanceThreshold, duplicateNormalAngleThreshold);

        this.vertices = new List<Vertex>(vertices.Count);
        for (var i = 0; i < vertices.Count; i++) {
            this.vertices.Add(new Vertex());
        }

        FillElementMetadata();

        return faceNormals;
    }

    private IEnumerable<float3> BuildElements(List<float3> vertices, List<int> triangles, List<float2> uvs, List<float2> correctUvs) {
        for (var i = 0; i < triangles.Count; i += 3) {
            var idxA = triangles[i];
            var idxB = triangles[i + 1];
            var idxC = triangles[i + 2];

            var vertA = vertices[idxA];
            var vertB = vertices[idxB];
            var vertC = vertices[idxC];

            var AB = vertB - vertA;
            var AC = vertC - vertA;
            yield return math.normalize(math.cross(AB, AC));

            var face = new Face(
                idxA, idxB, idxC,
                faceCorners.Count, faceCorners.Count + 1, faceCorners.Count + 2,
                edges.Count, edges.Count + 1, edges.Count + 2
            );

            var edgeA = new Edge(idxA, idxB) { FaceA = faces.Count };
            var edgeB = new Edge(idxB, idxC) { FaceA = faces.Count };
            var edgeC = new Edge(idxC, idxA) { FaceA = faces.Count };

            var faceCornerA = new FaceCorner(faces.Count);
            var faceCornerB = new FaceCorner(faces.Count);
            var faceCornerC = new FaceCorner(faces.Count);

            faces.Add(face);
            edges.Add(edgeA);
            edges.Add(edgeB);
            edges.Add(edgeC);
            faceCorners.Add(faceCornerA);
            faceCorners.Add(faceCornerB);
            faceCorners.Add(faceCornerC);

            if (uvs.Count > 0) {
                correctUvs.Add(uvs[idxA]);
                correctUvs.Add(uvs[idxB]);
                correctUvs.Add(uvs[idxC]);
            }
        }
    }

    private void RemoveDuplicates(List<float3> vertices, List<float3> faceNormals, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
        var duplicates = GetDuplicateEdges(vertices, faceNormals, duplicateDistanceThreshold * duplicateDistanceThreshold, duplicateNormalAngleThreshold);
        var duplicateVerticesMap = GetDuplicateVerticesMap(duplicates);
        var reverseDuplicatesMap = RemoveInvalidDuplicates(duplicateVerticesMap);

        RemapDuplicateElements(duplicates, reverseDuplicatesMap);
        RemoveDuplicateElements(vertices, duplicates, reverseDuplicatesMap);

        CheckForErrors(vertices);
    }

    private void FillElementMetadata() {
        // Vertex Metadata ==> Edges, Faces
        FillVertexMetadata();

        // Face Metadata ==> Adjacent faces
        FillFaceMetadata();

        // Face Corner Metadata ==> Backing Vertex
        FillFaceCornerMetadata();
    }

    private List<(int, int, bool)> GetDuplicateEdges(List<float3> vertices, List<float3> faceNormals, float sqrDistanceThreshold, float duplicateNormalAngleThreshold) {
        var potentialDuplicates = new List<(int, int, bool)>();
        // Find potential duplicate edges
        for (var i = 0; i < edges.Count - 1; i++) {
            for (var j = i + 1; j < edges.Count; j++) {
                var edgeA = edges[i];
                var edgeB = edges[j];
                var edgeAVertA = vertices[edgeA.VertA];
                var edgeAVertB = vertices[edgeA.VertB];
                var edgeBVertA = vertices[edgeB.VertA];
                var edgeBVertB = vertices[edgeB.VertB];

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

                var checkA = edgeAVertA.Equals(edgeBVertA) && edgeAVertB.Equals(edgeBVertB);
                var checkB = edgeAVertA.Equals(edgeBVertB) && edgeAVertB.Equals(edgeBVertA);
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
            var edgeA = edges[edgeAIndex];
            var edgeB = edges[edgeBIndex];
            var normalAngle = math_util.angle(faceNormals[edgeA.FaceA], faceNormals[edgeB.FaceA]);

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
            var edgeA = edges[duplicate.Item1];
            var edgeB = edges[duplicate.Item2];

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

    private void RemapDuplicateElements(List<(int, int, bool)> duplicates, Dictionary<int, int> reverseDuplicatesMap) {
        // Remap the vertex indices for faces and edges
        var edgeReverseMap = new Dictionary<int, int>();
        foreach (var duplicate in duplicates) {
            edgeReverseMap[duplicate.Item2] = duplicate.Item1;
        }

        var allEdgeIndices = faces.SelectMany(face => new[] { face.EdgeA, face.EdgeB, face.EdgeC }).Distinct().Except(edgeReverseMap.Keys).QuickSorted();
        var edgeRemap = new Dictionary<int, int>();
        var remapIndex = 0;
        foreach (var sortedKey in allEdgeIndices) {
            edgeRemap[sortedKey] = remapIndex++;
        }

        foreach (var face in faces) {
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
        var edge = edges[edgeIndex];
        if (reverseDuplicateMap.ContainsKey(edge.VertA)) {
            edge.VertA = reverseDuplicateMap[edge.VertA];
        }

        if (reverseDuplicateMap.ContainsKey(edge.VertB)) {
            edge.VertB = reverseDuplicateMap[edge.VertB];
        }
    }

    private void RemoveDuplicateElements(List<float3> vertices, List<(int, int, bool)> duplicates, Dictionary<int, int> reverseDuplicatesMap) {
        // Remove duplicate edges
        foreach (var edge in duplicates.Select(tuple => edges[tuple.Item2]).ToList()) {
            edges.Remove(edge);
        }

        // Remove duplicate vertices
        var sortedDuplicateVertices = reverseDuplicatesMap.Keys.QuickSorted().Reverse().ToList();
        foreach (var vertexIndex in sortedDuplicateVertices) {
            vertices.RemoveAt(vertexIndex);
        }
    }

    private void CheckForErrors(List<float3> vertices) {
        // Check if there are any invalid edges
        for (var i = 0; i < edges.Count; i++) {
            var edge = edges[i];
            if (edge.VertA >= vertices.Count || edge.VertB >= vertices.Count) {
                Debug.LogError($"Edge at index {i} contains invalid vertices");
            }
        }

        // Check if there are any invalid faces
        for (var i = 0; i < faces.Count; i++) {
            var face = faces[i];
            if (face.EdgeA >= edges.Count || face.EdgeB >= edges.Count || face.EdgeC >= edges.Count ||
                face.EdgeA == -1 || face.EdgeB == -1 || face.EdgeC == -1) {
                Debug.LogError($"Face at index {i} contains invalid edges");
            }

            if (face.VertA >= vertices.Count || face.VertB >= vertices.Count || face.VertC >= vertices.Count) {
                Debug.LogError($"Face at index {i} contains invalid vertices");
            }
        }
    }

    private void FillVertexMetadata() {
        // Edges
        for (var i = 0; i < edges.Count; i++) {
            var edge = edges[i];
            vertices[edge.VertA].Edges.Add(i);
            vertices[edge.VertB].Edges.Add(i);
        }

        //Faces
        for (var i = 0; i < faces.Count; i++) {
            var face = faces[i];
            vertices[face.VertA].Faces.Add(i);
            vertices[face.VertB].Faces.Add(i);
            vertices[face.VertC].Faces.Add(i);
        }

        // Cleanup
        foreach (var vertex in vertices) {
            vertex.Edges.RemoveDuplicates();
            vertex.Faces.RemoveDuplicates();
        }
    }

    private void FillFaceMetadata() {
        for (var i = 0; i < faces.Count; i++) {
            var face = faces[i];
            var edgeA = edges[face.EdgeA];
            var edgeB = edges[face.EdgeB];
            var edgeC = edges[face.EdgeC];
            face.AdjacentFaces.AddRange(new[] { edgeA.FaceA, edgeA.FaceB, edgeB.FaceA, edgeB.FaceB, edgeC.FaceA, edgeC.FaceB });

            // Cleanup
            face.AdjacentFaces.RemoveDuplicates();
            face.AdjacentFaces.RemoveAll(adjacentIndex => adjacentIndex == i);
        }
    }

    private void FillFaceCornerMetadata() {
        foreach (var face in faces) {
            var fcA = faceCorners[face.FaceCornerA];
            var fcB = faceCorners[face.FaceCornerB];
            var fcC = faceCorners[face.FaceCornerC];

            fcA.Vert = face.VertA;
            fcB.Vert = face.VertB;
            fcC.Vert = face.VertC;
        }
    }

    [Serializable]
    public class Vertex {
        public List<int> Edges;
        public List<int> Faces;

        public Vertex() {
            Edges = new List<int>();
            Faces = new List<int>();
        }
    }

    [Serializable]
    public class Edge {
        public int VertA;
        public int VertB;
        public int FaceA = -1;
        public int FaceB = -1;

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

        public int FaceCornerA;
        public int FaceCornerB;
        public int FaceCornerC;

        public int EdgeA = -1;
        public int EdgeB = -1;
        public int EdgeC = -1;

        public List<int> AdjacentFaces;

        public Face(int vertA, int vertB, int vertC, int faceCornerA, int faceCornerB, int faceCornerC, int edgeA, int edgeB, int edgeC) {
            VertA = vertA;
            VertB = vertB;
            VertC = vertC;

            FaceCornerA = faceCornerA;
            FaceCornerB = faceCornerB;
            FaceCornerC = faceCornerC;

            EdgeA = edgeA;
            EdgeB = edgeB;
            EdgeC = edgeC;

            AdjacentFaces = new List<int>();
        }
    }

    [Serializable]
    public class FaceCorner {
        public int Vert = -1;
        public int Face = -1;

        public FaceCorner(int face) {
            Face = face;
        }
    }
}