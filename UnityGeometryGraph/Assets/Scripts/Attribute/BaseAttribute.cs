using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.Serialization;
using UnityCommons;
using UnityEngine;

namespace Attribute {
    [Serializable]
    public abstract class BaseAttribute : IEnumerable {
        private static readonly Type objectType = typeof(object);
        
        public abstract AttributeType Type { get; }
        public virtual Type ElementType => objectType;
        
        public string Name;
        public AttributeDomain Domain;
        public List<object> Values;

        protected BaseAttribute(string name) {
            Name = name;
            Values = new List<object>();
        }

        public virtual void Fill(IEnumerable values) {
            Values.Clear();
            foreach (var value in values) {
                Values.Add(value);
            }
        }

        public object GetValue(int index) {
            return Values[index];
        }
        
        public object this[int index] {
            get => GetValue(index);
            set => Values[index] = value;
        }

        public IEnumerator GetEnumerator() {
            return Values.GetEnumerator();
        }
    }
    
    [Serializable]
    public abstract class BaseAttribute<T> : BaseAttribute, IEnumerable<T> {
        private static readonly Type elementType = typeof(T);
        public override Type ElementType => elementType;
        
        public new T GetValue(int index) {
            return (T) Values[index];
        }

        public void Execute(Func<T, T> action) {
            if(action == null) return;
            for (var i = 0; i < Values.Count; i++) {
                Values[i] = action((T)Values[i]);
            }
        }

        public sealed override void Fill(IEnumerable values) {
            Fill((IEnumerable<T>)values);
        }

        public virtual void Fill(IEnumerable<T> values) {
            Values.Clear();
            foreach (var value in values) {
                Values.Add(value);
            }
        }

        public IEnumerable<T> Yield(Func<T, T> action) {
            if (action == null) yield break;

            foreach (T value in Values) {
                yield return action(value);
            }
        }

        public IEnumerable<T> YieldWithAttribute(BaseAttribute other, Func<T, T, T> action) {
            if (action == null || other == null) yield break;

            var count = Math.Min(Values.Count, other.Values.Count);
            var extraSelf = Values.Count - count;
            var extraOther = other.Values.Count - count;
            
            for (var i = 0; i < count; i++) {
                yield return action((T)Values[i], AttributeConvert.ConvertType<T>(other.Values[i], other.Type, Type));
            }
            
            // Only one of these for loops will run
            for (var i = 0; i < extraSelf; i++) {
                yield return action((T)Values[count + i], default);
            }
            for (var i = 0; i < extraOther; i++) {
                yield return action(default, AttributeConvert.ConvertType<T>(other.Values[count + i], other.Type, Type));
            }
        }

        public new T this[int index] {
            get => GetValue(index);
            set => Values[index] = value;
        }

        protected BaseAttribute(string name) : base(name) {
        }

        protected BaseAttribute(string name, IEnumerable<T> values) : base(name) {
            Fill(values);
        }

        public new Enumerator GetEnumerator() => new Enumerator(base.GetEnumerator());
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(base.GetEnumerator());
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(base.GetEnumerator());

        
        public struct Enumerator : IEnumerator<T> {
            private readonly IEnumerator baseEnumerator;
            
            public Enumerator(IEnumerator baseEnumerator) {
                this.baseEnumerator = baseEnumerator;
            }
            
            public bool MoveNext() {
                return baseEnumerator.MoveNext();
            }

            public void Reset() {
                baseEnumerator.Reset();
            }

            public T Current => (T) baseEnumerator.Current;
            object IEnumerator.Current => Current;

            public void Dispose() {
                
            }
        }
    }

    public static class AttributeExtensions {
        public static BaseAttribute Into(this IEnumerable values, string name, AttributeDomain domain, Type attributeType) {
            var attribute = (BaseAttribute) Activator.CreateInstance(attributeType, name);
            attribute.Domain = domain;
            attribute.Fill(values);
            return attribute;
        }

        public static BaseAttribute Into(this BaseAttribute attribute, string name, AttributeDomain? domain, Type attributeType) {
            var otherAttribute = (BaseAttribute) Activator.CreateInstance(attributeType, name);
            otherAttribute.Domain = domain ?? attribute.Domain;
            otherAttribute.Fill(attribute.Values.Select(val => AttributeConvert.ConvertType<object>(val, attribute.Type, otherAttribute.Type)));
            return otherAttribute;
        }

        public static TAttribute Into<TAttribute>(this BaseAttribute attribute, string name, AttributeDomain? domain = null) where TAttribute : BaseAttribute {
            return (TAttribute)Into(attribute, name, new AttributeDomain?(domain ?? attribute.Domain) , typeof(TAttribute));
        }

        public static TAttribute Into<TAttribute>(this IEnumerable values, string name, AttributeDomain domain) where TAttribute : BaseAttribute {
            return (TAttribute)Into(values, name, domain, typeof(TAttribute));
        }

        public static BaseAttribute Into(this IEnumerable values, BaseAttribute otherAttribute) {
            otherAttribute.Fill(values);
            return otherAttribute;
        }
        
        public static TAttribute Into<TAttribute>(this IEnumerable values, TAttribute otherAttribute) where TAttribute : BaseAttribute {
            otherAttribute.Fill(values);
            return otherAttribute;
        }

        public static void Print(this BaseAttribute attribute) {
            var sb = new StringBuilder($"\"{attribute.Name}\":\n");
            sb.AppendLine($"Config: [{attribute.Domain} ; {attribute.Type}]");
            sb.AppendLine($"Values: {attribute.Values.ToListString()}");
            Debug.Log(sb.ToString());
        }

        public static IEnumerable<T> Yield<T>(this IEnumerable<T> enumerable, Func<T, T> action) {
            action ??= AttributeActions.NoOp<T>();

            foreach (var value in enumerable) {
                yield return action(value);
            }
        }
        
        public static IEnumerable<T> YieldWithAttribute<T>(this IEnumerable<T> enumerable, AttributeType selfType, BaseAttribute other, Func<T, T, T> action) {
            action ??= AttributeActions.NoOp<T, T>();
           
            if (other == null) {
                foreach (var value in enumerable) {
                    yield return action(value, default);
                }
                yield break;
            }

            var otherIndex = 0;
            foreach (var value in enumerable) {
                var otherValue = AttributeConvert.ConvertType<T>(otherIndex >= other.Values.Count ? default(T) : other.Values[otherIndex], other.Type, selfType);
                yield return action(value, otherValue);

                otherIndex++;
            }

            if (otherIndex >= other.Values.Count) yield break;
            
            for (var i = otherIndex; i < other.Values.Count; i++) {
                var otherValue = AttributeConvert.ConvertType<T>(other.Values[otherIndex], other.Type, selfType);
                yield return action(default, otherValue);
            }
        }
    }
}