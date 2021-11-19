using System;
using System.Collections.Generic;

namespace GeometryGraph.Runtime.AttributeSystem {
    [Serializable]
    public class BoolAttribute : BaseAttribute<bool> {
        public override AttributeType Type => AttributeType.Boolean;
        
        public BoolAttribute(string name) : base(name) {
        }
        
        public BoolAttribute(string name, IEnumerable<bool> values) : base(name, values) {
        }
    }
}