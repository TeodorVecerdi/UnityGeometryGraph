using System;
using System.Linq;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.AttributeSystem;
using NUnit.Framework;
using UnityCommons;

namespace GeometryGraph.Tests.Runtime.Unit.Attribute.Operations {
    public class AttributeInto {
        [Test]
        public void WithIEnumerable_UsingNameDomain() {
            var sourceValues = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();

            var attribute = sourceValues.Into<FloatAttribute>("attr", AttributeDomain.Vertex);
            Assert.AreEqual(attribute.Type, AttributeType.Float, "{0} == AttributeType.Float", attribute.Type);
            Assert.AreEqual(attribute.Domain, AttributeDomain.Vertex, "{0} == AttributeDomain.Vertex", attribute.Domain);
            Assert.IsTrue(attribute.Name.Equals("attr", StringComparison.InvariantCulture), "'{0}'.Equals('attr', StringComparison.InvariantCulture)", attribute.Name);
            Assert.IsTrue(attribute.SequenceEqual(sourceValues), "attribute.SequenceEqual(sourceValues)");
        }

        [Test]
        public void WithBaseAttribute_UsingNameDomain() {
            var sourceValues = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var sourceAttribute = new FloatAttribute("srcAttr", sourceValues) { Domain = AttributeDomain.Vertex };
            var attribute = sourceAttribute.Into<FloatAttribute>("attr", new AttributeDomain?(AttributeDomain.Vertex));

            Assert.AreEqual(attribute.Type, AttributeType.Float, "{0} == AttributeType.Float", attribute.Type);
            Assert.AreEqual(attribute.Domain, AttributeDomain.Vertex, "{0} == AttributeDomain.Vertex", attribute.Domain);
            Assert.IsTrue(attribute.Name.Equals("attr", StringComparison.InvariantCulture), "'{0}'.Equals('attr', StringComparison.InvariantCulture)", attribute.Name);
            Assert.IsTrue(attribute.SequenceEqual(sourceAttribute), "attribute.SequenceEqual(sourceAttribute)");
        }

        [Test]
        public void WithIEnumerable_IntoTAttribute() {
            var sourceValues = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var destAttribute = new FloatAttribute("destination", sourceValues) { Domain = AttributeDomain.Vertex };

            var sourceValues2 = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var destAttribute2 = sourceValues2.Into<FloatAttribute>(destAttribute);

            Assert.AreSame(destAttribute2, destAttribute, "destAttribute2 == destAttribute");
            Assert.IsTrue(destAttribute2.SequenceEqual(sourceValues2), "destAttribute2.SequenceEqual(sourceValues2)");
        }

        [Test]
        public void WithIEnumerable_IntoBaseAttribute() {
            var sourceValues = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var destAttribute = new FloatAttribute("destination", sourceValues) { Domain = AttributeDomain.Vertex };

            var sourceValues2 = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var destAttribute2 = sourceValues2.Into((BaseAttribute)destAttribute);

            Assert.AreSame(destAttribute2, destAttribute, "destAttribute2 == destAttribute");
            Assert.IsTrue(destAttribute2.Values.SequenceEqual(sourceValues2.Convert(input => (object)input)),
                          "destAttribute2.SequenceEqual(sourceValues2.Convert(input => (object)input))");
        }
    }
}