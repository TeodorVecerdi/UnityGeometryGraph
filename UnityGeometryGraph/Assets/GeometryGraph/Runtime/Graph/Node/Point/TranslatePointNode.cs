using System;
using GeometryGraph.Runtime.Attribute;
using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class TranslatePointNode : RuntimeNode {
        private GeometryData geometry;
        private float3 translation;
        private string attributeName;
        private TranslatePointNode_Mode mode;
        
        private GeometryData result;
        
        public RuntimePort InputPort { get; private set; }
        public RuntimePort TranslationPort { get; private set; }
        public RuntimePort AttributePort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public TranslatePointNode(string guid) : base(guid) {
            InputPort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
            TranslationPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            AttributePort = RuntimePort.Create(PortType.String, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
        }

        public void UpdateMode(TranslatePointNode_Mode newMode) {
            if(newMode == mode) return;

            mode = newMode;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateValue(object value, TranslatePointNode_Which which) {
            switch (which) {
                case TranslatePointNode_Which.Translation: translation = (float3)value; break;
                case TranslatePointNode_Which.AttributeName: attributeName = (string)value; break;
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
            } else if (port == TranslationPort) {
                translation = GetValue(connection, translation);
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == AttributePort) {
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
            TranslationPort = Ports[1];
            AttributePort = Ports[2];
            ResultPort = Ports[3];
        }

        public void Calculate() {
            if (geometry == null) return;
            result = (GeometryData) geometry.Clone();
            var positionAttr = result.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            if (mode == TranslatePointNode_Mode.Vector) {
                positionAttr.Yield(position => position + translation).Into(positionAttr);
                
                /*
                Storing even though I'm writing to the same attribute (with the .Into call) because AttributeManager
                makes no guarantee that it will return the original attribute and not a clone of the attribute.
                
                Cloning happens when there is a domain or type mismatch and the original attribute gets converted.
                
                Also, storing the same attribute twice has no side effects and it's safe. The StoreAttribute method 
                returns a boolean indicating whether an attribute was overwritten by the Store operation in case you 
                care about that.
                */
                result.StoreAttribute(positionAttr);
            } else {
                if (!result.HasAttribute(attributeName)) {
                    Debug.LogWarning($"Couldn't find attribute [{attributeName}]");
                    return;
                }
                
                var otherAttribute = result.GetAttribute<Vector3Attribute>(attributeName, AttributeDomain.Vertex);
                positionAttr.YieldWithAttribute(otherAttribute, (position, translation) => position + translation).Into(positionAttr);
                result.StoreAttribute(positionAttr);
            }
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["t"] = JsonConvert.SerializeObject(translation, Formatting.None, float3Converter.Converter),
                ["a"] = attributeName,
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            translation = JsonConvert.DeserializeObject<float3>(data.Value<string>("t"), float3Converter.Converter);
            attributeName = data.Value<string>("a");
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public enum TranslatePointNode_Which {Translation = 0, AttributeName = 1}
        public enum TranslatePointNode_Mode {Vector = 0, Attribute = 1}
    }
}