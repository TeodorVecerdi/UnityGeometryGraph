using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class IntegerMathNode {
        [In] public int X { get; private set; }
        [In] public int Y { get; private set; }
        [In] public float Tolerance { get; private set; }
        [In] public int Extra { get; private set; }
        [Setting] public IntegerMathNode_Operation Operation { get; private set; }
        [Out] public int Result { get; private set; }

        private readonly List<int> results = new();
        private bool resultsDirty = true;

        [CalculatesAllProperties] private void MarkResultsDirty() => resultsDirty = true;

        [GetterMethod(nameof(Result))]
        private int Calculate() {
            return Calculate(X, Y, Tolerance, Extra);
        }

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if(port != ResultPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }

                yield break;
            }

            List<int> xs = GetValues(XPort, count, X).ToList();
            List<int> ys = GetValues(YPort, count, Y).ToList();
            List<float> tolerances = GetValues(TolerancePort, count, Tolerance).ToList();
            List<int> extras = GetValues(ExtraPort, count, Extra).ToList();
            results.Clear();
            
            for (int i = 0; i < count; i++) {
                int result = Calculate(xs[i], ys[i], tolerances[i], extras[i]);
                results.Add(result);
                yield return result;
            }
            
            resultsDirty = false;
        }

        private int Calculate(int x, int y, float tolerance, int extra) {
            return Operation switch {
                IntegerMathNode_Operation.Add => x + y,
                IntegerMathNode_Operation.Subtract => x - y,
                IntegerMathNode_Operation.Multiply => x * y,
                IntegerMathNode_Operation.IntegerDivision => x / y,
                IntegerMathNode_Operation.FloatDivision => (int)(x / (float)y),
                IntegerMathNode_Operation.Power => (int)Math.Pow(x, y),
                IntegerMathNode_Operation.Logarithm => (int)Math.Log(x, y),
                IntegerMathNode_Operation.SquareRoot => (int)Math.Sqrt(x),
                IntegerMathNode_Operation.Absolute => Math.Abs(x),
                IntegerMathNode_Operation.Exponent => (int)Math.Exp(x),

                IntegerMathNode_Operation.Minimum => Math.Min(x, y),
                IntegerMathNode_Operation.Maximum => Math.Max(x, y),
                IntegerMathNode_Operation.LessThan => x < y ? 1 : 0,
                IntegerMathNode_Operation.GreaterThan => x > y ? 1 : 0,
                IntegerMathNode_Operation.Sign => x < 0 ? -1 : x == 0 ? 0 : 1,
                IntegerMathNode_Operation.Compare => x == y ? 1 : 0,
                IntegerMathNode_Operation.SmoothMinimum => (int)math_ext.smooth_min(x, y, tolerance),
                IntegerMathNode_Operation.SmoothMaximum => (int)math_ext.smooth_max(x, y, tolerance),

                IntegerMathNode_Operation.Modulo => x % y,
                IntegerMathNode_Operation.Wrap => ((x - y) % (extra - y) + (extra - y)) % (extra - y) + y,
                IntegerMathNode_Operation.Snap => (int) Math.Round((float)x / y) * y,

                _ => throw new ArgumentOutOfRangeException(nameof(Operation), Operation, null)
            };
        }

        public enum IntegerMathNode_Operation {
            // Operations
            Add = 0, Subtract = 1, Multiply = 2, IntegerDivision = 3, Power = 4,
            Logarithm = 5, SquareRoot = 6, FloatDivision = 7, Absolute = 8, Exponent = 9,

            // Comparison
            Minimum = 10, Maximum = 11, LessThan = 12, GreaterThan = 13,
            Sign = 14, Compare = 15, SmoothMinimum = 16, SmoothMaximum = 17,

            // Rounding
            Modulo = 18, Wrap = 19, Snap = 20
        }
    }
}