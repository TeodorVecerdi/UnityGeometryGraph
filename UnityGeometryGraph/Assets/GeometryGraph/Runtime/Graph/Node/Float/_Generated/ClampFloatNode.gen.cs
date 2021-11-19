// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GeometryGraph Code Generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [SourceClass("GeometryGraph.Runtime::GeometryGraph.Runtime.Graph::ClampFloatNode")]
    public partial class ClampFloatNode : RuntimeNode {
        public RuntimePort InputPort { get; }
        public RuntimePort MinPort { get; }
        public RuntimePort MaxPort { get; }
        public RuntimePort ResultPort { get; }

        public ClampFloatNode(string guid) : base(guid) {
            InputPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            MinPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            MaxPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
        }

        public void UpdateInput(float newValue) {
            if(Math.Abs(Input - newValue) < Constants.FLOAT_TOLERANCE) return;
            Input = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateMin(float newValue) {
            if(Math.Abs(Min - newValue) < Constants.FLOAT_TOLERANCE) return;
            Min = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateMax(float newValue) {
            if(Math.Abs(Max - newValue) < Constants.FLOAT_TOLERANCE) return;
            Max = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port == ResultPort) return Input.Clamped(Min, Max);
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == InputPort) {
                var newValue = GetValue(connection, Input);
                if(Math.Abs(Input - newValue) < Constants.FLOAT_TOLERANCE) return;
                Input = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == MinPort) {
                var newValue = GetValue(connection, Min);
                if(Math.Abs(Min - newValue) < Constants.FLOAT_TOLERANCE) return;
                Min = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == MaxPort) {
                var newValue = GetValue(connection, Max);
                if(Math.Abs(Max - newValue) < Constants.FLOAT_TOLERANCE) return;
                Max = newValue;
                NotifyPortValueChanged(ResultPort);
            }
        }

        public override string GetCustomData() {
            return new JArray {
                Input,
                Min,
                Max,
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string data) {
            JArray array = JArray.Parse(data);
            Input = array.Value<float>(0);
            Min = array.Value<float>(1);
            Max = array.Value<float>(2);

            NotifyPortValueChanged(ResultPort);
        }
    }
}