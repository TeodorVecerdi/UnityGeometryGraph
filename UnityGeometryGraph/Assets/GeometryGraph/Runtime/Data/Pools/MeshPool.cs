using System;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace GeometryGraph.Runtime.Data {
    [Serializable]
    public class MeshPool : ObjectPool<Mesh> {
        [SerializeField] private IndexFormat meshIndexFormat;

        public MeshPool(IndexFormat meshIndexFormat, int initialPoolSize = 4, float growthFactor = 2.0f) : base(initialPoolSize, growthFactor) {
            Debug.Log("Created mesh pool");
            this.meshIndexFormat = meshIndexFormat;
        }

        protected override Mesh CreatePooled() {
            Debug.Log("Creating mesh");
            return new Mesh {
                name = $"PooledMesh_{Guid.NewGuid():N}",
                indexFormat = meshIndexFormat
            };
        }

        protected override void OnReturn(Mesh pooled) {
            pooled.Clear();
        }

        protected override void DestroyPooled(Mesh obj) {
            Debug.Log("Cleaning mesh pool");
            if(Application.isPlaying) {
                Object.Destroy(obj);
            } else {
                Object.DestroyImmediate(obj);
            }
        }
    }
}