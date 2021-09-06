using System.Collections.Generic;
using UnityEngine;

namespace GraphFramework.Runtime {
    public abstract class ObjectCollection<TChild, TProcessed> : MonoBehaviour {
        [SerializeField, HideInInspector] private int childrenHashCode = 0;
        [SerializeField, HideInInspector] private TProcessed processed;

        public void Process() {
            var newHashCode = ComputeChildrenHashCode(transform);
            
            if (childrenHashCode != newHashCode || processed == null) {
                childrenHashCode = newHashCode;
                processed = Process(CollectChildren());
            }
        }
        
        protected abstract TProcessed Process(List<TChild> children);
        protected abstract TChild CollectChild(Transform childTransform);

        private List<TChild> CollectChildren() {
            var list = new List<TChild>();
            for (var i = 0; i < transform.childCount; i++) {
                list.Add(CollectChild(transform.GetChild(i)));
            }

            return list;
        }

        private int ComputeChildrenHashCode(Transform transform) {
            var hashCode = transform.GetHashCode();
            if (transform.childCount == 0) return hashCode;
            
            for (var i = 0; i < transform.childCount; i++) {
                hashCode = hashCode * 37 + ComputeChildrenHashCode(transform.GetChild(i));
            }

            return hashCode;
        }

        private void Reset() {
            Process();
        }
    }
}