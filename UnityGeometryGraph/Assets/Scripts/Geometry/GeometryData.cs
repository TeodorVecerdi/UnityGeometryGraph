﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Attribute;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Geometry {
    [Serializable]
    public class GeometryData {
        [SerializeField] private List<Vertex> vertices;
        [SerializeField] private List<Edge> edges;
        [SerializeField] private List<Face> faces;
        [SerializeField] private List<FaceCorner> faceCorners;
        [SerializeField] private AttributeManager attributeManager;
        [SerializeField] private int submeshCount;

        public IReadOnlyList<Vertex> Vertices => vertices.AsReadOnly();
        public IReadOnlyList<Edge> Edges => edges.AsReadOnly();
        public IReadOnlyList<Face> Faces => faces.AsReadOnly();
        public IReadOnlyList<FaceCorner> FaceCorners => faceCorners.AsReadOnly();
        public int SubmeshCount => submeshCount;

        public GeometryData(Mesh mesh, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
            submeshCount = mesh.subMeshCount;
            var vertices = new List<float3>(mesh.vertices.Select(vertex => new float3(vertex.x, vertex.y, vertex.z)));
            var meshUvs = new List<float2>(mesh.uv.Select(uv => new float2(uv.x, uv.y)));

            var uvs = new List<float2>();
            var faceMaterialIndices = new List<int>();
            var faceSmoothShaded = new List<bool>();

            var faceNormals = BuildMetadata(mesh, vertices, meshUvs, uvs, faceMaterialIndices, 
                                            faceSmoothShaded, duplicateDistanceThreshold, duplicateNormalAngleThreshold);

            attributeManager = new AttributeManager();
            FillBuiltinAttributes(vertices, uvs, faceNormals, faceMaterialIndices, faceSmoothShaded);
        }

        public TAttribute GetAttribute<TAttribute>(string name) where TAttribute : BaseAttribute {
            return (TAttribute)attributeManager.Request(name);
        }

        public TAttribute GetAttribute<TAttribute>(string name, AttributeDomain domain) where TAttribute : BaseAttribute {
            return (TAttribute)attributeManager.Request(name, domain);
        }

        private void FillBuiltinAttributes(
            IEnumerable<float3> vertices, List<float2> uvs, 
            IEnumerable<float3> faceNormals, IEnumerable<int> faceMaterialIndices, IEnumerable<bool> faceSmoothShaded
        ) {
            attributeManager.Store(vertices.Into<Vector3Attribute>("position", AttributeDomain.Vertex));

            attributeManager.Store(faceNormals.Into<Vector3Attribute>("normal", AttributeDomain.Face));
            attributeManager.Store(faceMaterialIndices.Into<IntAttribute>("material_index", AttributeDomain.Face));
            attributeManager.Store(faceSmoothShaded.Into<BoolAttribute>("shade_smooth", AttributeDomain.Face));

            attributeManager.Store(new float[edges.Count].Into<ClampedFloatAttribute>("crease", AttributeDomain.Edge));

            if (uvs.Count > 0) attributeManager.Store(uvs.Into<Vector2Attribute>("uv", AttributeDomain.FaceCorner));
        }

        private IEnumerable<float3> BuildMetadata(
            Mesh mesh, List<float3> vertices, List<float2> uvs, List<float2> correctUvs, List<int> faceMaterialIndices, 
            List<bool> smoothShaded, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold
        ) {
            edges = new List<Edge>(mesh.triangles.Length);
            faces = new List<Face>(mesh.triangles.Length / 3);
            faceCorners = new List<FaceCorner>(mesh.triangles.Length);
            var faceNormals = BuildElements(mesh, vertices, uvs, correctUvs, faceMaterialIndices, smoothShaded).ToList();
            RemoveDuplicates(vertices, faceNormals, duplicateDistanceThreshold, duplicateNormalAngleThreshold);

            this.vertices = new List<Vertex>(vertices.Count);
            for (var i = 0; i < vertices.Count; i++) {
                this.vertices.Add(new Vertex());
            }

            FillElementMetadata();

            return faceNormals;
        }

        private IEnumerable<float3> BuildElements(
            Mesh mesh, List<float3> vertices, List<float2> uvs, 
            List<float2> correctUvs, List<int> materialIndices, List<bool> smoothShaded
        ) {
            var meshNormals = mesh.normals;
            for (var submesh = 0; submesh < mesh.subMeshCount; submesh++) {
                var triangles = mesh.GetTriangles(submesh);
                var length = triangles.Length;
            
                for (var i = 0; i < length; i += 3) {
                    var idxA = triangles[i];
                    var idxB = triangles[i + 1];
                    var idxC = triangles[i + 2];

                    var vertA = vertices[idxA];
                    var vertB = vertices[idxB];
                    var vertC = vertices[idxC];

                    var AB = vertB - vertA;
                    var AC = vertC - vertA;
                    var computedNormal = math.normalize(math.cross(AB, AC));
                    var meshNormal = (float3)(meshNormals[idxA] + meshNormals[idxB] + meshNormals[idxC]) / 3.0f;
                    yield return computedNormal;
                    materialIndices.Add(submesh);

                    smoothShaded.Add(math.lengthsq(computedNormal - meshNormal) > 0.0001f); 

                    var face = new Face(
                        idxA, idxB, idxC,
                        faceCorners.Count, faceCorners.Count + 1, faceCorners.Count + 2,
                        edges.Count, edges.Count + 1, edges.Count + 2
                    );

                    var edgeA = new Edge(idxA, idxB, edges.Count) { FaceA = faces.Count };
                    var edgeB = new Edge(idxB, idxC, edges.Count + 1) { FaceA = faces.Count };
                    var edgeC = new Edge(idxC, idxA, edges.Count + 2) { FaceA = faces.Count };

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
        }

        private void RemoveDuplicates(List<float3> vertices, List<float3> faceNormals, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
            var sw = Stopwatch.StartNew();
            var duplicates = GetDuplicateEdges(vertices, faceNormals, duplicateDistanceThreshold * duplicateDistanceThreshold, duplicateNormalAngleThreshold);
            var el1 = sw.Elapsed;
            // Debug.Log(duplicates.Count);
            sw.Restart();
            var duplicateVerticesMap = GetDuplicateVerticesMap(duplicates);
            var el2 = sw.Elapsed;
            Debug.Log(duplicateVerticesMap.Select(pair => (pair.Key, pair.Value.ToListString())).ToListString());
            // Debug.Log(duplicateVerticesMap.Count);
            sw.Restart();
            var reverseDuplicatesMap = RemoveInvalidDuplicates(duplicateVerticesMap);
            var el3 = sw.Elapsed;
            Debug.Log($"GetDuplicateEdges={el1.TotalMilliseconds}; GetDuplicateVerticesMap={el2.TotalMilliseconds}; RemoveInvalidDuplicates={el3.TotalMilliseconds}");
            RemapDuplicateElements(vertices, duplicates, reverseDuplicatesMap);
            RemoveDuplicateElements(vertices, duplicates, reverseDuplicatesMap);
            CheckForErrors(vertices);
        }

        private void FillElementMetadata() {
            // Face Corner Metadata ==> Backing Vertex
            FillFaceCornerMetadata();
        
            // Vertex Metadata ==> Edges, Faces, FaceCorners
            FillVertexMetadata();

            // Face Metadata ==> Adjacent faces
            FillFaceMetadata();
        }

        private List<(int, int, bool)> GetDuplicateEdges(List<float3> vertices, List<float3> faceNormals, float sqrDistanceThreshold, float duplicateNormalAngleThreshold) {
            var equalityComparer = new EdgeEqualityComparer(vertices);
            // NOTE: I don't actually care about the HashSet, everything I need is stored in the comparer lol
            var _ = new HashSet<Edge>(edges, equalityComparer);

            IEnumerable<(int EdgeA, int EdgeB)> potentialDuplicates = equalityComparer.Duplicates
                                                      // Convert (Key, {values}) to {Key, ...values}
                                                      .Select(pair => pair.Value.Prepend(pair.Key).ToList()) 
                                                      // Get all subsets of length 2, so {1,2,3} becomes {{1,2}, {1,3}, {2,3}}
                                                      .SelectMany(values => values.SubSets2());

            var excluded = new HashSet<int>();
            // Find real duplicates based on the face normal angle
            var actualDuplicates = new List<(int, int, bool)>();
            foreach (var (edgeAIndex, edgeBIndex) in potentialDuplicates) {
                if (excluded.Contains(edgeAIndex) || excluded.Contains(edgeBIndex)) continue;
                
                var edgeA = edges[edgeAIndex];
                var edgeB = edges[edgeBIndex];
                var normalAngle = math_util.angle(faceNormals[edgeA.FaceA], faceNormals[edgeB.FaceA]);

                if (normalAngle > duplicateNormalAngleThreshold) continue;

                excluded.Add(edgeAIndex);
                excluded.Add(edgeBIndex);
                
                var edgeAVertA = vertices[edgeA.VertA];
                var edgeAVertB = vertices[edgeA.VertB];
                var edgeBVertA = vertices[edgeB.VertA];
                var edgeBVertB = vertices[edgeB.VertB];

                actualDuplicates.Add((edgeAIndex, edgeBIndex, edgeAVertA.Equals(edgeBVertA) && edgeAVertB.Equals(edgeBVertB)));
                edgeA.FaceB = edgeB.FaceA;
            }

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
            Debug.Log(duplicateVerticesMap.Select(pair => (pair.Key, pair.Value.ToListString())).ToListString());

            // Remove remaining invalid entries
            var sortedKeys = duplicateVerticesMap.Keys.QuickSorted().ToList();
            var actualMap = new Dictionary<int, HashSet<int>>();
            var reverseDuplicateMap = new Dictionary<int, int>();

            foreach (var sortedKey in sortedKeys) {
                var alreadyExistsKey = -1;
                var alreadyExistsValue = -1;

                foreach (var pair in actualMap) {
                    if (pair.Value.Contains(sortedKey)) {
                        alreadyExistsKey = pair.Key;
                        break;                        
                    }

                    if (pair.Value.Any(value => duplicateVerticesMap[sortedKey].Contains(value))) {
                        alreadyExistsValue = pair.Key;
                        if (!actualMap.ContainsKey(pair.Key)) actualMap[pair.Key] = new HashSet<int>();
                        actualMap[pair.Key].AddRange(duplicateVerticesMap[sortedKey]);
                        actualMap[pair.Key].Add(sortedKey);
                        
                        reverseDuplicateMap[sortedKey] = pair.Key;
                        foreach (var duplicateVertex in duplicateVerticesMap[sortedKey]) {
                            reverseDuplicateMap[duplicateVertex] = pair.Key;
                        }
                    }
                }

                if (alreadyExistsKey != -1) {
                    actualMap[alreadyExistsKey].AddRange(duplicateVerticesMap[sortedKey]);
                    foreach (var duplicate in duplicateVerticesMap[sortedKey]) {
                        reverseDuplicateMap[duplicate] = alreadyExistsKey;
                    }
                } else if (alreadyExistsValue == -1) {
                    actualMap.Add(sortedKey, new HashSet<int>());
                    actualMap[sortedKey].AddRange(duplicateVerticesMap[sortedKey]);
                    foreach (var duplicate in duplicateVerticesMap[sortedKey]) {
                        reverseDuplicateMap[duplicate] = sortedKey;
                    }
                }
            }
            
            // Copy actualMap to duplicateVerticesMap
            duplicateVerticesMap.Clear();
            foreach (var pair in actualMap) {
                duplicateVerticesMap[pair.Key] = new List<int>(pair.Value);
            }

            return reverseDuplicateMap;
        }

        private void RemapDuplicateElements(List<float3> vertices, List<(int, int, bool)> duplicates, Dictionary<int, int> reverseDuplicatesMap) {
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

            var allVertexIndices = Enumerable.Range(0, vertices.Count).Except(reverseDuplicatesMap.Keys);
            var vertexRemap = new Dictionary<int, int>();
            remapIndex = 0;
            foreach (var key in allVertexIndices) {
                vertexRemap[key] = remapIndex++;
            }
            
            /*Debug.Log(reverseDuplicatesMap.ToListString());
            foreach (var pair in duplicateVerticesMap) {
                Debug.Log($"{pair.Key}: {pair.Value.ToListString()}");
            }

            Debug.Log(vertexRemap.ToListString());*/

            foreach (var face in faces) {
                RemapEdge(face.EdgeA, reverseDuplicatesMap, vertexRemap);
                RemapEdge(face.EdgeB, reverseDuplicatesMap, vertexRemap);
                RemapEdge(face.EdgeC, reverseDuplicatesMap, vertexRemap);

                RemapFace(face, reverseDuplicatesMap, edgeReverseMap, edgeRemap, vertexRemap);
            }
        }

        private void RemapFace(Face face, Dictionary<int, int> reverseDuplicateMap, Dictionary<int, int> edgeReverseMap, Dictionary<int, int> edgeIndexRemap, Dictionary<int, int> vertexIndexRemap) {
            if (reverseDuplicateMap.ContainsKey(face.VertA)) {
                face.VertA = vertexIndexRemap[reverseDuplicateMap[face.VertA]];
            } else if (vertexIndexRemap.ContainsKey(face.VertA)) {
                face.VertA = vertexIndexRemap[face.VertA];
            }

            if (reverseDuplicateMap.ContainsKey(face.VertB)) {
                face.VertB = vertexIndexRemap[reverseDuplicateMap[face.VertB]];
            } else if (vertexIndexRemap.ContainsKey(face.VertB)) {
                face.VertB = vertexIndexRemap[face.VertB];
            }

            if (reverseDuplicateMap.ContainsKey(face.VertC)) {
                face.VertC = vertexIndexRemap[reverseDuplicateMap[face.VertC]];
            } else if (vertexIndexRemap.ContainsKey(face.VertC)) {
                face.VertC = vertexIndexRemap[face.VertC];
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

        private void RemapEdge(int edgeIndex, Dictionary<int, int> reverseDuplicateMap, Dictionary<int, int> vertexRemap) {
            var edge = edges[edgeIndex];
            if (reverseDuplicateMap.ContainsKey(edge.VertA)) {
                edge.VertA = vertexRemap[reverseDuplicateMap[edge.VertA]];
            } else if (vertexRemap.ContainsKey(edge.VertA)) {
                edge.VertA = vertexRemap[edge.VertA];
            }

            if (reverseDuplicateMap.ContainsKey(edge.VertB)) {
                edge.VertB = vertexRemap[reverseDuplicateMap[edge.VertB]];
            } else if (vertexRemap.ContainsKey(edge.VertB)) {
                edge.VertB = vertexRemap[edge.VertB];
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
        
            // Face Corners
            for (var i = 0; i < faceCorners.Count; i++) {
                var fc = faceCorners[i];
                vertices[fc.Vert].FaceCorners.Add(i);
            }

            // Cleanup
            foreach (var vertex in vertices) {
                vertex.Edges.RemoveDuplicates();
                vertex.Faces.RemoveDuplicates();
                vertex.FaceCorners.RemoveDuplicates();
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
                face.AdjacentFaces.RemoveAll(adjacentIndex => adjacentIndex == i || adjacentIndex == -1);
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
            public List<int> FaceCorners;

            public Vertex() {
                Edges = new List<int>();
                Faces = new List<int>();
                FaceCorners = new List<int>();
            }
        }

        [Serializable]
        public class Edge {
            public int VertA;
            public int VertB;
            public int FaceA = -1;
            public int FaceB = -1;

            public int SelfIndex;

            public Edge(int vertA, int vertB, int selfIndex) {
                VertA = vertA;
                VertB = vertB;
                SelfIndex = selfIndex;
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
}