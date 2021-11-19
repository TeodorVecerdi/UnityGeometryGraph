using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    public class ClampIntegerNode : RuntimeNode {
        private int inputValue;
        private int minValue = 0;
        private int maxValue = 1;

        public RuntimePort InputPort { get; private set; }
        public RuntimePort MinPort { get; private set; }
        public RuntimePort MaxPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public ClampIntegerNode(string guid) : base(guid) {
            InputPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            MinPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            MaxPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Integer, PortDirection.Output, this);
        }

        public void UpdateValue(int value, ClampIntegerNode_Which which) {
            DebugUtility.Log($"Updated {which} to {value}");
            switch (which) {
                case ClampIntegerNode_Which.Input: inputValue = value; break;
                case ClampIntegerNode_Which.Min: minValue = value; break;
                case ClampIntegerNode_Which.Max: maxValue = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            DebugUtility.Log("Calculated and returned result");
            return inputValue.Clamped(minValue, maxValue);
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if(port == ResultPort) return;
            if (port == InputPort) {
                var newValue = GetValue(InputPort, inputValue);
                DebugUtility.Log($"Input value changed {inputValue}=>{newValue}");
                if (newValue != inputValue) {
                    inputValue = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            } else if (port == MinPort) {
                var newValue = GetValue(MinPort, minValue);
                DebugUtility.Log($"Min value changed {minValue}=>{newValue}");
                if (newValue != minValue) {
                    minValue = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            } else if (port == MaxPort) {
                var newValue = GetValue(MaxPort, maxValue);
                DebugUtility.Log($"Max value changed {maxValue}=>{newValue}");
                if (newValue != maxValue) {
                    maxValue = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            }
        }
        
        public override string GetCustomData() {
            var data = new JObject {
                ["i"] = inputValue,
                ["m"] = minValue,
                ["M"] = maxValue
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            inputValue = data.Value<int>("i");
            minValue = data.Value<int>("m");
            maxValue = data.Value<int>("M");
            NotifyPortValueChanged(ResultPort);
        }

        public enum ClampIntegerNode_Which {Input = 0, Min = 1, Max = 2}
    }
}