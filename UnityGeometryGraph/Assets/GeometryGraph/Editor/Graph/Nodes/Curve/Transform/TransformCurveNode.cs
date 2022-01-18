using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using IsClosedMode = GeometryGraph.Runtime.Graph.TransformCurveNode.TransformCurveNode_IsClosedMode;

namespace GeometryGraph.Editor {
    [Title("Curve", "Transform Curve")]
    public class TransformCurveNode : AbstractNode<GeometryGraph.Runtime.Graph.TransformCurveNode> {
        protected override string Title => "Transform Curve";
        protected override NodeCategory Category => NodeCategory.Curve;

        private float3 translation;
        private float3 rotation;
        private float3 scale = float3_ext.one;
        private IsClosedMode closedMode = IsClosedMode.Unchanged;
        private bool isClosed;

        private Vector3Field translationField;
        private Vector3Field rotationField;
        private Vector3Field scaleField;
        private Toggle isClosedToggle;
        private EnumSelectionDropdown<IsClosedMode> closedModeDropdown;

        private GraphFrameworkPort inputCurvePort;
        private GraphFrameworkPort translationPort;
        private GraphFrameworkPort rotationPort;
        private GraphFrameworkPort scalePort;
        private GraphFrameworkPort isClosedPort;
        private GraphFrameworkPort resultCurvePort;

        private static readonly SelectionTree closedModeTree = new(new List<object>(Enum.GetValues(typeof(IsClosedMode)).Convert(o => o))) {
            new SelectionCategory("Closed Mode", false, SelectionCategory.CategorySize.Medium) {
                new("Leave the IsClosed unchanged", 0, false),
                new("Result curve will be closed", 1, false),
                new("Result curve will be open", 2, false),
                new("Use the `Is Closed` port to determine whether the curve will be closed.", 3, false)
            }
        };

        protected override void CreateNode() {
            inputCurvePort = GraphFrameworkPort.Create("Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);

            (translationPort, translationField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "Translation", PortType.Vector, this, showLabelOnField: false,
                onDisconnect: (_, _) => RuntimeNode.UpdateTranslation(translation)
            );
            (rotationPort, rotationField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "Rotation", PortType.Vector, this, showLabelOnField: false,
                onDisconnect: (_, _) => RuntimeNode.UpdateRotation(rotation)
            );
            (scalePort, scaleField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "Scale", PortType.Vector, this, showLabelOnField: false,
                onDisconnect: (_, _) => RuntimeNode.UpdateScale(scale)
            );
            (isClosedPort, isClosedToggle) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>(
                "Is Closed", PortType.Boolean, this,
                onDisconnect: (_, _) => RuntimeNode.UpdateIsClosed(isClosed)
            );

            resultCurvePort = GraphFrameworkPort.Create("Curve", Direction.Output, Port.Capacity.Multi, PortType.Curve, this);

            translationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change curve translation");
                translation = evt.newValue;
                RuntimeNode.UpdateTranslation(translation);
            });
            rotationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change curve rotation");
                rotation = math_ext.wrap(evt.newValue, -180.0f, 180.0f);
                RuntimeNode.UpdateRotation(rotation);
            });
            scaleField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change curve scale");
                scale = evt.newValue;
                RuntimeNode.UpdateScale(scale);
            });
            isClosedToggle.RegisterValueChangedCallback(evt => {
                if (isClosed == evt.newValue) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Toggle isClosed");
                isClosed = evt.newValue;
                RuntimeNode.UpdateIsClosed(isClosed);
            });

            closedModeDropdown = new EnumSelectionDropdown<IsClosedMode>(closedMode, closedModeTree, "Closed Mode");
            closedModeDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == closedMode) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change closed mode");
                closedMode = evt.newValue;
                RuntimeNode.UpdateClosedMode(closedMode);
                OnClosedModeChanged();
            });

            isClosedPort.Add(isClosedToggle);

            inputContainer.Add(closedModeDropdown);
            AddPort(isClosedPort);
            AddPort(inputCurvePort);
            AddPort(translationPort);
            inputContainer.Add(translationField);
            AddPort(rotationPort);
            inputContainer.Add(rotationField);
            AddPort(scalePort);
            inputContainer.Add(scaleField);
            AddPort(resultCurvePort);

            OnClosedModeChanged();
        }

        private void OnClosedModeChanged() {
            if (closedMode == IsClosedMode.Variable) {
                isClosedPort.Show();
            } else {
                isClosedPort.HideAndDisconnect();
            }
        }

        protected override void BindPorts() {
            BindPort(inputCurvePort, RuntimeNode.InputPort);
            BindPort(translationPort, RuntimeNode.TranslationPort);
            BindPort(rotationPort, RuntimeNode.RotationPort);
            BindPort(scalePort, RuntimeNode.ScalePort);
            BindPort(isClosedPort, RuntimeNode.IsClosedPort);
            BindPort(resultCurvePort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray array = new() {
                JsonConvert.SerializeObject(translation, float3Converter.Converter),
                JsonConvert.SerializeObject(rotation, float3Converter.Converter),
                JsonConvert.SerializeObject(scale, float3Converter.Converter),
                isClosed ? 1 : 0,
                (int)closedMode
            };
            root["d"] = array;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;

            translation = JsonConvert.DeserializeObject<float3>(array!.Value<string>(0), float3Converter.Converter);
            rotation = JsonConvert.DeserializeObject<float3>(array.Value<string>(1), float3Converter.Converter);
            scale = JsonConvert.DeserializeObject<float3>(array.Value<string>(2), float3Converter.Converter);
            isClosed = array.Value<int>(3) == 1;
            closedMode = (IsClosedMode) array.Value<int>(4);

            translationField.SetValueWithoutNotify(translation);
            rotationField.SetValueWithoutNotify(rotation);
            scaleField.SetValueWithoutNotify(scale);
            isClosedToggle.SetValueWithoutNotify(isClosed);
            closedModeDropdown.SetValueWithoutNotify(closedMode);

            RuntimeNode.UpdateTranslation(translation);
            RuntimeNode.UpdateRotation(rotation);
            RuntimeNode.UpdateScale(scale);
            RuntimeNode.UpdateIsClosed(isClosed);
            RuntimeNode.UpdateClosedMode(closedMode);

            OnClosedModeChanged();

            base.Deserialize(data);
        }
    }
}