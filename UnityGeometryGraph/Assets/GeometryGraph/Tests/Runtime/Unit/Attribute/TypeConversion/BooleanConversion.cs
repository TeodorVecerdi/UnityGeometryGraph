using System.Linq;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.AttributeSystem;
using NUnit.Framework;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Tests.Runtime.Unit.Attribute.TypeConversion {
    public class BooleanConversion {
        [Test]
        public void BooleanToInteger() {
            var boolAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Bool).Into<BoolAttribute>("boolAttribute", AttributeDomain.Vertex);
            var intAttribute = boolAttribute.Into<IntAttribute>("intAttribute", AttributeDomain.Vertex as AttributeDomain?);

            Assert.AreEqual(intAttribute.Values.Count, boolAttribute.Values.Count, "Attribute length doesn't match: int:{0} == bool:{1}", intAttribute.Values.Count, boolAttribute.Values.Count);
            
            Assert.IsTrue(
                boolAttribute.Zip(intAttribute, (boolean, integer) => (boolean, integer))
                             .All(pair => (pair.boolean ? 1 : 0) == pair.integer), 
                "boolAttribute.Zip(intAttribute, (boolean, integer) => (boolean, integer)).All(pair => (pair.boolean ? 1 : 0) == pair.integer)"
            );
        }

        [Test]
        public void BooleanToFloat() {
            var boolAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Bool).Into<BoolAttribute>("boolAttribute", AttributeDomain.Vertex);
            var floatAttribute = boolAttribute.Into<FloatAttribute>("floatAttribute", AttributeDomain.Vertex as AttributeDomain?);
            Assert.AreEqual(floatAttribute.Values.Count, boolAttribute.Values.Count, "Attribute length doesn't match: float:{0} == bool:{1}", floatAttribute.Values.Count, boolAttribute.Values.Count);

            Assert.IsTrue(
                boolAttribute.Zip(floatAttribute, (boolean, @float) => (boolean, @float))
                             .All(pair => pair.boolean ? pair.@float == 1.0f : pair.@float == 0.0f), 
                "boolAttribute.Zip(intAttribute, (boolean, float) => (boolean, float)).All(pair => pair.boolean ? pair.float == 1.0f : pair.float == 0.0f)"
            );
        }

        [Test]
        public void BooleanToVector2() {
            var boolAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Bool).Into<BoolAttribute>("boolAttribute", AttributeDomain.Vertex);
            var vec2Attribute = boolAttribute.Into<Vector2Attribute>("vec2Attribute", AttributeDomain.Vertex as AttributeDomain?);
            Assert.AreEqual(vec2Attribute.Values.Count, boolAttribute.Values.Count, "Attribute length doesn't match: vec2:{0} == bool:{1}", vec2Attribute.Values.Count, boolAttribute.Values.Count);

            Assert.IsTrue(
                boolAttribute.Zip(vec2Attribute, (boolean, vec2) => (boolean, vec2))
                             .All(pair => pair.boolean ? pair.vec2.Equals(float2_ext.one) : pair.vec2.Equals(float2.zero)), 
                "boolAttribute.Zip(vec2Attribute, (boolean, vec2) => (boolean, vec2)).All(pair => pair.boolean ? pair.vec2.Equals(float2_util.one) : pair.vec2.Equals(float2.zero))"
            );
        }
        
        [Test]
        public void BooleanToVector3() {
            var boolAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Bool).Into<BoolAttribute>("boolAttribute", AttributeDomain.Vertex);
            var vec3Attribute = boolAttribute.Into<Vector3Attribute>("vec3Attribute", AttributeDomain.Vertex as AttributeDomain?);
            Assert.AreEqual(vec3Attribute.Values.Count, boolAttribute.Values.Count, "Attribute length doesn't match: vec3:{0} == bool:{1}", vec3Attribute.Values.Count, boolAttribute.Values.Count);

            Assert.IsTrue(
                boolAttribute.Zip(vec3Attribute, (boolean, vec3) => (boolean, vec3))
                             .All(pair => pair.boolean ? pair.vec3.Equals(float3_ext.one) : pair.vec3.Equals(float3.zero)), 
                "boolAttribute.Zip(vec3Attribute, (boolean, vec3) => (boolean, vec3)).All(pair => pair.boolean ? pair.vec3.Equals(float3_util.one) : pair.vec3.Equals(float3.zero))"
            );
        }
    }
}