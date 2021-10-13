using System;
using GeometryGraph.Runtime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.Graph {
    public class CylinderPrimitiveNode : RuntimeNode {
        private float bottomRadius = 1.0f;
        private float topRadius = 1.0f;
        private float height = 2.0f;
        private int points = 8;
        
        private GeometryData result;

        public RuntimePort BottomRadiusPort { get; private set; }
        public RuntimePort TopRadiusPort { get; private set; }
        public RuntimePort HeightPort { get; private set; }
        public RuntimePort PointsPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public CylinderPrimitiveNode(string guid) : base(guid) {
            BottomRadiusPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            TopRadiusPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            HeightPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            PointsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateValue(object value, CylinderPrimitiveNode_Which which) {
            switch (which) {
                case CylinderPrimitiveNode_Which.BottomRadius: bottomRadius = (float)value; break;
                case CylinderPrimitiveNode_Which.TopRadius: topRadius = (float)value; break;
                case CylinderPrimitiveNode_Which.Height: height = (float)value; break;
                case CylinderPrimitiveNode_Which.Points: points = (int)value; break;
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
            
            if (port == BottomRadiusPort) {
                var newValue = GetValue(connection, bottomRadius);
                if (Math.Abs(newValue - bottomRadius) > 0.000001f) {
                    bottomRadius = newValue;
                    CalculateResult();
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == TopRadiusPort) {
                var newValue = GetValue(connection, topRadius);
                if (Math.Abs(newValue - topRadius) > 0.000001f) {
                    topRadius = newValue;
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
        
        public override void RebindPorts() {
            BottomRadiusPort = Ports[0];
            TopRadiusPort = Ports[1];
            HeightPort = Ports[2];
            PointsPort = Ports[3];
            ResultPort = Ports[4];
        }

        private void CalculateResult() {
            result = Primitive.Cylinder(bottomRadius, topRadius, height, points);
        }
        
        public override string GetCustomData() {
            var data = new JObject {
                ["r"] = bottomRadius,
                ["R"] = topRadius,
                ["h"] = height,
                ["p"] = points
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            bottomRadius = data.Value<float>("r");
            topRadius = data.Value<float>("R");
            height = data.Value<float>("h");
            points = data.Value<int>("p");
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public enum CylinderPrimitiveNode_Which {BottomRadius = 0, TopRadius = 1, Height = 2, Points = 3}
    }
}