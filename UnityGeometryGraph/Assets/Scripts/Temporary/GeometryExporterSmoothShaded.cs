using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeometryGraph.Runtime.Attribute;
using GeometryGraph.Runtime.Data;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GeometryGraph.Runtime.Geometry {
    public class GeometryExporterSmoothShaded : SerializedMonoBehaviour {
        [SerializeField] private MeshFilter target;
        [SerializeField, OnStateUpdate(nameof(__OnStateUpdate_GeometrySource))] private IGeometryProvider source;

        [SerializeField] private Mesh mesh;
        [SerializeField] private float normalAngleThreshold = 0.1f;
        [ReadOnly, SerializeField] private int sharedFaceCount;
        [ShowInInspector] private int vertexCount => vertices.Count;

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

        private GeometryData geometry => source?.Geometry;

        [Button]
        public void ExportSmoothShaded() {
            if (target == null || source == null || source.Geometry == null) {
                Debug.LogError("Target MeshFilter or Source is null");
                return;
            }

            var sw = Stopwatch.StartNew();

            PrepareMesh();
            sharedFaceCount = 0;
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
                    normal0 = math.normalize(geometry.Vertices[face.VertA].Faces.Select(i => normalAttr[i]).Aggregate((n1, n2) => n1 + n2));
                    normal1 = math.normalize(geometry.Vertices[face.VertB].Faces.Select(i => normalAttr[i]).Aggregate((n1, n2) => n1 + n2));
                    normal2 = math.normalize(geometry.Vertices[face.VertC].Faces.Select(i => normalAttr[i]).Aggregate((n1, n2) => n1 + n2));
                }
                
                var (t0, t1, t2) = AddFace(faceIndex, normal0, normal1, normal2);
                var triangleOffset = vertices.Count;

                if (sharedA != -1) {
                    exportedFaces.Add(sharedA);
                    AddAdjacentFace(sharedA, face.VertA, face.VertB, triangleOffset, t1, t0);
                    triangleOffset++;
                    sharedFaceCount++;
                }

                if (sharedB != -1) {
                    exportedFaces.Add(sharedB);
                    AddAdjacentFace(sharedB, face.VertB, face.VertC, triangleOffset, t2, t1);
                    triangleOffset++;
                    sharedFaceCount++;
                }
                
                if (sharedC != -1) {
                    exportedFaces.Add(sharedC);
                    AddAdjacentFace(sharedC, face.VertC, face.VertA, triangleOffset, t0, t2);
                    sharedFaceCount++;
                }
            }
            
            ApplyMesh();
            
            Debug.Log(sw.Elapsed.TotalMilliseconds);
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
            triangles[submesh].Add(t0);
            triangles[submesh].Add(t1);
            triangles[submesh].Add(t2);
            
            return (t0, t1, t2);
        }

        private void PrepareMesh() {
            if (mesh == null) mesh = target.sharedMesh;
            if (mesh == null) {
                mesh = new Mesh { name = "Exported Mesh" };
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
            // if (sharedFaceIndex != -1 && normalAttr[sharedFaceIndex].Equals( normal)) {
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
                : math.normalize(geometry.Vertices[otherVertexIndex].Faces.Select(i => normalAttr[i]).Aggregate((n1, n2) => n1 + n2));

            var submesh = submeshAttr[sharedA];
            vertices.Add(otherVertex);
            normals.Add(normal);
            uvs.Add(otherUV);
                    
            triangles[submesh].Add(triangle0);
            triangles[submesh].Add(triangle1);
            triangles[submesh].Add(triangle2);
        }
        
        private void __OnStateUpdate_GeometrySource() {
            if(source == null) return;
            if (source != null && (Object)source == null) source = null;
            if (!(Object)source) source = null;
        }
    }
}