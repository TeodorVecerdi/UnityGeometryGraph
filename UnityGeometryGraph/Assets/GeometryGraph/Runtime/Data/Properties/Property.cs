using System;

namespace GraphFramework.Runtime {
    [Serializable]
    public class Property {
        public string Guid;
        public string ReferenceName;
        public string DisplayName;
        public PropertyType Type;
    }
}