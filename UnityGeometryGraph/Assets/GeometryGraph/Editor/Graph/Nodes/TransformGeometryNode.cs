using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using WhichDefaultValue = GeometryGraph.Runtime.Graph.TransformGeometryNode.WhichDefaultValue;

namespace GeometryGraph.Editor {
    [Title("Transform Geometry")]
    public class TransformGeometryNode : AbstractNode<GeometryGraph.Runtime.Graph.TransformGeometryNode> {
        private float3 defaultTranslation;
        private float3 defaultEulerRotation;
        private float3 defaultScale = float3_util.one;

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
            Initialize("Transform Geometry", EditorView.DefaultNodePosition);

            inputGeometryPort = GraphFrameworkPort.Create("Geometry", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Geometry, edgeConnectorListener);
            outputGeometryPort = GraphFrameworkPort.Create("Geometry", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener);

            (translationPort, translationField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "Translation", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, showLabelOnField: false
            );
            translationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Translation value");
                defaultTranslation = evt.newValue;
                RuntimeNode.UpdateDefaultValue(defaultTranslation, WhichDefaultValue.Translation);
            });
            (rotationPort, eulerRotationField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "Rotation", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, showLabelOnField: false
            );
            eulerRotationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Rotation value");
                defaultEulerRotation = MathUtilities.WrapPI(evt.newValue);
                RuntimeNode.UpdateDefaultValue(defaultEulerRotation, WhichDefaultValue.Rotation);
            });
            
            (scalePort, scaleField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "Scale", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, showLabelOnField: false
            );
            scaleField.SetValueWithoutNotify(float3_util.one);
            scaleField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Scale value");
                defaultScale = evt.newValue;
                RuntimeNode.UpdateDefaultValue(defaultScale, WhichDefaultValue.Scale);
            });
            
            AddPort(inputGeometryPort);
            AddPort(translationPort);
            inputContainer.Add(translationField);
            AddPort(rotationPort);
            inputContainer.Add(eulerRotationField);
            AddPort(scalePort);
            inputContainer.Add(scaleField);
            
            AddPort(outputGeometryPort);
            
            RefreshExpandedState();
        }
        
        public override void BindPorts() {
            BindPort(inputGeometryPort, RuntimeNode.InputGeometryPort);
            BindPort(translationPort, RuntimeNode.TranslationPort);
            BindPort(rotationPort, RuntimeNode.RotationPort);
            BindPort(scalePort, RuntimeNode.ScalePort);
            BindPort(outputGeometryPort, RuntimeNode.OutputGeometryPort);
        }

        protected internal override void OnPortValueChanged(Edge edge, GraphFrameworkPort port) {
            /*// TODO: change this to account for multiple values
            if (port == aPort) {
                a = (GeometryData)GetValueFromEdge(edge, a).Clone();
                RuntimeNode.NotifyPortValueChanged(RuntimePortDictionary[aPort]);
            }
            */

            // UpdateResult();
        }

        /*private void UpdateResult() {
            CalculateResult();
            NotifyPortValueChanged(resultPort);
        }*/

        public override object GetValueForPort(GraphFrameworkPort port) {
            return GeometryData.Empty;
            /*if (port != resultPort) return null;

            CalculateResult();
            return result;*/
        }

        /*private void CalculateResult() {
        }*/

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["t"] = JsonConvert.SerializeObject(defaultTranslation, Formatting.None);
            root["r"] = JsonConvert.SerializeObject(defaultEulerRotation, Formatting.None);
            root["s"] = JsonConvert.SerializeObject(defaultScale, Formatting.None);
            
            return root;
        }

        public override void SetNodeData(JObject jsonData) {

            defaultTranslation = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("t")!);
            defaultEulerRotation = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("r")!);
            defaultScale = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("s")!);
            
            translationField.SetValueWithoutNotify(defaultTranslation);
            eulerRotationField.SetValueWithoutNotify(defaultEulerRotation);
            scaleField.SetValueWithoutNotify(defaultScale);
            
            RuntimeNode.UpdateDefaultValue(defaultTranslation, WhichDefaultValue.Translation);
            RuntimeNode.UpdateDefaultValue(defaultEulerRotation, WhichDefaultValue.Rotation);
            RuntimeNode.UpdateDefaultValue(defaultScale, WhichDefaultValue.Scale);
            
            base.SetNodeData(jsonData); 
        }
    }
}