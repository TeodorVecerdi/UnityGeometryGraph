using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.AttributeSystem;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;

namespace GeometryGraph.Runtime.Geometry {
    [Serializable]
    public partial class GeometryData : ICloneable, ISerializationCallbackReceiver {
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
            
            FillBuiltinAttributes(new List<float3>(), new List<float2>(), new List<float3>(), new List<int>(), new List<bool>());
            
            submeshCount = 0;
        }

        public GeometryData(IEnumerable<Edge> edges, IEnumerable<Face> faces, IEnumerable<FaceCorner> faceCorners, int submeshCount,
                             IEnumerable<float3> vertexPositions, IEnumerable<float3> faceNormals, IEnumerable<int> materialIndices,
                             IEnumerable<bool> smoothShaded, IEnumerable<float2> uvs) {
            vertices = new List<Vertex>();
            this.edges = edges is List<Edge> edgeList ? edgeList : new List<Edge>(edges);
            this.faces = faces is List<Face> facesList ? facesList : new List<Face>(faces);
            this.faceCorners = faceCorners is List<FaceCorner> faceCornersList ? faceCornersList : new List<FaceCorner>(faceCorners);
            attributeManager = new AttributeManager(this);

            List<int> materialIndicesList = materialIndices is List<int> indicesList ? indicesList : materialIndices.ToList();
            List<float3> vertexPositionsList = vertexPositions is List<float3> positionsList ? positionsList : vertexPositions.ToList();
            List<float2> uvsList = uvs is List<float2> uvList ? uvList : uvs.ToList();
            List<float3> faceNormalsList = faceNormals is List<float3> faceNormalList ? faceNormalList : faceNormals.ToList();
            List<bool> smoothShadedList = smoothShaded is List<bool> smoothShadeList ? smoothShadeList : smoothShaded.ToList();
            
            if (submeshCount == -1) submeshCount = materialIndicesList.Max() + 1;
            this.submeshCount = submeshCount;
            
            vertexPositionsList.ForEach(_ => vertices.Add(new Vertex()));
            FillElementMetadata();
            
            FillBuiltinAttributes(vertexPositionsList, uvsList, faceNormalsList, materialIndicesList, smoothShadedList);
        }
        
        
        object ICloneable.Clone() {
            return Clone();
        }

        public GeometryData Clone() {
            // TODO/NOTE: Should probably write a proper Clone method some time
            GeometryData clone = GeometryData.Empty;
            GeometryData.Merge(clone, this);
            return clone;
        }
        
        public static GeometryData Empty => new();

        private void FillBuiltinAttributes(
            List<float3> vertices, List<float2> uvs,
            List<float3> faceNormals, List<int> faceMaterialIndices, List<bool> faceSmoothShaded
        ) {
            using IDisposable method = Profiler.ProfileMethod();
            if(vertices.Count > 0) attributeManager.Store(vertices.Into<Vector3Attribute>(AttributeId.Position, AttributeDomain.Vertex));

            if(faceNormals.Count > 0) attributeManager.Store(faceNormals.Into<Vector3Attribute>(AttributeId.Normal, AttributeDomain.Face));
            if(faceMaterialIndices.Count > 0) attributeManager.Store(faceMaterialIndices.Into<IntAttribute>(AttributeId.Material, AttributeDomain.Face));
            if(faceSmoothShaded.Count > 0) attributeManager.Store(faceSmoothShaded.Into<BoolAttribute>(AttributeId.ShadeSmooth, AttributeDomain.Face));

            if (uvs.Count > 0) attributeManager.Store(uvs.Into<Vector2Attribute>(AttributeId.UV, AttributeDomain.FaceCorner));
        }

        private void Cleanup() {
            for (int i = 0; i < edges.Count; i++) {
                edges[i].SelfIndex = i;
            }
        }

        private void FillElementMetadata() {
            using IDisposable method = Profiler.ProfileMethod();
            // Face Corner Metadata ==> Backing Vertex
            FillFaceCornerMetadata();
        
            // Vertex Metadata ==> Edges, Faces, FaceCorners
            FillVertexMetadata();

            // Face Metadata ==> Adjacent faces
            FillFaceMetadata();
        }
        
        private void FillVertexMetadata() {
            using IDisposable method = Profiler.ProfileMethod();
            // Edges
            for (int i = 0; i < edges.Count; i++) {
                Edge edge = edges[i];
                vertices[edge.VertA].Edges.Add(i);
                vertices[edge.VertB].Edges.Add(i);
            }

            //Faces
            for (int i = 0; i < faces.Count; i++) {
                Face face = faces[i];
                vertices[face.VertA].Faces.Add(i);
                vertices[face.VertB].Faces.Add(i);
                vertices[face.VertC].Faces.Add(i);
            }
        
            // Face Corners
            for (int i = 0; i < faceCorners.Count; i++) {
                FaceCorner fc = faceCorners[i];
                vertices[fc.Vert].FaceCorners.Add(i);
            }

            // Cleanup
            foreach (Vertex vertex in vertices) {
                vertex.Edges.RemoveDuplicates();
                vertex.Faces.RemoveDuplicates();
                vertex.FaceCorners.RemoveDuplicates();
            }
        }

        private void FillFaceMetadata() {
            using IDisposable method = Profiler.ProfileMethod();
            for (int i = 0; i < faces.Count; i++) {
                Face face = faces[i];
                Edge edgeA = edges[face.EdgeA];
                Edge edgeB = edges[face.EdgeB];
                Edge edgeC = edges[face.EdgeC];
                face.AdjacentFaces.AddRange(new[] { edgeA.FaceA, edgeA.FaceB, edgeB.FaceA, edgeB.FaceB, edgeC.FaceA, edgeC.FaceB });

                // Cleanup
                face.AdjacentFaces.RemoveDuplicates();
                face.AdjacentFaces.RemoveAll(adjacentIndex => /*self*/adjacentIndex == i || /*no face*/adjacentIndex == -1);
            }
        }

        private void FillFaceCornerMetadata() {
            using IDisposable method = Profiler.ProfileMethod();
            foreach (Face face in faces) {
                FaceCorner fcA = faceCorners[face.FaceCornerA];
                FaceCorner fcB = faceCorners[face.FaceCornerB];
                FaceCorner fcC = faceCorners[face.FaceCornerC];

                fcA.Vert = face.VertA;
                fcB.Vert = face.VertB;
                fcC.Vert = face.VertC;
            }
        }
        
        #region HashCode Implementations

        private int CalculateHashCodeElementCount() {
            return HashCode.Combine(vertices.Count, edges.Count, faces.Count, faceCorners.Count);
        }

        private int CalculateHashCodeAttributeCount() {
            return HashHelpers.Combine(
                CalculateHashCodeElementCount(),
                HashCode.Combine(
                    attributeManager.VertexAttributes.Count,
                    attributeManager.EdgeAttributes.Count,
                    attributeManager.FaceAttributes.Count,
                    attributeManager.FaceCornerAttributes.Count
                )
            );
        }

        private int CalculateHashCodeAttributeValues() {
            // TODO(#19): Implement CalculateHashCodeAttributeValues
            // Located in `GeometryData.cs`
            return 0;
        }
            
        private int CalculateHashCodeFull() {
            // TODO: Implement CalculateHashCodeFull
            // Located in `GeometryData.cs`
            return 0;
        }

        #endregion

        public void OnBeforeSerialize() {
        }

        public void OnAfterDeserialize() {
            attributeManager.SetOwner(this);
        }
    }
}
