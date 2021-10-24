using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attribute;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;

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
            
            FillBuiltinAttributes(new List<float3>(), new List<float2>(), new List<float>(), new List<float3>(), new List<int>(), new List<bool>());
            
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
            var creasesList = creases is List<float> creaseList ? creaseList : creases.ToList();
            var faceNormalsList = faceNormals is List<float3> faceNormalList ? faceNormalList : faceNormals.ToList();
            var smoothShadedList = smoothShaded is List<bool> smoothShadeList ? smoothShadeList : smoothShaded.ToList();
            
            if (submeshCount == -1) submeshCount = materialIndicesList.Max() + 1;
            this.submeshCount = submeshCount;
            
            vertexPositionsList.ForEach(_ => vertices.Add(new Vertex()));
            FillElementMetadata();
            
            FillBuiltinAttributes(vertexPositionsList, uvsList, creasesList, faceNormalsList, materialIndicesList, smoothShadedList);
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
        
        public static GeometryData Empty => new GeometryData();

        private void FillBuiltinAttributes(
            List<float3> vertices, List<float2> uvs, List<float> creases,
            List<float3> faceNormals, List<int> faceMaterialIndices, List<bool> faceSmoothShaded
        ) {
            using var method = Profiler.ProfileMethod();
            if(vertices.Count > 0) attributeManager.Store(vertices.Into<Vector3Attribute>("position", AttributeDomain.Vertex));

            if(faceNormals.Count > 0) attributeManager.Store(faceNormals.Into<Vector3Attribute>("normal", AttributeDomain.Face));
            if(faceMaterialIndices.Count > 0) attributeManager.Store(faceMaterialIndices.Into<IntAttribute>("material_index", AttributeDomain.Face));
            if(faceSmoothShaded.Count > 0) attributeManager.Store(faceSmoothShaded.Into<BoolAttribute>("shade_smooth", AttributeDomain.Face));

            if(creases.Count > 0) attributeManager.Store(creases.Into<ClampedFloatAttribute>("crease", AttributeDomain.Edge));

            if (uvs.Count > 0) attributeManager.Store(uvs.Into<Vector2Attribute>("uv", AttributeDomain.FaceCorner));
        }

        private void Cleanup() {
            for (var i = 0; i < edges.Count; i++) {
                edges[i].SelfIndex = i;
            }
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
    }
}