using System;
using System.Collections.Generic;
using System.Linq;
using UnityCommons;

namespace Attribute.Test {
    public class FloatAttribute : BaseAttribute<float> {
        protected internal override AttributeType Type => AttributeType.Float;
   
        public FloatAttribute(string name) : base(name) {
        }
        
        public FloatAttribute(string name, IEnumerable<float> values) : base(name, values) {
        }
    }
    
    public class ClampedFloatAttribute : BaseAttribute<float> {
        protected internal override AttributeType Type => AttributeType.ClampedFloat;
        
        public override void Fill(IEnumerable<float> values) {
            Values.Clear();
            foreach (var value in values) {
                Values.Add(Convert.ToSingle(value).Clamped01());
            }
        }

        public ClampedFloatAttribute(string name) : base(name) {
        }

        public ClampedFloatAttribute(string name, IEnumerable<float> values) : base(name, values.Select(value => value.Clamped01())) {
        }
    }

}