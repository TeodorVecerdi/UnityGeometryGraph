using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Gradient = GeometryGraph.Runtime.Data.Gradient;

namespace GeometryGraph.Editor {
    [Title("Float", "Gradient Map")]
    public class FloatGradientMapNode : AbstractNode<GeometryGraph.Runtime.Graph.FloatGradientMapNode> {
        protected override string Title => "Gradient Map (Float)";
        protected override NodeCategory Category => NodeCategory.Float;

        private GraphFrameworkPort valuePort;
        private GraphFrameworkPort resultRGBPort;
        private GraphFrameworkPort resultAlphaPort;

        private FloatField valueField;
        private GradientField gradientField;
        private GradientField colorOnlyField;

        private float value;
        private Gradient gradient = (Gradient) GeometryGraph.Runtime.Graph.FloatGradientMapNode.Default;

        protected override void CreateNode() {
            (valuePort, valueField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Value", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(value));
            resultRGBPort = GraphFrameworkPort.Create("Color", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);
            resultAlphaPort = GraphFrameworkPort.Create("Alpha", Direction.Output, Port.Capacity.Multi, PortType.Float, this);

            valueField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - value) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                value = evt.newValue;
                RuntimeNode.UpdateValue(value);
            });

            gradientField = new GradientField();
            gradientField.AddToClassList("gradient-field-main");
            colorOnlyField = new GradientField();
            colorOnlyField.AddToClassList("gradient-field-color");
            colorOnlyField.SetEnabled(false);

            gradientField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change gradient");
                gradient = (Gradient) evt.newValue;
                SetColorField(evt.newValue);
                RuntimeNode.UpdateGradient(gradient);
            });

            gradientField.SetValueWithoutNotify(gradient.UnityGradient);
            SetColorField(gradient.UnityGradient);
            valueField.SetValueWithoutNotify(0.0f);

            VisualElement gradientContainer = new VisualElement();
            gradientContainer.AddToClassList("gradient-container");
            gradientContainer.Add(gradientField);
            gradientContainer.Add(colorOnlyField);
            
            inputContainer.Add(gradientContainer);
            valuePort.Add(valueField);
            
            AddPort(valuePort);
            AddPort(resultRGBPort);
            AddPort(resultAlphaPort);

            Refresh();
        }

        private void SetColorField(UnityEngine.Gradient gradient) {
            colorOnlyField.value = new UnityEngine.Gradient {colorKeys = gradient.colorKeys, alphaKeys = new []{new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f)}};
        }

        protected override void BindPorts() {
            BindPort(valuePort, RuntimeNode.ValuePort);
            BindPort(resultRGBPort, RuntimeNode.ResultRGBPort);
            BindPort(resultAlphaPort, RuntimeNode.ResultAlphaPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            root["d"] = new JArray {
                value,
                Serializer.Gradient(gradient),
            };
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;
            value = array.Value<float>(0);
            gradient = Deserializer.Gradient(array[1] as JObject);
            
            valueField.SetValueWithoutNotify(value);
            gradientField.SetValueWithoutNotify(gradient.UnityGradient);
            SetColorField(gradient.UnityGradient);
            
            RuntimeNode.UpdateValue(value);
            RuntimeNode.UpdateGradient(gradient);

            base.Deserialize(data);
        }
    }
}