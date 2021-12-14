using System;
using System.Collections.Generic;
using System.Linq;
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
        
        [Out] public float3 VectorResult { get; private set; }
        [Out] public float FloatResult { get; private set; }

        private readonly List<float3> vectorResults = new List<float3>();
        private readonly List<float> floatResults = new List<float>();
        private bool vectorResultDirty = true;
        private bool floatResultDirty = true;
        
        [CalculatesProperty(nameof(VectorResult))] private void MarkVectorResultDirty() => vectorResultDirty = true;
        [CalculatesProperty(nameof(FloatResult))] private void MarkFloatResultDirty() => floatResultDirty = true;

        [GetterMethod(nameof(FloatResult), Inline = true)]
        private float CalculateFloat() => CalculateFloat(X, Y);

        [GetterMethod(nameof(VectorResult), Inline = true)]
        private float3 CalculateVector() => CalculateVector(X, Y, WrapMax, IOR, Scale, Distance);

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (count <= 0) yield break;
            if (port == VectorResultPort) {
                if (!vectorResultDirty && vectorResults.Count == count) {
                    for (int i = 0; i < count; i++) {
                        yield return vectorResults[i];
                    }
                    yield break;
                }
                
                vectorResultDirty = false;
                vectorResults.Clear();
                List<float3> x = GetValues(XPort, count, X).ToList();
                List<float3> y = GetValues(YPort, count, Y).ToList();
                List<float3> wrapMax = GetValues(WrapMaxPort, count, WrapMax).ToList();
                List<float> ior = GetValues(IORPort, count, IOR).ToList();
                List<float> scale = GetValues(ScalePort, count, Scale).ToList();
                List<float> distance = GetValues(DistancePort, count, Distance).ToList();
                
                for (int i = 0; i < count; i++) {
                    float3 result = CalculateVector(x[i], y[i], wrapMax[i], ior[i], scale[i], distance[i]);
                    vectorResults.Add(result);
                    yield return result;
                }
            } else if (port == FloatResultPort) {
                if (!floatResultDirty && floatResults.Count == count) {
                    for (int i = 0; i < count; i++) {
                        yield return floatResults[i];
                    }
                    yield break;
                }
                
                floatResultDirty = false;
                floatResults.Clear();
                List<float3> x = GetValues(XPort, count, X).ToList();
                List<float3> y = GetValues(YPort, count, Y).ToList();
                for (int i = 0; i < count; i++) {
                    float result = CalculateFloat(x[i], y[i]);
                    floatResults.Add(result);
                    yield return result;
                }
            }
        }

        private float CalculateFloat(float3 x, float3 y) {
            return Operation switch {
                VectorMathNode_Operation.Length => math.length(x),
                VectorMathNode_Operation.LengthSquared => math.lengthsq(x),
                VectorMathNode_Operation.Distance => math.distance(x, y),
                VectorMathNode_Operation.DistanceSquared => math.distancesq(x, y),
                VectorMathNode_Operation.DotProduct => math.dot(x, y),
                _ => 0.0f,
            };
        }

        private float3 CalculateVector(float3 x, float3 y, float3 wrapMax, float ior, float scale, float distance) {
            return Operation switch {
                VectorMathNode_Operation.Add => x + y,
                VectorMathNode_Operation.Subtract => x - y,
                VectorMathNode_Operation.Multiply => x * y,
                VectorMathNode_Operation.Divide => x / y,
                VectorMathNode_Operation.Scale => x * scale,
                VectorMathNode_Operation.Normalize => math.normalize(x),
                VectorMathNode_Operation.CrossProduct => math.cross(x, y),
                VectorMathNode_Operation.Project => math.project(x, y),
                VectorMathNode_Operation.Reflect => math.reflect(x, y),
                VectorMathNode_Operation.Refract => math.refract(x, y, ior),
                VectorMathNode_Operation.Absolute => math.abs(x),
                VectorMathNode_Operation.Minimum => math.min(x, y),
                VectorMathNode_Operation.Maximum => math.max(x, y),
                VectorMathNode_Operation.LessThan => new float3(x.x < y.x ? 1.0f : 0.0f, x.y < y.y ? 1.0f : 0.0f, x.z < y.z ? 1.0f : 0.0f),
                VectorMathNode_Operation.GreaterThan => new float3(x.x > y.x ? 1.0f : 0.0f, x.y > y.y ? 1.0f : 0.0f, x.z > y.z ? 1.0f : 0.0f),
                VectorMathNode_Operation.Sign => math.sign(x),
                VectorMathNode_Operation.Compare => new float3(MathF.Abs(x.x - y.x) < distance ? 1.0f : 0.0f, 
                                                               MathF.Abs(x.y - y.y) < distance ? 1.0f : 0.0f, 
                                                               MathF.Abs(x.z - y.z) < distance ? 1.0f : 0.0f),
                VectorMathNode_Operation.SmoothMinimum => new float3(math_ext.smooth_min(x.x, y.x, distance), 
                                                                     math_ext.smooth_min(x.y, y.y, distance), 
                                                                     math_ext.smooth_min(x.z, y.z, distance)),
                VectorMathNode_Operation.SmoothMaximum => new float3(math_ext.smooth_max(x.x, y.x, distance), 
                                                                     math_ext.smooth_max(x.y, y.y, distance), 
                                                                     math_ext.smooth_max(x.z, y.z, distance)),
                VectorMathNode_Operation.Round => math.round(x),
                VectorMathNode_Operation.Floor => math.floor(x),
                VectorMathNode_Operation.Ceil => math.ceil(x),
                VectorMathNode_Operation.Truncate => math.trunc(x),
                VectorMathNode_Operation.Fraction => math.frac(x),
                VectorMathNode_Operation.Modulo => math.fmod(x, y),
                VectorMathNode_Operation.Wrap => new float3(math_ext.wrap(x.x, y.x, wrapMax.x),
                                                            math_ext.wrap(x.y, y.y, wrapMax.y),
                                                            math_ext.wrap(x.z, y.z, wrapMax.z)),
                VectorMathNode_Operation.Snap => new float3(MathF.Round(x.x / y.x) * y.x,
                                                            MathF.Round(x.y / y.y) * y.y,
                                                            MathF.Round(x.z / y.z) * y.z),
                VectorMathNode_Operation.Sine => math.sin(x),
                VectorMathNode_Operation.Cosine => math.cos(x),
                VectorMathNode_Operation.Tangent => math.tan(x),
                VectorMathNode_Operation.Arcsine => math.asin(x),
                VectorMathNode_Operation.Arccosine => math.acos(x),
                VectorMathNode_Operation.Arctangent => math.atan(x),
                VectorMathNode_Operation.Atan2 => math.atan2(x, y),
                VectorMathNode_Operation.ToRadians => math.radians(x),
                VectorMathNode_Operation.ToDegrees => math.degrees(x),
                VectorMathNode_Operation.Lerp => math.lerp(x, y, distance),
                _ => float3.zero,
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