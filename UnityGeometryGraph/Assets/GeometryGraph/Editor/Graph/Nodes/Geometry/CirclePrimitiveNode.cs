using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.CirclePrimitiveNode.CirclePrimitiveNode_Which;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Primitive", "Circle")]
    public class CirclePrimitiveNode : AbstractNode<GeometryGraph.Runtime.Graph.CirclePrimitiveNode> {
        
        private GraphFrameworkPort radiusPort;
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort resultPort;

        private FloatField radiusField;
        private IntegerField pointsField;

        private float radius = 1.0f;
        private int points = 8;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Circle Primitive", EditorView.DefaultNodePosition);

            (radiusPort, radiusField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Radius", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this);
            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Points", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this);
           resultPort = GraphFrameworkPort.Create("Circle", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

           radiusField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                if (evt.newValue < 0.0f) {
                    radius = 0.0f;
                    radiusField.SetValueWithoutNotify(0.0f);
                } else radius = evt.newValue;
                
                RuntimeNode.UpdateValue(radius, Which.Radius);
            });
            
           pointsField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                if (evt.newValue < 3) {
                    points = 3;
                    pointsField.SetValueWithoutNotify(3);
                } else points = evt.newValue;
                RuntimeNode.UpdateValue(points, Which.Points);
            });

            radiusField.SetValueWithoutNotify(1.0f);
            pointsField.SetValueWithoutNotify(8);

            radiusPort.Add(radiusField);
            pointsPort.Add(pointsField);
            
            AddPort(radiusPort);
            AddPort(pointsPort);
            AddPort(resultPort);
            
            Refresh();
        }
        
        public override void BindPorts() {
            BindPort(radiusPort, RuntimeNode.RadiusPort);
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["r"] = radius;
            root["p"] = points;
            
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            radius = jsonData.Value<float>("r");
            points = jsonData.Value<int>("p");
            
            radiusField.SetValueWithoutNotify(radius);
            pointsField.SetValueWithoutNotify(points);
            
            RuntimeNode.UpdateValue(radius, Which.Radius);
            RuntimeNode.UpdateValue(points, Which.Points);

            base.SetNodeData(jsonData);
        }
    }
}