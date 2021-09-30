using System;
using GeometryGraph.Runtime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    public class PlanePrimitiveNode : RuntimeNode {
        private float2 size = float2_util.one;
        private int subdivisions;
        
        private GeometryData result;

        public RuntimePort WidthPort { get; private set; }
        public RuntimePort HeightPort { get; private set; }
        public RuntimePort SubdivisionsPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public PlanePrimitiveNode(string guid) : base(guid) {
            WidthPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            HeightPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            SubdivisionsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateValue(object value, PlanePrimitiveNode_Which which) {
            switch (which) {
                case PlanePrimitiveNode_Which.Width: size.x = (float)value; break;
                case PlanePrimitiveNode_Which.Height: size.y = (float)value; break;
                case PlanePrimitiveNode_Which.Subdivisions: subdivisions = (int)value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }
            
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            
            if (port == WidthPort) {
                var newValue = GetValue(connection, size.x);
                if (Math.Abs(newValue - size.x) > 0.000001f) {
                    size.x = newValue;
                    CalculateResult();
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == HeightPort) {
                var newValue = GetValue(connection, size.y);
                if (Math.Abs(newValue - size.x) > 0.000001f) {
                    size.y = newValue;
                    CalculateResult();
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == SubdivisionsPort) {
                var newValue = GetValue(connection, subdivisions);
                if (newValue != subdivisions) {
                    subdivisions = newValue;
                    CalculateResult();
                    NotifyPortValueChanged(ResultPort);
                }
            }
        }
        
        public override void RebindPorts() {
            WidthPort = Ports[0];
            HeightPort = Ports[1];
            SubdivisionsPort = Ports[2];
            ResultPort = Ports[3];
        }

        private void CalculateResult() {
            result = GeometryData.MakePlane(size, subdivisions);
        }
        
        public override string GetCustomData() {
            var data = new JObject {
                ["w"] = size.x,
                ["h"] = size.y,
                ["s"] = subdivisions
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            size.x = data.Value<float>("w");
            size.y = data.Value<float>("h");
            subdivisions = data.Value<int>("s");
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public enum PlanePrimitiveNode_Which {Width = 0, Height = 1, Subdivisions = 2}
    }
}