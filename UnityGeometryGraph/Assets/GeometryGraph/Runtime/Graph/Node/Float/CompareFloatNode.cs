using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.Graph {
    public class CompareFloatNode : RuntimeNode {
        private CompareFloatNode_CompareOperation operation;
        private float tolerance;
        private float a;
        private float b;

        public RuntimePort APort { get; private set; }
        public RuntimePort BPort { get; private set; }
        public RuntimePort TolerancePort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public CompareFloatNode(string guid) : base(guid) {
            TolerancePort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            APort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            BPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Boolean, PortDirection.Output, this);
        }

        public void UpdateCompareOperation(CompareFloatNode_CompareOperation newOperation) {
            if (newOperation == operation) return;
            
            operation = newOperation;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateValue(float value, CompareFloatNode_Which which) {
            switch (which) {
                case CompareFloatNode_Which.A: a = value; break;
                case CompareFloatNode_Which.B: b = value; break;
                case CompareFloatNode_Which.Tolerance: tolerance = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            return operation switch {
                CompareFloatNode_CompareOperation.LessThan => a < b,
                CompareFloatNode_CompareOperation.LessThanOrEqual => a <= b,
                CompareFloatNode_CompareOperation.GreaterThan => a > b,
                CompareFloatNode_CompareOperation.GreaterThanOrEqual => a >= b,
                CompareFloatNode_CompareOperation.Equal => Math.Abs(a - b) < tolerance,
                CompareFloatNode_CompareOperation.NotEqual => Math.Abs(a - b) > tolerance,
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
            };
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if(port == ResultPort) return;
            if (port == TolerancePort) {
                var newValue = GetValue(TolerancePort, tolerance);
                if (Math.Abs(newValue - tolerance) > 0.000001f) {
                    tolerance = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            } else if (port == APort) {
                var newValue = GetValue(APort, a);
                if (Math.Abs(newValue - a) > 0.000001f) {
                    a = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            } else if (port == BPort) {
                var newValue = GetValue(BPort, b);
                if (Math.Abs(newValue - b) > 0.000001f) {
                    b = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            }
        }
        
        public override void RebindPorts() {
            TolerancePort = Ports[0];
            APort = Ports[1];
            BPort = Ports[2];
            ResultPort = Ports[3];
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["o"] = (int)operation,
                ["t"] = tolerance,
                ["a"] = a,
                ["b"] = b
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            operation = (CompareFloatNode_CompareOperation)data.Value<int>("o");
            tolerance = data.Value<float>("t");
            a = data.Value<float>("a");
            b = data.Value<float>("b");
            NotifyPortValueChanged(ResultPort);
        }

        public enum CompareFloatNode_Which {A = 0, B = 1, Tolerance = 2}
        public enum CompareFloatNode_CompareOperation {LessThan = 0, LessThanOrEqual = 1, GreaterThan = 2, GreaterThanOrEqual = 3, Equal = 4, NotEqual = 5}
    }
}