using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Subdivide (Simple)")]
    public class SubdivideNode : AbstractNode<GeometryGraph.Runtime.Graph.SubdivideNode> {
        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort levelsPort;
        private GraphFrameworkPort resultPort;

        private IntegerField levelsField;

        private int levels = 1;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Subdivide (Simple)", EditorView.DefaultNodePosition);

            inputPort = GraphFrameworkPort.Create("Geometry", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Geometry, edgeConnectorListener, this);
            (levelsPort, levelsField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Levels", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateLevels(levels));
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            levelsField.RegisterValueChangedCallback(evt => {
                var newValue = evt.newValue;
                if (newValue < 0) newValue = 0;
               
                if (newValue == levels) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change subdivision levels");
                levels = newValue;
            });

            levelsPort.Add(levelsField);
            
            AddPort(inputPort);
            AddPort(levelsPort);
            AddPort(resultPort);
            
            levelsField.SetValueWithoutNotify(levels);
            
            Refresh();
        }

        public override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(levelsPort, RuntimeNode.LevelsPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["l"] = levels;
            
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            levels = jsonData.Value<int>("l");
            
            levelsField.SetValueWithoutNotify(levels);
            
            RuntimeNode.UpdateLevels(levels);

            base.SetNodeData(jsonData);
        }
    }
}