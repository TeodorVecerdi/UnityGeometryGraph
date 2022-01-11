using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Curve.TEMP {
    [ExecuteAlways]
    public class TestRealtimeGenerator : MonoBehaviour {
        [Required] public GeometryGraph GeometryGraph;
        public bool Enabled;
        public bool UpdateGraph;
        public bool DrawInstances;

        private void Update() {
            if (!Enabled || GeometryGraph == null) return;

            if (UpdateGraph) GeometryGraph.Evaluate();
            if (DrawInstances) GeometryGraph.RenderInstances();
        }
    }
}