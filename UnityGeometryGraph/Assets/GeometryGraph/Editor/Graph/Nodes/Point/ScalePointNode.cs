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
using Which = GeometryGraph.Runtime.Graph.ScalePointNode.ScalePointNode_Which;
using Mode = GeometryGraph.Runtime.Graph.ScalePointNode.ScalePointNode_Mode;

namespace GeometryGraph.Editor {
    [Title("Point", "Scale Points")]
    public class ScalePointNode : AbstractNode<GeometryGraph.Runtime.Graph.ScalePointNode> {
        
        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort vectorPort;
        private GraphFrameworkPort scalarPort;
        private GraphFrameworkPort attributePort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionButton<Mode> modeButton;
        private Vector3Field vectorField;
        private FloatField scalarField;
        private TextField attributeNameField;

        private float3 vector;
        private float scalar;
        private string attributeName;
        private Mode mode;

        private static readonly SelectionTree tree = new SelectionTree(new List<object>(Enum.GetValues(typeof(Mode)).Convert(o => o))) {
            new SelectionCategory("Type", false, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("Scale every point using a vector", 0, false),
                new SelectionEntry("Scale every point using a float value", 1, false),
                new SelectionEntry("Translate each point using an attribute", 2, false)
            }
        };

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Scale Points", EditorView.DefaultNodePosition);

            inputPort = GraphFrameworkPort.Create("Geometry", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Geometry, edgeConnectorListener, this);
            (vectorPort, vectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Factor", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateValue(vector, Which.Vector));
            (scalarPort, scalarField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Factor", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(scalar, Which.Scalar));
            (attributePort, attributeNameField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Attribute", Orientation.Horizontal, PortType.String, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(attributeName, Which.AttributeName));
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            modeButton = new EnumSelectionButton<Mode>(mode, tree);
            modeButton.RegisterCallback<ChangeEvent<Mode>>(evt => {
                if (evt.newValue == mode) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change scale type");
                mode = evt.newValue;
                RuntimeNode.UpdateMode(mode);
                OnModeChanged();
            });
            
            vectorField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vector scale");
                vector = evt.newValue;
                RuntimeNode.UpdateValue(vector, Which.Vector);
            });
            
            scalarField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change scalar scale");
                scalar = evt.newValue;
                RuntimeNode.UpdateValue(vector, Which.Scalar);
            });
            
            attributeNameField.RegisterValueChangedCallback(evt => {
                if (string.Equals(evt.newValue, attributeName, StringComparison.InvariantCulture)) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change attribute name");
                attributeName = evt.newValue;
                RuntimeNode.UpdateValue(attributeName, Which.AttributeName);
            });

            scalarPort.Add(scalarField);
            attributePort.Add(attributeNameField);

            inputContainer.Add(modeButton);
            AddPort(inputPort);
            AddPort(vectorPort);
            inputContainer.Add(vectorField);
            AddPort(scalarPort);
            AddPort(attributePort);
            AddPort(resultPort);

            OnModeChanged();
            
            Refresh();
        }

        private void OnModeChanged() {
            switch (mode) {
                case Mode.Vector:
                    vectorPort.Show();
                    scalarPort.HideAndDisconnect();
                    attributePort.HideAndDisconnect();
                    break;
                case Mode.Float:
                    scalarPort.Show();
                    vectorPort.HideAndDisconnect();
                    attributePort.HideAndDisconnect();
                    break;
                case Mode.Attribute:
                    attributePort.Show();
                    scalarPort.HideAndDisconnect();
                    vectorPort.HideAndDisconnect();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
        
        public override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(vectorPort, RuntimeNode.VectorPort);
            BindPort(scalarPort, RuntimeNode.ScalarPort);
            BindPort(attributePort, RuntimeNode.AttributePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["v"] = JsonConvert.SerializeObject(vector, Formatting.None, float3Converter.Converter);
            root["s"] = scalar;
            root["a"] = attributeName;
            root["m"] = (int)mode;
            
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            vector = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("v"), float3Converter.Converter);
            scalar = jsonData.Value<float>("s");
            attributeName = jsonData.Value<string>("a");
            mode = (Mode) jsonData.Value<int>("m");
            
            vectorField.SetValueWithoutNotify(vector);
            scalarField.SetValueWithoutNotify(scalar);
            attributeNameField.SetValueWithoutNotify(attributeName);
            modeButton.SetValueWithoutNotify(mode);
            
            RuntimeNode.UpdateValue(vector, Which.Vector);
            RuntimeNode.UpdateValue(scalar, Which.Scalar);
            RuntimeNode.UpdateValue(attributeName, Which.AttributeName);
            RuntimeNode.UpdateMode(mode);

            OnModeChanged();

            base.SetNodeData(jsonData);
        }
    }
}