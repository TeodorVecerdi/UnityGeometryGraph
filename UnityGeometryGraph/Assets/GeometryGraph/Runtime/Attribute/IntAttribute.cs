using System;
using System.Collections.Generic;

namespace GeometryGraph.Runtime.AttributeSystem {
    [Serializable]
    public class IntAttribute : BaseAttribute<int> {
        public override AttributeType Type => AttributeType.Integer;
        
        public IntAttribute(string name) : base(name) {
        }
        
        public IntAttribute(string name, IEnumerable<int> values) : base(name, values) {
        }
    }

}