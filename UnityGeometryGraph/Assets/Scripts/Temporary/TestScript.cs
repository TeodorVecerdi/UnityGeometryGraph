using System.Text;
using GeometryGraph.Runtime.Graph;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Geometry {
    public class TestScript : MonoBehaviour {
        public RuntimeGraphObject RuntimeGraphObject;
        
        [Button]
        private void PrintChildHierarchy() {
            var allChildTransforms = GetComponentsInChildren<Transform>(true);
            var sb = new StringBuilder();
            foreach (var childTransform in allChildTransforms) {
                sb.AppendLine($"{childTransform.gameObject.name} {childTransform.gameObject.activeSelf}");
            }

            Debug.Log(sb.ToString());
        }
    }
}