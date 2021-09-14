using System;
using System.Collections.Generic;
using Attribute;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Geometry {
    public class GeometryExporter : MonoBehaviour {
        [SerializeField] private MeshFilter target;
        [SerializeField] private GeometryImporter source;

        [SerializeField] private Mesh mesh;

        [Button]
        public void ExportBasic() {
            if (target == null || source == null || source.GeometryData == null) {
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
            
            var geometry = source.GeometryData;
            
            var positionAttr = geometry.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            var normalAttr = geometry.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face);
            var uvAttr = geometry.GetAttribute<Vector2Attribute>("uv", AttributeDomain.FaceCorner);

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            var exportedFaces = new HashSet<int>(); 

            for (var faceIndex = 0; faceIndex < geometry.Faces.Count; faceIndex++) {
                if(exportedFaces.Contains(faceIndex)) continue;
                exportedFaces.Add(faceIndex);
                
                var face = geometry.Faces[faceIndex];
                
                var v0 = positionAttr[face.VertA];
                var v1 = positionAttr[face.VertB];
                var v2 = positionAttr[face.VertC];
                var uv0 = uvAttr[face.FaceCornerA];
                var uv1 = uvAttr[face.FaceCornerB];
                var uv2 = uvAttr[face.FaceCornerC];
                var normal = normalAttr[faceIndex];

                var triangleOffset = vertices.Count;

                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);
                uvs.Add(uv0);
                uvs.Add(uv1);
                uvs.Add(uv2);
                
                triangles.Add(0 + triangleOffset);
                triangles.Add(1 + triangleOffset);
                triangles.Add(2 + triangleOffset);
            }
            
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents();
            mesh.Optimize();
        }

        private void PrepareMesh() {
            if (mesh == null) mesh = target.sharedMesh;
            if (mesh == null) {
                mesh = new Mesh { name = $"{source.gameObject.name} Mesh" };
                target.sharedMesh = mesh;
            }

            mesh.Clear();
        }
    }
}