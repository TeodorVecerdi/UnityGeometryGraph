using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityCommons;
using UnityEngine;

namespace GeometryGraph.Runtime.Attribute {
    [Serializable]
    public abstract class BaseAttribute : IEnumerable, ICloneable {
        private static readonly Type objectType = typeof(object);
        
        public abstract AttributeType Type { get; }
        public virtual Type ElementType => objectType;
        
        public string Name;
        public AttributeDomain Domain;
        public List<object> Values;

        public int Count => Values.Count;

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

        public object Clone() {
            var clone = (BaseAttribute) Activator.CreateInstance(GetType(), Name);
            clone.Domain = Domain;
            clone.Fill(Values);
            return clone;
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
            Fill(values.Cast<T>());
        }

        public virtual void Fill(IEnumerable<T> values) {
            Values.Clear();
            foreach (var value in values) {
                Values.Add(value);
            }
        }

        public IEnumerable<T> Yield(Func<T, T> action) {
            action ??= AttributeActions.NoOp<T>();
            
            foreach (T value in Values) {
                yield return action(value);
            }
        }

        public IEnumerable<T> YieldWithAttribute<T0>(BaseAttribute<T0> other, Func<T, T0, T> action) {
            action ??= AttributeActions.NoOp<T, T0>();
           
            if (other == null) {
                foreach (T value in Values) {
                    yield return action(value, default);
                }
                yield break;
            }
            
            var otherIndex = 0;
            foreach (T value in Values) {
                yield return action(value, otherIndex >= other.Values.Count ? default : other[otherIndex]);

                otherIndex++;
            }

            if (otherIndex >= other.Values.Count) yield break;
            
            for (var i = otherIndex; i < other.Values.Count; i++) {
                yield return action(default, other[i]);
            }
        }

        public IEnumerable<T> YieldWithAttribute<T0, T1>(BaseAttribute<T0> attribute0, BaseAttribute<T1> attribute1, Func<T, T0, T1, T> action) {
            action ??= AttributeActions.NoOp<T, T0, T1>();
            
            
            if (attribute0 == null && attribute1 == null) {
                foreach (T value in Values) {
                    yield return action(value, default, default);
                }
                yield break;
            }
            
            if (attribute0 == null && attribute1 != null) {
                var index = 0;
                foreach (T value in Values) {
                    yield return action(value, default, index >= attribute1.Values.Count ? default : attribute1[index]);
                }
                
                if (index >= attribute1.Count) yield break;
                
                for(var i = index; i < attribute1.Count; i++) {
                    yield return action(default, default, attribute1[i]);
                }
                
                yield break;
            } 
            
            if (attribute0 != null && attribute1 == null) {
                var index = 0;
                foreach (T value in Values) {
                    yield return action(value, index >= attribute0.Values.Count ? default : attribute0[index], default);
                }
                
                if (index >= attribute0.Count) yield break;
                
                for(var i = index; i < attribute0.Count; i++) {
                    yield return action(default, attribute0[i], default);
                }
                
                yield break;
            }

            var currentIndex = 0;
            var maxIndex = Mathf.Max(Count, attribute0.Count, attribute1.Count);
            for (var i = 0; i < maxIndex; i++) {
                var self = currentIndex < Count ? this[currentIndex] : default;
                var a0 = currentIndex < attribute0.Count ? attribute0[currentIndex] : default;
                var a1 = currentIndex < attribute1.Count ? attribute1[currentIndex] : default;
                yield return action(self, a0, a1);
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
        private static BaseAttribute Into(this IEnumerable values, string name, AttributeDomain domain, Type attributeType) {
            var attribute = (BaseAttribute) Activator.CreateInstance(attributeType, name);
            attribute.Domain = domain;
            var valuesList = values.Convert(o => o).ToList();
            var type = attribute.Type;
            if (valuesList.Count > 0) type = AttributeConvert.GetType(valuesList[0]);
            
            attribute.Fill(valuesList.Select(val => AttributeConvert.ConvertType<object>(val, type, attribute.Type)));
            return attribute;
        }

        private static BaseAttribute Into(this BaseAttribute attribute, string name, AttributeDomain? domain, Type attributeType) {
            var otherAttribute = (BaseAttribute) Activator.CreateInstance(attributeType, name);
            otherAttribute.Domain = domain ?? attribute.Domain;
            otherAttribute.Fill(attribute.Values.Select(val => AttributeConvert.ConvertType<object>(val, attribute.Type, otherAttribute.Type)));
            return otherAttribute;
        }

        public static BaseAttribute Into(this IEnumerable values, string name, AttributeType type, AttributeDomain domain) {
            return Into(values, name, domain, AttributeUtility.AttributeTypeToSystemType(type));
        }

        public static TAttribute Into<TAttribute>(this BaseAttribute attribute, string name, AttributeDomain? domain = null) where TAttribute : BaseAttribute {
            return (TAttribute)Into(attribute, name, new AttributeDomain?(domain ?? attribute.Domain), typeof(TAttribute));
        }

        public static TAttribute Into<TAttribute>(this IEnumerable values, string name, AttributeDomain domain) where TAttribute : BaseAttribute {
            return (TAttribute)Into(values, name, domain, typeof(TAttribute));
        }

        public static BaseAttribute Into(this IEnumerable values, BaseAttribute otherAttribute) {
            var valuesList = values.Convert(o => o).ToList();
            var type = otherAttribute.Type;
            if (valuesList.Count > 0) type = AttributeConvert.GetType(valuesList[0]);

            otherAttribute.Fill(valuesList.Select(val => AttributeConvert.ConvertType<object>(val, type, otherAttribute.Type)));
            return otherAttribute;
        }
        
        public static TAttribute Into<TAttribute>(this IEnumerable values, TAttribute otherAttribute) where TAttribute : BaseAttribute {
            var valuesList = values.Convert(o => o).ToList();
            var type = otherAttribute.Type;
            if (valuesList.Count > 0) type = AttributeConvert.GetType(valuesList[0]);

            otherAttribute.Fill(valuesList.Select(val => AttributeConvert.ConvertType<object>(val, type, otherAttribute.Type)));
            return otherAttribute;
        }

        public static void Print(this BaseAttribute attribute) {
            var sb = new StringBuilder($"\"{attribute.Name}\":\n");
            sb.AppendLine($"Config: [{attribute.Domain} ; {attribute.Type}]");
            sb.AppendLine($"Values: {attribute.Values.ToListString()}");
            Debug.Log(sb.ToString());
        }
        
        public static IEnumerable Yield(this IEnumerable enumerable, Func<object, object> action) {
            action ??= AttributeActions.NoOp<object>();

            foreach (var value in enumerable) {
                yield return action(value);
            }
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