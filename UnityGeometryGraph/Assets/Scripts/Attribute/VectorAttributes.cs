using System;
using System.Collections.Generic;
using UnityEngine;

namespace Attribute {
    [Serializable]
    public class Vector2Attribute : BaseAttribute<Vector2> {
        public override AttributeType Type => AttributeType.Vector2;

        public Vector2Attribute(string name) : base(name) {
        }

        public Vector2Attribute(string name, IEnumerable<Vector2> values) : base(name, values) {
        }
    }

    [Serializable]
    public class Vector3Attribute : BaseAttribute<Vector3> {
        public override AttributeType Type => AttributeType.Vector3;

        public Vector3Attribute(string name) : base(name) {
        }

        public Vector3Attribute(string name, IEnumerable<Vector3> values) : base(name, values) {
        }
    }
}