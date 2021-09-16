using System;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public class Connection {
        [SerializeReference] public RuntimePort Output;
        [SerializeReference] public RuntimePort Input;
    }
}