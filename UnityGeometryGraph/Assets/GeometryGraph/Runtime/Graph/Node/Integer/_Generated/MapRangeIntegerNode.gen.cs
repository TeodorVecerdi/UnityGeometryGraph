// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GeometryGraph Code Generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using GeometryGraph.Runtime.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [SourceClass("GeometryGraph.Runtime::GeometryGraph.Runtime.Graph::MapRangeIntegerNode")]
    public partial class MapRangeIntegerNode : RuntimeNode {
        public RuntimePort ValuePort { get; }
        public RuntimePort FromMinPort { get; }
        public RuntimePort FromMaxPort { get; }
        public RuntimePort ToMinPort { get; }
        public RuntimePort ToMaxPort { get; }
        public RuntimePort ResultPort { get; }

        public MapRangeIntegerNode(string guid) : base(guid) {
            ValuePort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            FromMinPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            FromMaxPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ToMinPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ToMaxPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Integer, PortDirection.Output, this);
        }

        public void UpdateValue(int newValue) {
            if(Value == newValue) return;
            Value = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateFromMin(int newValue) {
            if(FromMin == newValue) return;
            FromMin = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateFromMax(int newValue) {
            if(FromMax == newValue) return;
            FromMax = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateToMin(int newValue) {
            if(ToMin == newValue) return;
            ToMin = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateToMax(int newValue) {
            if(ToMax == newValue) return;
            ToMax = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateClamp(bool newValue) {
            if(Clamp == newValue) return;
            Clamp = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port == ResultPort) {
                int value = Value.Map(FromMin, FromMax, ToMin, ToMax);
                return Clamp ? value.Clamped(ToMin, ToMax) : value;
            }
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == ValuePort) {
                var newValue = GetValue(connection, Value);
                if(Value == newValue) return;
                Value = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == FromMinPort) {
                var newValue = GetValue(connection, FromMin);
                if(FromMin == newValue) return;
                FromMin = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == FromMaxPort) {
                var newValue = GetValue(connection, FromMax);
                if(FromMax == newValue) return;
                FromMax = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == ToMinPort) {
                var newValue = GetValue(connection, ToMin);
                if(ToMin == newValue) return;
                ToMin = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == ToMaxPort) {
                var newValue = GetValue(connection, ToMax);
                if(ToMax == newValue) return;
                ToMax = newValue;
                NotifyPortValueChanged(ResultPort);
            }
        }

        public override string GetCustomData() {
            return new JArray {
                Value,
                FromMin,
                FromMax,
                ToMin,
                ToMax,
                Clamp ? 1 : 0,
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string data) {
            JArray array = JArray.Parse(data);
            Value = array.Value<int>(0);
            FromMin = array.Value<int>(1);
            FromMax = array.Value<int>(2);
            ToMin = array.Value<int>(3);
            ToMax = array.Value<int>(4);
            Clamp = array.Value<int>(5) == 1;

            NotifyPortValueChanged(ResultPort);
        }
    }
}