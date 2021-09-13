using System.Linq;
using Attribute;
using NUnit.Framework;
using UnityCommons;

namespace Tests.Unit.Attribute {
    public class Basic {
        [Test]
        public void ClampedFloatIsClampedOnCreation() {
            var clampedAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100f, 100f)).Into<ClampedFloatAttribute>("clamped", AttributeDomain.Vertex);
            Assert.True(clampedAttribute.All(f => f >= 0.0f && f <= 1.0f));
        }

        [Test]
        public void ClampedFloatIsClampedAfterOperation() {
            var clampedAttributeA = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100f, 100f)).Into<ClampedFloatAttribute>("clampedA", AttributeDomain.Vertex);
            var clampedAttributeB = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100f, 100f)).Into<ClampedFloatAttribute>("clampedB", AttributeDomain.Vertex);
            var clampedAttribute = clampedAttributeA.YieldWithAttribute(clampedAttributeB, (f1, f2) => f1 + f2).Into<ClampedFloatAttribute>("clampedC", AttributeDomain.Vertex);
            Assert.True(clampedAttribute.All(f => f >= 0.0f && f <= 1.0f));
        }
    }
}