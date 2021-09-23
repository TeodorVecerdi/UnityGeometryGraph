﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.Graph {
    public class IntegerValueNode : RuntimeNode {
        private int value;

        public RuntimePort ValuePort { get; private set; }

        public IntegerValueNode(string guid) : base(guid) {
            ValuePort = RuntimePort.Create(PortType.Integer, PortDirection.Output, this);
        }

        public void UpdateValue(int newValue) {
            value = newValue;
            NotifyPortValueChanged(ValuePort);
        }

        public override object GetValueForPort(RuntimePort port) {
            return port == ValuePort ? value : 0;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            // not needed
        }
        
        public override void RebindPorts() {
            ValuePort = Ports[0];
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["v"] = value
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            var data = JObject.Parse(json);
            value = data.Value<int>("v");
            NotifyPortValueChanged(ValuePort);
        }
    }
}