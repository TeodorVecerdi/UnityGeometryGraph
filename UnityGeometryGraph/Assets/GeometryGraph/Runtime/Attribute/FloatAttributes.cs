using System;
using System.Collections.Generic;
using System.Linq;
using UnityCommons;

namespace GeometryGraph.Runtime.AttributeSystem {
    [Serializable]
    public class FloatAttribute : BaseAttribute<float> {
        public override AttributeType Type => AttributeType.Float;
   
        public FloatAttribute(string name) : base(name) {
        }
        
        public FloatAttribute(string name, IEnumerable<float> values) : base(name, values) {
        }
    }
    
    [Serializable]
    public class ClampedFloatAttribute : BaseAttribute<float> {
        public override AttributeType Type => AttributeType.ClampedFloat;
        
        public override void Fill(IEnumerable<float> values) {
            Values.Clear();
            foreach (float value in values) {
                Values.Add(value.Clamped01());
            }
        }

        public ClampedFloatAttribute(string name) : base(name) {
        }

        public ClampedFloatAttribute(string name, IEnumerable<float> values) : base(name, values.Select(value => value.Clamped01())) {
        }
    }

}