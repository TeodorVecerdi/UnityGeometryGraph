using System;
using System.Linq;
using GeometryGraph.Runtime.AttributeSystem;
using NUnit.Framework;
using UnityCommons;

namespace GeometryGraph.Tests.Runtime.Unit.Attribute.Operations {
    public class AttributeExecute {
        [Test]
        public void ExecuteNull() {
            var values = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var attribute = new FloatAttribute("attr", values) { Domain = AttributeDomain.Vertex };

            attribute.Execute(null);
            Assert.IsTrue(attribute.SequenceEqual(values), "attribute.SequenceEqual(values)");
        }

        [Test]
        public void ExecuteNotNull() {
            var values = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            Func<float, float> func = a => a * a;

            var calculated = values.Select(func);

            var attribute = new FloatAttribute("attr", values) { Domain = AttributeDomain.Vertex };
            attribute.Execute(func);

            Assert.IsTrue(attribute.SequenceEqual(calculated), "attribute.SequenceEqual(calculated)");
        }
    }
}