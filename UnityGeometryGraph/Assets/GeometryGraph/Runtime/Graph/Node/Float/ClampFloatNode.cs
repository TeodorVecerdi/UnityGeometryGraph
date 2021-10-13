using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    public class ClampFloatNode : RuntimeNode {
        private float inputValue;
        private float minValue = 0.0f;
        private float maxValue = 1.0f;

        public RuntimePort InputPort { get; private set; }
        public RuntimePort MinPort { get; private set; }
        public RuntimePort MaxPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public ClampFloatNode(string guid) : base(guid) {
            InputPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            MinPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            MaxPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
        }

        public void UpdateValue(float value, ClampFloatNode_Which which) {
            switch (which) {
                case ClampFloatNode_Which.Input: inputValue = value; break;
                case ClampFloatNode_Which.Min: minValue = value; break;
                case ClampFloatNode_Which.Max: maxValue = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            return inputValue.Clamped(minValue, maxValue);
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if(port == ResultPort) return;
            if (port == InputPort) {
                var newValue = GetValue(InputPort, inputValue);
                if (Math.Abs(newValue - inputValue) > 0.000001f) {
                    inputValue = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            } else if (port == MinPort) {
                var newValue = GetValue(MinPort, minValue);
                if (Math.Abs(newValue - minValue) > 0.000001f) {
                    minValue = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            } else if (port == MaxPort) {
                var newValue = GetValue(MaxPort, maxValue);
                if (Math.Abs(newValue - maxValue) > 0.000001f) {
                    maxValue = newValue;
                    NotifyPortValueChanged(ResultPort);
                } 
            }
        }
        
        public override void RebindPorts() {
            InputPort = Ports[0];
            MinPort = Ports[1];
            MaxPort = Ports[2];
            ResultPort = Ports[3];
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
            inputValue = data.Value<float>("i");
            minValue = data.Value<float>("m");
            maxValue = data.Value<float>("M");
            NotifyPortValueChanged(ResultPort);
        }

        public enum ClampFloatNode_Which {Input = 0, Min = 1, Max = 2}
    }
}