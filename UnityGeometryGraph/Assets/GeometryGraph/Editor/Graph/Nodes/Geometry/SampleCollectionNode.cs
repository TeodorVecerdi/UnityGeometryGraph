using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using SampleType = GeometryGraph.Runtime.Graph.SampleCollectionNode.SampleCollectionNode_SampleType;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Sample Collection")]
    public class SampleCollectionNode : AbstractNode<GeometryGraph.Runtime.Graph.SampleCollectionNode> {
        
        private GraphFrameworkPort collectionPort;
        private GraphFrameworkPort indexPort;
        private GraphFrameworkPort seedPort;
        private GraphFrameworkPort resultPort;

        private IntegerField indexField;
        private IntegerField seedField;
        private EnumSelectionDropdown<SampleType> sampleTypeDropdown;

        private int index;
        private int seed;
        private SampleType sampleType;

        private static readonly SelectionTree tree = new SelectionTree(new List<object>(Enum.GetValues(typeof(SampleType)).Convert(o => o))) {
            new SelectionCategory("Sample Type", false, SelectionCategory.CategorySize.Medium) {
                new SelectionEntry("Get geometry at index", 0, false),
                new SelectionEntry("Sample at random using seed", 1, false),
            }
        };

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Sample Collection");

            collectionPort = GraphFrameworkPort.Create("Collection", Direction.Input, Port.Capacity.Single, PortType.Collection, this);
            (indexPort, indexField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Index", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateIndex(index));
            (seedPort, seedField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Seed", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateSeed(seed));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            sampleTypeDropdown = new EnumSelectionDropdown<SampleType>(sampleType, tree);
            sampleTypeDropdown.RegisterCallback<ChangeEvent<SampleType>>(evt => {
                if (evt.newValue == sampleType) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change collection sample type");
                sampleType = evt.newValue;
                RuntimeNode.UpdateSampleType(sampleType);
                OnSampleTypeChanged();
            });
            
            indexField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == index) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change collection sample index");
                index = evt.newValue;
                RuntimeNode.UpdateIndex(index);
            });
            
            seedField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == seed) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change collection sample seed");
                seed = evt.newValue;
                RuntimeNode.UpdateSeed(seed);
            });

            indexPort.Add(indexField);
            seedPort.Add(seedField);
            
            inputContainer.Add(sampleTypeDropdown);
            AddPort(collectionPort);
            AddPort(indexPort);
            AddPort(seedPort);
            AddPort(resultPort);
            
            OnSampleTypeChanged();
            
            Refresh();
        }

        private void OnSampleTypeChanged() {
            if (sampleType == SampleType.AtIndex) {
                indexPort.Show();
                seedPort.HideAndDisconnect();
            } else {
                seedPort.Show();
                indexPort.HideAndDisconnect();
            }
        }

        public override void BindPorts() {
            BindPort(collectionPort, RuntimeNode.CollectionPort);
            BindPort(indexPort, RuntimeNode.IndexPort);
            BindPort(seedPort, RuntimeNode.SeedPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            JObject root = base.GetNodeData();

            root["i"] = index;
            root["s"] = seed;
            root["m"] = (int)sampleType;
            
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            index = jsonData.Value<int>("i");
            seed = jsonData.Value<int>("s");
            sampleType = (SampleType) jsonData.Value<int>("m");
            
            indexField.SetValueWithoutNotify(index);
            seedField.SetValueWithoutNotify(seed);
            sampleTypeDropdown.SetValueWithoutNotify(sampleType);
            
            RuntimeNode.UpdateIndex(index);
            RuntimeNode.UpdateSeed(seed);
            RuntimeNode.UpdateSampleType(sampleType);
            
            OnSampleTypeChanged();

            base.SetNodeData(jsonData);
        }
    }
}