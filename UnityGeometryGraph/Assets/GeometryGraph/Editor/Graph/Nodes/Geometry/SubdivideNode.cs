using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityCommons;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Subdivide (Simple)")]
    public class SubdivideNode : AbstractNode<GeometryGraph.Runtime.Graph.SubdivideNode> {
        protected override string Title => "Subdivide (Simple)";
        protected override NodeCategory Category => NodeCategory.Geometry;

        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort levelsPort;
        private GraphFrameworkPort resultPort;

        private ClampedIntegerField levelsField;

        private int levels = 1;

        public override void CreateNode() {
            inputPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            (levelsPort, levelsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Levels", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateLevels(levels));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            levelsField.Min = 0;
            levelsField.Max = Constants.MAX_SUBDIVISIONS;
            levelsField.RegisterValueChangedCallback(evt => {
                int newValue = evt.newValue.Clamped(0, Constants.MAX_SUBDIVISIONS);
                if (newValue == levels) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change subdivision levels");
                levels = newValue;
                RuntimeNode.UpdateLevels(levels);
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
            JObject root = base.GetNodeData();

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