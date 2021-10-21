using System;
using GeometryGraph.Runtime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.Graph {
    public class CirclePrimitiveNode : RuntimeNode {
        private float radius = 1.0f;
        private int points = 8;
        
        private GeometryData result;

        public RuntimePort RadiusPort { get; private set; }
        public RuntimePort PointsPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public CirclePrimitiveNode(string guid) : base(guid) {
            RadiusPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            PointsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateValue(object value, CirclePrimitiveNode_Which which) {
            DebugUtility.Log($"Updated {which} to {value}");
            switch (which) {
                case CirclePrimitiveNode_Which.Radius: radius = (float)value; break;
                case CirclePrimitiveNode_Which.Points: points = (int)value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }
            
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            DebugUtility.Log("Returning result");
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            
            if (port == RadiusPort) {
                var newValue = GetValue(connection, radius);
                DebugUtility.Log("Updated radius");
                if (Math.Abs(newValue - radius) > 0.000001f) {
                    radius = newValue;
                    CalculateResult();
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == PointsPort) {
                var newValue = GetValue(connection, points);
                DebugUtility.Log("Updated points");
                if (newValue < 3) newValue = 3;
                if (newValue != points) {
                    points = newValue;
                    CalculateResult();
                    NotifyPortValueChanged(ResultPort);
                }
            }
        }
        
        public override void RebindPorts() {
            RadiusPort = Ports[0];
            PointsPort = Ports[1];
            ResultPort = Ports[2];
        }

        private void CalculateResult() {
            DebugUtility.Log("Calculated result");
            result = GeometryPrimitive.Circle(radius, points);
        }
        
        public override string GetCustomData() {
            var data = new JObject {
                ["r"] = radius,
                ["p"] = points
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            radius = data.Value<float>("r");
            points = data.Value<int>("p");
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public enum CirclePrimitiveNode_Which {Radius = 0, Points = 1}
    }
}