using System;
using System.Collections.Generic;

namespace Attribute {
    [Serializable]
    public class BoolAttribute : BaseAttribute<bool> {
        public override AttributeType Type => AttributeType.Boolean;
        
        public BoolAttribute(string name) : base(name) {
        }
        
        public BoolAttribute(string name, IEnumerable<bool> values) : base(name, values) {
        }
    }
}