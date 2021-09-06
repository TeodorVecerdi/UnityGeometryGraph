using System;
using System.Collections.Generic;

namespace GraphFramework.Runtime {
    [Serializable]
    public class Node {
        public NodeType Type;
        public string Guid;
    }

    public enum NodeType {
        Property
    }
}