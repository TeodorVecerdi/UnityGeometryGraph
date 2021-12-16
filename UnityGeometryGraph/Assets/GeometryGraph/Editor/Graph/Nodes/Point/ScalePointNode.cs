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
using Mode = GeometryGraph.Runtime.Graph.ScalePointNode.ScalePointNode_Mode;

namespace GeometryGraph.Editor {
    [Title("Point", "Scale Points")]
    public class ScalePointNode : AbstractNode<GeometryGraph.Runtime.Graph.ScalePointNode> {
        protected override string Title => "Scale Points";
        protected override NodeCategory Category => NodeCategory.Point;

        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort vectorPort;
        private GraphFrameworkPort scalarPort;
        private GraphFrameworkPort attributePort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionDropdown<Mode> modeDropdown;
        private Vector3Field vectorField;
        private FloatField scalarField;
        private TextField attributeNameField;

        private float3 vector;
        private float scalar;
        private string attributeName;
        private Mode mode;

        private static readonly SelectionTree tree = new SelectionTree(new List<object>(Enum.GetValues(typeof(Mode)).Convert(o => o))) {
            new SelectionCategory("Type", false, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("Scale each point using a vector", 0, false),
                new SelectionEntry("Scale each point using a float value", 1, false),
                new SelectionEntry("Scale each point using an attribute", 2, false)
            }
        };

        protected override void CreateNode() {
            inputPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            (vectorPort, vectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Factor", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateVector(vector));
            (scalarPort, scalarField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Factor", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateScalar(scalar));
            (attributePort, attributeNameField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Attribute", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateAttributeName(attributeName));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            modeDropdown = new EnumSelectionDropdown<Mode>(mode, tree);
            modeDropdown.RegisterCallback<ChangeEvent<Mode>>(evt => {
                if (evt.newValue == mode) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change scale type");
                mode = evt.newValue;
                RuntimeNode.UpdateMode(mode);
                OnModeChanged();
            });
            
            vectorField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vector scale");
                vector = evt.newValue;
                RuntimeNode.UpdateVector(vector);
            });
            
            scalarField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change scalar scale");
                scalar = evt.newValue;
                RuntimeNode.UpdateScalar(scalar);
            });
            
            attributeNameField.RegisterValueChangedCallback(evt => {
                if (string.Equals(evt.newValue, attributeName, StringComparison.InvariantCulture)) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change attribute name");
                attributeName = evt.newValue;
                RuntimeNode.UpdateAttributeName(attributeName);
            });

            scalarPort.Add(scalarField);
            attributePort.Add(attributeNameField);

            inputContainer.Add(modeDropdown);
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
        
        protected override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(vectorPort, RuntimeNode.VectorPort);
            BindPort(scalarPort, RuntimeNode.ScalarPort);
            BindPort(attributePort, RuntimeNode.AttributeNamePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();

            root["v"] = JsonConvert.SerializeObject(vector, Formatting.None, float3Converter.Converter);
            root["s"] = scalar;
            root["a"] = attributeName;
            root["m"] = (int)mode;
            
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            vector = JsonConvert.DeserializeObject<float3>(data.Value<string>("v"), float3Converter.Converter);
            scalar = data.Value<float>("s");
            attributeName = data.Value<string>("a");
            mode = (Mode) data.Value<int>("m");
            
            vectorField.SetValueWithoutNotify(vector);
            scalarField.SetValueWithoutNotify(scalar);
            attributeNameField.SetValueWithoutNotify(attributeName);
            modeDropdown.SetValueWithoutNotify(mode);
            
            RuntimeNode.UpdateVector(vector);
            RuntimeNode.UpdateScalar(scalar);
            RuntimeNode.UpdateAttributeName(attributeName);
            RuntimeNode.UpdateMode(mode);

            OnModeChanged();

            base.Deserialize(data);
        }
    }
}