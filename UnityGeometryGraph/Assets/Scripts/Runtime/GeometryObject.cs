using System.Collections.Generic;
using System.Linq;
using Geometry;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime {
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
            var data = GeometryData.Empty;
            foreach (var geometryData in children) {
                GeometryData.Merge(data, geometryData);
            }

            return data;
        }

        protected override GeometryData CollectChild(Transform childTransform) {
            var data = GeometryData.Empty;
            var childrenMeshFilters = childTransform.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in childrenMeshFilters) {
                var transformedChildMesh = TransformMeshToRootLocal(meshFilter);
                GeometryData.Merge(data, transformedChildMesh);
            }

            return data;
        }

        private Mesh TransformMeshToRootLocal(MeshFilter filter) {
            var filterLocalToRootLocalMatrix = transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            var rotation = Quaternion.LookRotation(
                filterLocalToRootLocalMatrix.GetColumn(2),
                filterLocalToRootLocalMatrix.GetColumn(1)
            );
            var sourceMesh = filter.sharedMesh;

            var mesh = new Mesh();
            var vertices = sourceMesh.vertices.Select(vertex => (Vector3)(filterLocalToRootLocalMatrix * new Vector4(vertex.x, vertex.y, vertex.z, 1.0f))).ToList();
            var normals = sourceMesh.normals.Select(normal => (rotation * normal).normalized).ToList();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, sourceMesh.uv);
            mesh.subMeshCount = sourceMesh.subMeshCount;
            for (var i = 0; i < mesh.subMeshCount; i++) {
                mesh.SetTriangles(sourceMesh.GetTriangles(i), i);
            }
            
            mesh.RecalculateBounds();
            return mesh;
        }

        public GeometryData Geometry => geometry;
        public Matrix4x4 LocalToWorldMatrix => transform.localToWorldMatrix;
    }
}