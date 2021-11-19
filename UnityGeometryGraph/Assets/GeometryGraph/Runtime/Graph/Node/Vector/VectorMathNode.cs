using System;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class VectorMathNode : RuntimeNode {
        private VectorMathNode_Operation operation;
        private float3 x;
        private float3 y;
        private float3 wrapMax;
        private float ior;
        private float scale;
        private float distance;

        public RuntimePort XPort { get; private set; }
        public RuntimePort YPort { get; private set; }
        public RuntimePort WrapMaxPort { get; private set; }
        public RuntimePort IorPort { get; private set; }
        public RuntimePort ScalePort { get; private set; }
        public RuntimePort DistancePort { get; private set; }
        public RuntimePort VectorResultPort { get; private set; }
        public RuntimePort FloatResultPort { get; private set; }

        public VectorMathNode(string guid) : base(guid) {
            XPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            YPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            WrapMaxPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            IorPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ScalePort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            DistancePort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            VectorResultPort = RuntimePort.Create(PortType.Vector, PortDirection.Output, this);
            FloatResultPort = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
        }

        public void UpdateOperation(VectorMathNode_Operation newOperation) {
            if (newOperation == operation) return;

            operation = newOperation;
            NotifyPortValueChanged(FloatResultPort);
            NotifyPortValueChanged(VectorResultPort);
        }

        public void UpdateValue(object value, VectorMathNode_Which which) {
            switch (which) {
                case VectorMathNode_Which.X:
                    x = (float3)value;
                    break;
                case VectorMathNode_Which.Y:
                    y = (float3)value;
                    break;
                case VectorMathNode_Which.WrapMax:
                    wrapMax = (float3)value;
                    break;
                case VectorMathNode_Which.IOR:
                    ior = (float)value;
                    break;
                case VectorMathNode_Which.Scale:
                    scale = (float)value;
                    break;
                case VectorMathNode_Which.Distance:
                    distance = (float)value;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            NotifyPortValueChanged(FloatResultPort);
            NotifyPortValueChanged(VectorResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port == FloatResultPort) return CalculateFloat();
            if (port == VectorResultPort) return CalculateVector();
            return null;
        }

        private float CalculateFloat() {
            return operation switch {
                VectorMathNode_Operation.Length => math.length(x),
                VectorMathNode_Operation.LengthSquared => math.lengthsq(x),
                VectorMathNode_Operation.Distance => math.distance(x, y),
                VectorMathNode_Operation.DistanceSquared => math.distancesq(x, y),
                VectorMathNode_Operation.DotProduct => math.dot(x, y),
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
            };
        }

        private float3 CalculateVector() {
            return operation switch {
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
                VectorMathNode_Operation.Compare => new float3(Mathf.Abs(x.x - y.x) < distance ? 1.0f : 0.0f, 
                                                               Mathf.Abs(x.y - y.y) < distance ? 1.0f : 0.0f, 
                                                               Mathf.Abs(x.z - y.z) < distance ? 1.0f : 0.0f),
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
                VectorMathNode_Operation.Snap => new float3(Mathf.Round(x.x / y.x) * y.x,
                                                            Mathf.Round(x.y / y.y) * y.y,
                                                            Mathf.Round(x.z / y.z) * y.z),
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
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == VectorResultPort || port == FloatResultPort) return;
            if (port == XPort) {
                x = GetValue(XPort, x);
                NotifyPortValueChanged(VectorResultPort);
                NotifyPortValueChanged(FloatResultPort);
            } else if (port == YPort) {
                y = GetValue(YPort, y);
                NotifyPortValueChanged(VectorResultPort);
                NotifyPortValueChanged(FloatResultPort);
            } else if (port == WrapMaxPort) {
                wrapMax = GetValue(WrapMaxPort, wrapMax);
                NotifyPortValueChanged(VectorResultPort);
                NotifyPortValueChanged(FloatResultPort);
            } else if (port == IorPort) {
                var newValue = GetValue(IorPort, ior);
                if (Math.Abs(ior - newValue) > 0.000001f) {
                    ior = newValue;
                    NotifyPortValueChanged(VectorResultPort);
                    NotifyPortValueChanged(FloatResultPort);
                }
            } else if (port == ScalePort) {
                var newValue = GetValue(ScalePort, scale);
                if (Math.Abs(scale - newValue) > 0.000001f) {
                    scale = newValue;
                    NotifyPortValueChanged(VectorResultPort);
                    NotifyPortValueChanged(FloatResultPort);
                }
            } else if (port == DistancePort) {
                var newValue = GetValue(DistancePort, distance);
                if (Math.Abs(distance - newValue) > 0.000001f) {
                    distance = newValue;
                    NotifyPortValueChanged(VectorResultPort);
                    NotifyPortValueChanged(FloatResultPort);
                }
            }
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["o"] = (int)operation,
                ["x"] = JsonConvert.SerializeObject(x, float3Converter.Converter),
                ["y"] = JsonConvert.SerializeObject(y, float3Converter.Converter),
                ["w"] = JsonConvert.SerializeObject(wrapMax, float3Converter.Converter),
                ["i"] = ior,
                ["s"] = scale,
                ["d"] = distance,
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if (string.IsNullOrEmpty(json)) return;

            var data = JObject.Parse(json);
            operation = (VectorMathNode_Operation)data.Value<int>("o");
            x = JsonConvert.DeserializeObject<float3>(data.Value<string>("x")!, float3Converter.Converter);
            y = JsonConvert.DeserializeObject<float3>(data.Value<string>("y")!, float3Converter.Converter);
            wrapMax = JsonConvert.DeserializeObject<float3>(data.Value<string>("w")!, float3Converter.Converter);
            ior = data.Value<float>("i");
            scale = data.Value<float>("s");
            distance = data.Value<float>("d");
            NotifyPortValueChanged(VectorResultPort);
            NotifyPortValueChanged(FloatResultPort);
        }

        public enum VectorMathNode_Which { X = 0, Y = 1, WrapMax = 2, IOR = 3, Scale = 4, Distance = 5 }

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