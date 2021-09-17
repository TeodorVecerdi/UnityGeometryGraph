using System;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public class Property {
        public string Guid;
        public string ReferenceName;
        public string DisplayName;
        public PropertyType Type;
        public object Value;
    }
}