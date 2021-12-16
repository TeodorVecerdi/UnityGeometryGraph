using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityCommons;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Primitive", "Plane")]
    public class PlanePrimitiveNode : AbstractNode<GeometryGraph.Runtime.Graph.PlanePrimitiveNode> {
        protected override string Title => "Plane Primitive";
        protected override NodeCategory Category => NodeCategory.Geometry;

        private GraphFrameworkPort widthPort;
        private GraphFrameworkPort heightPort;
        private GraphFrameworkPort subdivisionsPort;
        private GraphFrameworkPort resultPort;

        private ClampedFloatField widthField;
        private ClampedFloatField heightField;
        private ClampedIntegerField subdivisionsField;

        private float2 size = float2_ext.one;
        private int subdivisions;

        protected override void CreateNode() {
            (widthPort, widthField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Width", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateWidth(size.x));
            (heightPort, heightField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Height", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateHeight(size.y));
            (subdivisionsPort, subdivisionsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Subdivisions", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateSubdivisions(subdivisions));
            resultPort = GraphFrameworkPort.Create("Plane", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            widthField.Min = 0.0f;
            widthField.RegisterValueChangedCallback(evt => {
                float newValue = evt.newValue.MinClamped(0.0f);
                if (MathF.Abs(newValue - size.x) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change width");
                size.x = newValue;
                RuntimeNode.UpdateWidth(size.x);
            });
            
            heightField.Min = 0.0f;
            heightField.RegisterValueChangedCallback(evt => {
                float newValue = evt.newValue.MinClamped(0.0f);
                if (MathF.Abs(newValue - size.y) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change height");
                size.y = newValue;
                RuntimeNode.UpdateHeight(size.y);
            });

            subdivisionsField.Min = 0;
            subdivisionsField.RegisterValueChangedCallback(evt => {
                int newValue = evt.newValue.MinClamped(0);
                if (newValue == subdivisions) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change subdivisions");
                subdivisions = newValue;
                RuntimeNode.UpdateSubdivisions(subdivisions);
            });

            widthField.SetValueWithoutNotify(1.0f);
            heightField.SetValueWithoutNotify(1.0f);
            subdivisionsField.SetValueWithoutNotify(0);

            widthPort.Add(widthField);
            heightPort.Add(heightField);
            subdivisionsPort.Add(subdivisionsField);

            AddPort(widthPort);
            AddPort(heightPort);
            AddPort(subdivisionsPort);
            AddPort(resultPort);

            Refresh();
        }

        protected override void BindPorts() {
            BindPort(widthPort, RuntimeNode.WidthPort);
            BindPort(heightPort, RuntimeNode.HeightPort);
            BindPort(subdivisionsPort, RuntimeNode.SubdivisionsPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject GetNodeData() {
            JObject root = base.GetNodeData();

            root["w"] = size.x;
            root["h"] = size.y;
            root["s"] = subdivisions;

            return root;
        }

        protected internal override void SetNodeData(JObject jsonData) {
            size.x = jsonData.Value<float>("w");
            size.y = jsonData.Value<float>("h");
            subdivisions = jsonData.Value<int>("s");

            widthField.SetValueWithoutNotify(size.x);
            heightField.SetValueWithoutNotify(size.y);
            subdivisionsField.SetValueWithoutNotify(subdivisions);

            RuntimeNode.UpdateWidth(size.x);
            RuntimeNode.UpdateHeight(size.y);
            RuntimeNode.UpdateSubdivisions(subdivisions);

            base.SetNodeData(jsonData);
        }
    }
}