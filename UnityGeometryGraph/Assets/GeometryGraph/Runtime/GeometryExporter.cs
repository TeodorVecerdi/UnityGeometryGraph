using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.AttributeSystem;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Geometry {
    [Serializable]
    public class GeometryExporter {
        private Vector3Attribute positionAttr;
        private Vector3Attribute normalAttr;
        private Vector2Attribute uvAttr;
        private IntAttribute submeshAttr;
        private BoolAttribute shadeSmoothAttr;

        private List<Vector3> vertices = new();
        private List<Vector3> normals = new();
        private List<Vector2> uvs = new();
        private List<List<int>> triangles = new();
        private HashSet<int> exportedFaces = new();
        private GeometryData geometry;

        public void Export(GeometryData geometry, Mesh mesh) {
            DebugUtility.Log("Exporting Mesh");
            if (geometry == null) {
                DebugUtility.Log("Source GeometryData is null");
                ClearMesh(mesh);
                return;
            }

            this.geometry = geometry;
            ExportGeometryImpl(mesh);
        }

        private void ExportGeometryImpl(Mesh targetMesh) {
            ClearMesh(targetMesh);
            ClearLists();
            positionAttr = geometry.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            normalAttr = geometry.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face);
            uvAttr = geometry.GetAttribute<Vector2Attribute>("uv", AttributeDomain.FaceCorner);
            submeshAttr = geometry.GetAttribute<IntAttribute>("material_index", AttributeDomain.Face);
            shadeSmoothAttr = geometry.GetAttribute<BoolAttribute>("shade_smooth", AttributeDomain.Face);

            for (int faceIndex = 0; faceIndex < geometry.Faces.Count; faceIndex++) {
                if (exportedFaces.Contains(faceIndex)) continue;
                exportedFaces.Add(faceIndex);

                float3 faceNormal = normalAttr[faceIndex];
                GeometryData.Face face = geometry.Faces[faceIndex];

                // Get shared faces' indices, -1 if no adjacent face or if normal doesn't match
                // TODO(#12): Re-enable sharing vertices with adjacent quad-like faces
                // Disabled because UVs are not correct in some cases (GeometryExporter.cs)
                int sharedA = -1; // GetSharedFace(faceIndex, face.EdgeA, faceNormal);
                int sharedB = -1; // GetSharedFace(faceIndex, face.EdgeB, faceNormal);
                int sharedC = -1; // GetSharedFace(faceIndex, face.EdgeC, faceNormal);

                float3 normal0 = faceNormal;
                float3 normal1 = faceNormal;
                float3 normal2 = faceNormal;

                if (shadeSmoothAttr[faceIndex]) {
                    normal0 = math.normalize(geometry.Vertices[face.VertA].Faces.Where(i => shadeSmoothAttr[i]).Select(i => normalAttr[i]).Aggregate((n1, n2) => n1 + n2));
                    normal1 = math.normalize(geometry.Vertices[face.VertB].Faces.Where(i => shadeSmoothAttr[i]).Select(i => normalAttr[i]).Aggregate((n1, n2) => n1 + n2));
                    normal2 = math.normalize(geometry.Vertices[face.VertC].Faces.Where(i => shadeSmoothAttr[i]).Select(i => normalAttr[i]).Aggregate((n1, n2) => n1 + n2));
                }

                (int t0, int t1, int t2) = AddFace(faceIndex, normal0, normal1, normal2);
                (int triA0, int triA1, int triB0, int triB1, int triC0, int triC1) = GetActualSharedTriangles(face, t0, t1, t2);
                int triangleOffset = vertices.Count;

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

            ApplyMesh(targetMesh);
        }

        private void ClearMesh(Mesh targetMesh) {
            targetMesh.Clear();
        }

        private void ApplyMesh(Mesh targetMesh) {
            targetMesh.subMeshCount = geometry.SubmeshCount;
            targetMesh.SetVertices(vertices);
            targetMesh.SetNormals(normals);
            targetMesh.SetUVs(0, uvs);
            for (int i = 0; i < triangles.Count; i++) {
                targetMesh.SetTriangles(triangles[i], i);
            }
            targetMesh.RecalculateTangents();
            targetMesh.RecalculateBounds();
            targetMesh.Optimize();
            targetMesh.MarkModified();
        }

        private void ClearLists() {
            vertices.Clear();
            normals.Clear();
            uvs.Clear();
            triangles.Clear();

            for (int i = 0; i < geometry.SubmeshCount; i++) {
                triangles.Add(new List<int>());
            }

            exportedFaces.Clear();
        }

        private (int v0Triangle, int v1Triangle, int v2Triangle) AddFace(int faceIndex, float3 normal0, float3 normal1, float3 normal2) {
            int triangleOffset = vertices.Count;
            GeometryData.Face face = geometry.Faces[faceIndex];
            float3 v0 = positionAttr[face.VertA];
            float3 v1 = positionAttr[face.VertB];
            float3 v2 = positionAttr[face.VertC];

            int t0 = 0 + triangleOffset;
            int t1 = 1 + triangleOffset;
            int t2 = 2 + triangleOffset;
            int submesh = submeshAttr[faceIndex];

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            normals.Add(normal0);
            normals.Add(normal1);
            normals.Add(normal2);

            if (uvAttr != null) {
                float2 uv0 = uvAttr[face.FaceCornerA];
                float2 uv1 = uvAttr[face.FaceCornerB];
                float2 uv2 = uvAttr[face.FaceCornerC];
                uvs.Add(uv0);
                uvs.Add(uv1);
                uvs.Add(uv2);
            } else {
                uvs.Add(Vector2.zero);
                uvs.Add(Vector2.right);
                uvs.Add(Vector2.up);
            }

            float3 faceNormal = normalAttr[faceIndex];
            float3 calculatedNormal = math.normalize(math.cross(vertices[t1] - vertices[t0], vertices[t2] - vertices[t0]));
            bool eqNegX = Math.Abs(faceNormal.x) > Constants.FLOAT_TOLERANCE && Math.Abs(faceNormal.x + calculatedNormal.x) < Constants.FLOAT_TOLERANCE;
            bool eqNegY = Math.Abs(faceNormal.y) > Constants.FLOAT_TOLERANCE && Math.Abs(faceNormal.y + calculatedNormal.y) < Constants.FLOAT_TOLERANCE;
            bool eqNegZ = Math.Abs(faceNormal.z) > Constants.FLOAT_TOLERANCE && Math.Abs(faceNormal.z + calculatedNormal.z) < Constants.FLOAT_TOLERANCE;

            if (eqNegX || eqNegY || eqNegZ) {
                // Debug.Log("flipped face");
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

        private int GetSharedFace(int faceIndex, int edgeIndex, float3 normal) {
            GeometryData.Edge edge = geometry.Edges[edgeIndex];
            int sharedFaceIndex = edge.FaceA != faceIndex ? edge.FaceA : edge.FaceB;

            if (exportedFaces.Contains(sharedFaceIndex)) return -1;
            if (sharedFaceIndex != -1 && math_ext.angle(normalAttr[sharedFaceIndex], normal) < 0.01f) {
            // if (sharedFaceIndex != -1 && normalAttr[sharedFaceIndex].Equals(normal)) {
                return sharedFaceIndex;
            }

            return -1;
        }

        private void AddAdjacentFace(int sharedA, int vertexA, int vertexB, int triangle0, int triangle1, int triangle2) {
            int otherVertexIndex;
            int otherFaceCornerIndex;
            GeometryData.Face sharedFace = geometry.Faces[sharedA];
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

            float3 otherVertex = positionAttr[otherVertexIndex];
            float2 otherUV = uvAttr != null ? uvAttr[otherFaceCornerIndex] : float2.zero;
            float3 normal = !shadeSmoothAttr[sharedA]
                ? normalAttr[sharedA]
                : math.normalize(geometry.Vertices[otherVertexIndex].Faces.Where(i => shadeSmoothAttr[i]).Select(i => normalAttr[i]).Aggregate((n1, n2) => n1 + n2));

            int submesh = submeshAttr[sharedA];
            vertices.Add(otherVertex);
            normals.Add(normal);
            uvs.Add(otherUV);

            float3 sharedFaceNormal = normalAttr[sharedA];
            float3 calculatedNormal = math.normalize(math.cross(vertices[triangle1] - vertices[triangle0], vertices[triangle2] - vertices[triangle0]));
            bool eqNegX = Math.Abs(sharedFaceNormal.x) > Constants.FLOAT_TOLERANCE && Math.Abs(sharedFaceNormal.x + calculatedNormal.x) < Constants.FLOAT_TOLERANCE;
            bool eqNegY = Math.Abs(sharedFaceNormal.y) > Constants.FLOAT_TOLERANCE && Math.Abs(sharedFaceNormal.y + calculatedNormal.y) < Constants.FLOAT_TOLERANCE;
            bool eqNegZ = Math.Abs(sharedFaceNormal.z) > Constants.FLOAT_TOLERANCE && Math.Abs(sharedFaceNormal.z + calculatedNormal.z) < Constants.FLOAT_TOLERANCE;

            if (eqNegX || eqNegY || eqNegZ) {
                // Debug.Log("flipped face");
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
            else if (geometry.Edges[face.EdgeA].VertA == face.VertB) triA0 = t1;
            else if (geometry.Edges[face.EdgeA].VertA == face.VertC) triA0 = t2;
            if (geometry.Edges[face.EdgeA].VertB == face.VertA) triA1 = t0;
            else if (geometry.Edges[face.EdgeA].VertB == face.VertB) triA1 = t1;
            else if (geometry.Edges[face.EdgeA].VertB == face.VertC) triA1 = t2;
            if (geometry.Edges[face.EdgeB].VertA == face.VertA) triB0 = t0;
            else if (geometry.Edges[face.EdgeB].VertA == face.VertB) triB0 = t1;
            else if (geometry.Edges[face.EdgeB].VertA == face.VertC) triB0 = t2;
            if (geometry.Edges[face.EdgeB].VertB == face.VertA) triB1 = t0;
            else if (geometry.Edges[face.EdgeB].VertB == face.VertB) triB1 = t1;
            else if (geometry.Edges[face.EdgeB].VertB == face.VertC) triB1 = t2;
            if (geometry.Edges[face.EdgeC].VertA == face.VertA) triC0 = t0;
            else if (geometry.Edges[face.EdgeC].VertA == face.VertB) triC0 = t1;
            else if (geometry.Edges[face.EdgeC].VertA == face.VertC) triC0 = t2;
            if (geometry.Edges[face.EdgeC].VertB == face.VertA) triC1 = t0;
            else if (geometry.Edges[face.EdgeC].VertB == face.VertB) triC1 = t1;
            else if (geometry.Edges[face.EdgeC].VertB == face.VertC) triC1 = t2;
            return (triA0, triA1, triB0, triB1, triC0, triC1);
        }

    }
}
