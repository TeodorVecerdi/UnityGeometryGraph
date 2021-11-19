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
    [Title("Geometry", "Transform Geometry")]
    public class TransformGeometryNode : AbstractNode<GeometryGraph.Runtime.Graph.TransformGeometryNode> {
        private float3 translation;
        private float3 eulerRotation;
        private float3 scale = float3_ext.one;

        private Vector3Field translationField;
        private Vector3Field eulerRotationField;
        private Vector3Field scaleField;
        
        private GraphFrameworkPort inputGeometryPort;
        private GraphFrameworkPort translationPort;
        private GraphFrameworkPort rotationPort;
        private GraphFrameworkPort scalePort;
        
        private GraphFrameworkPort outputGeometryPort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Transform Geometry");

            inputGeometryPort = GraphFrameworkPort.Create("Geometry", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Geometry, edgeConnectorListener, this);
            outputGeometryPort = GraphFrameworkPort.Create("Geometry", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            (translationPort, translationField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "Translation", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, 
                onDisconnect: (_, _) => RuntimeNode.UpdateTranslation(translation)
            );
            translationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Translation value");
                translation = evt.newValue;
                RuntimeNode.UpdateTranslation(translation);
            });
            (rotationPort, eulerRotationField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "Rotation", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, 
                onDisconnect: (_, _) => RuntimeNode.UpdateRotation(eulerRotation)
            );
            eulerRotationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Rotation value");
                eulerRotation = math_ext.wrap(evt.newValue, -180.0f, 180.0f);
                RuntimeNode.UpdateRotation(eulerRotation);
            });
            
            (scalePort, scaleField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "Scale", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, 
                onDisconnect: (_, _) => RuntimeNode.UpdateScale(scale)
            );
            scaleField.SetValueWithoutNotify(float3_ext.one);
            scaleField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Scale value");
                scale = evt.newValue;
                RuntimeNode.UpdateScale(scale);
            });
            
            AddPort(inputGeometryPort);
            AddPort(translationPort);
            inputContainer.Add(translationField);
            AddPort(rotationPort);
            inputContainer.Add(eulerRotationField);
            AddPort(scalePort);
            inputContainer.Add(scaleField);
            
            AddPort(outputGeometryPort);
            
            Refresh();
        }
        
        public override void BindPorts() {
            BindPort(inputGeometryPort, RuntimeNode.InputPort);
            BindPort(translationPort, RuntimeNode.TranslationPort);
            BindPort(rotationPort, RuntimeNode.RotationPort);
            BindPort(scalePort, RuntimeNode.ScalePort);
            BindPort(outputGeometryPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["t"] = JsonConvert.SerializeObject(translation, Formatting.None, float3Converter.Converter);
            root["r"] = JsonConvert.SerializeObject(eulerRotation, Formatting.None, float3Converter.Converter);
            root["s"] = JsonConvert.SerializeObject(scale, Formatting.None, float3Converter.Converter);
            
            return root;
        }

        public override void SetNodeData(JObject jsonData) {

            translation = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("t")!, float3Converter.Converter);
            eulerRotation = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("r")!, float3Converter.Converter);
            scale = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("s")!, float3Converter.Converter);
            
            translationField.SetValueWithoutNotify(translation);
            eulerRotationField.SetValueWithoutNotify(eulerRotation);
            scaleField.SetValueWithoutNotify(scale);
            
            RuntimeNode.UpdateTranslation(translation);
            RuntimeNode.UpdateRotation(eulerRotation);
            RuntimeNode.UpdateScale(scale);
            
            base.SetNodeData(jsonData); 
        }
    }
}