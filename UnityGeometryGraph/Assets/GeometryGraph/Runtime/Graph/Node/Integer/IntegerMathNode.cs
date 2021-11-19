using System;
using GeometryGraph.Runtime.Attributes;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettings(OutputPath = "_Generated")]
    public partial class IntegerMathNode {
        [In] public int X { get; private set; }
        [In] public int Y { get; private set; }
        [In] public float Tolerance { get; private set; }
        [In] public int Extra { get; private set; }
        [Setting] public IntegerMathNode_Operation Operation { get; private set; }
        [Out] public int Result { get; private set; }

        [GetterMethod(nameof(Result))]
        private int Calculate() {
            return Operation switch {
                IntegerMathNode_Operation.Add => X + Y,
                IntegerMathNode_Operation.Subtract => X - Y,
                IntegerMathNode_Operation.Multiply => X * Y,
                IntegerMathNode_Operation.IntegerDivision => X / Y,
                IntegerMathNode_Operation.FloatDivision => (int)(X / (float)Y),
                IntegerMathNode_Operation.Power => (int)Math.Pow(X, Y),
                IntegerMathNode_Operation.Logarithm => (int)Math.Log(X, Y),
                IntegerMathNode_Operation.SquareRoot => (int)Math.Sqrt(X),
                IntegerMathNode_Operation.Absolute => Math.Abs(X),
                IntegerMathNode_Operation.Exponent => (int)Math.Exp(X),

                IntegerMathNode_Operation.Minimum => Math.Min(X, Y),
                IntegerMathNode_Operation.Maximum => Math.Max(X, Y),
                IntegerMathNode_Operation.LessThan => X < Y ? 1 : 0,
                IntegerMathNode_Operation.GreaterThan => X > Y ? 1 : 0,
                IntegerMathNode_Operation.Sign => X < 0 ? -1 : X == 0 ? 0 : 1,
                IntegerMathNode_Operation.Compare => X == Y ? 1 : 0,
                IntegerMathNode_Operation.SmoothMinimum => (int)math_ext.smooth_min(X, Y, Tolerance),
                IntegerMathNode_Operation.SmoothMaximum => (int)math_ext.smooth_max(X, Y, Tolerance),

                IntegerMathNode_Operation.Modulo => X % Y,
                IntegerMathNode_Operation.Wrap => X = ((X - Y) % (Extra - Y) + (Extra - Y)) % (Extra - Y) + Y,
                IntegerMathNode_Operation.Snap => (int) Math.Round((float)X / Y) * Y,

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