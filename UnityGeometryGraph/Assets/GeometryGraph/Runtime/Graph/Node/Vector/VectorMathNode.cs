using System;
using GeometryGraph.Runtime.Attributes;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class VectorMathNode {
        [Setting] 
        public VectorMathNode_Operation Operation { get; private set; }
        
        [In(GenerateEquality = false)] 
        public float3 X { get; private set; }
        
        [In(GenerateEquality = false)] 
        public float3 Y { get; private set; }
        
        [In(GenerateEquality = false)] 
        public float3 WrapMax { get; private set; }
        
        [In, UpdatesProperties(nameof(VectorResult))] 
        public float IOR { get; private set; }
        
        [In, UpdatesProperties(nameof(VectorResult))] 
        public float Scale { get; private set; }
        
        [In, UpdatesProperties(nameof(VectorResult))] 
        public float Distance { get; private set; }
        
        [Out] 
        public float3 VectorResult { get; private set; }
        [Out] 
        public float FloatResult { get; private set; }


        [GetterMethod(nameof(FloatResult))]
        private float CalculateFloat() {
            return Operation switch {
                VectorMathNode_Operation.Length => math.length(X),
                VectorMathNode_Operation.LengthSquared => math.lengthsq(X),
                VectorMathNode_Operation.Distance => math.distance(X, Y),
                VectorMathNode_Operation.DistanceSquared => math.distancesq(X, Y),
                VectorMathNode_Operation.DotProduct => math.dot(X, Y),
                _ => throw new ArgumentOutOfRangeException(nameof(Operation), Operation, null)
            };
        }

        [GetterMethod(nameof(VectorResult))]
        private float3 CalculateVector() {
            return Operation switch {
                VectorMathNode_Operation.Add => X + Y,
                VectorMathNode_Operation.Subtract => X - Y,
                VectorMathNode_Operation.Multiply => X * Y,
                VectorMathNode_Operation.Divide => X / Y,
                VectorMathNode_Operation.Scale => X * Scale,
                VectorMathNode_Operation.Normalize => math.normalize(X),
                VectorMathNode_Operation.CrossProduct => math.cross(X, Y),
                VectorMathNode_Operation.Project => math.project(X, Y),
                VectorMathNode_Operation.Reflect => math.reflect(X, Y),
                VectorMathNode_Operation.Refract => math.refract(X, Y, IOR),
                VectorMathNode_Operation.Absolute => math.abs(X),
                VectorMathNode_Operation.Minimum => math.min(X, Y),
                VectorMathNode_Operation.Maximum => math.max(X, Y),
                VectorMathNode_Operation.LessThan => new float3(X.x < Y.x ? 1.0f : 0.0f, X.y < Y.y ? 1.0f : 0.0f, X.z < Y.z ? 1.0f : 0.0f),
                VectorMathNode_Operation.GreaterThan => new float3(X.x > Y.x ? 1.0f : 0.0f, X.y > Y.y ? 1.0f : 0.0f, X.z > Y.z ? 1.0f : 0.0f),
                VectorMathNode_Operation.Sign => math.sign(X),
                VectorMathNode_Operation.Compare => new float3(MathF.Abs(X.x - Y.x) < Distance ? 1.0f : 0.0f, 
                                                               MathF.Abs(X.y - Y.y) < Distance ? 1.0f : 0.0f, 
                                                               MathF.Abs(X.z - Y.z) < Distance ? 1.0f : 0.0f),
                VectorMathNode_Operation.SmoothMinimum => new float3(math_ext.smooth_min(X.x, Y.x, Distance), 
                                                                     math_ext.smooth_min(X.y, Y.y, Distance), 
                                                                     math_ext.smooth_min(X.z, Y.z, Distance)),
                VectorMathNode_Operation.SmoothMaximum => new float3(math_ext.smooth_max(X.x, Y.x, Distance), 
                                                                     math_ext.smooth_max(X.y, Y.y, Distance), 
                                                                     math_ext.smooth_max(X.z, Y.z, Distance)),
                VectorMathNode_Operation.Round => math.round(X),
                VectorMathNode_Operation.Floor => math.floor(X),
                VectorMathNode_Operation.Ceil => math.ceil(X),
                VectorMathNode_Operation.Truncate => math.trunc(X),
                VectorMathNode_Operation.Fraction => math.frac(X),
                VectorMathNode_Operation.Modulo => math.fmod(X, Y),
                VectorMathNode_Operation.Wrap => new float3(math_ext.wrap(X.x, Y.x, WrapMax.x),
                                                            math_ext.wrap(X.y, Y.y, WrapMax.y),
                                                            math_ext.wrap(X.z, Y.z, WrapMax.z)),
                VectorMathNode_Operation.Snap => new float3(MathF.Round(X.x / Y.x) * Y.x,
                                                            MathF.Round(X.y / Y.y) * Y.y,
                                                            MathF.Round(X.z / Y.z) * Y.z),
                VectorMathNode_Operation.Sine => math.sin(X),
                VectorMathNode_Operation.Cosine => math.cos(X),
                VectorMathNode_Operation.Tangent => math.tan(X),
                VectorMathNode_Operation.Arcsine => math.asin(X),
                VectorMathNode_Operation.Arccosine => math.acos(X),
                VectorMathNode_Operation.Arctangent => math.atan(X),
                VectorMathNode_Operation.Atan2 => math.atan2(X, Y),
                VectorMathNode_Operation.ToRadians => math.radians(X),
                VectorMathNode_Operation.ToDegrees => math.degrees(X),
                VectorMathNode_Operation.Lerp => math.lerp(X, Y, Distance),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public enum VectorMathNode_Operation {
            
            // Operations
            Add = 0, Subtract = 1, Multiply = 2, Divide = 3,
            // -
            Scale = 4, Length = 5, LengthSquared = 6, Distance = 7, DistanceSquared = 8, Normalize = 9,
            // -
            DotProduct = 10, CrossProduct = 11, Project = 12, Reflect = 13, Refract = 14,
            
            // Per-Component Comparison
            Absolute = 15, Minimum = 16, Maximum = 17, LessThan = 18, GreaterThan = 19,
            Sign = 20, Compare = 21, SmoothMinimum = 22, SmoothMaximum = 23,

            // Rounding
            Round = 24, Floor = 25, Ceil = 26, Truncate = 27,
            Fraction = 28, Modulo = 29, Wrap = 30, Snap = 31,

            // Trig
            Sine = 32, Cosine = 33, Tangent = 34,
            Arcsine = 35, Arccosine = 36, Arctangent = 37, [DisplayName("Atan2")] Atan2 = 38,
            
            // Conversion
            ToRadians = 39, ToDegrees = 40,
            
            // Added later, was too lazy to redo numbers
            Lerp = 41,
        }
    }
}