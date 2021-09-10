using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityCommons;
using UnityEngine;

namespace Attributes {
    [Serializable]
    public abstract class BaseAttribute {
        protected internal string Name;
        protected internal AttributeDomain Domain;
        protected internal abstract AttributeType Type { get; }
        protected internal List<object> Values;

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
    }
    
    [Serializable]
    public abstract class BaseAttribute<T> : BaseAttribute {
        public T GetValue(int index) {
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

        public T this[int index] {
            get => GetValue(index);
            set => Values[index] = value;
        }

        protected BaseAttribute(string name) : base(name) {
        }

        protected BaseAttribute(string name, IEnumerable<T> values) : base(name) {
            Fill(values);
        }
    }

    public static class AttributeExtensions {
        public static BaseAttribute Into(this IEnumerable values, string name, Type attributeType) {
            var attribute = (BaseAttribute) Activator.CreateInstance(attributeType, name);
            attribute.Fill(values);
            return attribute;
        }

        public static BaseAttribute Into(this BaseAttribute attribute, string name, Type attributeType) {
            var otherAttribute = (BaseAttribute) Activator.CreateInstance(attributeType, name);
            otherAttribute.Fill(attribute.Values.Select(val => AttributeConvert.ConvertType<object>(val, attribute.Type, otherAttribute.Type)));
            return otherAttribute;
        }

        public static TAttribute Into<TAttribute>(this BaseAttribute attribute, string name) where TAttribute : BaseAttribute {
            return (TAttribute)Into(attribute, name, typeof(TAttribute));
        }

        public static TAttribute Into<TAttribute>(this IEnumerable values, string name) where TAttribute : BaseAttribute {
            return (TAttribute)Into(values, name, typeof(TAttribute));
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
            action ??= AttributeActions.NoOp2<T>();
           
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