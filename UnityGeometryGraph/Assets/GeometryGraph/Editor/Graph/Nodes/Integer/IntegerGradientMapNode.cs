using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Gradient = GeometryGraph.Runtime.Data.Gradient;

namespace GeometryGraph.Editor {
    [Title("Integer", "Gradient Map")]
    public class IntegerGradientMapNode : AbstractNode<GeometryGraph.Runtime.Graph.IntegerGradientMapNode> {
        protected override string Title => "Gradient Map (Integer)";
        protected override NodeCategory Category => NodeCategory.Integer;

        private int value;
        private int min = 0;
        private int max = 100;
        
        private IntegerField valueField;
        private IntegerField minField;
        private IntegerField maxField;
        
        private GraphFrameworkPort valuePort;
        private GraphFrameworkPort minPort;
        private GraphFrameworkPort maxPort;
        private GraphFrameworkPort resultRGBPort;
        private GraphFrameworkPort resultAlphaPort;

        private GradientField gradientField;
        private GradientField colorOnlyField;

        private Gradient gradient = (Gradient) GeometryGraph.Runtime.Graph.FloatGradientMapNode.Default;

        protected override void CreateNode() {
            (valuePort, valueField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Value", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(value));
            (minPort, minField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Min", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateMin(min));
            (maxPort, maxField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Max", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateMax(max));
            
            resultRGBPort = GraphFrameworkPort.Create("Color", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);
            resultAlphaPort = GraphFrameworkPort.Create("Alpha", Direction.Output, Port.Capacity.Multi, PortType.Float, this);

            valueField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == value) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                value = evt.newValue;
                RuntimeNode.UpdateValue(value);
            });
            
            minField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == min) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change min");
                min = evt.newValue;
                RuntimeNode.UpdateMin(min);
            });
            
            maxField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == max) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change max");
                max = evt.newValue;
                RuntimeNode.UpdateMax(max);
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
            valueField.SetValueWithoutNotify(value);
            minField.SetValueWithoutNotify(min);
            maxField.SetValueWithoutNotify(max);

            VisualElement gradientContainer = new();
            gradientContainer.AddToClassList("gradient-container");
            gradientContainer.Add(gradientField);
            gradientContainer.Add(colorOnlyField);
            
            inputContainer.Add(gradientContainer);
            valuePort.Add(valueField);
            minPort.Add(minField);
            maxPort.Add(maxField);
            
            AddPort(valuePort);
            AddPort(minPort);
            AddPort(maxPort);
            AddPort(resultRGBPort);
            AddPort(resultAlphaPort);

            Refresh();
        }

        private void SetColorField(UnityEngine.Gradient gradient) {
            colorOnlyField.value = new UnityEngine.Gradient {colorKeys = gradient.colorKeys, alphaKeys = new []{new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f)}};
        }

        protected override void BindPorts() {
            BindPort(valuePort, RuntimeNode.ValuePort);
            BindPort(minPort, RuntimeNode.MinPort);
            BindPort(maxPort, RuntimeNode.MaxPort);
            BindPort(resultRGBPort, RuntimeNode.ResultRGBPort);
            BindPort(resultAlphaPort, RuntimeNode.ResultAlphaPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            root["d"] = new JArray {
                value,
                min,
                max,
                Serializer.Gradient(gradient),
            };
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;
            value = array.Value<int>(0);
            min = array.Value<int>(1);
            max = array.Value<int>(2);
            gradient = Deserializer.Gradient(array[3] as JObject);
            
            valueField.SetValueWithoutNotify(value);
            minField.SetValueWithoutNotify(min);
            maxField.SetValueWithoutNotify(max);
            gradientField.SetValueWithoutNotify(gradient.UnityGradient);
            SetColorField(gradient.UnityGradient);
            
            RuntimeNode.UpdateValue(value);
            RuntimeNode.UpdateMin(min);
            RuntimeNode.UpdateMax(max);
            RuntimeNode.UpdateGradient(gradient);

            base.Deserialize(data);
        }
    }
}