using System;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Float", "Math")]
    public class FloatMathNode : AbstractNode {
        private float a;
        private float b;
        private float result;

        private enum MathOperation {Add, Subtract, Multiply, Divide}
        private MathOperation operation;
        
        private GraphFrameworkPort aPort;
        private GraphFrameworkPort bPort;
        private GraphFrameworkPort resultPort;

        private FloatField aField;
        private FloatField bField;
        private EnumField operationField;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Math", EditorView.DefaultNodePosition);

            operationField = new EnumField("Operation", MathOperation.Add);
            operationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Math Operation");
                operation = (MathOperation)evt.newValue;
                NotifyPortValueChanged(resultPort);
            });
            
            extensionContainer.Add(operationField);
            

            (aPort, aField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>(
                "A", Orientation.Horizontal, PortType.Float, edgeConnectorListener, onDisconnect: (edge, port) => {
                    a = aField.value;
                    NotifyPortValueChanged(resultPort);
                });
            (bPort, bField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>(
                "B", Orientation.Horizontal, PortType.Float, edgeConnectorListener, onDisconnect: (edge, port) => {
                    b = bField.value;
                    NotifyPortValueChanged(resultPort);
                });
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Float, edgeConnectorListener);
        
            aPort.Add(aField);
            bPort.Add(bField);
            
            aField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Math input (A)");
                a = evt.newValue;
                NotifyPortValueChanged(resultPort);
            });
           
            bField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Math input (B)");
                b = evt.newValue;
                NotifyPortValueChanged(resultPort);
            });

            AddPort(aPort);
            AddPort(bPort);
            AddPort(resultPort);
            
            RefreshExpandedState();
        }

        protected override void OnPortValueChanged(Edge edge, GraphFrameworkPort port) {
            if (port == aPort) {
                a = GetValueFromEdge(edge, a);
            } else if (port == bPort) {
                b = GetValueFromEdge(edge, b);
            }

            UpdateResult();
        }

        private void UpdateResult() {
            var newResult = CalculateResult();
            if (Math.Abs(result - newResult) < 0.00001f) return;

            result = newResult;
            NotifyPortValueChanged(resultPort);
        }

        public override object GetValueForPort(GraphFrameworkPort port) {
            if (port != resultPort) return null;

            result = CalculateResult();

            return result;
        }

        private float CalculateResult() {
            return operation switch {
                MathOperation.Add => a + b,
                MathOperation.Subtract => a - b,
                MathOperation.Multiply => a * b,
                MathOperation.Divide => a / b,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override JObject GetNodeData() {
            var root =  base.GetNodeData();
            root["a"] = a;
            root["af"] = aField.enabledSelf ? a : aField.value;
            root["b"] = b;
            root["bf"] = bField.enabledSelf ? b : bField.value;
            root["op"] = (int)operation;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            a = jsonData.Value<float>("a");
            b = jsonData.Value<float>("b");
            aField.SetValueWithoutNotify(jsonData.Value<float>("af"));
            bField.SetValueWithoutNotify(jsonData.Value<float>("bf"));
            operation = (MathOperation)jsonData.Value<int>("op");
            operationField.SetValueWithoutNotify(operation);

            base.SetNodeData(jsonData); 
        }
    }
}