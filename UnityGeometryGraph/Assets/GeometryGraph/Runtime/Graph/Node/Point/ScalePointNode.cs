using System;
using System.Linq;
using GeometryGraph.Runtime.Attribute;
using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class ScalePointNode : RuntimeNode {
        private GeometryData geometry;
        private float3 vector;
        private float scalar;
        private string attributeName;
        private ScalePointNode_Mode mode;
        
        private GeometryData result;
        
        public RuntimePort InputPort { get; private set; }
        public RuntimePort VectorPort { get; private set; }
        public RuntimePort ScalarPort { get; private set; }
        public RuntimePort AttributePort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public ScalePointNode(string guid) : base(guid) {
            InputPort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
            VectorPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            ScalarPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            AttributePort = RuntimePort.Create(PortType.String, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
        }

        public void UpdateMode(ScalePointNode_Mode newMode) {
            if(newMode == mode) return;

            mode = newMode;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateValue(object value, ScalePointNode_Which which) {
            switch (which) {
                case ScalePointNode_Which.Vector: vector = (float3)value; break;
                case ScalePointNode_Which.Scalar: scalar = (float)value; break;
                case ScalePointNode_Which.AttributeName: attributeName = (string)value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if(port == ResultPort) return;
            if (port == InputPort) {
                geometry = GetValue(connection, geometry);
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == VectorPort) {
                vector = GetValue(connection, vector);
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == ScalarPort) {
                var newValue = GetValue(connection, scalar);
                if (Math.Abs(newValue - scalar) > 0.000001f) {
                    scalar = newValue;
                    Calculate();
                    NotifyPortValueChanged(ResultPort);
                }
            }else if (port == AttributePort) {
                var newValue = GetValue(connection, attributeName);
                if (!string.Equals(newValue, attributeName, StringComparison.InvariantCulture)) {
                    attributeName = newValue;
                    Calculate();
                    NotifyPortValueChanged(ResultPort);
                } 
            }
        }
        
        public override void RebindPorts() {
            InputPort = Ports[0];
            VectorPort = Ports[1];
            ScalarPort = Ports[2];
            AttributePort = Ports[3];
            ResultPort = Ports[4];
        }

        public void Calculate() {
            if (geometry == null) return;
            result = (GeometryData) geometry.Clone();
            var scaleAttr = result.GetAttribute<Vector3Attribute>("scale", AttributeDomain.Vertex);
            scaleAttr ??= Enumerable.Repeat(float3_util.one, result.Vertices.Count).Into<Vector3Attribute>("scale", AttributeDomain.Vertex);
            
            if (mode == ScalePointNode_Mode.Vector || mode == ScalePointNode_Mode.Float) {
                var multiplier = mode == ScalePointNode_Mode.Vector ? vector : new float3(scalar);
                scaleAttr.Yield(scale => scale * multiplier).Into(scaleAttr);
                result.StoreAttribute(scaleAttr);
            } else {
                if (!result.HasAttribute(attributeName)) {
                    Debug.LogWarning($"Couldn't find attribute [{attributeName}]");
                    return;
                }
                
                var otherAttribute = result.GetAttribute<Vector3Attribute>(attributeName, AttributeDomain.Vertex);
                scaleAttr.YieldWithAttribute(otherAttribute, (scale, multiplier) => scale * multiplier).Into(scaleAttr);
                result.StoreAttribute(scaleAttr);
            }
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["v"] = JsonConvert.SerializeObject(vector, Formatting.None, float3Converter.Converter),
                ["s"] = scalar,
                ["a"] = attributeName,
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            vector = JsonConvert.DeserializeObject<float3>(data.Value<string>("v"), float3Converter.Converter);
            scalar = data.Value<float>("s");
            attributeName = data.Value<string>("a");
           
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public enum ScalePointNode_Which {Vector = 0, Scalar = 1, AttributeName = 2}
        public enum ScalePointNode_Mode {Vector = 0, Float = 1, Attribute = 2}
    }
}