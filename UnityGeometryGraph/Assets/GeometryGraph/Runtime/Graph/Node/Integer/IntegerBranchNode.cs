using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.Graph {
    public class IntegerBranchNode : RuntimeNode {
        private bool condition;
        private int ifTrue;
        private int ifFalse;

        public RuntimePort ConditionPort { get; private set; }
        public RuntimePort IfTruePort { get; private set; }
        public RuntimePort IfFalsePort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public IntegerBranchNode(string guid) : base(guid) {
            ConditionPort = RuntimePort.Create(PortType.Boolean, PortDirection.Input, this);
            IfTruePort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            IfFalsePort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Integer, PortDirection.Output, this);
        }

        public void UpdateCondition(bool newValue) {
            if (newValue == condition) return;
            condition = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateIfTrue(int newValue) {
            if (newValue == ifTrue) return;
            ifTrue = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateIfFalse(int newValue) {
            if (newValue == ifFalse) return;
            ifFalse = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            return condition ? ifTrue : ifFalse;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == ConditionPort) {
                var newValue = GetValue(ConditionPort, condition);
                if (newValue != condition) {
                    condition = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == IfTruePort) {
                var newValue = GetValue(IfTruePort, ifTrue);
                if (newValue != ifTrue) {
                    ifTrue = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == IfFalsePort) {
                var newValue = GetValue(IfFalsePort, ifFalse);
                if (newValue != ifFalse) {
                    ifFalse = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            }
        }

        public override void RebindPorts() {
            throw new InvalidOperationException();
        }

        public override string GetCustomData() {
            return new JArray {
                condition ? 1 : 0,
                ifTrue,
                ifFalse
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            JArray array = JArray.Parse(json);
            condition = array.Value<int>(0) == 1;
            ifTrue = array.Value<int>(1);
            ifFalse = array.Value<int>(2);

            NotifyPortValueChanged(ResultPort);
        }
    }
}