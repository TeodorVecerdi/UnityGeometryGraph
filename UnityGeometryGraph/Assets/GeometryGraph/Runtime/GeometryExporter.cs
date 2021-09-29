using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attribute;
using GeometryGraph.Runtime.Data;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace GeometryGraph.Runtime.Geometry {
    public class GeometryExporter : MonoBehaviour, IGeometryProvider {
        [SerializeField] private MeshFilter target;

        [SerializeField] private Mesh mesh;
        [SerializeField] private float normalAngleThreshold = 0.05f;

        private Vector3Attribute positionAttr;
        private Vector3Attribute normalAttr;
        private Vector2Attribute uvAttr;
        private IntAttribute submeshAttr;
        private BoolAttribute shadeSmoothAttr;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<List<int>> triangles = new List<List<int>>();
        private HashSet<int> exportedFaces = new HashSet<int>();
        [ShowInInspector] private GeometryData geometry;

        public GeometryData Geometry => geometry;
        public Matrix4x4 LocalToWorldMatrix => transform.localToWorldMatrix;

        public void Export(GeometryData geometry) {
            if (target == null || geometry == null) {
                Debug.LogError("Target MeshFilter or Source is null");
                return;
            }

            this.geometry = geometry;

            PrepareMesh();
            positionAttr = geometry.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            normalAttr = geometry.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face);
            uvAttr = geometry.GetAttribute<Vector2Attribute>("uv", AttributeDomain.FaceCorner);
            submeshAttr = geometry.GetAttribute<IntAttribute>("material_index", AttributeDomain.Face);
            shadeSmoothAttr = geometry.GetAttribute<BoolAttribute>("shade_smooth", AttributeDomain.Face);

            for (var faceIndex = 0; faceIndex < geometry.Faces.Count; faceIndex++) {
                if(exportedFaces.Contains(faceIndex)) continue;
                exportedFaces.Add(faceIndex);
                
                var faceNormal = normalAttr[faceIndex];
                var face = geometry.Faces[faceIndex];
                
                // Get shared faces' indices, -1 if no adjacent face or if normal doesn't match 
                var sharedA = GetSharedFace(faceIndex, face.EdgeA, faceNormal);
                var sharedB = GetSharedFace(faceIndex, face.EdgeB, faceNormal);
                var sharedC = GetSharedFace(faceIndex, face.EdgeC, faceNormal);

                var normal0 = faceNormal;
                var normal1 = faceNormal;
                var normal2 = faceNormal;

                if (shadeSmoothAttr[faceIndex]) {
                    normal0 = math.normalize(geometry.Vertices[face.VertA].Faces.Where(i => shadeSmoothAttr[i]).Select(i => normalAttr[i]).Aggregate((n1, n2) => n1 + n2));
                    normal1 = math.normalize(geometry.Vertices[face.VertB].Faces.Where(i => shadeSmoothAttr[i]).Select(i => normalAttr[i]).Aggregate((n1, n2) => n1 + n2));
                    normal2 = math.normalize(geometry.Vertices[face.VertC].Faces.Where(i => shadeSmoothAttr[i]).Select(i => normalAttr[i]).Aggregate((n1, n2) => n1 + n2));
                }
                
                var (t0, t1, t2) = AddFace(faceIndex, normal0, normal1, normal2);
                var (triA0, triA1, triB0, triB1, triC0, triC1) = GetActualSharedTriangles(face, t0, t1, t2);
                var triangleOffset = vertices.Count;

                if (sharedA != -1) {
                    exportedFaces.Add(sharedA);
                    AddAdjacentFace(sharedA, geometry.Edges[face.EdgeA].VertA, geometry.Edges[face.EdgeA].VertB, triangleOffset, triA0, triA1);
                    triangleOffset++;
                }

                if (sharedB != -1) {
                    exportedFaces.Add(sharedB);
                    AddAdjacentFace(sharedB, geometry.Edges[face.EdgeB].VertA, geometry.Edges[face.EdgeB].VertB, triangleOffset, triB0, triB1);
                    triangleOffset++;
                }
                
                if (sharedC != -1) {
                    exportedFaces.Add(sharedC);
                    AddAdjacentFace(sharedC, geometry.Edges[face.EdgeC].VertA, geometry.Edges[face.EdgeC].VertB, triangleOffset, triC0, triC1);
                }
            }
            
            ApplyMesh();
        }

        private void ApplyMesh() {
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            for (var i = 0; i < triangles.Count; i++) {
                mesh.SetTriangles(triangles[i], i);
            }
            mesh.RecalculateTangents();
            mesh.Optimize();
        }

        private (int v0Triangle, int v1Triangle, int v2Triangle) AddFace(int faceIndex, float3 normal0, float3 normal1, float3 normal2) {
            var triangleOffset = vertices.Count;
            var face = geometry.Faces[faceIndex];
            var v0 = positionAttr[face.VertA];
            var v1 = positionAttr[face.VertB];
            var v2 = positionAttr[face.VertC];
            var uv0 = uvAttr[face.FaceCornerA];
            var uv1 = uvAttr[face.FaceCornerB];
            var uv2 = uvAttr[face.FaceCornerC];
            var t0 = 0 + triangleOffset;
            var t1 = 1 + triangleOffset;
            var t2 = 2 + triangleOffset;
            var submesh = submeshAttr[faceIndex];
            
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            normals.Add(normal0);
            normals.Add(normal1);
            normals.Add(normal2);
            uvs.Add(uv0);
            uvs.Add(uv1);
            uvs.Add(uv2);

            var faceNormal = normalAttr[faceIndex];
            var calculatedNormal = math.normalize(math.cross(vertices[t1] - vertices[t0], vertices[t2] - vertices[t0]));
            var eqNegX = Math.Abs(faceNormal.x) > 0.0001f && Math.Abs(faceNormal.x - -calculatedNormal.x) < 0.001f;
            var eqNegY = Math.Abs(faceNormal.y) > 0.0001f && Math.Abs(faceNormal.y - -calculatedNormal.y) < 0.001f;
            var eqNegZ = Math.Abs(faceNormal.z) > 0.0001f && Math.Abs(faceNormal.z - -calculatedNormal.z) < 0.001f;

            if (eqNegX || eqNegY || eqNegZ) {
                triangles[submesh].Add(t1);
                triangles[submesh].Add(t0);
                triangles[submesh].Add(t2);
            } else {
                triangles[submesh].Add(t0);
                triangles[submesh].Add(t1);
                triangles[submesh].Add(t2);
            }
            
            return (t0, t1, t2);
        }

        private void PrepareMesh() {
            if (mesh == null) mesh = target.sharedMesh;
            if (mesh == null) {
                mesh = new Mesh {
                    name = "Exported Mesh",
                    indexFormat = IndexFormat.UInt32
                };
                target.sharedMesh = mesh;
            }

            mesh.Clear();
            mesh.subMeshCount = geometry.SubmeshCount;
            vertices.Clear();
            normals.Clear();
            uvs.Clear();
            triangles.Clear();

            for (var i = 0; i < geometry.SubmeshCount; i++) {
                triangles.Add(new List<int>());
            }
            
            exportedFaces.Clear();
        }

        private int GetSharedFace(int faceIndex, int edgeIndex, float3 normal) {
            var edge = geometry.Edges[edgeIndex];
            var sharedFaceIndex = edge.FaceA != faceIndex ? edge.FaceA : edge.FaceB;
            
            if (exportedFaces.Contains(sharedFaceIndex)) return -1;
            if (sharedFaceIndex != -1 && math_util.angle(normalAttr[sharedFaceIndex], normal) < normalAngleThreshold) {
            // if (sharedFaceIndex != -1 && normalAttr[sharedFaceIndex].Equals(normal)) {
                return sharedFaceIndex;
            }

            return -1;
        }

        private void AddAdjacentFace(int sharedA, int vertexA, int vertexB, int triangle0, int triangle1, int triangle2) {
            int otherVertexIndex;
            int otherFaceCornerIndex;
            var sharedFace = geometry.Faces[sharedA];
            if (sharedFace.VertA != vertexA && sharedFace.VertA != vertexB) {
                otherVertexIndex = sharedFace.VertA;
                otherFaceCornerIndex = sharedFace.FaceCornerA;
            } else if (sharedFace.VertB != vertexA && sharedFace.VertB != vertexB) {
                otherVertexIndex = sharedFace.VertB;
                otherFaceCornerIndex = sharedFace.FaceCornerB;
            } else {
                otherVertexIndex = sharedFace.VertC;
                otherFaceCornerIndex = sharedFace.FaceCornerC;
            }
            
            var otherVertex = positionAttr[otherVertexIndex];
            var otherUV = uvAttr[otherFaceCornerIndex];
            var normal = !shadeSmoothAttr[sharedA] 
                ? normalAttr[sharedA]
                : math.normalize(geometry.Vertices[otherVertexIndex].Faces.Where(i => shadeSmoothAttr[i]).Select(i => normalAttr[i]).Aggregate((n1, n2) => n1 + n2));

            var submesh = submeshAttr[sharedA];
            vertices.Add(otherVertex);
            normals.Add(normal);
            uvs.Add(otherUV);

            var sharedFaceNormal = normalAttr[sharedA];
            var calculatedNormal = math.normalize(math.cross(vertices[triangle1] - vertices[triangle0], vertices[triangle2] - vertices[triangle0]));
            var eqNegX = Math.Abs(sharedFaceNormal.x) > 0.0001f && Math.Abs(sharedFaceNormal.x - -calculatedNormal.x) < 0.001f;
            var eqNegY = Math.Abs(sharedFaceNormal.y) > 0.0001f && Math.Abs(sharedFaceNormal.y - -calculatedNormal.y) < 0.001f;
            var eqNegZ = Math.Abs(sharedFaceNormal.z) > 0.0001f && Math.Abs(sharedFaceNormal.z - -calculatedNormal.z) < 0.001f;
            
            if (eqNegX || eqNegY || eqNegZ) {
                triangles[submesh].Add(triangle1);
                triangles[submesh].Add(triangle0);
                triangles[submesh].Add(triangle2);
            } else {
                triangles[submesh].Add(triangle0);
                triangles[submesh].Add(triangle1);
                triangles[submesh].Add(triangle2);
            }
        }
        
        private (int triA0, int triA1, int triB0, int triB1, int triC0, int triC1) GetActualSharedTriangles(GeometryData.Face face, int t0, int t1, int t2) {
            int triA0 = -1, triA1 = -1, triB0 = -1, triB1 = -1, triC0 = -1, triC1 = -1;
            if (geometry.Edges[face.EdgeA].VertA == face.VertA) triA0 = t0;
            if (geometry.Edges[face.EdgeA].VertA == face.VertB) triA0 = t1;
            if (geometry.Edges[face.EdgeA].VertA == face.VertC) triA0 = t2;
            if (geometry.Edges[face.EdgeA].VertB == face.VertA) triA1 = t0;
            if (geometry.Edges[face.EdgeA].VertB == face.VertB) triA1 = t1;
            if (geometry.Edges[face.EdgeA].VertB == face.VertC) triA1 = t2;
            if (geometry.Edges[face.EdgeB].VertA == face.VertA) triB0 = t0;
            if (geometry.Edges[face.EdgeB].VertA == face.VertB) triB0 = t1;
            if (geometry.Edges[face.EdgeB].VertA == face.VertC) triB0 = t2;
            if (geometry.Edges[face.EdgeB].VertB == face.VertA) triB1 = t0;
            if (geometry.Edges[face.EdgeB].VertB == face.VertB) triB1 = t1;
            if (geometry.Edges[face.EdgeB].VertB == face.VertC) triB1 = t2;
            if (geometry.Edges[face.EdgeC].VertA == face.VertA) triC0 = t0;
            if (geometry.Edges[face.EdgeC].VertA == face.VertB) triC0 = t1;
            if (geometry.Edges[face.EdgeC].VertA == face.VertC) triC0 = t2;
            if (geometry.Edges[face.EdgeC].VertB == face.VertA) triC1 = t0;
            if (geometry.Edges[face.EdgeC].VertB == face.VertB) triC1 = t1;
            if (geometry.Edges[face.EdgeC].VertB == face.VertC) triC1 = t2;
            return (triA0, triA1, triB0, triB1, triC0, triC1);
        }

    }
}