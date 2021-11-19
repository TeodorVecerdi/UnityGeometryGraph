using System;
using GeometryGraph.Runtime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.Graph {
    public class ConePrimitiveNode : RuntimeNode {
        private float radius = 1.0f;
        private float height = 2.0f;
        private int points = 8;
        
        private GeometryData result;

        public RuntimePort RadiusPort { get; private set; }
        public RuntimePort HeightPort { get; private set; }
        public RuntimePort PointsPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public ConePrimitiveNode(string guid) : base(guid) {
            RadiusPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            HeightPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            PointsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateValue(object value, ConePrimitiveNode_Which which) {
            switch (which) {
                case ConePrimitiveNode_Which.Radius: radius = (float)value; break;
                case ConePrimitiveNode_Which.Height: height = (float)value; break;
                case ConePrimitiveNode_Which.Points: points = (int)value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }
            
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            
            if (port == RadiusPort) {
                var newValue = GetValue(connection, radius);
                if (Math.Abs(newValue - radius) > 0.000001f) {
                    radius = newValue;
                    CalculateResult();
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == HeightPort) {
                var newValue = GetValue(connection, height);
                if (Math.Abs(newValue - height) > 0.000001f) {
                    height = newValue;
                    CalculateResult();
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == PointsPort) {
                var newValue = GetValue(connection, points);
                if (newValue < 3) newValue = 3;
                if (newValue != points) {
                    points = newValue;
                    CalculateResult();
                    NotifyPortValueChanged(ResultPort);
                }
            }
        }
        
        private void CalculateResult() {
            result = GeometryPrimitive.Cone(radius, height, points);
        }
        
        public override string GetCustomData() {
            var data = new JObject {
                ["r"] = radius,
                ["h"] = height,
                ["p"] = points
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            radius = data.Value<float>("r");
            height = data.Value<float>("h");
            points = data.Value<int>("p");
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public enum ConePrimitiveNode_Which {Radius = 0, Height = 1, Points = 2}
    }
}