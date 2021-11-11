using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.Graph {
    public class FloatBranchNode : RuntimeNode {
        private bool condition;
        private float ifTrue;
        private float ifFalse;

        public RuntimePort ConditionPort { get; private set; }
        public RuntimePort IfTruePort { get; private set; }
        public RuntimePort IfFalsePort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public FloatBranchNode(string guid) : base(guid) {
            ConditionPort = RuntimePort.Create(PortType.Boolean, PortDirection.Input, this);
            IfTruePort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            IfFalsePort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
        }

        public void UpdateCondition(bool newValue) {
            if (newValue == condition) return;
            condition = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateIfTrue(float newValue) {
            if (Math.Abs(newValue - ifTrue) < Constants.FLOAT_TOLERANCE) return;
            ifTrue = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateIfFalse(float newValue) {
            if (Math.Abs(newValue - ifFalse) < Constants.FLOAT_TOLERANCE) return;
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
                if (Math.Abs(newValue - ifTrue) > 0.000001f) {
                    ifTrue = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == IfFalsePort) {
                var newValue = GetValue(IfFalsePort, ifFalse);
                if (Math.Abs(newValue - ifFalse) > 0.000001f) {
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
            ifTrue = array.Value<float>(1);
            ifFalse = array.Value<float>(2);

            NotifyPortValueChanged(ResultPort);
        }
    }
}