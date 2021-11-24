using System;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryGraph.Runtime.Data {
    public abstract class GameObjectProcessor<TChild, TProcessed> : MonoBehaviour {
        protected abstract TProcessed Processed { get; set; }
        [SerializeField, HideInInspector] protected int ChildrenHashCode = 0;

        public virtual void Process() {
            int newHashCode = ComputeChildrenHashCode(transform);

            if (ChildrenHashCode != newHashCode || Processed == null) {
                ChildrenHashCode = newHashCode;
                Processed = Process(CollectChildren());
            }
        }

        protected abstract TProcessed Process(List<TChild> children);
        protected abstract TChild CollectChild(Transform childTransform);

        protected List<TChild> CollectChildren() {
            List<TChild> list = new List<TChild>();
            for (int i = 0; i < transform.childCount; i++) {
                list.Add(CollectChild(transform.GetChild(i)));
            }

            return list;
        }

        protected virtual int ComputeChildrenHashCode(Transform transform) {
            unchecked {
                int hashCode = HashCode.Combine(transform.gameObject,
                                                transform.gameObject.activeSelf,
                                                transform.localPosition,
                                                transform.localRotation,
                                                transform.localScale,
                                                transform.childCount,
                                                transform.GetComponents<Renderer>().Length
                );
                if (transform.childCount == 0) return hashCode;

                for (int i = 0; i < transform.childCount; i++) {
                    hashCode = HashHelpers.Combine(hashCode, ComputeChildrenHashCode(transform.GetChild(i)));
                }

                return hashCode;
            }
        }

        private void Reset() {
            Process();
        }
    }
}