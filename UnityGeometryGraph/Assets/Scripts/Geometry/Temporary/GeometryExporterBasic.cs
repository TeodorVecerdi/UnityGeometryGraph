using System.Collections.Generic;
using Attribute;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry {
    public class GeometryExporterBasic : MonoBehaviour {
        [SerializeField] private MeshFilter target;
        [SerializeField] private GeometryImporter source;

        [SerializeField] private Mesh mesh;

        private Vector3Attribute positionAttr;
        private Vector3Attribute normalAttr;
        private Vector2Attribute uvAttr;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<int> triangles = new List<int>();
        private HashSet<int> exportedFaces = new HashSet<int>();

        private GeometryData geometry => source?.Geometry;

        [Button]
        public void ExportBasic() {
            if (target == null || source == null || source.Geometry == null) {
                Debug.LogError("Target MeshFilter or Source is null");
                return;
            }

            /* ALGO
             For each face, add a face in resulting mesh.
             
             How to share vertices in triangles? 
                - Maybe keep track of already done faces, then create triangles out of adjacent faces as well if their normals are the same
                - Then each loop only do the face (and adjacent faces) if they haven't already been done
            */

            PrepareMesh();
            
            positionAttr = geometry.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            normalAttr = geometry.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face);
            uvAttr = geometry.GetAttribute<Vector2Attribute>("uv", AttributeDomain.FaceCorner);

            for (var faceIndex = 0; faceIndex < geometry.Faces.Count; faceIndex++) {
                if(exportedFaces.Contains(faceIndex)) continue;
                exportedFaces.Add(faceIndex);
                
                var normal = normalAttr[faceIndex];
                var face = geometry.Faces[faceIndex];
                
                // Get shared faces, -1 if no adjacent face or if normal doesn't match 
                var sharedA = GetSharedFace(faceIndex, face.EdgeA, normal);
                var sharedB = GetSharedFace(faceIndex, face.EdgeB, normal);
                var sharedC = GetSharedFace(faceIndex, face.EdgeC, normal);

                var (t0, t1, t2) = AddFace(face, normal);
                var triangleOffset = vertices.Count;

                if (sharedA != -1) {
                    exportedFaces.Add(sharedA);
                    AddAdjacentFace(sharedA, face.VertA, face.VertB, triangleOffset, t1, t0, normal);
                    triangleOffset++;
                }

                if (sharedB != -1) {
                    exportedFaces.Add(sharedB);
                    AddAdjacentFace(sharedB, face.VertB, face.VertC, triangleOffset, t2, t1, normal);
                    triangleOffset++;
                }
                
                if (sharedC != -1) {
                    exportedFaces.Add(sharedC);
                    AddAdjacentFace(sharedC, face.VertC, face.VertA, triangleOffset, t0, t2, normal);
                }
            }
            
            ApplyMesh();
        }

        private void ApplyMesh() {
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents();
            mesh.Optimize();
        }

        private (int v0Triangle, int v1Triangle, int v2Triangle) AddFace(GeometryData.Face face, float3 normal) {
            var triangleOffset = vertices.Count;
            var v0 = positionAttr[face.VertA];
            var v1 = positionAttr[face.VertB];
            var v2 = positionAttr[face.VertC];
            var uv0 = uvAttr[face.FaceCornerA];
            var uv1 = uvAttr[face.FaceCornerB];
            var uv2 = uvAttr[face.FaceCornerC];
            var t0 = 0 + triangleOffset;
            var t1 = 1 + triangleOffset;
            var t2 = 2 + triangleOffset;
            
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            uvs.Add(uv0);
            uvs.Add(uv1);
            uvs.Add(uv2);
            triangles.Add(t0);
            triangles.Add(t1);
            triangles.Add(t2);
            
            return (t0, t1, t2);
        }

        private void PrepareMesh() {
            if (mesh == null) mesh = target.sharedMesh;
            if (mesh == null) {
                mesh = new Mesh { name = $"{source.gameObject.name} Mesh" };
                target.sharedMesh = mesh;
            }

            mesh.Clear();
            
            vertices.Clear();
            normals.Clear();
            uvs.Clear();
            triangles.Clear();
            exportedFaces.Clear();
        }

        private int GetSharedFace(int faceIndex, int edgeIndex, float3 normal) {
            var edge = geometry.Edges[edgeIndex];
            var sharedFaceIndex = edge.FaceA != faceIndex ? edge.FaceA : edge.FaceB;
            
            if (exportedFaces.Contains(sharedFaceIndex)) return -1;
            if (sharedFaceIndex != -1 && normalAttr[sharedFaceIndex].Equals(normal)) {
                return sharedFaceIndex;
            }

            return -1;
        }

        private void AddAdjacentFace(int sharedA, int vertexA, int vertexB, int triangle0, int triangle1, int triangle2, float3 normal) {
            Vector3 otherVertex;
            Vector2 otherUV;
            
            var sharedFace = geometry.Faces[sharedA];
            if (sharedFace.VertA != vertexA && sharedFace.VertA != vertexB) {
                otherVertex = positionAttr[sharedFace.VertA];
                otherUV = uvAttr[sharedFace.FaceCornerA];
            } else if (sharedFace.VertB != vertexA && sharedFace.VertB != vertexB) {
                otherVertex = positionAttr[sharedFace.VertB];
                otherUV = uvAttr[sharedFace.FaceCornerB];
            } else {
                otherVertex = positionAttr[sharedFace.VertC];
                otherUV = uvAttr[sharedFace.FaceCornerC];
            }
            
            vertices.Add(otherVertex);
            normals.Add(normal);
            uvs.Add(otherUV);
                    
            triangles.Add(triangle0);
            triangles.Add(triangle1);
            triangles.Add(triangle2);
        }
    }
}