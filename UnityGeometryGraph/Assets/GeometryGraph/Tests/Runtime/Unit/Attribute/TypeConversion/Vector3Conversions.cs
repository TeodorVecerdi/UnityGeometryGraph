using System.Linq;
using GeometryGraph.Runtime.AttributeSystem;
using NUnit.Framework;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Tests.Runtime.Unit.Attribute.TypeConversion {
    public class Vector3Conversions {
        [Test]
        public void Vector3ToBoolean() {
            var vec3Attribute = Enumerable.Range(0, 100).Select(_ => new float3(Rand.Range(0, 2), Rand.Range(0, 2), Rand.Range(0, 2))).Into<Vector3Attribute>("src", AttributeDomain.Vertex);
            var boolAttribute = vec3Attribute.Into<BoolAttribute>("dst", AttributeDomain.Vertex as AttributeDomain?);
           
            Assert.AreEqual(vec3Attribute.Values.Count, boolAttribute.Values.Count, "Attribute length doesn't match: vec3:{0} == bool:{1}", vec3Attribute.Values.Count, boolAttribute.Values.Count);

            Assert.IsTrue(
                boolAttribute.Zip(vec3Attribute, (boolean, vec3) => (boolean, vec3))
                             .All(pair => pair.boolean == (pair.vec3.x != 0.0f && pair.vec3.y != 0.0f && pair.vec3.z != 0.0f)),
                "boolAttribute.Zip(vec3Attribute, (boolean, vec3) => (boolean, vec3)).All(pair => pair.boolean == (pair.vec3.x != 0.0f && pair.vec3.y != 0.0f && pair.vec3.z != 0.0f))"
            );
        }

        [Test]
        public void Vector3ToInteger() {
            var vec3Attribute = Enumerable.Range(0, 100).Select(_ => new float3(Rand.Range(-100f, 100f), Rand.Range(-100f, 100f), Rand.Range(-100f, 100f))).Into<Vector3Attribute>("src", AttributeDomain.Vertex);
            var intAttribute = vec3Attribute.Into<IntAttribute>("dst", AttributeDomain.Vertex as AttributeDomain?);
           
            Assert.AreEqual(vec3Attribute.Values.Count, intAttribute.Values.Count, "Attribute length doesn't match: vec3:{0} == int:{1}", vec3Attribute.Values.Count, intAttribute.Values.Count);

            Assert.IsTrue(
                intAttribute.Zip(vec3Attribute, (integer, vec3) => (integer, vec3))
                            .All(pair => pair.integer == (int)pair.vec3.x),
                "intAttribute.Zip(vec3Attribute, (integer, vec3) => (integer, vec3)).All(pair => pair.integer == (int)pair.vec3.x)"
            );
        }

        [Test]
        public void Vector3ToFloat() {
            var vec3Attribute = Enumerable.Range(0, 100).Select(_ => new float3(Rand.Range(-100f, 100f), Rand.Range(-100f, 100f), Rand.Range(-100f, 100f))).Into<Vector3Attribute>("src", AttributeDomain.Vertex);
            var floatAttribute = vec3Attribute.Into<FloatAttribute>("dst", AttributeDomain.Vertex as AttributeDomain?);
           
            Assert.AreEqual(vec3Attribute.Values.Count, floatAttribute.Values.Count, "Attribute length doesn't match: vec3:{0} == float:{1}", vec3Attribute.Values.Count, floatAttribute.Values.Count);

            Assert.IsTrue(
                floatAttribute.Zip(vec3Attribute, (@float, vec3) => (@float, vec3))
                              .All(pair => pair.@float == pair.vec3.x),
                "floatAttribute.Zip(vec3Attribute, (@float, vec3) => (@float, vec3)).All(pair => pair.@float == pair.vec3.x)"
            );
        }


        [Test]
        public void Vector3ToClampedFloat() {
            var vec3Attribute = Enumerable.Range(0, 100).Select(_ => new float3(Rand.Range(-100f, 100f), Rand.Range(-100f, 100f), Rand.Range(-100f, 100f))).Into<Vector3Attribute>("src", AttributeDomain.Vertex);
            var clampedFloatAttribute = vec3Attribute.Into<ClampedFloatAttribute>("dst", AttributeDomain.Vertex as AttributeDomain?);
           
            Assert.AreEqual(vec3Attribute.Values.Count, clampedFloatAttribute.Values.Count, "Attribute length doesn't match: vec3:{0} == clampedFloat:{1}", vec3Attribute.Values.Count, clampedFloatAttribute.Values.Count);

            Assert.IsTrue(
                clampedFloatAttribute.Zip(vec3Attribute, (@float, vec3) => (@float, vec3))
                                     .All(pair => pair.vec3.x < 0.0f ? pair.@float == 0.0f : pair.vec3.x > 1.0f ? pair.@float == 1.0f : pair.@float == pair.vec3.x),
                "clampedFloatAttribute.Zip(vec3Attribute, (@float, vec3) => (@float, vec3)).All(pair => pair.vec3.x < 0.0f ? pair.@float == 0.0f : pair.vec3.x > 1.0f ? pair.@float == 1.0f : pair.@float == pair.vec3.x)"
            );
        }

        [Test]
        public void Vector3ToVector2() {
            var vec3Attribute = Enumerable.Range(0, 100).Select(_ => new float3(Rand.Range(-100f, 100f), Rand.Range(-100f, 100f), Rand.Range(-100f, 100f))).Into<Vector3Attribute>("src", AttributeDomain.Vertex);
            var vec2Attribute = vec3Attribute.Into<Vector2Attribute>("dst", AttributeDomain.Vertex as AttributeDomain?);
           
            Assert.AreEqual(vec3Attribute.Values.Count, vec2Attribute.Values.Count, "Attribute length doesn't match: vec3:{0} == vec2:{1}", vec3Attribute.Values.Count, vec2Attribute.Values.Count);

            Assert.IsTrue(
                vec2Attribute.Zip(vec3Attribute, (vec2, vec3) => (vec2, vec3))
                             .All(pair => pair.vec2.Equals(pair.vec3.xy)),
                "vec2Attribute.Zip(vec3Attribute, (vec2, vec3) => (vec2, vec3)).All(pair => pair.vec2.Equals(pair.vec3.xy))"
            );
        }
    }
}