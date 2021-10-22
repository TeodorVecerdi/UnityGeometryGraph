﻿using GeometryGraph.Runtime;
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

        private int points = 2;
        private bool closed = false;
        private float3 start = float3.zero;
        private float3 control = float3_util.right;
        private float3 end = float3_util.forward;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Quadratic Bezier Primitive Curve");

            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdatePoints(points));
            (closedPort, closedToggle) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>("Closed", Orientation.Horizontal, PortType.Boolean, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateClosed(closed));
            (startPort, startField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Start", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateStart(start));
            (controlPort, controlField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Control", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateControl(control));
            (endPort, endField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("End", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateEnd(end));
            resultPort = GraphFrameworkPort.Create("Curve", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Curve, edgeConnectorListener, this);

            pointsField.Min = Constants.MIN_LINE_CURVE_RESOLUTION + 1;
            pointsField.Max = Constants.MAX_CURVE_RESOLUTION + 1;
            pointsField.RegisterValueChangedCallback(evt => {
                var newValue = evt.newValue.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
                if (newValue == points) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change quadratic bezier curve points");
                points = newValue;
                RuntimeNode.UpdatePoints(points);
            });

            closedToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == closed) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change quadratic bezier curve closed");
                closed = evt.newValue;
                RuntimeNode.UpdateClosed(closed);
            });

            startField.RegisterValueChangedCallback(evt => {
                var newValue = (float3)evt.newValue;
                if (newValue.Equals(start)) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change quadratic bezier curve start position");
                start = newValue;
                RuntimeNode.UpdateStart(start);
            });
            
            controlField.RegisterValueChangedCallback(evt => {
                var newValue = (float3)evt.newValue;
                if (newValue.Equals(control)) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change quadratic bezier curve control position");
                control = newValue;
                RuntimeNode.UpdateControl(control);
            });
            
            endField.RegisterValueChangedCallback(evt => {
                var newValue = (float3)evt.newValue;
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

        public override void BindPorts() {
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(closedPort, RuntimeNode.ClosedPort);
            BindPort(startPort, RuntimeNode.StartPort);
            BindPort(controlPort, RuntimeNode.ControlPort);
            BindPort(endPort, RuntimeNode.EndPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root =  base.GetNodeData();
            var array = new JArray {
                points,
                closed ? 1 : 0,
                JsonConvert.SerializeObject(start, float3Converter.Converter),
                JsonConvert.SerializeObject(control, float3Converter.Converter),
                JsonConvert.SerializeObject(end, float3Converter.Converter),
            };
            root["d"] = array;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            var array = jsonData["d"] as JArray;

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
            RuntimeNode.UpdateClosed(closed);
            RuntimeNode.UpdateStart(start);
            RuntimeNode.UpdateControl(control);
            RuntimeNode.UpdateEnd(end);
            
            base.SetNodeData(jsonData);
        }
    }
}