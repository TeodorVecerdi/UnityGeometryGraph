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
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [SourceClass("GeometryGraph.Runtime::GeometryGraph.Runtime.Graph::CylinderPrimitiveNode")]
    public partial class CylinderPrimitiveNode : RuntimeNode {
        public RuntimePort BottomRadiusPort { get; }
        public RuntimePort TopRadiusPort { get; }
        public RuntimePort HeightPort { get; }
        public RuntimePort PointsPort { get; }
        public RuntimePort ResultPort { get; }

        public CylinderPrimitiveNode(string guid) : base(guid) {
            BottomRadiusPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            TopRadiusPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            HeightPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            PointsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateBottomRadius(float newValue) {
            if(Math.Abs(BottomRadius - newValue) < Constants.FLOAT_TOLERANCE) return;
            BottomRadius = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateTopRadius(float newValue) {
            if(Math.Abs(TopRadius - newValue) < Constants.FLOAT_TOLERANCE) return;
            TopRadius = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateHeight(float newValue) {
            if(Math.Abs(Height - newValue) < Constants.FLOAT_TOLERANCE) return;
            Height = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdatePoints(int newValue) {
            if(Points == newValue) return;
            Points = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port == ResultPort) return Result;
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == BottomRadiusPort) {
                var newValue = GetValue(connection, BottomRadius);
                newValue = newValue.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);
                if(Math.Abs(BottomRadius - newValue) < Constants.FLOAT_TOLERANCE) return;
                BottomRadius = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == TopRadiusPort) {
                var newValue = GetValue(connection, TopRadius);
                newValue = newValue.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);
                if(Math.Abs(TopRadius - newValue) < Constants.FLOAT_TOLERANCE) return;
                TopRadius = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == HeightPort) {
                var newValue = GetValue(connection, Height);
                newValue = newValue.MinClamped(Constants.MIN_GEOMETRY_HEIGHT);
                if(Math.Abs(Height - newValue) < Constants.FLOAT_TOLERANCE) return;
                Height = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == PointsPort) {
                var newValue = GetValue(connection, Points);
                newValue = newValue.Clamped(Constants.MIN_CIRCULAR_GEOMETRY_POINTS, Constants.MAX_CIRCULAR_GEOMETRY_POINTS);
                if(Points == newValue) return;
                Points = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            }
        }

        public override string GetCustomData() {
            return new JArray {
                BottomRadius,
                TopRadius,
                Height,
                Points,
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string data) {
            JArray array = JArray.Parse(data);
            BottomRadius = array.Value<float>(0);
            TopRadius = array.Value<float>(1);
            Height = array.Value<float>(2);
            Points = array.Value<int>(3);

            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
    }
}