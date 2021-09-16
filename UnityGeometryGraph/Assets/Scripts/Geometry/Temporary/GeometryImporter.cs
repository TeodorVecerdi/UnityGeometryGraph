using System.Diagnostics;
using Misc;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Geometry {
    public class GeometryImporter : MonoBehaviour {
        [SerializeField] private MeshFilter geometrySource;
        [SerializeField] private GeometryData geometryData;

        public GeometryData GeometryData => geometryData;

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
            geometryData = new GeometryData(geometrySource.sharedMesh, 0.0f, 179.9f);
            Debug.Log(sw.Elapsed.TotalMilliseconds);
        }
    }
}