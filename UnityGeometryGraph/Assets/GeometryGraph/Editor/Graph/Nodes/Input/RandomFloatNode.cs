using System;
using GeometryGraph.Runtime;
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
                if (seed == evt.newValue) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change seed value");
                seed = evt.newValue;
                RuntimeNode.UpdateSeed(seed);
            });
            
            minField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(min - evt.newValue) < Constants.FLOAT_TOLERANCE) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change min value");
                min = evt.newValue;
                RuntimeNode.UpdateMin(min);
            });
            
            maxField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(max - evt.newValue) < Constants.FLOAT_TOLERANCE) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change max value");
                max = evt.newValue;
                RuntimeNode.UpdateMax(max);
            });
            
            minField.SetValueWithoutNotify(0.0f);
            maxField.SetValueWithoutNotify(1.0f);
            
            seedPort.Add(seedField);
            minPort.Add(minField);
            maxPort.Add(maxField);
            
            AddPort(seedPort);
            AddPort(minPort);
            AddPort(maxPort);
            AddPort(valuePort);
            
            Refresh();
        }
        
        public override void BindPorts() {
            BindPort(seedPort, RuntimeNode.SeedPort);
            BindPort(minPort, RuntimeNode.MinPort);
            BindPort(maxPort, RuntimeNode.MaxPort);
            BindPort(valuePort, RuntimeNode.ValuePort);
        }

        public override JObject GetNodeData() {
            JObject root =  base.GetNodeData();
            root["d"] = new JArray {
                seed,
                min,
                max
            };
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            JArray array = jsonData["d"] as JArray;
            seed = array.Value<int>(0);
            min = array.Value<float>(1);
            max = array.Value<float>(2);
            
            seedField.SetValueWithoutNotify(seed);
            minField.SetValueWithoutNotify(min);
            maxField.SetValueWithoutNotify(max);
            
            RuntimeNode.UpdateSeed(seed);
            RuntimeNode.UpdateMin(min);
            RuntimeNode.UpdateMax(max);
            
            base.SetNodeData(jsonData);
        }
    }
}