using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.Graph {
    public class BooleanMathNode : RuntimeNode {
        private BooleanMathNode_Operation operation;
        private bool a;
        private bool b;

        public RuntimePort APort { get; private set; }
        public RuntimePort BPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public BooleanMathNode(string guid) : base(guid) {
            APort = RuntimePort.Create(PortType.Boolean, PortDirection.Input, this);
            BPort = RuntimePort.Create(PortType.Boolean, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Boolean, PortDirection.Input, this);
        }

        public void UpdateCompareOperation(BooleanMathNode_Operation newOperation) {
            if (newOperation == operation) return;
            
            operation = newOperation;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateValue(bool value, BooleanMathNode_Which which) {
            switch (which) {
                case BooleanMathNode_Which.A: a = value; break;
                case BooleanMathNode_Which.B: b = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            return operation switch {
                BooleanMathNode_Operation.AND => a & b,
                BooleanMathNode_Operation.OR => a | b,
                BooleanMathNode_Operation.XOR => a ^ b,
                BooleanMathNode_Operation.NOT => !a,
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
                ["a"] = a ? 1 : 0,
                ["b"] = b ? 1 : 0
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            operation = (BooleanMathNode_Operation)data.Value<int>("o");
            a = data.Value<int>("a") == 1;
            b = data.Value<int>("b") == 1;
            NotifyPortValueChanged(ResultPort);
        }

        public enum BooleanMathNode_Which {A = 0, B = 1}
        public enum BooleanMathNode_Operation {AND = 0, OR = 1, XOR = 2, NOT = 3}
    }
}