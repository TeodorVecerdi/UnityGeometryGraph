using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    public class MapRangeIntegerNode : RuntimeNode {
        private bool clamp;
        private int inputValue;
        private int fromMin = 0;
        private int fromMax = 1;
        private int toMin = 0;
        private int toMax = 1;

        public RuntimePort InputPort { get; private set; }
        public RuntimePort FromMinPort { get; private set; }
        public RuntimePort FromMaxPort { get; private set; }
        public RuntimePort ToMinPort { get; private set; }
        public RuntimePort ToMaxPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public MapRangeIntegerNode(string guid) : base(guid) {
            InputPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            FromMinPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            FromMaxPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ToMinPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ToMaxPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Integer, PortDirection.Output, this);
        }

        public void UpdateClamped(bool newValue) {
            if (clamp == newValue) return;
            
            clamp = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateValue(int value, MapRangeIntegerNode_Which which) {
            switch (which) {
                case MapRangeIntegerNode_Which.Input: inputValue = value; break;
                case MapRangeIntegerNode_Which.FromMin: fromMin = value; break;
                case MapRangeIntegerNode_Which.FromMax: fromMax = value; break;
                case MapRangeIntegerNode_Which.ToMin: toMin = value; break;
                case MapRangeIntegerNode_Which.ToMax: toMax = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            var value = inputValue.Map(fromMin, fromMax, toMin, toMax);
            return clamp ? value.Clamped(toMin, toMax) : value;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if(port == ResultPort) return;
            if (port == InputPort) {
                var newValue = GetValue(InputPort, inputValue);
                if (newValue != inputValue) {
                    inputValue = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == FromMinPort) {
                var newValue = GetValue(FromMinPort, fromMin);
                if (newValue != fromMin) {
                    fromMin = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == FromMaxPort) {
                var newValue = GetValue(FromMaxPort, fromMax);
                if (newValue != fromMax) {
                    fromMax = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == ToMinPort) {
                var newValue = GetValue(ToMinPort, toMin);
                if (newValue != toMin) {
                    toMin = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == ToMaxPort) {
                var newValue = GetValue(ToMaxPort, toMax);
                if (newValue != toMax) {
                    toMax = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            }
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["c"] = clamp ? 1 : 0,
                ["i"] = inputValue,
                ["f"] = fromMin,
                ["F"] = fromMax,
                ["t"] = toMin,
                ["T"] = toMax
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            clamp = data.Value<int>("c") == 1;
            inputValue = data.Value<int>("i");
            fromMin = data.Value<int>("f");
            fromMax = data.Value<int>("F");
            toMin = data.Value<int>("t");
            toMax = data.Value<int>("T");
            NotifyPortValueChanged(ResultPort);
        }

        public enum MapRangeIntegerNode_Which {Input = 0, FromMin = 1, FromMax = 2, ToMin = 3, ToMax = 4}
    }
}