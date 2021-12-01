using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Curve.TEMP {
    [ExecuteAlways]
    public class TestRealtimeGenerator : MonoBehaviour {
        [Required] public GeometryGraph GeometryGraph;
        public bool Enabled;

        private void Update() {
            if (!Enabled || GeometryGraph == null) return;
            GeometryGraph.Evaluate();
        }
    }
}