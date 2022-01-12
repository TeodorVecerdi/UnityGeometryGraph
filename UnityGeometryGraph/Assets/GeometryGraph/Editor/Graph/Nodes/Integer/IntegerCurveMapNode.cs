using System.Reflection;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using AnimationCurve = GeometryGraph.Runtime.Data.AnimationCurve;

namespace GeometryGraph.Editor {
    [Title("Integer", "Curve Map")]
    public class IntegerCurveMapNode : AbstractNode<GeometryGraph.Runtime.Graph.IntegerCurveMapNode> {
        protected override string Title => "Curve Map (Integer)";
        protected override NodeCategory Category => NodeCategory.Integer;

        private int value;
        private int min = 0;
        private int max = 100;

        private IntegerField valueField;
        private IntegerField minField;
        private IntegerField maxField;
        private CurveField curveField;

        private GraphFrameworkPort valuePort;
        private GraphFrameworkPort minPort;
        private GraphFrameworkPort maxPort;
        private GraphFrameworkPort resultPort;

        private AnimationCurve curve = new(UnityEngine.AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f));

        protected override void CreateNode() {
            (valuePort, valueField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Value", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(value));
            (minPort, minField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Min", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateMin(min));
            (maxPort, maxField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Max", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateMax(max));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Integer, this);

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

            curveField = new CurveField { renderMode = CurveField.RenderMode.Mesh };
            InjectStyle();

            curveField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change curve");
                curve = (AnimationCurve) evt.newValue;
                RuntimeNode.UpdateCurve(curve);
            });

            curveField.SetValueWithoutNotify(UnityEngine.AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f));
            valueField.SetValueWithoutNotify(value);
            minField.SetValueWithoutNotify(min);
            maxField.SetValueWithoutNotify(max);

            inputContainer.Add(curveField);
            valuePort.Add(valueField);
            minPort.Add(minField);
            maxPort.Add(maxField);

            AddPort(valuePort);
            AddPort(minPort);
            AddPort(maxPort);
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

        protected override void BindPorts() {
            BindPort(valuePort, RuntimeNode.ValuePort);
            BindPort(minPort, RuntimeNode.MinPort);
            BindPort(maxPort, RuntimeNode.MaxPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            root["d"] = new JArray {
                value,
                min,
                max,
                Serializer.AnimationCurve(curve),
            };
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;
            value = array.Value<int>(0);
            min = array.Value<int>(1);
            max = array.Value<int>(2);
            curve = Deserializer.AnimationCurve(array[3] as JObject);

            valueField.SetValueWithoutNotify(value);
            minField.SetValueWithoutNotify(min);
            maxField.SetValueWithoutNotify(max);
            curveField.SetValueWithoutNotify(curve.UnityCurve);

            RuntimeNode.UpdateValue(value);
            RuntimeNode.UpdateMin(min);
            RuntimeNode.UpdateMax(max);
            RuntimeNode.UpdateCurve(curve);

            base.Deserialize(data);
        }
    }
}