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
    [Title("Curve", "Primitive", "Line")]
    public class LinePrimitiveCurveNode : AbstractNode<GeometryGraph.Runtime.Graph.LinePrimitiveCurveNode> {
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort startPort;
        private GraphFrameworkPort endPort;
        private GraphFrameworkPort resultPort;

        private ClampedIntegerField pointsField;
        private Vector3Field startField;
        private Vector3Field endField;

        private int points = 2;
        private float3 start = float3.zero;
        private float3 end = float3_ext.right;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Line Primitive Curve");

            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdatePoints(points));
            (startPort, startField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Start", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateStart(start));
            (endPort, endField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("End", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateEnd(end));
            resultPort = GraphFrameworkPort.Create("Curve", Direction.Output, Port.Capacity.Multi, PortType.Curve, this);

            pointsField.Min = Constants.MIN_LINE_CURVE_RESOLUTION + 1;
            pointsField.Max = Constants.MAX_CURVE_RESOLUTION + 1;
            pointsField.RegisterValueChangedCallback(evt => {
                var newValue = evt.newValue.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
                if (newValue == points) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change line curve points");
                points = newValue;
                RuntimeNode.UpdatePoints(points);
            });

            startField.RegisterValueChangedCallback(evt => {
                var newValue = (float3)evt.newValue;
                if (newValue.Equals(start)) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change line curve start position");
                start = newValue;
                RuntimeNode.UpdateStart(start);
            });
            
            endField.RegisterValueChangedCallback(evt => {
                var newValue = (float3)evt.newValue;
                if (newValue.Equals(end)) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change line curve end position");
                end = newValue;
                RuntimeNode.UpdateEnd(end);
            });
            
            pointsField.SetValueWithoutNotify(points);
            startField.SetValueWithoutNotify(start);
            endField.SetValueWithoutNotify(end);
            
            pointsPort.Add(pointsField);
            AddPort(pointsPort);
            AddPort(startPort);
            inputContainer.Add(startField);
            AddPort(endPort);
            inputContainer.Add(endField);
            AddPort(resultPort);
            
            Refresh();
        }

        public override void BindPorts() {
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(startPort, RuntimeNode.StartPort);
            BindPort(endPort, RuntimeNode.EndPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root =  base.GetNodeData();
            var array = new JArray {
                points,
                JsonConvert.SerializeObject(start, float3Converter.Converter),
                JsonConvert.SerializeObject(end, float3Converter.Converter),
            };
            root["d"] = array;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            var array = jsonData["d"] as JArray;

            points = array!.Value<int>(0);
            start = JsonConvert.DeserializeObject<float3>(array!.Value<string>(1)!, float3Converter.Converter);
            end = JsonConvert.DeserializeObject<float3>(array!.Value<string>(2)!, float3Converter.Converter);
            
            pointsField.SetValueWithoutNotify(points);
            startField.SetValueWithoutNotify(start);
            endField.SetValueWithoutNotify(end);
            
            RuntimeNode.UpdatePoints(points);
            RuntimeNode.UpdateStart(start);
            RuntimeNode.UpdateEnd(end);
            
            base.SetNodeData(jsonData);
        }
    }
}