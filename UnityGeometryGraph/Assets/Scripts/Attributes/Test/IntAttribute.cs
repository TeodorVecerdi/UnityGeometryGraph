using System.Collections.Generic;

namespace Attributes.Test {
    public class IntAttribute : BaseAttribute<int> {
        protected internal override AttributeType Type => AttributeType.Integer;
        
        public IntAttribute(string name) : base(name) {
        }
        
        public IntAttribute(string name, IEnumerable<int> values) : base(name, values) {
        }
    }

}