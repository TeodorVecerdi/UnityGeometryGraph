﻿using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Geometry;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Data {
    public class GeometryCollection : GameObjectProcessor<GeometryData, List<GeometryData>>, IGeometryProvider {
        [SerializeField, ShowIf("@"+ nameof(objects) + " != null && "+ nameof(objects) + ".Count > 0"), MinValue(0), MaxValue(nameof(__GetMaxIndex))]
        [InlineButton("@__ChangeIndex(1)", "+")]
        [InlineButton("@__ChangeIndex(-1)", "-")]
        private int index;
        
        [SerializeField] private List<GeometryData> objects;
        [SerializeField, HideInInspector] private List<Transform> children = new List<Transform>();
        protected override List<GeometryData> Processed { get => objects; set => objects = value; }
        
        
        [Button]
        public override void Process() {
            var newHashCode = ComputeChildrenHashCode(transform);
            
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
            var data = GeometryData.Empty;
            var childrenMeshFilters = childTransform.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in childrenMeshFilters) {
                var transformedChildMesh = TransformMeshToRootLocal(meshFilter, childTransform);
                GeometryData.Merge(data, transformedChildMesh);
            }

            return data;
        }
        
        private Mesh TransformMeshToRootLocal(MeshFilter filter, Transform parentTransform) {
            var filterLocalToRootLocalMatrix = parentTransform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
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
            mesh.SetTangents(sourceMesh.tangents);
            mesh.SetUVs(0, sourceMesh.uv);
            mesh.subMeshCount = sourceMesh.subMeshCount;
            for (var i = 0; i < mesh.subMeshCount; i++) {
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
            var maxIndex = __GetMaxIndex();
            if (maxIndex == -1) return;
            if (index + amount > maxIndex) index = 0;
            else if (index + amount < 0) index = maxIndex;
            else index += amount;
        }
    }
}