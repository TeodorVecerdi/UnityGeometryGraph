using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attribute;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GeometryGraph.Runtime.Geometry {
    [Serializable]
    public partial class GeometryData : ICloneable {
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

        private GeometryData() {
            vertices = new List<Vertex>();
            edges = new List<Edge>();
            faces = new List<Face>();
            faceCorners = new List<FaceCorner>();
            attributeManager = new AttributeManager(this);
            
            FillBuiltinAttributes(Array.Empty<float3>(), new List<float2>(), Array.Empty<float>(), Array.Empty<float3>(), Array.Empty<int>(), Array.Empty<bool>());
            
            submeshCount = 0;
        }

        public GeometryData(IEnumerable<Edge> edges, IEnumerable<Face> faces, IEnumerable<FaceCorner> faceCorners, int submeshCount,
                             IEnumerable<float3> vertexPositions, IEnumerable<float3> faceNormals, IEnumerable<int> materialIndices,
                             IEnumerable<bool> smoothShaded, IEnumerable<float> creases, IEnumerable<float2> uvs) {
            vertices = new List<Vertex>();
            this.edges = edges is List<Edge> edgeList ? edgeList : new List<Edge>(edges);
            this.faces = faces is List<Face> facesList ? facesList : new List<Face>(faces);
            this.faceCorners = faceCorners is List<FaceCorner> faceCornersList ? faceCornersList : new List<FaceCorner>(faceCorners);
            attributeManager = new AttributeManager(this);

            var materialIndicesList = materialIndices is List<int> indicesList ? indicesList : materialIndices.ToList();
            var vertexPositionsList = vertexPositions is List<float3> positionsList ? positionsList : vertexPositions.ToList();
            var uvsList = uvs is List<float2> uvList ? uvList : uvs.ToList();
            
            if (submeshCount == -1) submeshCount = materialIndicesList.Max() + 1;
            this.submeshCount = submeshCount;
            
            vertexPositionsList.ForEach(_ => vertices.Add(new Vertex()));
            FillElementMetadata();
            
            FillBuiltinAttributes(vertexPositionsList, uvsList, creases, faceNormals, materialIndicesList, smoothShaded);
        }

        public GeometryData(Mesh mesh, float duplicateDistanceThreshold, float duplicateNormalAngleThreshold) {
            using var method = Profiler.ProfileMethod();
            submeshCount = mesh.subMeshCount;
            var vertices = new List<float3>(mesh.vertices.Select(vertex => new float3(vertex.x, vertex.y, vertex.z)));
            var meshUvs = new List<float2>(mesh.uv.Select(uv => new float2(uv.x, uv.y)));

            var faceNormals = new List<float3>();
            var uvs = new List<float2>();
            var faceMaterialIndices = new List<int>();
            var faceSmoothShaded = new List<bool>();

            BuildMetadata(mesh, vertices, meshUvs, /*out*/faceNormals, /*out*/uvs, /*out*/faceMaterialIndices, /*out*/faceSmoothShaded, 
                          duplicateDistanceThreshold, duplicateNormalAngleThreshold);

            attributeManager = new AttributeManager(this);
            FillBuiltinAttributes(vertices, uvs, new float[edges.Count], faceNormals, faceMaterialIndices, faceSmoothShaded);

        }

        [Pure] public bool HasAttribute(string name) => attributeManager.HasAttribute(name);
        [Pure] public bool HasAttribute(string name, AttributeType type) => attributeManager.HasAttribute(name, type);
        [Pure] public bool HasAttribute(string name, AttributeDomain domain) => attributeManager.HasAttribute(name, domain);
        [Pure] public bool HasAttribute(string name, AttributeType type, AttributeDomain domain) => attributeManager.HasAttribute(name, type, domain);
        
        [CanBeNull, MustUseReturnValue]
        public BaseAttribute GetAttribute(string name) {
            return attributeManager.Request(name);
        }

        [CanBeNull, MustUseReturnValue]
        public BaseAttribute GetAttribute(string name, AttributeDomain domain) {
            return attributeManager.Request(name, domain);
        }
        
        [CanBeNull, MustUseReturnValue]
        public BaseAttribute GetAttribute(string name, AttributeType type, AttributeDomain domain) {
            return attributeManager.Request(name, type, domain);
        }

        [CanBeNull, MustUseReturnValue]
        public TAttribute GetAttribute<TAttribute>(string name) where TAttribute : BaseAttribute {
            return (TAttribute)attributeManager.Request(name, AttributeUtility.SystemTypeToAttributeType(typeof(TAttribute)));
        }

        [CanBeNull, MustUseReturnValue]
        public TAttribute GetAttribute<TAttribute>(string name, AttributeDomain domain) where TAttribute : BaseAttribute {
            return (TAttribute)attributeManager.Request(name, AttributeUtility.SystemTypeToAttributeType(typeof(TAttribute)), domain);
        }
        
        [NotNull, MustUseReturnValue]
        public TAttribute GetAttributeOrDefault<TAttribute, T>(string name, AttributeDomain domain, T defaultValue) where TAttribute : BaseAttribute<T> {
            if (HasAttribute(name, domain)) return GetAttribute<TAttribute>(name, domain)!;
            var attribute = Enumerable.Repeat(defaultValue, domain switch {
                AttributeDomain.Vertex => vertices.Count,
                AttributeDomain.Edge => edges.Count,
                AttributeDomain.Face => faces.Count,
                AttributeDomain.FaceCorner => faceCorners.Count,
                _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, null)
            }).Into<TAttribute>(name, domain);
            return attribute;
        }

        public IEnumerable<BaseAttribute> GetAttributes(AttributeDomain domain) {
            return domain switch {
                AttributeDomain.Vertex => attributeManager.VertexAttributes.Values,
                AttributeDomain.Edge => attributeManager.EdgeAttributes.Values,
                AttributeDomain.Face => attributeManager.FaceAttributes.Values,
                AttributeDomain.FaceCorner => attributeManager.FaceCornerAttributes.Values,
                _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, null)
            };
        }

        public bool StoreAttribute(BaseAttribute attribute) {
            return attributeManager.Store(attribute);
        }
        
        public bool StoreAttribute(BaseAttribute attribute, AttributeDomain targetDomain) {
            return attributeManager.Store(attribute, targetDomain);
        }

        private void FillBuiltinAttributes(
            IEnumerable<float3> vertices, List<float2> uvs, IEnumerable<float> creases,
            IEnumerable<float3> faceNormals, IEnumerable<int> faceMaterialIndices, IEnumerable<bool> faceSmoothShaded
        ) {
            using var method = Profiler.ProfileMethod();
            attributeManager.Store(vertices.Into<Vector3Attribute>("position", AttributeDomain.Vertex));

            attributeManager.Store(faceNormals.Into<Vector3Attribute>("normal", AttributeDomain.Face));
            attributeManager.Store(faceMaterialIndices.Into<IntAttribute>("material_index", AttributeDomain.Face));
            attributeManager.Store(faceSmoothShaded.Into<BoolAttribute>("shade_smooth", AttributeDomain.Face));

            attributeManager.Store(creases.Into<ClampedFloatAttribute>("crease", AttributeDomain.Edge));

            if (uvs.Count > 0) attributeManager.Store(uvs.Into<Vector2Attribute>("uv", AttributeDomain.FaceCorner));
        }

        private void BuildMetadata(
            Mesh mesh, List<float3> vertices, List<float2> uvs, 
            /*out*/ List<float3> faceNormals, List<float2> correctUvs, List<int> faceMaterialIndices, List<bool> smoothShaded, 
            float duplicateDistanceThreshold, float duplicateNormalAngleThreshold
        ) {
            using var method = Profiler.ProfileMethod();
            edges = new List<Edge>(mesh.triangles.Length);
            faces = new List<Face>(mesh.triangles.Length / 3);
            faceCorners = new List<FaceCorner>(mesh.triangles.Length);
            BuildElements(mesh, vertices, uvs, faceNormals, correctUvs, faceMaterialIndices, smoothShaded);
            RemoveDuplicates(vertices, faceNormals, duplicateDistanceThreshold, duplicateNormalAngleThreshold);

            this.vertices = new List<Vertex>(vertices.Count);
            for (var i = 0; i < vertices.Count; i++) {
                this.vertices.Add(new Vertex());
            }

            FillElementMetadata();

            Cleanup();
        }

        private void Cleanup() {
            for (var i = 0; i < edges.Count; i++) {
                edges[i].SelfIndex = i;
            }
        }

        private void BuildElements(
            Mesh mesh, List<float3> vertices, List<float2> uvs, 
            List<float3> faceNormals, List<float2> correctUvs, List<int> materialIndices, List<bool> smoothShaded
        ) {
            using var method = Profiler.ProfileMethod();
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
                    faceNormals.Add(computedNormal);
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
            using var method = Profiler.ProfileMethod();
            var duplicates = GetDuplicateEdges(vertices, faceNormals, duplicateDistanceThreshold * duplicateDistanceThreshold, duplicateNormalAngleThreshold);
            
            /*unique vertex => [duplicate vertices]*/
            var duplicateVerticesMap = GetDuplicateVerticesMap(duplicates);
            
            /*duplicate vertex => unique vertex*/
            var reverseDuplicatesMap = RemoveInvalidDuplicates(duplicateVerticesMap);
            RemapDuplicateElements(vertices, duplicates, reverseDuplicatesMap);
            RemoveDuplicateElements(vertices, duplicates, reverseDuplicatesMap);
            CheckForErrors(vertices.Count);
        }

        private void FillElementMetadata() {
            using var method = Profiler.ProfileMethod();
            // Face Corner Metadata ==> Backing Vertex
            FillFaceCornerMetadata();
        
            // Vertex Metadata ==> Edges, Faces, FaceCorners
            FillVertexMetadata();

            // Face Metadata ==> Adjacent faces
            FillFaceMetadata();
        }

        private List<(int, int, bool)> GetDuplicateEdges(List<float3> vertices, List<float3> faceNormals, float sqrDistanceThreshold, float duplicateNormalAngleThreshold) {
            using var method = Profiler.ProfileMethod();
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
            using var method = Profiler.ProfileMethod();
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
            using var method = Profiler.ProfileMethod();
            // Remove trivial invalid entries
            var removalList = new List<int>();
            foreach (var pair in duplicateVerticesMap) {
                pair.Value.RemoveAll(value => value == pair.Key);
                if (pair.Value.Count == 0) removalList.Add(pair.Key);
            }

            removalList.ForEach(key => duplicateVerticesMap.Remove(key));

            // Remove remaining invalid entries by joining all duplicate entries
            // For example (1=>{2, 3}, 3=>{7,12}, 4=>{12, 14}) becomes (1=>{2, 3, 4, 7, 8, 12, 14})
            var sortedKeys = duplicateVerticesMap.Keys.ToList().QuickSorted();
            var existsMap = new Dictionary<int, int>(); // maps a vertex index to the actual map
            var actualMap = new Dictionary<int, HashSet<int>>();

            foreach (var sortedKey in sortedKeys) {
                if (existsMap.ContainsKey(sortedKey)) {
                    var target = existsMap[sortedKey];
                    if (actualMap.ContainsKey(target)) {
                        actualMap[target].AddRange(duplicateVerticesMap[sortedKey]);
                        duplicateVerticesMap[sortedKey].ForEach(i => existsMap[i] = target);
                        continue;
                    }
                }
                
                var valueExists = duplicateVerticesMap[sortedKey].FirstOrGivenDefault(i => existsMap.ContainsKey(i), -1);
                if (valueExists != -1) {
                    var target = existsMap[valueExists]; 
                    actualMap[target].Add(sortedKey);
                    actualMap[target].AddRange(duplicateVerticesMap[sortedKey]);
                    existsMap[sortedKey] = target;
                    duplicateVerticesMap[sortedKey].ForEach(i => existsMap[i] = target);
                } else {
                    actualMap[sortedKey] = new HashSet<int>(duplicateVerticesMap[sortedKey]);
                    existsMap[sortedKey] = sortedKey;
                    duplicateVerticesMap[sortedKey].ForEach(i => existsMap[i] = sortedKey);    
                }
            }
            
            // Copy actualMap to duplicateVerticesMap
            duplicateVerticesMap.Clear();
            foreach (var pair in actualMap) {
                duplicateVerticesMap[pair.Key] = new List<int>(pair.Value);
                existsMap.Remove(pair.Key);
            }

            return existsMap;
        }
        
        private void RemapDuplicateElements(List<float3> vertices, List<(int, int, bool)> duplicates, Dictionary<int, int> reverseDuplicatesMap) {
            using var method = Profiler.ProfileMethod();
            // Remap the vertex indices for faces and edges
            var edgeReverseMap = new Dictionary<int, int>();
            foreach (var duplicate in duplicates) {
                edgeReverseMap[duplicate.Item2] = duplicate.Item1;
            }

            var allEdgeIndices = faces.SelectMany(face => new[] { face.EdgeA, face.EdgeB, face.EdgeC }).Distinct().Except(edgeReverseMap.Keys).ToList().InsertionSorted();
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
            } else {
                face.VertA = vertexIndexRemap[face.VertA];
            }

            if (reverseDuplicateMap.ContainsKey(face.VertB)) {
                face.VertB = vertexIndexRemap[reverseDuplicateMap[face.VertB]];
            } else {
                face.VertB = vertexIndexRemap[face.VertB];
            }

            if (reverseDuplicateMap.ContainsKey(face.VertC)) {
                face.VertC = vertexIndexRemap[reverseDuplicateMap[face.VertC]];
            } else {
                face.VertC = vertexIndexRemap[face.VertC];
            }

            if (edgeReverseMap.ContainsKey(face.EdgeA)) {
                face.EdgeA = edgeIndexRemap[edgeReverseMap[face.EdgeA]];
            } else {
                face.EdgeA = edgeIndexRemap[face.EdgeA];
            }

            if (edgeReverseMap.ContainsKey(face.EdgeB)) {
                face.EdgeB = edgeIndexRemap[edgeReverseMap[face.EdgeB]];
            } else {
                face.EdgeB = edgeIndexRemap[face.EdgeB];
            }

            if (edgeReverseMap.ContainsKey(face.EdgeC)) {
                face.EdgeC = edgeIndexRemap[edgeReverseMap[face.EdgeC]];
            } else {
                face.EdgeC = edgeIndexRemap[face.EdgeC];
            }
        }

        private void RemapEdge(int edgeIndex, Dictionary<int, int> reverseDuplicateMap, Dictionary<int, int> vertexRemap) {
            var edge = edges[edgeIndex];
            if (reverseDuplicateMap.ContainsKey(edge.VertA)) {
                edge.VertA = vertexRemap[reverseDuplicateMap[edge.VertA]];
            } else {
                edge.VertA = vertexRemap[edge.VertA];
            }

            if (reverseDuplicateMap.ContainsKey(edge.VertB)) {
                edge.VertB = vertexRemap[reverseDuplicateMap[edge.VertB]];
            } else {
                edge.VertB = vertexRemap[edge.VertB];
            }
        }

        private void RemoveDuplicateElements(List<float3> vertices, List<(int, int, bool)> duplicates, Dictionary<int, int> reverseDuplicatesMap) {
            using var method = Profiler.ProfileMethod();
            // Remove duplicate edges
            foreach (var edge in duplicates.Select(t=>edges[t.Item2]).ToList()) {
                edges.Remove(edge);
            }

            // Remove duplicate vertices
            var sortedDuplicateVertices = reverseDuplicatesMap.Keys.ToList().QuickSorted((i, i1) => i1.CompareTo(i));
            foreach (var vertexIndex in sortedDuplicateVertices) {
                vertices.RemoveAt(vertexIndex);
            }
        }

        private void CheckForErrors(int vertexCount) {
            using var method = Profiler.ProfileMethod();
            // Check if there are any invalid`edges
            for (var i = 0; i < edges.Count; i++) {
                var edge = edges[i];
                if (edge.VertA >= vertexCount || edge.VertB >= vertexCount) {
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

                if (face.VertA >= vertexCount || face.VertB >= vertexCount || face.VertC >= vertexCount) {
                    Debug.LogError($"Face at index {i} contains invalid vertices");
                }
            }
        }

        private void FillVertexMetadata() {
            using var method = Profiler.ProfileMethod();
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
            using var method = Profiler.ProfileMethod();
            for (var i = 0; i < faces.Count; i++) {
                var face = faces[i];
                var edgeA = edges[face.EdgeA];
                var edgeB = edges[face.EdgeB];
                var edgeC = edges[face.EdgeC];
                face.AdjacentFaces.AddRange(new[] { edgeA.FaceA, edgeA.FaceB, edgeB.FaceA, edgeB.FaceB, edgeC.FaceA, edgeC.FaceB });

                // Cleanup
                face.AdjacentFaces.RemoveDuplicates();
                face.AdjacentFaces.RemoveAll(adjacentIndex => /*self*/adjacentIndex == i || /*no face*/adjacentIndex == -1);
            }
        }

        private void FillFaceCornerMetadata() {
            using var method = Profiler.ProfileMethod();
            foreach (var face in faces) {
                var fcA = faceCorners[face.FaceCornerA];
                var fcB = faceCorners[face.FaceCornerB];
                var fcC = faceCorners[face.FaceCornerC];

                fcA.Vert = face.VertA;
                fcB.Vert = face.VertB;
                fcC.Vert = face.VertC;
            }
        }

        object ICloneable.Clone() {
            return Clone();
        }

        public GeometryData Clone() {
            // TODO: Should probably write a proper Clone method some time
            var clone = GeometryData.Empty;
            GeometryData.Merge(clone, this);
            return clone;
        }
    }
}