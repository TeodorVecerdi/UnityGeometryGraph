using System.Linq;
using Attribute;
using NUnit.Framework;
using UnityCommons;

namespace Tests.Unit.Attribute.TypeConversion {
    public class IntegerConversion {
        [Test]
        public void IntegerToBoolean() {
            var intAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100, 100)).Into<IntAttribute>("intAttribute", AttributeDomain.Vertex);
            var boolAttribute = intAttribute.Into<BoolAttribute>("boolAttribute", AttributeDomain.Vertex as AttributeDomain?);

            Assert.AreEqual(intAttribute.Values.Count, boolAttribute.Values.Count, "Attribute length doesn't match: int:{0} == bool:{1}", intAttribute.Values.Count, boolAttribute.Values.Count);
            
            Assert.IsTrue(
                boolAttribute.Zip(intAttribute, (boolean, integer) => (boolean, integer))
                             .All(pair => pair.integer != 0 == pair.boolean), 
                "boolAttribute.Zip(intAttribute, (boolean, integer) => (boolean, integer)).All(pair => pair.integer != 0 == pair.boolean)"
            );
        }
        
        [Test]
        public void IntegerToFloat() {
            var intAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100, 100)).Into<IntAttribute>("src", AttributeDomain.Vertex);
            var floatAttribute = intAttribute.Into<FloatAttribute>("into", AttributeDomain.Vertex as AttributeDomain?);

            Assert.AreEqual(intAttribute.Values.Count, floatAttribute.Values.Count, "Attribute length doesn't match: int:{0} == float:{1}", intAttribute.Values.Count, floatAttribute.Values.Count);
            
            Assert.IsTrue(
                floatAttribute.Zip(intAttribute, (@float, integer) => (@float, integer))
                              .All(pair => pair.integer == (int)pair.@float), 
                "floatAttribute.Zip(intAttribute, (@float, integer) => (@float, integer)).All(pair => pair.integer == (int)pair.@float)"
            );
        }
        
        [Test]
        public void IntegerToClampedFloat() {
            var intAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100, 100)).Into<IntAttribute>("src", AttributeDomain.Vertex);
            var floatAttribute = intAttribute.Into<ClampedFloatAttribute>("into", AttributeDomain.Vertex as AttributeDomain?);

            Assert.AreEqual(intAttribute.Values.Count, floatAttribute.Values.Count, "Attribute length doesn't match: int:{0} == float:{1}", intAttribute.Values.Count, floatAttribute.Values.Count);
            
            Assert.IsTrue(
                floatAttribute.Zip(intAttribute, (@float, integer) => (@float, integer))
                              .All(pair => pair.integer > 1 ? pair.@float == 1.0f : pair.integer < 0 ? pair.@float == 0.0f : pair.@float == pair.integer), 
                "floatAttribute.Zip(intAttribute, (@float, integer) => (@float, integer)).All(pair => pair.integer > 1 ? pair.@float == 1.0f : pair.integer < 0 ? pair.@float == 0.0f : pair.@float == pair.integer)"
            );
        }
        
        [Test]
        public void IntegerToVector2() {
            var intAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100, 100)).Into<IntAttribute>("src", AttributeDomain.Vertex);
            var vec2Attribute = intAttribute.Into<Vector2Attribute>("into", AttributeDomain.Vertex as AttributeDomain?);

            Assert.AreEqual(intAttribute.Values.Count, vec2Attribute.Values.Count, "Attribute length doesn't match: int:{0} == vec2:{1}", intAttribute.Values.Count, vec2Attribute.Values.Count);
            
            Assert.IsTrue(
                vec2Attribute.Zip(intAttribute, (vec2, integer) => (vec2, integer))
                             .All(pair => pair.vec2.x == pair.integer && pair.vec2.y == pair.integer), 
                "floatAttribute.Zip(intAttribute, (@float, integer) => (@float, integer)).All(pair => pair.vec2.x == pair.integer && pair.vec2.y == pair.integer)"
            );
        }
        
        [Test]
        public void IntegerToVector3() {
            var intAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100, 100)).Into<IntAttribute>("src", AttributeDomain.Vertex);
            var vec3Attribute = intAttribute.Into<Vector3Attribute>("into", AttributeDomain.Vertex as AttributeDomain?);

            Assert.AreEqual(intAttribute.Values.Count, vec3Attribute.Values.Count, "Attribute length doesn't match: int:{0} == vec3:{1}", intAttribute.Values.Count, vec3Attribute.Values.Count);
            
            Assert.IsTrue(
                vec3Attribute.Zip(intAttribute, (vec3, integer) => (vec3, integer))
                             .All(pair => pair.vec3.x == pair.integer && pair.vec3.y == pair.integer && pair.vec3.z == pair.integer), 
                "floatAttribute.Zip(intAttribute, (@float, integer) => (@float, integer)).All(pair => pair.vec3.x == pair.integer && pair.vec3.y == pair.integer && pair.vec3.z == pair.integer)"
            );
        }
    }
}