using System;
using System.Collections.Generic;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public abstract class RuntimeNode {
        public string Guid;
        public List<string> PortGuids;
    }
}