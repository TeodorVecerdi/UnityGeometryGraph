using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Input", "Random Float")]
    public class RandomFloatNode : AbstractNode<GeometryGraph.Runtime.Graph.RandomFloatNode> {
        private int seed;
        private float min = 0.0f;
        private float max = 1.0f;
        
        private IntegerField seedField;
        private FloatField minField;
        private FloatField maxField;
        
        private GraphFrameworkPort seedPort;
        private GraphFrameworkPort minPort;
        private GraphFrameworkPort maxPort;
        private GraphFrameworkPort valuePort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Random Float");

            valuePort = GraphFrameworkPort.Create("Value", Direction.Output, Port.Capacity.Multi, PortType.Float, this);
            (seedPort, seedField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Seed", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateSeed(seed));
            (minPort, minField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Min", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateMin(min));
            (maxPort, maxField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Max", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateMax(max));

            seedField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed seed");
                seed = evt.newValue;
                RuntimeNode.UpdateSeed(seed);
            });
            AddPort(seedPort);
            seedPort.Add(seedField);
            AddPort(valuePort);
            
            Refresh();
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