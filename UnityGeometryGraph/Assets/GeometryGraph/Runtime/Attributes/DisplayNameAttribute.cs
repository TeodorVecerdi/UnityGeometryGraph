using System;

namespace GeometryGraph.Runtime.Attributes {
    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayNameAttribute : System.Attribute {
        public string Name { get; }
        public DisplayNameAttribute(string name) {
            Name = name;
        }
    }
}