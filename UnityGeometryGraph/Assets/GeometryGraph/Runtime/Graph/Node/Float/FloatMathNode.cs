using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class FloatMathNode {
        [In] public float X { get; private set; }
        [In] public float Y { get; private set; }
        [In] public float Tolerance { get; private set; }
        [In] public float Extra { get; private set; }
        [Setting] public FloatMathNode_Operation Operation { get; private set; }
        [Out] public float Result { get; private set; }
        
        private readonly List<float> results = new();
        private bool resultsDirty = true;

        [GetterMethod(nameof(Result), Inline = true)]
        private float GetResult() {
            return CalculateResult(X, Y, Tolerance, Extra);
        }

        [CalculatesAllProperties]
        private void MarkResultsDirty() => resultsDirty = true;

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ResultPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }
                
                yield break;
            }

            List<float> xs = GetValues(XPort, count, X).ToList();
            List<float> ys = GetValues(YPort, count, Y).ToList();
            List<float> tolerances = GetValues(TolerancePort, count, Tolerance).ToList();
            List<float> extras = GetValues(ExtraPort, count, Extra).ToList();
            results.Clear();
            
            for (int i = 0; i < count; i++) {
                float result = CalculateResult(xs[i], ys[i], tolerances[i], extras[i]);
                results.Add(result);
                yield return result;
            }
            
            resultsDirty = false;
        }

        private float CalculateResult(float x, float y, float tolerance, float extra) {
            return Operation switch {
                FloatMathNode_Operation.Add => x + y,
                FloatMathNode_Operation.Subtract => x - y,
                FloatMathNode_Operation.Multiply => x * y,
                FloatMathNode_Operation.Divide => x / y,
                FloatMathNode_Operation.Power => math.pow(x, y),
                FloatMathNode_Operation.Logarithm => MathF.Log(x, y),
                FloatMathNode_Operation.SquareRoot => math.sqrt(x),
                FloatMathNode_Operation.InverseSquareRoot => math.rsqrt(x),
                FloatMathNode_Operation.Absolute => math.abs(x),
                FloatMathNode_Operation.Exponent => math.exp(x),

                FloatMathNode_Operation.Minimum => math.min(x, y),
                FloatMathNode_Operation.Maximum => math.max(x, y),
                FloatMathNode_Operation.LessThan => x < y ? 1.0f : 0.0f,
                FloatMathNode_Operation.GreaterThan => x > y ? 1.0f : 0.0f,
                FloatMathNode_Operation.Sign => x < 0 ? -1.0f : x == 0.0f ? 0.0f : 1.0f,
                FloatMathNode_Operation.Compare => math.abs(x - y) < tolerance ? 1.0f : 0.0f,
                FloatMathNode_Operation.SmoothMinimum => math_ext.smooth_min(x, y, tolerance),
                FloatMathNode_Operation.SmoothMaximum => math_ext.smooth_max(x, y, tolerance),

                FloatMathNode_Operation.Round => math.round(x),
                FloatMathNode_Operation.Floor => math.floor(x),
                FloatMathNode_Operation.Ceil => math.ceil(x),
                FloatMathNode_Operation.Truncate => (int)x,
                FloatMathNode_Operation.Fraction => x - (int)x,
                FloatMathNode_Operation.Modulo => MathF.IEEERemainder(x, y),
                FloatMathNode_Operation.Wrap => math_ext.wrap(x, y, extra),
                FloatMathNode_Operation.Snap => math.round(x / y) * y,

                FloatMathNode_Operation.Sine => math.sin(x),
                FloatMathNode_Operation.Cosine => math.cos(x),
                FloatMathNode_Operation.Tangent => math.tan(x),
                FloatMathNode_Operation.Arcsine => math.asin(x),
                FloatMathNode_Operation.Arccosine => math.acos(x),
                FloatMathNode_Operation.Arctangent => math.atan(x),
                FloatMathNode_Operation.Atan2 => math.atan2(x, y),
                FloatMathNode_Operation.ToRadians => math.radians(x),
                FloatMathNode_Operation.ToDegrees => math.degrees(x),
                FloatMathNode_Operation.Lerp => math.lerp(x, y, extra),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public enum FloatMathNode_Operation {
            // Operations
            Add = 0, Subtract = 1, Multiply = 2, Divide = 3, Power = 4,
            Logarithm = 5, SquareRoot = 6, InverseSquareRoot = 7, Absolute = 8, Exponent = 9,

            // Comparison
            Minimum = 10, Maximum = 11, LessThan = 12, GreaterThan = 13,
            Sign = 14, Compare = 15, SmoothMinimum = 16, SmoothMaximum = 17,

            // Rounding
            Round = 18, Floor = 19, Ceil = 20, Truncate = 21,
            Fraction = 22, Modulo = 23, Wrap = 24, Snap = 25,

            // Trig
            Sine = 26, Cosine = 27, Tangent = 28,
            Arcsine = 29, Arccosine = 30, Arctangent = 31, [DisplayName("Atan2")] Atan2 = 32,

            // Conversion
            ToRadians = 33, ToDegrees = 34,
            
            // Added later, was too lazy to redo numbers
            Lerp = 35,
        }
    }
}