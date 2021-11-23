using System;
using System.Reflection;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using AnimationCurve = GeometryGraph.Runtime.Data.AnimationCurve;

namespace GeometryGraph.Editor {
    [Title("Float", "Curve Map")]
    public class FloatCurveMapNode : AbstractNode<GeometryGraph.Runtime.Graph.FloatCurveMapNode> {
        
        private GraphFrameworkPort valuePort;
        private GraphFrameworkPort resultPort;

        private FloatField valueField;
        private CurveField curveField;
        private FloatField maxField;

        private float value;
        private AnimationCurve curve = new AnimationCurve(UnityEngine.AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f));

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Curve Map");

            (valuePort, valueField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Value", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(value));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Float, this);

            valueField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - value) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                value = evt.newValue;
                RuntimeNode.UpdateValue(value);
            });

            curveField = new CurveField();
            curveField.renderMode = CurveField.RenderMode.Mesh;
            InjectStyle();

            curveField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change curve");
                curve = (AnimationCurve) evt.newValue;
                RuntimeNode.UpdateCurve(curve);
            });

            curveField.SetValueWithoutNotify(UnityEngine.AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f));
            valueField.SetValueWithoutNotify(0.0f);

            inputContainer.Add(curveField);
            valuePort.Add(valueField);
            
            AddPort(valuePort);
            AddPort(resultPort);
            
            Refresh();
        }

        private void InjectStyle() {
            // I honestly don't know why unity wouldn't make this accessible.
            // They even have a property for it but decided to make it private: `private Color curveColor => this.m_CurveColor;`
            // Honestly, the class is a big mess in general.
            typeof(CurveField).GetField("m_CurveColor", BindingFlags.Instance | BindingFlags.NonPublic)!
                              .SetValue(curveField, new Color(0.8f, 0.8f, 0.8f));
            
            // The curve field has a weird 1px high element with a different background color.
            StyleColor backgroundColor = curveField[0][0][0].style.backgroundColor;
            backgroundColor.value = new Color(0x2E / (float)0xFF, 0x2E / (float)0xFF, 0x2E / (float)0xFF);
            curveField[0][0][0].style.backgroundColor = backgroundColor;
        }

        public override void BindPorts() {
            BindPort(valuePort, RuntimeNode.ValuePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            JObject root = base.GetNodeData();
            root["d"] = new JArray {
                value,
                Serializer.AnimationCurve(curve),
            };
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            JArray array = jsonData["d"] as JArray;
            value = array.Value<float>(0);
            curve = Deserializer.AnimationCurve(array[1] as JObject);
            
            valueField.SetValueWithoutNotify(value);
            curveField.SetValueWithoutNotify(curve.UnityCurve);
            
            RuntimeNode.UpdateValue(value);
            RuntimeNode.UpdateCurve(curve);

            base.SetNodeData(jsonData);
        }
    }
}