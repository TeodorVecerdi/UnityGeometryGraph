using System;
using System.Linq;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.AttributeSystem;
using NUnit.Framework;
using UnityCommons;

namespace GeometryGraph.Tests.Runtime.Unit.Attribute.Operations {
    public class AttributeIntoWithTypeConversion {
        [Test]
        public void WithIEnumerable_UsingNameDomain() {
            var sourceValues = Enumerable.Range(0, 100).Select(_ => Rand.Bool).ToList();
            var expected = sourceValues.Select(b => b ? 1.0f : 0.0f);
            
            var attribute = sourceValues.Into<FloatAttribute>("attr", AttributeDomain.Vertex);
            Assert.AreEqual(attribute.Type, AttributeType.Float, "{0} == AttributeType.Float", attribute.Type);
            Assert.AreEqual(attribute.Domain, AttributeDomain.Vertex, "{0} == AttributeDomain.Vertex", attribute.Domain);
            Assert.IsTrue(attribute.Name.Equals("attr", StringComparison.InvariantCulture), "'{0}'.Equals('attr', StringComparison.InvariantCulture)", attribute.Name);
            Assert.IsTrue(attribute.SequenceEqual(expected), "attribute.SequenceEqual(expected)");
        }

        [Test]
        public void WithBaseAttribute_UsingNameDomain() {
            var sourceValues = Enumerable.Range(0, 100).Select(_ => Rand.Bool).ToList();
            var sourceAttribute = new BoolAttribute("source", sourceValues);
            var expected = sourceValues.Select(b => b ? 1.0f : 0.0f);

            var attribute = sourceAttribute.Into<FloatAttribute>("attr", new AttributeDomain?(AttributeDomain.Vertex));
            Assert.AreEqual(attribute.Type, AttributeType.Float, "{0} == AttributeType.Float", attribute.Type);
            Assert.AreEqual(attribute.Domain, AttributeDomain.Vertex, "{0} == AttributeDomain.Vertex", attribute.Domain);
            Assert.IsTrue(attribute.Name.Equals("attr", StringComparison.InvariantCulture), "'{0}'.Equals('attr', StringComparison.InvariantCulture)", attribute.Name);
            Assert.IsTrue(attribute.SequenceEqual(expected), "attribute.SequenceEqual(expected)");
        }
        
        [Test]
        public void WithIEnumerable_IntoTAttribute() {
            var sourceValues = Enumerable.Range(0, 100).Select(_ => Rand.Float);
            var destAttr = new FloatAttribute("dest", sourceValues) { Domain = AttributeDomain.Vertex };

            var sourceValues2 = Enumerable.Range(0, 100).Select(_ => Rand.Bool).ToList();
            var attr = sourceValues2.Into<FloatAttribute>(destAttr);
            var expected = sourceValues2.Select(b => b ? 1.0f : 0.0f);
            
            Assert.AreSame(attr, destAttr, "attr == destAttr");
            Assert.IsTrue(attr.SequenceEqual(expected), "attr.SequenceEqual(expected)");
        }
        
        [Test]
        public void WithIEnumerable_IntoBaseAttribute() {
            var sourceValues = Enumerable.Range(0, 100).Select(_ => Rand.Float);
            var destAttr = new FloatAttribute("dest", sourceValues) { Domain = AttributeDomain.Vertex };

            var sourceValues2 = Enumerable.Range(0, 100).Select(_ => Rand.Bool).ToList();
            var attr = sourceValues2.Into((BaseAttribute)destAttr);
            var expected = sourceValues2.Select(b => b ? 1.0f : 0.0f);
            
            
            Assert.AreSame(attr, destAttr, "attr == destAttr");
            Assert.IsTrue(attr.Values.SequenceEqual(expected.Convert(input => (object)input)), "attr.Values.SequenceEqual(expected.Convert(input => (object)input))");
        }
    }
}