using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Input", "Random Float")]
    public class RandomFloatNode : AbstractNode<GeometryGraph.Runtime.Graph.RandomFloatNode> {
        private int seed;
        private IntegerField seedField;
        private GraphFrameworkPort seedPort;
        private GraphFrameworkPort valuePort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Random Float", EditorView.DefaultNodePosition);

            valuePort = GraphFrameworkPort.Create("Value", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Float, edgeConnectorListener, this);
            (seedPort, seedField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Seed", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this);
            seedField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed seed");
                seed = evt.newValue;
                RuntimeNode.UpdateSeed(seed);
            });
            AddPort(seedPort);
            seedPort.Add(seedField);
            AddPort(valuePort);
            
            RefreshExpandedState();
        }
        
        public override void BindPorts() {
            BindPort(seedPort, RuntimeNode.SeedPort);
            BindPort(valuePort, RuntimeNode.ValuePort);
        }

        public override JObject GetNodeData() {
            var root =  base.GetNodeData();
            root["s"] = seed;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            seed = jsonData.Value<int>("s");
            seedField.SetValueWithoutNotify(seed);
            RuntimeNode.UpdateSeed(seed);
            
            base.SetNodeData(jsonData);
        }
    }
}