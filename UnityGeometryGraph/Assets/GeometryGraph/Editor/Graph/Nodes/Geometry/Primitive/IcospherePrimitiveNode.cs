using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityCommons;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.IcospherePrimitiveNode.IcospherePrimitiveNode_Which;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Primitive", "Icosphere")]
    public class IcospherePrimitiveNode : AbstractNode<GeometryGraph.Runtime.Graph.IcospherePrimitiveNode> {
        private GraphFrameworkPort radiusPort;
        private GraphFrameworkPort subdivisionsPort;
        private GraphFrameworkPort resultPort;

        private ClampedFloatField radiusField;
        private ClampedIntegerField subdivisionsField;

        private float radius = 1.0f;
        private int subdivisions = 2;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Icosphere Primitive");

            (radiusPort, radiusField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Radius", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(radius, Which.Radius));
            (subdivisionsPort, subdivisionsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Subdivisions", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(subdivisions, Which.Subdivisions));
            resultPort = GraphFrameworkPort.Create("Icosphere", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            radiusField.Min = Constants.MIN_CIRCULAR_GEOMETRY_RADIUS;
            radiusField.RegisterValueChangedCallback(evt => {
                var newValue = evt.newValue.Min(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);
                if (MathF.Abs(newValue - radius) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change radius");
                radius = newValue;
                RuntimeNode.UpdateValue(radius, Which.Radius);
            });

            subdivisionsField.Min = 0;
            subdivisionsField.Max = Constants.MAX_ICOSPHERE_SUBDIVISIONS;
            subdivisionsField.RegisterValueChangedCallback(evt => {
                var newValue = evt.newValue.Clamped(0, Constants.MAX_ICOSPHERE_SUBDIVISIONS);
                if (newValue == subdivisions) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change subdivisions");
                subdivisions = newValue;
                RuntimeNode.UpdateValue(subdivisions, Which.Subdivisions);
            });

            radiusField.SetValueWithoutNotify(1.0f);
            subdivisionsField.SetValueWithoutNotify(2);

            radiusPort.Add(radiusField);
            subdivisionsPort.Add(subdivisionsField);

            AddPort(radiusPort);
            AddPort(subdivisionsPort);
            AddPort(resultPort);

            Refresh();
        }

        public override void BindPorts() {
            BindPort(radiusPort, RuntimeNode.RadiusPort);
            BindPort(subdivisionsPort, RuntimeNode.SubdivisionsPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["r"] = radius;
            root["s"] = subdivisions;

            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            radius = jsonData.Value<float>("r");
            subdivisions = jsonData.Value<int>("s");

            radiusField.SetValueWithoutNotify(radius);
            subdivisionsField.SetValueWithoutNotify(subdivisions);

            RuntimeNode.UpdateValue(radius, Which.Radius);
            RuntimeNode.UpdateValue(subdivisions, Which.Subdivisions);

            base.SetNodeData(jsonData);
        }
    }
}