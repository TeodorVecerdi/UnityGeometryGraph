using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Attribute {
    [Serializable]
    public class Vector2Attribute : BaseAttribute<float2> {
        public override AttributeType Type => AttributeType.Vector2;

        public Vector2Attribute(string name) : base(name) {
        }

        public Vector2Attribute(string name, IEnumerable<float2> values) : base(name, values) {
        }
    }

    [Serializable]
    public class Vector3Attribute : BaseAttribute<float3> {
        public override AttributeType Type => AttributeType.Vector3;

        public Vector3Attribute(string name) : base(name) {
        }

        public Vector3Attribute(string name, IEnumerable<float3> values) : base(name, values) {
        }
    }
}