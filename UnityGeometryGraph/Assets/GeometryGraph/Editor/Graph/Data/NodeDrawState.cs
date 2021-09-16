using System;
using UnityEngine;

namespace GeometryGraph.Editor {
    [Serializable]
    public struct NodeDrawState {
        [SerializeField] public Rect Position;
        [SerializeField] public bool Expanded;
    }
}