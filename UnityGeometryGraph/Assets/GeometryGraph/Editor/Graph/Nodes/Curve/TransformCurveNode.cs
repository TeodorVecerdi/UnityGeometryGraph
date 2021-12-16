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

namespace GeometryGraph.Editor {
    [Title("Curve", "Transform Curve")]
    public class TransformCurveNode : AbstractNode<GeometryGraph.Runtime.Graph.TransformCurveNode> {
        protected override string Title => "Transform Curve";
        protected override NodeCategory Category => NodeCategory.Curve;

        private float3 translation;
        private float3 rotation;
        private float3 scale = float3_ext.one;
        private bool isClosed;
        private bool changeClosed;
        
        private Vector3Field translationField;
        private Vector3Field rotationField;
        private Vector3Field scaleField;
        private Toggle isClosedToggle;
        private Toggle changeClosedToggle;
        
        private GraphFrameworkPort inputCurvePort;
        private GraphFrameworkPort translationPort;
        private GraphFrameworkPort rotationPort;
        private GraphFrameworkPort scalePort;
        private GraphFrameworkPort isClosedPort;
        private GraphFrameworkPort resultCurvePort;

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
            
            changeClosedToggle = new Toggle("Change IsClosed");
            changeClosedToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == changeClosed) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Toggle change closed");
                isClosedToggle.SetEnabled(evt.newValue);
                changeClosed = evt.newValue;
                RuntimeNode.UpdateChangeClosed(changeClosed);
            });
            isClosedToggle.SetEnabled(false);
            
            inputContainer.Add(changeClosedToggle);
            isClosedPort.Add(isClosedToggle);
            AddPort(translationPort);
            inputContainer.Add(translationField);
            AddPort(rotationPort);
            inputContainer.Add(rotationField);
            AddPort(scalePort);
            inputContainer.Add(scaleField);
            AddPort(isClosedPort);
            
            AddPort(resultCurvePort);
        }

        protected override void BindPorts() {
            BindPort(inputCurvePort, RuntimeNode.InputPort);
            BindPort(translationPort, RuntimeNode.TranslationPort);
            BindPort(rotationPort, RuntimeNode.RotationPort);
            BindPort(scalePort, RuntimeNode.ScalePort);
            BindPort(isClosedPort, RuntimeNode.IsClosedPort);
            BindPort(resultCurvePort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            JObject root = base.GetNodeData();
            JArray array = new JArray {
                JsonConvert.SerializeObject(translation, float3Converter.Converter),
                JsonConvert.SerializeObject(rotation, float3Converter.Converter),
                JsonConvert.SerializeObject(scale, float3Converter.Converter),
                isClosed ? 1 : 0,
                changeClosed ? 1 : 0
            };
            root["d"] = array;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            JArray array = jsonData["d"] as JArray;
            
            translation = JsonConvert.DeserializeObject<float3>(array!.Value<string>(0), float3Converter.Converter);
            rotation = JsonConvert.DeserializeObject<float3>(array!.Value<string>(1), float3Converter.Converter);
            scale = JsonConvert.DeserializeObject<float3>(array!.Value<string>(2), float3Converter.Converter);
            isClosed = array!.Value<int>(3) == 1;
            changeClosed = array!.Value<int>(4) == 1;
            
            translationField.SetValueWithoutNotify(translation);
            rotationField.SetValueWithoutNotify(rotation);
            scaleField.SetValueWithoutNotify(scale);
            isClosedToggle.SetValueWithoutNotify(isClosed);
            changeClosedToggle.SetValueWithoutNotify(changeClosed);
            isClosedToggle.SetEnabled(changeClosed);
            
            RuntimeNode.UpdateTranslation(translation);
            RuntimeNode.UpdateRotation(rotation);
            RuntimeNode.UpdateScale(scale);
            RuntimeNode.UpdateIsClosed(isClosed);
            RuntimeNode.UpdateChangeClosed(changeClosed);
           
            base.SetNodeData(jsonData);
        }
    }
}