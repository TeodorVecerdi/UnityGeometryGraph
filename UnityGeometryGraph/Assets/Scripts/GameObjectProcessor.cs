using System.Collections.Generic;
using UnityEngine;

public abstract class GameObjectProcessor<TChild, TProcessed> : MonoBehaviour {
    protected abstract TProcessed Processed { get; set; }
    [SerializeField, HideInInspector] protected int ChildrenHashCode = 0;

    public void Process() {
        var newHashCode = ComputeChildrenHashCode(transform);
        
        if (ChildrenHashCode != newHashCode || Processed == null) {
            ChildrenHashCode = newHashCode;
            Processed = Process(CollectChildren());
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

    protected virtual int ComputeChildrenHashCode(Transform transform) {
        unchecked {
            var hashCode = transform.gameObject.GetHashCode() * 37 + transform.gameObject.activeSelf.GetHashCode();
            if (transform.childCount == 0) return hashCode;
            
            for (var i = 0; i < transform.childCount; i++) {
                hashCode = hashCode * 37 + ComputeChildrenHashCode(transform.GetChild(i));
            }

            return hashCode;
        }
    }

    private void Reset() {
        Process();
    }
}