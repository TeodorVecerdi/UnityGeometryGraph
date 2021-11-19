using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    public class MapRangeFloatNode : RuntimeNode {
        private bool clamp;
        private float inputValue;
        private float fromMin = 0.0f;
        private float fromMax = 1.0f;
        private float toMin = 0.0f;
        private float toMax = 1.0f;

        public RuntimePort InputPort { get; private set; }
        public RuntimePort FromMinPort { get; private set; }
        public RuntimePort FromMaxPort { get; private set; }
        public RuntimePort ToMinPort { get; private set; }
        public RuntimePort ToMaxPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public MapRangeFloatNode(string guid) : base(guid) {
            InputPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            FromMinPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            FromMaxPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ToMinPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ToMaxPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
        }

        public void UpdateClamped(bool newValue) {
            if (clamp == newValue) return;
            
            clamp = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateValue(float value, MapRangeFloatNode_Which which) {
            switch (which) {
                case MapRangeFloatNode_Which.Input: inputValue = value; break;
                case MapRangeFloatNode_Which.FromMin: fromMin = value; break;
                case MapRangeFloatNode_Which.FromMax: fromMax = value; break;
                case MapRangeFloatNode_Which.ToMin: toMin = value; break;
                case MapRangeFloatNode_Which.ToMax: toMax = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            var value = inputValue.Map(fromMin, fromMax, toMin, toMax);
            return clamp ? value.Clamped(toMin, toMax) : value;
        }

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ResultPort) yield break;
            if (count <= 0) yield break;

            IEnumerable<float> values;
            if (InputPort.IsConnected) values = GetValues(InputPort, count, inputValue);
            else values = Enumerable.Repeat(inputValue, count);
            
            foreach (var value in values) {
                var mapped = value.Map(fromMin, fromMax, toMin, toMax);
                yield return clamp ? mapped.Clamped(toMin, toMax) : mapped;
            }
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if(port == ResultPort) return;
            if (port == InputPort) {
                var newValue = GetValue(InputPort, inputValue);
                if (Math.Abs(newValue - inputValue) > 0.000001f) {
                    inputValue = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == FromMinPort) {
                var newValue = GetValue(FromMinPort, fromMin);
                if (Math.Abs(newValue - fromMin) > 0.000001f) {
                    fromMin = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == FromMaxPort) {
                var newValue = GetValue(FromMaxPort, fromMax);
                if (Math.Abs(newValue - fromMax) > 0.000001f) {
                    fromMax = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == ToMinPort) {
                var newValue = GetValue(ToMinPort, toMin);
                if (Math.Abs(newValue - toMin) > 0.000001f) {
                    toMin = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == ToMaxPort) {
                var newValue = GetValue(ToMaxPort, toMax);
                if (Math.Abs(newValue - toMax) > 0.000001f) {
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
            inputValue = data.Value<float>("i");
            fromMin = data.Value<float>("f");
            fromMax = data.Value<float>("F");
            toMin = data.Value<float>("t");
            toMax = data.Value<float>("T");
            NotifyPortValueChanged(ResultPort);
        }

        public enum MapRangeFloatNode_Which {Input = 0, FromMin = 1, FromMax = 2, ToMin = 3, ToMax = 4}
    }
}