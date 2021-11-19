// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GeometryGraph Code Generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [SourceClass("GeometryGraph.Runtime::GeometryGraph.Runtime.Graph::MapRangeFloatNode")]
    public partial class MapRangeFloatNode : RuntimeNode {
        public RuntimePort ValuePort { get; }
        public RuntimePort FromMinPort { get; }
        public RuntimePort FromMaxPort { get; }
        public RuntimePort ToMinPort { get; }
        public RuntimePort ToMaxPort { get; }
        public RuntimePort ResultPort { get; }

        public MapRangeFloatNode(string guid) : base(guid) {
            ValuePort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            FromMinPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            FromMaxPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ToMinPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ToMaxPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
        }

        public void UpdateValue(float newValue) {
            if(Math.Abs(Value - newValue) < Constants.FLOAT_TOLERANCE) return;
            Value = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateFromMin(float newValue) {
            if(Math.Abs(FromMin - newValue) < Constants.FLOAT_TOLERANCE) return;
            FromMin = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateFromMax(float newValue) {
            if(Math.Abs(FromMax - newValue) < Constants.FLOAT_TOLERANCE) return;
            FromMax = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateToMin(float newValue) {
            if(Math.Abs(ToMin - newValue) < Constants.FLOAT_TOLERANCE) return;
            ToMin = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateToMax(float newValue) {
            if(Math.Abs(ToMax - newValue) < Constants.FLOAT_TOLERANCE) return;
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
                float value = Value.Map(FromMin, FromMax, ToMin, ToMax);
                return Clamp ? value.Clamped(ToMin, ToMax) : value;
            }
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == ValuePort) {
                var newValue = GetValue(connection, Value);
                if(Math.Abs(Value - newValue) < Constants.FLOAT_TOLERANCE) return;
                Value = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == FromMinPort) {
                var newValue = GetValue(connection, FromMin);
                if(Math.Abs(FromMin - newValue) < Constants.FLOAT_TOLERANCE) return;
                FromMin = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == FromMaxPort) {
                var newValue = GetValue(connection, FromMax);
                if(Math.Abs(FromMax - newValue) < Constants.FLOAT_TOLERANCE) return;
                FromMax = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == ToMinPort) {
                var newValue = GetValue(connection, ToMin);
                if(Math.Abs(ToMin - newValue) < Constants.FLOAT_TOLERANCE) return;
                ToMin = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == ToMaxPort) {
                var newValue = GetValue(connection, ToMax);
                if(Math.Abs(ToMax - newValue) < Constants.FLOAT_TOLERANCE) return;
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
            Value = array.Value<float>(0);
            FromMin = array.Value<float>(1);
            FromMax = array.Value<float>(2);
            ToMin = array.Value<float>(3);
            ToMax = array.Value<float>(4);
            Clamp = array.Value<int>(5) == 1;

            NotifyPortValueChanged(ResultPort);
        }
    }
}