using System;
using System.Collections;
using System.Linq;
using Attribute;
using NUnit.Framework;
using UnityCommons;

namespace Tests.Unit.Attribute.Operations {
    public class AttributeYield {
        private static readonly Func<float, float> func = a => a * a;
        private static readonly Func<object, object> funcObjParams = a => (float)a * (float)a;

        
        [Test] 
        public void BaseAttribute_YieldNull() {
            var values = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var attribute = new FloatAttribute("attr", values);
            var result = attribute.Yield(null);
            
            Assert.IsTrue(result.SequenceEqual(attribute), "result.SequenceEqual(attribute)");
        }

        [Test] 
        public void BaseAttribute_Yield() {
            var values = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var attribute = new FloatAttribute("attr", values);
            var expected = values.Select(func);
            var actual = attribute.Yield(func);
            
            Assert.IsTrue(actual.SequenceEqual(expected), "actual.SequenceEqual(expected)");
        }

        [Test] 
        public void IEnumerable_YieldNull() {
            var values = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var result = ((IEnumerable)values).Yield(null).Convert(o => (float)o);
            
            Assert.IsTrue(result.SequenceEqual(values), "result.SequenceEqual(values)");
        }

        [Test] 
        public void IEnumerable_Yield() {
            var values = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var expected = values.Select(func);
            var actual = values.Yield(funcObjParams).Convert(o => (float)o);
            
            Assert.IsTrue(actual.SequenceEqual(expected), "actual.SequenceEqual(expected)");
        }

        [Test] 
        public void IEnumerable_YieldNull_T() {
            var values = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var result = values.Yield(null);
            
            Assert.IsTrue(result.SequenceEqual(values), "result.SequenceEqual(values)");
        }

        [Test] 
        public void IEnumerable_Yield_T() {
            var values = Enumerable.Range(0, 100).Select(_ => Rand.Float).ToList();
            var expected = values.Select(func);
            var actual = values.Yield(func);
            
            Assert.IsTrue(actual.SequenceEqual(expected), "actual.SequenceEqual(expected)");
        }
    }
}