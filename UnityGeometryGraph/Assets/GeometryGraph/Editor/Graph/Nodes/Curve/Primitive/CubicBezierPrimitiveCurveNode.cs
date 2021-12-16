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
    [Title("Curve", "Primitive", "Cubic Bezier")]
    public class CubicBezierPrimitiveCurveNode : AbstractNode<GeometryGraph.Runtime.Graph.CubicBezierPrimitiveCurveNode> {
        protected override string Title => "Cubic Bezier Primitive Curve";
        protected override NodeCategory Category => NodeCategory.Curve;

        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort closedPort;
        private GraphFrameworkPort startPort;
        private GraphFrameworkPort controlAPort;
        private GraphFrameworkPort controlBPort;
        private GraphFrameworkPort endPort;
        private GraphFrameworkPort resultPort;

        private ClampedIntegerField pointsField;
        private Toggle closedToggle;
        private Vector3Field startField;
        private Vector3Field controlAField;
        private Vector3Field controlBField;
        private Vector3Field endField;

        private int points = 32;
        private bool closed;
        private float3 start = float3.zero;
        private float3 controlA = float3_ext.forward;
        private float3 controlB = float3_ext.right + float3_ext.forward;
        private float3 end = float3_ext.right;

        protected override void CreateNode() {
            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdatePoints(points));
            (closedPort, closedToggle) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>("Closed", PortType.Boolean, this, onDisconnect: (_, _) => RuntimeNode.UpdateClosed(closed));
            (startPort, startField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Start", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateStart(start));
            (controlAPort, controlAField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Control A", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateControlA(controlA));
            (controlBPort, controlBField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Control B", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateControlB(controlB));
            (endPort, endField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("End", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateEnd(end));
            resultPort = GraphFrameworkPort.Create("Curve", Direction.Output, Port.Capacity.Multi, PortType.Curve, this);

            pointsField.Min = Constants.MIN_LINE_CURVE_RESOLUTION + 1;
            pointsField.Max = Constants.MAX_CURVE_RESOLUTION + 1;
            pointsField.RegisterValueChangedCallback(evt => {
                int newValue = evt.newValue.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
                if (newValue == points) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change cubic bezier curve points");
                points = newValue;
                RuntimeNode.UpdatePoints(points);
            });

            closedToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == closed) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change cubic bezier curve closed");
                closed = evt.newValue;
                RuntimeNode.UpdateClosed(closed);
            });

            startField.RegisterValueChangedCallback(evt => {
                float3 newValue = (float3)evt.newValue;
                if (newValue.Equals(start)) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change cubic bezier curve start position");
                start = newValue;
                RuntimeNode.UpdateStart(start);
            });
            
            controlAField.RegisterValueChangedCallback(evt => {
                float3 newValue = (float3)evt.newValue;
                if (newValue.Equals(controlA)) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change cubic bezier curve control position");
                controlA = newValue;
                RuntimeNode.UpdateControlA(controlA);
            });
            
            controlBField.RegisterValueChangedCallback(evt => {
                float3 newValue = (float3)evt.newValue;
                if (newValue.Equals(controlB)) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change cubic bezier curve control position");
                controlB = newValue;
                RuntimeNode.UpdateControlB(controlB);
            });
            
            endField.RegisterValueChangedCallback(evt => {
                float3 newValue = (float3)evt.newValue;
                if (newValue.Equals(end)) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change cubic bezier curve end position");
                end = newValue;
                RuntimeNode.UpdateEnd(end);
            });
            
            pointsField.SetValueWithoutNotify(points);
            startField.SetValueWithoutNotify(start);
            controlAField.SetValueWithoutNotify(controlA);
            controlBField.SetValueWithoutNotify(controlB);
            endField.SetValueWithoutNotify(end);
            
            pointsPort.Add(pointsField);
            closedPort.Add(closedToggle);
            AddPort(closedPort);
            AddPort(pointsPort);
            AddPort(startPort);
            inputContainer.Add(startField);
            AddPort(controlAPort);
            inputContainer.Add(controlAField);
            AddPort(controlBPort);
            inputContainer.Add(controlBField);
            AddPort(endPort);
            inputContainer.Add(endField);
            AddPort(resultPort);
            
            Refresh();
        }

        protected override void BindPorts() {
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(closedPort, RuntimeNode.ClosedPort);
            BindPort(startPort, RuntimeNode.StartPort);
            BindPort(controlAPort, RuntimeNode.ControlAPort);
            BindPort(controlBPort, RuntimeNode.ControlBPort);
            BindPort(endPort, RuntimeNode.EndPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            JObject root =  base.GetNodeData();
            JArray array = new JArray {
                points,
                closed ? 1 : 0,
                JsonConvert.SerializeObject(start, float3Converter.Converter),
                JsonConvert.SerializeObject(controlA, float3Converter.Converter),
                JsonConvert.SerializeObject(controlB, float3Converter.Converter),
                JsonConvert.SerializeObject(end, float3Converter.Converter),
            };
            root["d"] = array;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            JArray array = jsonData["d"] as JArray;

            points = array!.Value<int>(0);
            closed = array!.Value<int>(1) == 1;
            start = JsonConvert.DeserializeObject<float3>(array!.Value<string>(2)!, float3Converter.Converter);
            controlA = JsonConvert.DeserializeObject<float3>(array!.Value<string>(3)!, float3Converter.Converter);
            controlB = JsonConvert.DeserializeObject<float3>(array!.Value<string>(4)!, float3Converter.Converter);
            end = JsonConvert.DeserializeObject<float3>(array!.Value<string>(5)!, float3Converter.Converter);
            
            pointsField.SetValueWithoutNotify(points);
            closedToggle.SetValueWithoutNotify(closed);
            startField.SetValueWithoutNotify(start);
            controlAField.SetValueWithoutNotify(controlA);
            controlBField.SetValueWithoutNotify(controlB);
            endField.SetValueWithoutNotify(end);
            
            RuntimeNode.UpdatePoints(points);
            RuntimeNode.UpdateClosed(closed);
            RuntimeNode.UpdateStart(start);
            RuntimeNode.UpdateControlA(controlA);
            RuntimeNode.UpdateControlB(controlB);
            RuntimeNode.UpdateEnd(end);
            
            base.SetNodeData(jsonData);
        }
    }
}