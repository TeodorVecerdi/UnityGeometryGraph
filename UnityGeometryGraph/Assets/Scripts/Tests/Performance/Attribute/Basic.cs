using System.Linq;
using Attribute;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityCommons;

namespace Tests.Performance.Attribute {
    public class Basic {
        [Test, Performance]
        public void ClampedFloatIsClampedOnCreation() {
            Measure.Method(() => {
                var clampedAttribute = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100f, 100f)).Into<ClampedFloatAttribute>("clamped", AttributeDomain.Vertex);
            })
            .WarmupCount(10)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void ClampedFloatIsClampedAfterOperation() {
            Measure.Method(() => {
               var clampedAttributeA = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100f, 100f)).Into<ClampedFloatAttribute>("clampedA", AttributeDomain.Vertex);
               var clampedAttributeB = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100f, 100f)).Into<ClampedFloatAttribute>("clampedB", AttributeDomain.Vertex);
               var clampedAttribute = clampedAttributeA.YieldWithAttribute(clampedAttributeB, (f1, f2) => f1 + f2).Into<ClampedFloatAttribute>("clampedC", AttributeDomain.Vertex);
           })
           .WarmupCount(10)
           .MeasurementCount(10)
           .GC()
           .Run();
        }
    }
}