using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.Graph {
    public class CompareIntegerNode : RuntimeNode {
        private CompareIntegerNode_CompareOperation operation;
        private int a;
        private int b;

        public RuntimePort APort { get; private set; }
        public RuntimePort BPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public CompareIntegerNode(string guid) : base(guid) {
            APort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            BPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Boolean, PortDirection.Output, this);
        }

        public void UpdateCompareOperation(CompareIntegerNode_CompareOperation newOperation) {
            if (newOperation == operation) return;
            
            operation = newOperation;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateValue(int value, CompareIntegerNode_Which which) {
            switch (which) {
                case CompareIntegerNode_Which.A: a = value; break;
                case CompareIntegerNode_Which.B: b = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            NotifyPortValueChanged(ResultPort);
        }

        public override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            return operation switch {
                CompareIntegerNode_CompareOperation.LessThan => a < b,
                CompareIntegerNode_CompareOperation.LessThanOrEqual => a <= b,
                CompareIntegerNode_CompareOperation.GreaterThan => a > b,
                CompareIntegerNode_CompareOperation.GreaterThanOrEqual => a >= b,
                CompareIntegerNode_CompareOperation.Equal => a == b,
                CompareIntegerNode_CompareOperation.NotEqual => a != b,
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
            };
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if(port == ResultPort) return;
            if (port == APort) {
                var newValue = GetValue(APort, a);
                if (newValue != a) {
                    a = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            } else if (port == BPort) {
                var newValue = GetValue(BPort, b);
                if (newValue != b) {
                    b = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            }
        }
        
        public override void RebindPorts() {
            APort = Ports[0];
            BPort = Ports[1];
            ResultPort = Ports[2];
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["o"] = (int)operation,
                ["a"] = a,
                ["b"] = b
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            operation = (CompareIntegerNode_CompareOperation)data.Value<int>("o");
            a = data.Value<int>("a");
            b = data.Value<int>("b");
            NotifyPortValueChanged(ResultPort);
        }

        public enum CompareIntegerNode_Which {A = 0, B = 1}
        public enum CompareIntegerNode_CompareOperation {LessThan = 0, LessThanOrEqual = 1, GreaterThan = 2, GreaterThanOrEqual = 3, Equal = 4, NotEqual = 5}
    }
}