
using System.Collections.Generic;

namespace Attributes.Test {
    public class BoolAttribute : BaseAttribute<bool> {
        protected internal override AttributeType Type => AttributeType.Boolean;
        
        public BoolAttribute(string name) : base(name) {
        }
        
        public BoolAttribute(string name, IEnumerable<bool> values) : base(name, values) {
        }
    }
}