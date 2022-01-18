using System.Diagnostics;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GeometryGraph.Runtime.Testing {
    public class GeometryImporter : MonoBehaviour, IGeometryProvider {
        [SerializeField] private MeshFilter geometrySource;
        [SerializeField] private GeometryData geometryData;

        public GeometryData Geometry => geometryData;
        public Matrix4x4 LocalToWorldMatrix => transform.localToWorldMatrix;

        [Button]
        private void ClearProfilingData() {
            Profiler.Cleanup();
        }

        [Button]
        internal void Load() {
            if (geometrySource == null) {
                Debug.LogError("Source MeshFilter is null");
                return;
            }


            using var session = Profiler.BeginSession($"Generate Geometry from '{geometrySource.sharedMesh.name}'", true);
            var sw = Stopwatch.StartNew();
            geometryData = new GeometryData(geometrySource.sharedMesh, 179.9f);
            Debug.Log(sw.Elapsed.TotalMilliseconds);
        }
    }
}