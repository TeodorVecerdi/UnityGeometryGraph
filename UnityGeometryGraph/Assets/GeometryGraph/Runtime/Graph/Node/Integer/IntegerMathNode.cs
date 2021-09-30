using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.Graph {
    public class IntegerMathNode : RuntimeNode {
        private IntegerMathNode_MathOperation operation;
        private int x;
        private int y;
        private float tolerance;
        private int extra;

        public RuntimePort XPort { get; private set; }
        public RuntimePort YPort { get; private set; }
        public RuntimePort TolerancePort { get; private set; }
        public RuntimePort ExtraPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public IntegerMathNode(string guid) : base(guid) {
            XPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            YPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            TolerancePort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ExtraPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Integer, PortDirection.Output, this);
        }

        public void UpdateOperation(IntegerMathNode_MathOperation newOperation) {
            if (newOperation == operation) return;

            operation = newOperation;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateValue(float value, IntegerMathNode_Which which) {
            switch (which) {
                case IntegerMathNode_Which.X:
                    x = (int)value;
                    break;
                case IntegerMathNode_Which.Y:
                    y = (int)value;
                    break;
                case IntegerMathNode_Which.Tolerance:
                    tolerance = value;
                    break;
                case IntegerMathNode_Which.Extra:
                    extra = (int)value;
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
                IntegerMathNode_MathOperation.Add => x + y,
                IntegerMathNode_MathOperation.Subtract => x - y,
                IntegerMathNode_MathOperation.Multiply => x * y,
                IntegerMathNode_MathOperation.IntegerDivision => x / y,
                IntegerMathNode_MathOperation.FloatDivision => (int)(x / (float)y),
                IntegerMathNode_MathOperation.Power => (int)Math.Pow(x, y),
                IntegerMathNode_MathOperation.Logarithm => (int)Math.Log(x, y),
                IntegerMathNode_MathOperation.SquareRoot => (int)Math.Sqrt(x),
                IntegerMathNode_MathOperation.Absolute => Math.Abs(x),
                IntegerMathNode_MathOperation.Exponent => (int)Math.Exp(x),

                IntegerMathNode_MathOperation.Minimum => Math.Min(x, y),
                IntegerMathNode_MathOperation.Maximum => Math.Max(x, y),
                IntegerMathNode_MathOperation.LessThan => x < y ? 1 : 0,
                IntegerMathNode_MathOperation.GreaterThan => x > y ? 1 : 0,
                IntegerMathNode_MathOperation.Sign => x < 0 ? -1 : x == 0 ? 0 : 1,
                IntegerMathNode_MathOperation.Compare => x == y ? 1 : 0,
                IntegerMathNode_MathOperation.SmoothMinimum => (int)ExtraMath.SmoothMinimum(x, y, tolerance),
                IntegerMathNode_MathOperation.SmoothMaximum => (int)ExtraMath.SmoothMaximum(x, y, tolerance),

                IntegerMathNode_MathOperation.Modulo => x % y,
                IntegerMathNode_MathOperation.Wrap => x = ((x - y) % (extra - y) + (extra - y)) % (extra - y) + y,
                IntegerMathNode_MathOperation.Snap => (int) Math.Round((float)x / y) * y,

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == XPort) {
                var newValue = GetValue(XPort, x);
                if (newValue != x) {
                    x = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == YPort) {
                var newValue = GetValue(YPort, y);
                if (newValue != y) {
                    y = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == TolerancePort) {
                var newValue = GetValue(TolerancePort, tolerance);
                if (Math.Abs(newValue - tolerance) > 0.000001f) {
                    tolerance = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == ExtraPort) {
                var newValue = GetValue(ExtraPort, extra);
                if (newValue != extra) {
                    extra = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
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
            operation = (IntegerMathNode_MathOperation)data.Value<int>("o");
            x = data.Value<int>("x");
            y = data.Value<int>("y");
            tolerance = data.Value<float>("t");
            extra = data.Value<int>("v");
            NotifyPortValueChanged(ResultPort);
        }

        public enum IntegerMathNode_Which { X = 0, Y = 1, Tolerance = 2, Extra = 3 }

        public enum IntegerMathNode_MathOperation {
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