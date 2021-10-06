using System;
using GeometryGraph.Runtime.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class FloatMathNode : RuntimeNode {
        private FloatMathNode_MathOperation operation;
        private float x;
        private float y;
        private float tolerance;
        private float extra;

        public RuntimePort XPort { get; private set; }
        public RuntimePort YPort { get; private set; }
        public RuntimePort TolerancePort { get; private set; }
        public RuntimePort ExtraPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public FloatMathNode(string guid) : base(guid) {
            XPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            YPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            TolerancePort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ExtraPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
        }

        public void UpdateOperation(FloatMathNode_MathOperation newOperation) {
            if (newOperation == operation) return;

            operation = newOperation;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateValue(float value, FloatMathNode_Which which) {
            switch (which) {
                case FloatMathNode_Which.X:
                    x = value;
                    break;
                case FloatMathNode_Which.Y:
                    y = value;
                    break;
                case FloatMathNode_Which.Tolerance:
                    tolerance = value;
                    break;
                case FloatMathNode_Which.Extra:
                    extra = value;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            NotifyPortValueChanged(ResultPort);
        }

        public override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            return Calculate();
        }

        private float Calculate() {
            return operation switch {
                FloatMathNode_MathOperation.Add => x + y,
                FloatMathNode_MathOperation.Subtract => x - y,
                FloatMathNode_MathOperation.Multiply => x * y,
                FloatMathNode_MathOperation.Divide => x / y,
                FloatMathNode_MathOperation.Power => Mathf.Pow(x, y),
                FloatMathNode_MathOperation.Logarithm => Mathf.Log(x, y),
                FloatMathNode_MathOperation.SquareRoot => Mathf.Sqrt(x),
                FloatMathNode_MathOperation.InverseSquareRoot => 1.0f / Mathf.Sqrt(x),
                FloatMathNode_MathOperation.Absolute => Mathf.Abs(x),
                FloatMathNode_MathOperation.Exponent => Mathf.Exp(x),

                FloatMathNode_MathOperation.Minimum => Mathf.Min(x, y),
                FloatMathNode_MathOperation.Maximum => Mathf.Max(x, y),
                FloatMathNode_MathOperation.LessThan => x < y ? 1.0f : 0.0f,
                FloatMathNode_MathOperation.GreaterThan => x > y ? 1.0f : 0.0f,
                FloatMathNode_MathOperation.Sign => x < 0 ? -1.0f : x == 0.0f ? 0.0f : 1.0f,
                FloatMathNode_MathOperation.Compare => Mathf.Abs(x - y) < tolerance ? 1.0f : 0.0f,
                FloatMathNode_MathOperation.SmoothMinimum => ExtraMath.SmoothMinimum(x, y, tolerance),
                FloatMathNode_MathOperation.SmoothMaximum => ExtraMath.SmoothMaximum(x, y, tolerance),

                FloatMathNode_MathOperation.Round => Mathf.Round(x),
                FloatMathNode_MathOperation.Floor => Mathf.Floor(x),
                FloatMathNode_MathOperation.Ceil => Mathf.Ceil(x),
                FloatMathNode_MathOperation.Truncate => (int)x,
                FloatMathNode_MathOperation.Fraction => x - (int)x,
                FloatMathNode_MathOperation.Modulo => (float)Math.IEEERemainder(x, y),
                FloatMathNode_MathOperation.Wrap => x = ExtraMath.Wrap(x, y, extra),
                FloatMathNode_MathOperation.Snap => Mathf.Round(x / y) * y,

                FloatMathNode_MathOperation.Sine => Mathf.Sin(x),
                FloatMathNode_MathOperation.Cosine => Mathf.Cos(x),
                FloatMathNode_MathOperation.Tangent => Mathf.Tan(x),
                FloatMathNode_MathOperation.Arcsine => Mathf.Asin(x),
                FloatMathNode_MathOperation.Arccosine => Mathf.Acos(x),
                FloatMathNode_MathOperation.Arctangent => Mathf.Atan(x),
                FloatMathNode_MathOperation.Atan2 => Mathf.Atan2(x, y),
                FloatMathNode_MathOperation.ToRadians => Mathf.Deg2Rad * x,
                FloatMathNode_MathOperation.ToDegrees => Mathf.Rad2Deg * x,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            DebugUtility.Log("Port value changed");
            if (port == XPort) {
                var newValue = GetValue(connection, x);
                if (Math.Abs(x - newValue) > 0.000001f) {
                    x = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == YPort) {
                var newValue = GetValue(connection, y);
                if (Math.Abs(y - newValue) > 0.000001f) {
                    y = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == TolerancePort) {
                var newValue = GetValue(connection, tolerance);
                if (Math.Abs(tolerance - newValue) > 0.000001f) {
                    tolerance = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == ExtraPort) {
                var newValue = GetValue(connection, extra);
                if (Math.Abs(extra - newValue) > 0.000001f) {
                    extra = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else {
                DebugUtility.Log("Invalid port");
            }
        }

        public override void RebindPorts() {
            XPort = Ports[0];
            YPort = Ports[1];
            TolerancePort = Ports[2];
            ExtraPort = Ports[3];
            ResultPort = Ports[4];
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["o"] = (int)operation,
                ["x"] = x,
                ["y"] = y,
                ["t"] = tolerance,
                ["v"] = extra,
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if (string.IsNullOrEmpty(json)) return;

            var data = JObject.Parse(json);
            operation = (FloatMathNode_MathOperation)data.Value<int>("o");
            x = data.Value<float>("x");
            y = data.Value<float>("y");
            tolerance = data.Value<float>("t");
            extra = data.Value<float>("v");
            NotifyPortValueChanged(ResultPort);
        }

        public enum FloatMathNode_Which { X = 0, Y = 1, Tolerance = 2, Extra = 3 }

        public enum FloatMathNode_MathOperation {
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
            ToRadians = 33, ToDegrees = 34
        }
    }
}