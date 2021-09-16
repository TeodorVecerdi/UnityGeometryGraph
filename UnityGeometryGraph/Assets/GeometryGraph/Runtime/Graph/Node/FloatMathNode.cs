using System;

namespace GeometryGraph.Runtime.Graph {
    public class FloatMathNode : RuntimeNode {
        private float a;
        private float b;
        private float result;
        
        private enum MathOperation {Add, Subtract, Multiply, Divide}
        private MathOperation operation;

        public RuntimePort APort { get; }
        public RuntimePort BPort { get; }
        public RuntimePort ResultPort { get; }

        public FloatMathNode(string guid) : base(guid) {
            APort = new RuntimePort(PortType.Float, PortDirection.Input, this);
            BPort = new RuntimePort(PortType.Float, PortDirection.Input, this);
            ResultPort = new RuntimePort(PortType.Float, PortDirection.Output, this);
        }

        // Note: This requires parity between graph enum and runtime enum.
        // Should probably change at some point into a shared enum
        public void UpdateOperation(int operationAsInt) {
            operation = (MathOperation) operationAsInt;
            NotifyPortValueChanged(ResultPort);
        }

        public override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            result = CalculateResult();
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == APort) a = GetValue(connection, a);
            else if (port == BPort) b = GetValue(connection, b);

            UpdateResult();
        }

        private void UpdateResult() {
            var newResult = CalculateResult();
            if(Math.Abs(result - newResult) < 0.00001f) return;

            result = newResult;
            NotifyPortValueChanged(ResultPort);
        }

        private float CalculateResult() {
            return operation switch {
                MathOperation.Add => a + b,
                MathOperation.Subtract => a - b,
                MathOperation.Multiply => a * b,
                MathOperation.Divide => a / b,
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
            };
        }
    }
}