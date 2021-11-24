using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Geometry;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Data {
    public class GeometryObject : GameObjectProcessor<GeometryData, GeometryData>, IGeometryProvider {
        [SerializeField] private GeometryData geometry;
        protected override GeometryData Processed { get => geometry; set => geometry = value; }
        
        [ShowInInspector] public int Hash => ChildrenHashCode;
        
        [Button]
        private void ProcessData() {
            Process();
        }

        [Button]
        private void ResetHash() {
            ChildrenHashCode = 0;
        }


        protected override GeometryData Process(List<GeometryData> children) {
            GeometryData data = GeometryData.Empty;
            foreach (GeometryData geometryData in children) {
                GeometryData.Merge(data, geometryData);
            }

            return data;
        }

        protected override GeometryData CollectChild(Transform childTransform) {
            GeometryData data = GeometryData.Empty;
            MeshFilter[] childrenMeshFilters = childTransform.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter meshFilter in childrenMeshFilters) {
                Mesh transformedChildMesh = TransformMeshToRootLocal(meshFilter);
                GeometryData.Merge(data, transformedChildMesh);
            }

            return data;
        }

        private Mesh TransformMeshToRootLocal(MeshFilter filter) {
            Matrix4x4 filterLocalToRootLocalMatrix = transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            Quaternion rotation = Quaternion.LookRotation(
                filterLocalToRootLocalMatrix.GetColumn(2),
                filterLocalToRootLocalMatrix.GetColumn(1)
            );
            Mesh sourceMesh = filter.sharedMesh;

            Mesh mesh = new Mesh();
            List<Vector3> vertices = sourceMesh.vertices.Select(vertex => (Vector3)(filterLocalToRootLocalMatrix * new Vector4(vertex.x, vertex.y, vertex.z, 1.0f))).ToList();
            List<Vector3> normals = sourceMesh.normals.Select(normal => (rotation * normal).normalized).ToList();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, sourceMesh.uv);
            mesh.subMeshCount = sourceMesh.subMeshCount;
            for (int i = 0; i < mesh.subMeshCount; i++) {
                mesh.SetTriangles(sourceMesh.GetTriangles(i), i);
            }
            
            mesh.RecalculateBounds();
            return mesh;
        }

        public GeometryData Geometry => geometry;
        public Matrix4x4 LocalToWorldMatrix => transform.localToWorldMatrix;
    }
}