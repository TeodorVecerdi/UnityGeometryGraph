using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Geometry;
using UnityEngine;

namespace GeometryGraph.Runtime.Data {
    public class GeometryCollection : GameObjectProcessor<GeometryData, List<GeometryData>>, IGeometryProvider {
        [SerializeField, Min(0)]
        private int index;

        [SerializeField] private List<GeometryData> objects;
        [SerializeField, HideInInspector] private List<Transform> children = new();
        protected override List<GeometryData> Processed { get => objects; set => objects = value; }

        public List<GeometryData> Collection => objects;

        public override void Process() {
            int newHashCode = ComputeChildrenHashCode(transform);

            if (ChildrenHashCode != newHashCode || Processed == null) {
                children.Clear();
                ChildrenHashCode = newHashCode;
                Processed = Process(CollectChildren());
            }
        }

        protected override List<GeometryData> Process(List<GeometryData> children) {
            return children;
        }

        protected override GeometryData CollectChild(Transform childTransform) {
            children.Add(childTransform);
            GeometryData data = GeometryData.Empty;
            MeshFilter[] childrenMeshFilters = childTransform.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter meshFilter in childrenMeshFilters) {
                Mesh transformedChildMesh = TransformMeshToRootLocal(meshFilter, childTransform);
                GeometryData.Merge(data, transformedChildMesh);
            }

            return data;
        }

        private Mesh TransformMeshToRootLocal(MeshFilter filter, Transform parentTransform) {
            Matrix4x4 filterLocalToRootLocalMatrix = parentTransform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            Quaternion rotation = Quaternion.LookRotation(
                filterLocalToRootLocalMatrix.GetColumn(2),
                filterLocalToRootLocalMatrix.GetColumn(1)
            );
            Mesh sourceMesh = filter.sharedMesh;

            Mesh mesh = new();
            List<Vector3> vertices = sourceMesh.vertices.Select(vertex => (Vector3)(filterLocalToRootLocalMatrix * new Vector4(vertex.x, vertex.y, vertex.z, 1.0f))).ToList();
            List<Vector3> normals = sourceMesh.normals.Select(normal => (rotation * normal).normalized).ToList();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTangents(sourceMesh.tangents);
            mesh.SetUVs(0, sourceMesh.uv);
            mesh.subMeshCount = sourceMesh.subMeshCount;
            for (int i = 0; i < mesh.subMeshCount; i++) {
                mesh.SetTriangles(sourceMesh.GetTriangles(i), i);
            }

            mesh.RecalculateBounds();
            return mesh;
        }


        private GeometryData selectedGeometry => objects == null || objects.Count == 0 ? null : objects[index];
        private Transform selectedChild => children == null || children.Count == 0 ? null : children[index];

        public GeometryData Geometry => selectedGeometry;
        public Matrix4x4 LocalToWorldMatrix => selectedChild.localToWorldMatrix;


        private int __GetMaxIndex() {
            return children?.Count - 1 ?? -1;
        }

        private void __ChangeIndex(int amount) {
            int maxIndex = __GetMaxIndex();
            if (maxIndex == -1) return;
            if (index + amount > maxIndex) index = 0;
            else if (index + amount < 0) index = maxIndex;
            else index += amount;
        }
    }
}