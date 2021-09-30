using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.TranslatePointNode.TranslatePointNode_Which;
using Mode = GeometryGraph.Runtime.Graph.TranslatePointNode.TranslatePointNode_Mode;

namespace GeometryGraph.Editor {
    [Title("Point", "Translate Points")]
    public class TranslatePointNode : AbstractNode<GeometryGraph.Runtime.Graph.TranslatePointNode> {
        
        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort translationPort;
        private GraphFrameworkPort attributePort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionButton<Mode> modeButton;
        private Vector3Field translationField;
        private TextField attributeNameField;

        private float3 translation;
        private string attributeName;
        private Mode mode;

        private static readonly SelectionTree tree = new SelectionTree(new List<object>(Enum.GetValues(typeof(Mode)).Convert(o => o))) {
            new SelectionCategory("Type", false, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("Translate every point using a vector", 0, false),
                new SelectionEntry("Translate each point using an attribute", 1, false)
            }
        };

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Translate Points", EditorView.DefaultNodePosition);

            inputPort = GraphFrameworkPort.Create("Geometry", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Geometry, edgeConnectorListener, this);
            (translationPort, translationField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Translation", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateValue(translation, Which.Translation));
            (attributePort, attributeNameField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Attribute", Orientation.Horizontal, PortType.String, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(attributeName, Which.AttributeName));
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            modeButton = new EnumSelectionButton<Mode>(mode, tree);
            modeButton.RegisterCallback<ChangeEvent<Mode>>(evt => {
                if (evt.newValue == mode) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change translation type");
                mode = evt.newValue;
                RuntimeNode.UpdateMode(mode);
                OnModeChanged();
            });
            
            translationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change translation");
                translation = evt.newValue;
                RuntimeNode.UpdateValue(translation, Which.Translation);
            });
            
            attributeNameField.RegisterValueChangedCallback(evt => {
                if (string.Equals(evt.newValue, attributeName, StringComparison.InvariantCulture)) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change attribute name");
                attributeName = evt.newValue;
                RuntimeNode.UpdateValue(attributeName, Which.AttributeName);
            });

            attributePort.Add(attributeNameField);
            
            inputContainer.Add(modeButton);
            AddPort(inputPort);
            AddPort(translationPort);
            inputContainer.Add(translationField);
            AddPort(attributePort);
            AddPort(resultPort);

            OnModeChanged();
            
            Refresh();
        }

        private void OnModeChanged() {
            if (mode == Mode.Attribute) {
                attributePort.Show();
                translationPort.HideAndDisconnect();
            } else {
                attributePort.HideAndDisconnect();
                translationPort.Show();
            }
        }
        
        public override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(translationPort, RuntimeNode.TranslationPort);
            BindPort(attributePort, RuntimeNode.AttributePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["t"] = JsonConvert.SerializeObject(translation, Formatting.None, float3Converter.Converter);
            root["a"] = attributeName;
            root["m"] = (int)mode;
            
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            translation = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("t"), float3Converter.Converter);
            attributeName = jsonData.Value<string>("a");
            mode = (Mode) jsonData.Value<int>("m");
            
            translationField.SetValueWithoutNotify(translation);
            attributeNameField.SetValueWithoutNotify(attributeName);
            modeButton.SetValueWithoutNotify(mode);
            
            RuntimeNode.UpdateValue(translation, Which.Translation);
            RuntimeNode.UpdateValue(attributeName, Which.AttributeName);
            RuntimeNode.UpdateMode(mode);

            OnModeChanged();

            base.SetNodeData(jsonData);
        }
    }
}