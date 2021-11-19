using System;
using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode(OutputPath = "_Generated")]
    public partial class FloatMathNode {
        [In] public float X { get; private set; }
        [In] public float Y { get; private set; }
        [In] public float Tolerance { get; private set; }
        [In] public float Extra { get; private set; }
        [Setting] public FloatMathNode_Operation Operation { get; private set; }
        [Out] public float Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        private float GetResult() {
            return Operation switch {
                FloatMathNode_Operation.Add => X + Y,
                FloatMathNode_Operation.Subtract => X - Y,
                FloatMathNode_Operation.Multiply => X * Y,
                FloatMathNode_Operation.Divide => X / Y,
                FloatMathNode_Operation.Power => math.pow(X, Y),
                FloatMathNode_Operation.Logarithm => MathF.Log(X, Y),
                FloatMathNode_Operation.SquareRoot => math.sqrt(X),
                FloatMathNode_Operation.InverseSquareRoot => math.rsqrt(X),
                FloatMathNode_Operation.Absolute => math.abs(X),
                FloatMathNode_Operation.Exponent => math.exp(X),

                FloatMathNode_Operation.Minimum => math.min(X, Y),
                FloatMathNode_Operation.Maximum => math.max(X, Y),
                FloatMathNode_Operation.LessThan => X < Y ? 1.0f : 0.0f,
                FloatMathNode_Operation.GreaterThan => X > Y ? 1.0f : 0.0f,
                FloatMathNode_Operation.Sign => X < 0 ? -1.0f : X == 0.0f ? 0.0f : 1.0f,
                FloatMathNode_Operation.Compare => math.abs(X - Y) < Tolerance ? 1.0f : 0.0f,
                FloatMathNode_Operation.SmoothMinimum => math_ext.smooth_min(X, Y, Tolerance),
                FloatMathNode_Operation.SmoothMaximum => math_ext.smooth_max(X, Y, Tolerance),

                FloatMathNode_Operation.Round => math.round(X),
                FloatMathNode_Operation.Floor => math.floor(X),
                FloatMathNode_Operation.Ceil => math.ceil(X),
                FloatMathNode_Operation.Truncate => (int)X,
                FloatMathNode_Operation.Fraction => X - (int)X,
                FloatMathNode_Operation.Modulo => MathF.IEEERemainder(X, Y),
                FloatMathNode_Operation.Wrap => X = math_ext.wrap(X, Y, Extra),
                FloatMathNode_Operation.Snap => math.round(X / Y) * Y,

                FloatMathNode_Operation.Sine => math.sin(X),
                FloatMathNode_Operation.Cosine => math.cos(X),
                FloatMathNode_Operation.Tangent => math.tan(X),
                FloatMathNode_Operation.Arcsine => math.asin(X),
                FloatMathNode_Operation.Arccosine => math.acos(X),
                FloatMathNode_Operation.Arctangent => math.atan(X),
                FloatMathNode_Operation.Atan2 => math.atan2(X, Y),
                FloatMathNode_Operation.ToRadians => math.radians(X),
                FloatMathNode_Operation.ToDegrees => math.degrees(X),
                FloatMathNode_Operation.Lerp => math.lerp(X, Y, Extra),
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