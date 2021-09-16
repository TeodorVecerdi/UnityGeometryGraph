using System.Linq;
using GeometryGraph.Runtime.Attribute;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityCommons;

namespace GeometryGraph.Tests.Runtime.Performance.Attribute {
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
            var clampedAttributeA = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100f, 100f)).Into<ClampedFloatAttribute>("clampedA", AttributeDomain.Vertex);
            var clampedAttributeB = Enumerable.Range(0, 100).Select(_ => Rand.Range(-100f, 100f)).Into<ClampedFloatAttribute>("clampedB", AttributeDomain.Vertex);
            Measure.Method(() => {
               var clampedAttribute = clampedAttributeA.YieldWithAttribute(clampedAttributeB, (f1, f2) => f1 + f2).Into<ClampedFloatAttribute>("clampedC", AttributeDomain.Vertex);
           })
           .WarmupCount(10)
           .MeasurementCount(10)
           .GC()
           .Run();
        }
    }
}