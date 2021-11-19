using System;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    public class IcospherePrimitiveNode : RuntimeNode {
        private float radius = 1.0f;
        private int subdivisions = 2;

        public float Radius {
            get => radius;
            set {
                DebugUtility.Log($"UPDATED RADIUS TO : {value}");
                radius = value;
            }
        }

        private GeometryData result;
        private bool geometryDirty;

        public RuntimePort RadiusPort { get; private set; }
        public RuntimePort SubdivisionsPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public IcospherePrimitiveNode(string guid) : base(guid) {
            RadiusPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            SubdivisionsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateValue(object value, IcospherePrimitiveNode_Which which) {
            switch (which) {
                case IcospherePrimitiveNode_Which.Radius: Radius = (float)value; break;
                case IcospherePrimitiveNode_Which.Subdivisions: {
                    subdivisions = (int)value;
                    geometryDirty = true;
                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }
            
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            DebugUtility.Log($"Returning result with Radius:`{radius}` Subdiv:`{subdivisions}`");
            if (result == null) CalculateResult();
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            
            if (port == RadiusPort) {
                var newValue = GetValue(connection, radius);
                if (newValue < 0.01f) newValue = 0.01f;
                DebugUtility.Log("Updated radius");
                if (Math.Abs(newValue - radius) > 0.000001f) {
                    Radius = newValue;
                    CalculateResult();
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == SubdivisionsPort) {
                var newValue = GetValue(connection, subdivisions).Clamped(0, Constants.MAX_ICOSPHERE_SUBDIVISIONS);
                DebugUtility.Log("Updated subdivisions");
                if (newValue != subdivisions) {
                    subdivisions = newValue;
                    geometryDirty = true;
                    CalculateResult();
                    NotifyPortValueChanged(ResultPort);
                }
            }
        }
        
        public override void RebindPorts() {
            RadiusPort = Ports[0];
            SubdivisionsPort = Ports[1];
            ResultPort = Ports[2];
        }

        private void CalculateResult() {
            DebugUtility.Log($"Calculating result with Radius:`{radius}` Subdiv:`{subdivisions}`");

            if (geometryDirty || result == null) {
                DebugUtility.Log("Regenerated geometry");
                // Recalculate new geometry
                result = GeometryPrimitive.Icosphere(radius, subdivisions);
                geometryDirty = false;
            } else {
                DebugUtility.Log("Recalculated radius on existing geometry");
                // Re-project on sphere with new radius
                var positionAttribute = result.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
                positionAttribute!.Yield(pos => math.normalize(pos) * radius).Into(positionAttribute);
                result.StoreAttribute(positionAttribute);
            }
        }
        
        public override string GetCustomData() {
            var data = new JObject {
                ["r"] = radius,
                ["p"] = subdivisions,
                ["d"] = geometryDirty ? 1 : 0,
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            Radius = data.Value<float>("r");
            subdivisions = data.Value<int>("p");
            geometryDirty = data.Value<int>("d") == 1 || result == null;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public enum IcospherePrimitiveNode_Which {Radius = 0, Subdivisions = 1}
    }
}