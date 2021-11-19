using System;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    public class VectorBranchNode : RuntimeNode {
        private bool condition;
        private float3 ifTrue;
        private float3 ifFalse;

        public RuntimePort ConditionPort { get; private set; }
        public RuntimePort IfTruePort { get; private set; }
        public RuntimePort IfFalsePort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public VectorBranchNode(string guid) : base(guid) {
            ConditionPort = RuntimePort.Create(PortType.Boolean, PortDirection.Input, this);
            IfTruePort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            IfFalsePort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Vector, PortDirection.Output, this);
        }

        public void UpdateCondition(bool newValue) {
            if (newValue == condition) return;
            condition = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateIfTrue(float3 newValue) {
            if (math.lengthsq(newValue - ifTrue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
            ifTrue = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateIfFalse(float3 newValue) {
            if (math.lengthsq(newValue - ifFalse) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
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
                if (math.lengthsq(newValue - ifTrue) > Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) {
                    ifTrue = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == IfFalsePort) {
                var newValue = GetValue(IfFalsePort, ifFalse);
                if (math.lengthsq(newValue - ifFalse) > Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) {
                    ifFalse = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            }
        }

        public override string GetCustomData() {
            return new JArray {
                condition ? 1 : 0,
                JsonConvert.SerializeObject(ifTrue, Formatting.None, float3Converter.Converter),
                JsonConvert.SerializeObject(ifFalse, Formatting.None, float3Converter.Converter)
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            JArray array = JArray.Parse(json);
            condition = array.Value<int>(0) == 1;
            ifTrue = JsonConvert.DeserializeObject<float3>(array.Value<string>(1), float3Converter.Converter);
            ifFalse = JsonConvert.DeserializeObject<float3>(array.Value<string>(2), float3Converter.Converter);

            NotifyPortValueChanged(ResultPort);
        }
    }
}