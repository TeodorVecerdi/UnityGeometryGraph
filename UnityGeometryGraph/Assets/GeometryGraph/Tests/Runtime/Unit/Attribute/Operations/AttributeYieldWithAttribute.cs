using System;
using System.Linq;
using GeometryGraph.Runtime.Attribute;
using NUnit.Framework;
using UnityCommons;

namespace GeometryGraph.Tests.Runtime.Unit.Attribute.Operations {
    public class AttributeYieldWithAttribute {
        private static readonly Func<float, float, float> func = (a, b) => a * b;

        [Test]
        public void BaseAttribute_YieldWithAttributeFuncNull() {
            var valuesA = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var valuesB = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();

            var attrA = new FloatAttribute("attrA", valuesA) { Domain = AttributeDomain.Vertex };
            var attrB = new FloatAttribute("attrB", valuesB) { Domain = AttributeDomain.Vertex };
            var result = attrA.YieldWithAttribute(attrB, null);
            
            Assert.IsTrue(result.SequenceEqual(valuesA), "result.SequenceEqual(valuesA)");
        }

        [Test]
        public void BaseAttribute_YieldWithAttributeOtherNull() {
            var valuesA = Enumerable.Range(0, 100).Select(_ => Rand.Float);
            var attrA = new FloatAttribute("attrA", valuesA) { Domain = AttributeDomain.Vertex };
            var result = attrA.YieldWithAttribute(null, func);
            
            // Since the default of float is 0 I expect all zeros
            Assert.IsTrue(result.SequenceEqual(Enumerable.Range(0, 100).Select(_ => 0f)), "result.SequenceEqual(Enumerable.Range(0, 100).Select(_ => 0f))");
        }

        [Test]
        public void BaseAttribute_YieldWithAttributeFuncAndOtherNull() {
            var valuesA = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var attrA = new FloatAttribute("attrA", valuesA) { Domain = AttributeDomain.Vertex };
            var result = attrA.YieldWithAttribute((FloatAttribute)null, null);

            Assert.IsTrue(result.SequenceEqual(valuesA), "result.SequenceEqual(valuesA)");
        }

        [Test]
        public void BaseAttribute_YieldWithAttribute() {
            var valuesA = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var valuesB = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();

            var attrA = new FloatAttribute("attrA", valuesA) { Domain = AttributeDomain.Vertex };
            var attrB = new FloatAttribute("attrB", valuesB) { Domain = AttributeDomain.Vertex };
            var expected = valuesA.Zip(valuesB, (f1, f2) => func(f1, f2));
            var result = attrA.YieldWithAttribute(attrB, func);
            
            Assert.IsTrue(result.SequenceEqual(expected), "result.SequenceEqual(expected)");
        }

        [Test]
        public void IEnumerable_YieldWithAttributeFuncNull() {
            var valuesA = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var valuesB = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();

            var attrB = new FloatAttribute("attrB", valuesB) { Domain = AttributeDomain.Vertex };
            var result = valuesA.YieldWithAttribute(AttributeType.Float, attrB, null);
            
            Assert.IsTrue(result.SequenceEqual(valuesA), "result.SequenceEqual(valuesA)");
        }

        [Test]
        public void IEnumerable_YieldWithAttributeOtherNull() {
            var valuesA = Enumerable.Range(0, 100).Select(_ => Rand.Float);
            var result = valuesA.YieldWithAttribute(AttributeType.Float, null, func);
            
            // Since the default of float is 0 I expect all zeros
            Assert.IsTrue(result.SequenceEqual(Enumerable.Range(0, 100).Select(_ => 0f)), "result.SequenceEqual(Enumerable.Range(0, 100).Select(_ => 0f))");
        }

        [Test]
        public void IEnumerable_YieldWithAttributeFuncAndOtherNull() {
            var valuesA = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var result = valuesA.YieldWithAttribute(AttributeType.Float, null, null);
            
            Assert.IsTrue(result.SequenceEqual(valuesA), "result.SequenceEqual(valuesA)");
        }

        [Test]
        public void IEnumerable_YieldWithAttribute() {
            var valuesA = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var valuesB = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();

            var attrB = new FloatAttribute("attrB", valuesB) { Domain = AttributeDomain.Vertex };
            var expected = valuesA.Zip(valuesB, (f1, f2) => func(f1, f2));
            var result = valuesA.YieldWithAttribute(AttributeType.Float, attrB, func);
            
            Assert.IsTrue(result.SequenceEqual(expected), "result.SequenceEqual(expected)");
        }
    }
}