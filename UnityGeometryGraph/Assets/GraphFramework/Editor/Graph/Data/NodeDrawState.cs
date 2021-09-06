using System;
using UnityEngine;

namespace GraphFramework.Editor {
    [Serializable]
    public struct NodeDrawState {
        [SerializeField] public Rect Position;
        [SerializeField] public bool Expanded;
    }
}