using System.Linq;
using Attribute;
using NUnit.Framework;
using Unity.Mathematics;
using UnityCommons;

namespace Tests.Attribute.TypeConversion {
    public class Vector2Conversions {
        [Test]
        public void Vector2ToBoolean() {
            var vec2Attribute = Enumerable.Range(0, 100).Select(_ => new float2(Rand.Range(0, 2), Rand.Range(0, 2))).Into<Vector2Attribute>("src", AttributeDomain.Vertex);
            var boolAttribute = vec2Attribute.Into<BoolAttribute>("dst", AttributeDomain.Vertex as AttributeDomain?);
           
            Assert.AreEqual(vec2Attribute.Values.Count, boolAttribute.Values.Count, "Attribute length doesn't match: vec2:{0} == bool:{1}", vec2Attribute.Values.Count, boolAttribute.Values.Count);

            Assert.IsTrue(
                boolAttribute.Zip(vec2Attribute, (boolean, vec2) => (boolean, vec2))
                             .All(pair => pair.boolean == (pair.vec2.x != 0.0f && pair.vec2.y != 0.0f)), 
                "boolAttribute.Zip(vec2Attribute, (boolean, vec2) => (boolean, vec2)).All(pair => pair.boolean == (pair.vec2.x != 0.0f && pair.vec2.y != 0.0f))"
            );
        }
        
        [Test]
        public void Vector2ToInteger() {
            var vec2Attribute = Enumerable.Range(0, 100).Select(_ => new float2(Rand.Range(-100f, 100f), Rand.Range(-100f, 100f))).Into<Vector2Attribute>("src", AttributeDomain.Vertex);
            var intAttribute = vec2Attribute.Into<IntAttribute>("dst", AttributeDomain.Vertex as AttributeDomain?);
           
            Assert.AreEqual(vec2Attribute.Values.Count, intAttribute.Values.Count, "Attribute length doesn't match: vec2:{0} == int:{1}", vec2Attribute.Values.Count, intAttribute.Values.Count);

            Assert.IsTrue(
                intAttribute.Zip(vec2Attribute, (integer, vec2) => (integer, vec2))
                            .All(pair => pair.integer == (int)pair.vec2.x), 
                "intAttribute.Zip(vec2Attribute, (integer, vec2) => (integer, vec2)).All(pair => pair.integer == (int)pair.vec2.x)"
            );
        }
        
        [Test]
        public void Vector2ToFloat() {
            var vec2Attribute = Enumerable.Range(0, 100).Select(_ => new float2(Rand.Range(-100f, 100f), Rand.Range(-100f, 100f))).Into<Vector2Attribute>("src", AttributeDomain.Vertex);
            var floatAttribute = vec2Attribute.Into<FloatAttribute>("dst", AttributeDomain.Vertex as AttributeDomain?);
           
            Assert.AreEqual(vec2Attribute.Values.Count, floatAttribute.Values.Count, "Attribute length doesn't match: vec2:{0} == float:{1}", vec2Attribute.Values.Count, floatAttribute.Values.Count);

            Assert.IsTrue(
                floatAttribute.Zip(vec2Attribute, (@float, vec2) => (@float, vec2))
                              .All(pair => pair.@float == pair.vec2.x), 
                "floatAttribute.Zip(vec2Attribute, (@float, vec2) => (@float, vec2)).All(pair => pair.@float == pair.vec2.x)"
            );
        }
        
        [Test]
        public void Vector2ToClampedFloat() {
            var vec2Attribute = Enumerable.Range(0, 100).Select(_ => new float2(Rand.Range(-100f, 100f), Rand.Range(-100f, 100f))).Into<Vector2Attribute>("src", AttributeDomain.Vertex);
            var clampedFloatAttribute = vec2Attribute.Into<ClampedFloatAttribute>("dst", AttributeDomain.Vertex as AttributeDomain?);
           
            Assert.AreEqual(vec2Attribute.Values.Count, clampedFloatAttribute.Values.Count, "Attribute length doesn't match: vec2:{0} == clampedFloat:{1}", vec2Attribute.Values.Count, clampedFloatAttribute.Values.Count);

            Assert.IsTrue(
                clampedFloatAttribute.Zip(vec2Attribute, (@float, vec2) => (@float, vec2))
                                     .All(pair => pair.vec2.x < 0.0f ? pair.@float == 0.0f : pair.vec2.x > 1.0f ? pair.@float == 1.0f : pair.@float == pair.vec2.x), 
                "clampedFloatAttribute.Zip(vec2Attribute, (@float, vec2) => (@float, vec2)).All(pair => pair.vec2.x < 0.0f ? pair.@float == 0.0f : pair.vec2.x > 1.0f ? pair.@float == 1.0f : pair.@float == pair.vec2.x)"
            );
        }
        
        [Test]
        public void Vector2ToVector3() {
            var vec2Attribute = Enumerable.Range(0, 100).Select(_ => new float2(Rand.Range(-100f, 100f), Rand.Range(-100f, 100f))).Into<Vector2Attribute>("src", AttributeDomain.Vertex);
            var vec3Attribute = vec2Attribute.Into<Vector3Attribute>("dst", AttributeDomain.Vertex as AttributeDomain?);
           
            Assert.AreEqual(vec2Attribute.Values.Count, vec3Attribute.Values.Count, "Attribute length doesn't match: vec2:{0} == vec3:{1}", vec2Attribute.Values.Count, vec3Attribute.Values.Count);

            Assert.IsTrue(
                vec3Attribute.Zip(vec2Attribute, (vec3, vec2) => (vec3, vec2))
                             .All(pair => pair.vec3.xy.Equals(pair.vec2) && pair.vec3.z == 0), 
                "vec3Attribute.Zip(vec2Attribute, (vec3, vec2) => (vec3, vec2)).All(pair => pair.vec3.xy.Equals(pair.vec2) && pair.vec3.z == 0)"
            );
        }
    }
}