using System.Diagnostics;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Geometry {
    public class GeometryImporter : MonoBehaviour {
        [SerializeField] private MeshFilter geometrySource;
        [SerializeField] private GeometryData geometryData;

        public GeometryData GeometryData => geometryData;

        [Button]
        internal void Load() {
            if (geometrySource == null) {
                Debug.LogError("Source MeshFilter is null");
                return;
            }

            
            var stopwatch = Stopwatch.StartNew();
            geometryData = new GeometryData(geometrySource.sharedMesh, 0.0f, 179.9f);
            var elapsed = stopwatch.Elapsed;
            Debug.Log($"{elapsed.TotalMilliseconds}ms / {elapsed.Ticks * 0.1f}µs");
        }
    }
}