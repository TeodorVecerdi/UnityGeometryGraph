using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Input", "Random Integer")]
    public class RandomIntegerNode : AbstractNode<GeometryGraph.Runtime.Graph.RandomIntegerNode> {
        protected override string Title => "Random Integer";
        protected override NodeCategory Category => NodeCategory.Input;

        private int seed;
        private int min = 0;
        private int max = 100;
        
        private IntegerField seedField;
        private IntegerField minField;
        private IntegerField maxField;
        
        private GraphFrameworkPort seedPort;
        private GraphFrameworkPort minPort;
        private GraphFrameworkPort maxPort;
        private GraphFrameworkPort valuePort;

        protected override void CreateNode() {
            valuePort = GraphFrameworkPort.Create("Value", Direction.Output, Port.Capacity.Multi, PortType.Integer, this);
            (seedPort, seedField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Seed", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateSeed(seed));
            (minPort, minField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Min", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateMin(min));
            (maxPort, maxField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Max", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateMax(max));

            seedField.RegisterValueChangedCallback(evt => {
                if (seed == evt.newValue) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change seed value");
                seed = evt.newValue;
                RuntimeNode.UpdateSeed(seed);
            });
            
            minField.RegisterValueChangedCallback(evt => {
                if (min == evt.newValue) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change min value");
                min = evt.newValue;
                RuntimeNode.UpdateMin(min);
            });
            
            maxField.RegisterValueChangedCallback(evt => {
                if (max == evt.newValue) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change max value");
                max = evt.newValue;
                RuntimeNode.UpdateMax(max);
            });
            
            minField.SetValueWithoutNotify(0);
            maxField.SetValueWithoutNotify(100);
            
            seedPort.Add(seedField);
            minPort.Add(minField);
            maxPort.Add(maxField);
            
            AddPort(seedPort);
            AddPort(minPort);
            AddPort(maxPort);
            AddPort(valuePort);
            
            Refresh();
        }
        
        protected override void BindPorts() {
            BindPort(seedPort, RuntimeNode.SeedPort);
            BindPort(minPort, RuntimeNode.MinPort);
            BindPort(maxPort, RuntimeNode.MaxPort);
            BindPort(valuePort, RuntimeNode.ValuePort);
        }

        protected internal override JObject Serialize() {
            JObject root =  base.Serialize();
            root["d"] = new JArray {
                seed,
                min,
                max
            };
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;
            seed = array.Value<int>(0);
            min = array.Value<int>(1);
            max = array.Value<int>(2);
            
            seedField.SetValueWithoutNotify(seed);
            minField.SetValueWithoutNotify(min);
            maxField.SetValueWithoutNotify(max);
            
            RuntimeNode.UpdateSeed(seed);
            RuntimeNode.UpdateMin(min);
            RuntimeNode.UpdateMax(max);
            
            base.Deserialize(data);
        }
    }
}