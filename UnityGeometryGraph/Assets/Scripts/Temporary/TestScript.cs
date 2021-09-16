using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Geometry {
    public class TestScript : MonoBehaviour {
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