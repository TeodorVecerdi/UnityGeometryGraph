using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityCommons;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Curve", "Primitive", "Quadratic Bezier")]
    public class QuadraticBezierPrimitiveCurveNode : AbstractNode<GeometryGraph.Runtime.Graph.QuadraticBezierPrimitiveCurveNode> {
        protected override string Title => "Quadratic Bezier Primitive Curve";
        protected override NodeCategory Category => NodeCategory.Curve;

        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort closedPort;
        private GraphFrameworkPort startPort;
        private GraphFrameworkPort controlPort;
        private GraphFrameworkPort endPort;
        private GraphFrameworkPort resultPort;

        private ClampedIntegerField pointsField;
        private Toggle closedToggle;
        private Vector3Field startField;
        private Vector3Field controlField;
        private Vector3Field endField;

        private int points = 32;
        private bool closed;
        private float3 start = float3.zero;
        private float3 control = float3_ext.right;
        private float3 end = float3_ext.forward;

        protected override void CreateNode() {
            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdatePoints(points));
            (closedPort, closedToggle) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>("Closed", PortType.Boolean, this, onDisconnect: (_, _) => RuntimeNode.UpdateIsClosed(closed));
            (startPort, startField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Start", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateStart(start));
            (controlPort, controlField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Control", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateControl(control));
            (endPort, endField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("End", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateEnd(end));
            resultPort = GraphFrameworkPort.Create("Curve", Direction.Output, Port.Capacity.Multi, PortType.Curve, this);

            pointsField.Min = Constants.MIN_LINE_CURVE_RESOLUTION + 1;
            pointsField.Max = Constants.MAX_CURVE_RESOLUTION + 1;
            pointsField.RegisterValueChangedCallback(evt => {
                int newValue = evt.newValue.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
                if (newValue == points) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change quadratic bezier curve points");
                points = newValue;
                RuntimeNode.UpdatePoints(points);
            });

            closedToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == closed) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change quadratic bezier curve closed");
                closed = evt.newValue;
                RuntimeNode.UpdateIsClosed(closed);
            });

            startField.RegisterValueChangedCallback(evt => {
                float3 newValue = (float3)evt.newValue;
                if (newValue.Equals(start)) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change quadratic bezier curve start position");
                start = newValue;
                RuntimeNode.UpdateStart(start);
            });

            controlField.RegisterValueChangedCallback(evt => {
                float3 newValue = (float3)evt.newValue;
                if (newValue.Equals(control)) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change quadratic bezier curve control position");
                control = newValue;
                RuntimeNode.UpdateControl(control);
            });

            endField.RegisterValueChangedCallback(evt => {
                float3 newValue = (float3)evt.newValue;
                if (newValue.Equals(end)) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change quadratic bezier curve end position");
                end = newValue;
                RuntimeNode.UpdateEnd(end);
            });

            pointsField.SetValueWithoutNotify(points);
            startField.SetValueWithoutNotify(start);
            controlField.SetValueWithoutNotify(control);
            endField.SetValueWithoutNotify(end);

            pointsPort.Add(pointsField);
            closedPort.Add(closedToggle);
            AddPort(closedPort);
            AddPort(pointsPort);
            AddPort(startPort);
            inputContainer.Add(startField);
            AddPort(controlPort);
            inputContainer.Add(controlField);
            AddPort(endPort);
            inputContainer.Add(endField);
            AddPort(resultPort);

            Refresh();
        }

        protected override void BindPorts() {
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(closedPort, RuntimeNode.IsClosedPort);
            BindPort(startPort, RuntimeNode.StartPort);
            BindPort(controlPort, RuntimeNode.ControlPort);
            BindPort(endPort, RuntimeNode.EndPort);
            BindPort(resultPort, RuntimeNode.CurvePort);
        }

        protected internal override JObject Serialize() {
            JObject root =  base.Serialize();
            JArray array = new() {
                points,
                closed ? 1 : 0,
                JsonConvert.SerializeObject(start, float3Converter.Converter),
                JsonConvert.SerializeObject(control, float3Converter.Converter),
                JsonConvert.SerializeObject(end, float3Converter.Converter),
            };
            root["d"] = array;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;

            points = array!.Value<int>(0);
            closed = array!.Value<int>(1) == 1;
            start = JsonConvert.DeserializeObject<float3>(array!.Value<string>(2)!, float3Converter.Converter);
            control = JsonConvert.DeserializeObject<float3>(array!.Value<string>(3)!, float3Converter.Converter);
            end = JsonConvert.DeserializeObject<float3>(array!.Value<string>(4)!, float3Converter.Converter);

            pointsField.SetValueWithoutNotify(points);
            closedToggle.SetValueWithoutNotify(closed);
            startField.SetValueWithoutNotify(start);
            controlField.SetValueWithoutNotify(control);
            endField.SetValueWithoutNotify(end);

            RuntimeNode.UpdatePoints(points);
            RuntimeNode.UpdateIsClosed(closed);
            RuntimeNode.UpdateStart(start);
            RuntimeNode.UpdateControl(control);
            RuntimeNode.UpdateEnd(end);

            base.Deserialize(data);
        }
    }
}