using System.Collections.Generic;
using UnityEngine;

namespace GeometryGraph.Runtime.Data {
    public abstract class GameObjectProcessor<TChild, TProcessed> : MonoBehaviour {
        protected abstract TProcessed Processed { get; set; }
        [SerializeField, HideInInspector] protected int ChildrenHashCode = 0;

        public virtual void Process() {
            var newHashCode = ComputeChildrenHashCode(transform);
        
            if (ChildrenHashCode != newHashCode || Processed == null) {
                ChildrenHashCode = newHashCode;
                Processed = Process(CollectChildren());
            }
        }
    
        protected abstract TProcessed Process(List<TChild> children);
        protected abstract TChild CollectChild(Transform childTransform);

        protected List<TChild> CollectChildren() {
            var list = new List<TChild>();
            for (var i = 0; i < transform.childCount; i++) {
                list.Add(CollectChild(transform.GetChild(i)));
            }

            return list;
        }

        protected virtual int ComputeChildrenHashCode(Transform transform) {
            // TODO(#14): Write a better ComputeChildrenHashCode function for GameObjectProcessor.
            // It should include things like transform data, childCount/child components.
            // Currently it changes only if you add/remove objects or disable/enable objects
            
            unchecked {
                var hashCode = transform.gameObject.GetHashCode() * 137 + transform.gameObject.activeSelf.GetHashCode();
                if (transform.childCount == 0) return hashCode;
            
                for (var i = 0; i < transform.childCount; i++) {
                    hashCode = hashCode * 239 + ComputeChildrenHashCode(transform.GetChild(i));
                }

                return hashCode;
            }
        }

        private void Reset() {
            Process();
        }
    }
}
