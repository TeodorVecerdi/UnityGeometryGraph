using System;

namespace GeometryGraph.Runtime.Graph {
    public class FloatMathNode : RuntimeNode {
        private float a;
        private float b;
        private float result;
        
        private enum MathOperation {Add, Subtract, Multiply, Divide}
        private MathOperation operation;

        private RuntimePort aPort;
        private RuntimePort bPort;
        private RuntimePort resultPort;
        
        public FloatMathNode(string guid, float a, float b) : base(guid) {
            this.a = a;
            this.b = b;
            result = CalculateResult();
        }

        // Note: This requires parity between graph enum and runtime enum.
        // Should probably change at some point into a shared enum
        public void UpdateOperation(int operationAsInt) {
            operation = (MathOperation) operationAsInt;
            NotifyPortValueChanged(resultPort);
        }

        public override object GetValueForPort(RuntimePort port) {
            if (port != resultPort) return null;
            result = CalculateResult();
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == aPort) a = GetValue(connection, a);
            else if (port == bPort) b = GetValue(connection, b);

            UpdateResult();
        }

        private void UpdateResult() {
            var newResult = CalculateResult();
            if(Math.Abs(result - newResult) < 0.00001f) return;

            result = newResult;
            NotifyPortValueChanged(resultPort);
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