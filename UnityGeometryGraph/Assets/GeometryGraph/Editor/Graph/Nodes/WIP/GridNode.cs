using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor.WIP {
    [Title("WIP", "Grid Node")]
    public class GridNode : AbstractNode<GeometryGraph.Runtime.Graph.GridNode> {
        private GraphFrameworkPort widthPort;
        private GraphFrameworkPort heightPort;
        private GraphFrameworkPort pointsXPort;
        private GraphFrameworkPort pointsYPort;
        private GraphFrameworkPort resultPort;
        
        private ClampedFloatField widthField;
        private ClampedFloatField heightField;
        private ClampedIntegerField pointsXField;
        private ClampedIntegerField pointsYField;

        private float width = 1.0f;
        private float height = 1.0f;
        private int pointsX = 4;
        private int pointsY = 4;

        public override void CreateNode() {
            Initialize("Grid", NodeCategory.Geometry);
            
            (widthPort, widthField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Width", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateWidth(width));
            (heightPort, heightField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Height", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateHeight(height));
            (pointsXPort, pointsXField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points X", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdatePointsX(pointsX));
            (pointsYPort, pointsYField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points Y", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdatePointsY(pointsY));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);
            
            
            widthField.Min = 0.01f;
            heightField.Min = 0.01f;
            pointsXField.Min = 1;
            pointsYField.Min = 1;

            widthField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - width) < Constants.FLOAT_TOLERANCE) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change grid width");
                width = evt.newValue;
                RuntimeNode.UpdateWidth(width);
            });
            
            heightField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - height) < Constants.FLOAT_TOLERANCE) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change grid height");
                height = evt.newValue;
                RuntimeNode.UpdateHeight(height);
            });
            
            pointsXField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == pointsX) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change grid X points");
                pointsX = evt.newValue;
                RuntimeNode.UpdatePointsX(pointsX);
            });
            
            pointsYField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == pointsY) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change grid Y points");
                pointsY = evt.newValue;
                RuntimeNode.UpdatePointsY(pointsY);
            });
            
            widthField.SetValueWithoutNotify(width);
            heightField.SetValueWithoutNotify(height);
            pointsXField.SetValueWithoutNotify(pointsX);
            pointsYField.SetValueWithoutNotify(pointsY);
            
            widthPort.Add(widthField);
            heightPort.Add(heightField);
            pointsXPort.Add(pointsXField);
            pointsYPort.Add(pointsYField);
            
            AddPort(widthPort);
            AddPort(heightPort);
            AddPort(pointsXPort);
            AddPort(pointsYPort);
            AddPort(resultPort);
        }

        public override void BindPorts() {
            BindPort(widthPort, RuntimeNode.WidthPort);
            BindPort(heightPort, RuntimeNode.HeightPort);
            BindPort(pointsXPort, RuntimeNode.PointsXPort);
            BindPort(pointsYPort, RuntimeNode.PointsYPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }
        
        public override JObject GetNodeData() {
            JObject root = base.GetNodeData();
            root["d"] = new JArray {
                width,
                height,
                pointsX,
                pointsY,
            };
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            JArray array = jsonData["d"] as JArray;
            width = array.Value<float>(0);
            height = array.Value<float>(1);
            pointsX = array.Value<int>(2);
            pointsY = array.Value<int>(3);
            
            widthField.SetValueWithoutNotify(width);
            heightField.SetValueWithoutNotify(height);
            pointsXField.SetValueWithoutNotify(pointsX);
            pointsYField.SetValueWithoutNotify(pointsY);
            
            RuntimeNode.UpdateWidth(width);
            RuntimeNode.UpdateHeight(height);
            RuntimeNode.UpdatePointsX(pointsX);
            RuntimeNode.UpdatePointsY(pointsY);

            base.SetNodeData(jsonData);
        }
    }
}