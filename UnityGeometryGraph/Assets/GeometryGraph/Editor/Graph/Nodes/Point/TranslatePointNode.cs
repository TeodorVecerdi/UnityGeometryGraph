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
using Mode = GeometryGraph.Runtime.Graph.TranslatePointNode.TranslatePointNode_Mode;

namespace GeometryGraph.Editor {
    [Title("Point", "Translate Points")]
    public class TranslatePointNode : AbstractNode<GeometryGraph.Runtime.Graph.TranslatePointNode> {
        protected override string Title => "Translate Points";
        protected override NodeCategory Category => NodeCategory.Point;

        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort translationPort;
        private GraphFrameworkPort attributePort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionDropdown<Mode> modeDropdown;
        private Vector3Field translationField;
        private TextField attributeNameField;

        private float3 translation;
        private string attributeName;
        private Mode mode;

        private static readonly SelectionTree tree = new(new List<object>(Enum.GetValues(typeof(Mode)).Convert(o => o))) {
            new SelectionCategory("Type", false, SelectionCategory.CategorySize.Normal) {
                new("Translate every point using a vector", 0, false),
                new("Translate each point using an attribute", 1, false)
            }
        };

        protected override void CreateNode() {
            inputPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            (translationPort, translationField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Translation", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateTranslation(translation));
            (attributePort, attributeNameField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Attribute", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateAttributeName(attributeName));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            modeDropdown = new EnumSelectionDropdown<Mode>(mode, tree);
            modeDropdown.RegisterCallback<ChangeEvent<Mode>>(evt => {
                if (evt.newValue == mode) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change translation type");
                mode = evt.newValue;
                RuntimeNode.UpdateMode(mode);
                OnModeChanged();
            });
            
            translationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change translation");
                translation = evt.newValue;
                RuntimeNode.UpdateTranslation(translation);
            });
            
            attributeNameField.RegisterValueChangedCallback(evt => {
                if (string.Equals(evt.newValue, attributeName, StringComparison.InvariantCulture)) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change attribute name");
                attributeName = evt.newValue;
                RuntimeNode.UpdateAttributeName(attributeName);
            });

            attributePort.Add(attributeNameField);
            
            inputContainer.Add(modeDropdown);
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

        protected override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(translationPort, RuntimeNode.TranslationPort);
            BindPort(attributePort, RuntimeNode.AttributeNamePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();

            root["t"] = JsonConvert.SerializeObject(translation, Formatting.None, float3Converter.Converter);
            root["a"] = attributeName;
            root["m"] = (int)mode;
            
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            translation = JsonConvert.DeserializeObject<float3>(data.Value<string>("t"), float3Converter.Converter);
            attributeName = data.Value<string>("a");
            mode = (Mode) data.Value<int>("m");
            
            translationField.SetValueWithoutNotify(translation);
            attributeNameField.SetValueWithoutNotify(attributeName);
            modeDropdown.SetValueWithoutNotify(mode);
            
            RuntimeNode.UpdateTranslation(translation);
            RuntimeNode.UpdateAttributeName(attributeName);
            RuntimeNode.UpdateMode(mode);

            OnModeChanged();

            base.Deserialize(data);
        }
    }
}