using System.Linq;
using Attribute;
using NUnit.Framework;
using UnityCommons;

namespace Tests.Unit.Attribute.TypeConversion {
    public class ClampedFloatConversion {
        [Test]
        public void ClampedFloatToBoolean() {
            var floatAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100f, 100f)).Into<ClampedFloatAttribute>("src", AttributeDomain.Vertex);
            var boolAttribute = floatAttribute.Into<BoolAttribute>("dst", AttributeDomain.Vertex as AttributeDomain?);

            Assert.AreEqual(floatAttribute.Values.Count, boolAttribute.Values.Count, "Attribute length doesn't match: float:{0} == bool:{1}", floatAttribute.Values.Count, boolAttribute.Values.Count);
            
            Assert.IsTrue(
                boolAttribute.Zip(floatAttribute, (boolean, @float) => (boolean, @float))
                             .All(pair => pair.@float != 0.0f == pair.boolean), 
                "boolAttribute.Zip(floatAttribute, (boolean, float) => (boolean, float)).All(pair => pair.float != 0 == pair.boolean)"
            );
        }
        
        [Test]
        public void ClampedFloatToInteger() {
            var floatAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100f, 100f)).Into<ClampedFloatAttribute>("src", AttributeDomain.Vertex);
            var intAttribute = floatAttribute.Into<IntAttribute>("dst", AttributeDomain.Vertex as AttributeDomain?);

            Assert.AreEqual(floatAttribute.Values.Count, intAttribute.Values.Count, "Attribute length doesn't match: float:{0} == int:{1}", floatAttribute.Values.Count, intAttribute.Values.Count);
            
            Assert.IsTrue(
                intAttribute.Zip(floatAttribute, (integer, @float) => (integer, @float))
                            .All(pair => (int)pair.@float == pair.integer), 
                "intAttribute.Zip(floatAttribute, (integer, @float) => (integer, @float)).All(pair => (int)pair.@float == pair.integer)"
            );
        }
        
        [Test]
        public void ClampedFloatToFloat() {
            var clampedFloatAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100f, 100f)).Into<ClampedFloatAttribute>("src", AttributeDomain.Vertex);
            var floatAttribute = clampedFloatAttribute.Into<FloatAttribute>("dst", AttributeDomain.Vertex as AttributeDomain?);

            Assert.AreEqual(clampedFloatAttribute.Values.Count, floatAttribute.Values.Count, "Attribute length doesn't match: float:{0} == clampedFloat:{1}", clampedFloatAttribute.Values.Count, floatAttribute.Values.Count);
            
            Assert.IsTrue(
                floatAttribute.Zip(clampedFloatAttribute, (@float, clampedFloat) => (@float, clampedFloat))
                                     .All(pair => pair.@float == pair.clampedFloat), 
                "floatAttribute.Zip(clampedFloatAttribute, (@float, clampedFloat) => (@float, clampedFloat)).All(pair => pair.@float == pair.clampedFloat)"
            );
        }
        
        [Test]
        public void ClampedFloatToVector2() {
            var floatAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100f, 100f)).Into<ClampedFloatAttribute>("src", AttributeDomain.Vertex);
            var vec2Attribute = floatAttribute.Into<Vector2Attribute>("dst", AttributeDomain.Vertex as AttributeDomain?);

            Assert.AreEqual(floatAttribute.Values.Count, vec2Attribute.Values.Count, "Attribute length doesn't match: float:{0} == vec2:{1}", floatAttribute.Values.Count, vec2Attribute.Values.Count);
            
            Assert.IsTrue(
                vec2Attribute.Zip(floatAttribute, (vec2, @float) => (vec2, @float))
                             .All(pair => pair.vec2.x == pair.@float && pair.vec2.y == pair.@float), 
                "vec2Attribute.Zip(floatAttribute, (vec2, @float) => (vec2, @float)).All(pair => pair.vec2.x == pair.@float && pair.vec2.y == pair.@float)"
            );
        }
        
        [Test]
        public void ClampedFloatToVector3() {
            var floatAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100f, 100f)).Into<ClampedFloatAttribute>("src", AttributeDomain.Vertex);
            var vec3Attribute = floatAttribute.Into<Vector3Attribute>("dst", AttributeDomain.Vertex as AttributeDomain?);

            Assert.AreEqual(floatAttribute.Values.Count, vec3Attribute.Values.Count, "Attribute length doesn't match: float:{0} == vec3:{1}", floatAttribute.Values.Count, vec3Attribute.Values.Count);
            
            Assert.IsTrue(
                vec3Attribute.Zip(floatAttribute, (vec3, @float) => (vec3, @float))
                             .All(pair => pair.vec3.x == pair.@float && pair.vec3.y == pair.@float && pair.vec3.z == pair.@float), 
                "vec3Attribute.Zip(floatAttribute, (vec3, @float) => (vec3, @float)).All(pair => pair.vec3.x == pair.@float && pair.vec3.y == pair.@float && pair.vec3.z == pair.@float)"
            );
        }
    }
}