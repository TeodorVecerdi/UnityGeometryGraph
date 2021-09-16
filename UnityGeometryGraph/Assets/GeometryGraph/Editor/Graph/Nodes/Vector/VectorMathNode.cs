using System;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace GeometryGraph.Editor.Vector {
    [Title("Vector", "Vector Math")]
    public class VectorMathNode : AbstractNode{
        private Vector3 a;
        private Vector3 b;
        private Vector3 result;
        
        private enum MathOperation {Add, Subtract, Scale}
        private MathOperation operation;
        
        private GraphFrameworkPort aPort;
        private GraphFrameworkPort bPort;
        private GraphFrameworkPort resultPort;

        private Vector3Field aField;
        private Vector3Field bField;
        private EnumField operationField;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Vector Math", EditorView.DefaultNodePosition);
            
            operationField = new EnumField("Operation", MathOperation.Add);
            operationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Math Operation");
                operation = (MathOperation)evt.newValue;
                NotifyPortValueChanged(resultPort);
            });
            
            extensionContainer.Add(operationField);
            
            (aPort, aField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "A", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, showLabelOnField: false, onDisconnect: (edge, port) => {
                    a = aField.value;
                    NotifyPortValueChanged(resultPort);
                });
            (bPort, bField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "B", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, showLabelOnField: false, onDisconnect: (edge, port) => {
                    b = bField.value;
                    NotifyPortValueChanged(resultPort);
                });
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Vector, edgeConnectorListener);

            aField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Vector Math input (A)");
                a = evt.newValue;
                NotifyPortValueChanged(resultPort);
            });
           
            bField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Vector Math input (B)");
                b = evt.newValue;
                NotifyPortValueChanged(resultPort);
            });
            
            AddPort(aPort);
            inputContainer.Add(aField);
            AddPort(bPort);
            inputContainer.Add(bField);
            AddPort(resultPort);
            
            RefreshExpandedState();
        }

        protected override void OnPortValueChanged(Edge edge, GraphFrameworkPort port) {
            if (port == aPort) a = GetValueFromEdge(edge, a);
            else if (port == bPort) b = GetValueFromEdge(edge, b);

            UpdateResult();
        }

        public override object GetValueForPort(GraphFrameworkPort port) {
            if (port != resultPort) return null;

            result = CalculateResult();
            
            return result;
        }

        private void UpdateResult() {
            var newResult = CalculateResult();
            if (result == newResult) return;

            result = newResult;
            NotifyPortValueChanged(resultPort);
        }

        private Vector3 CalculateResult() {
            Vector3 value;
            switch (operation) {
                case MathOperation.Add:
                    value = a + b;
                    break;
                case MathOperation.Subtract:
                    value = a - b;
                    break;
                case MathOperation.Scale:
                    value = a;
                    value.Scale(b);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return value;
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();
            root["a"] = JsonConvert.SerializeObject(a, Formatting.None);
            root["b"] = JsonConvert.SerializeObject(b, Formatting.None);
            root["af"] =JsonConvert.SerializeObject(aField.enabledSelf ? a : aField.value, Formatting.None);
            root["bf"] = JsonConvert.SerializeObject(bField.enabledSelf ? b : bField.value, Formatting.None);
            root["op"] = (int)operation;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            a = JsonConvert.DeserializeObject<Vector3>(jsonData.Value<string>("a"));
            b = JsonConvert.DeserializeObject<Vector3>(jsonData.Value<string>("b"));
            aField.SetValueWithoutNotify(JsonConvert.DeserializeObject<Vector3>(jsonData.Value<string>("af")));
            bField.SetValueWithoutNotify(JsonConvert.DeserializeObject<Vector3>(jsonData.Value<string>("bf")));
            operation = (MathOperation)jsonData.Value<int>("op");
            operationField.SetValueWithoutNotify(operation);
            
            base.SetNodeData(jsonData);
        }
    }
}