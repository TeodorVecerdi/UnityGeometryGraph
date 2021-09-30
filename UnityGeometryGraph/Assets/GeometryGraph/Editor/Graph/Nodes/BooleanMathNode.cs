using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.BooleanMathNode.BooleanMathNode_Which;
using Operation = GeometryGraph.Runtime.Graph.BooleanMathNode.BooleanMathNode_Operation;

namespace GeometryGraph.Editor {
    [Title("Boolean", "Math")]
    public class BooleanMathNode : AbstractNode<GeometryGraph.Runtime.Graph.BooleanMathNode> {
        
        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionButton<Operation> operationButton;
        private Toggle xField;
        private Toggle yField;

        private Operation operation;
        private bool x;
        private bool y;

        private static readonly SelectionTree compareOperationTree = new SelectionTree(new List<object>(Enum.GetValues(typeof(Operation)).Convert(o => o))) {
            new SelectionCategory("Operation", false, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("True if both x and y are true", 0, false),
                new SelectionEntry("True if either x or y are true", 1, false),
                new SelectionEntry("True if only one of x and y are true", 2, false),
                new SelectionEntry("True if x is false", 3, false),
            }
        };

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Boolean Math", EditorView.DefaultNodePosition);

            (xPort, xField) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>("X", Orientation.Horizontal, PortType.Boolean, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(x, Which.A));
            (yPort, yField) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>("Y", Orientation.Horizontal, PortType.Boolean, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(y, Which.B));
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Boolean, edgeConnectorListener, this);

            operationButton = new EnumSelectionButton<Operation>(operation, compareOperationTree);
            operationButton.RegisterCallback<ChangeEvent<Operation>>(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change operation");
                operation = evt.newValue;
                RuntimeNode.UpdateCompareOperation(operation);
                OnOperationChanged();
            });

            xField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                x = evt.newValue;
                RuntimeNode.UpdateValue(x, Which.A);
            });
            
            yField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                y = evt.newValue;
                RuntimeNode.UpdateValue(y, Which.B);
            });

            xPort.Add(xField);
            yPort.Add(yField);
            
            inputContainer.Add(operationButton);
            AddPort(xPort);
            AddPort(yPort);
            AddPort(resultPort);
            
            OnOperationChanged();

            Refresh();
        }

        public override void BindPorts() {
            BindPort(xPort, RuntimeNode.APort);
            BindPort(yPort, RuntimeNode.BPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["o"] = (int)operation;
            root["a"] = x ? 1 : 0;
            root["b"] = y ? 1 : 0;
            
            return root;
        }

        private void OnOperationChanged() {
            if (operation == Operation.NOT) {
                yPort.HideAndDisconnect();
            } else {
                yPort.Show();
            }
        }

        public override void SetNodeData(JObject jsonData) {
            operation = (Operation) jsonData.Value<int>("o");
            x = jsonData.Value<int>("a") == 1;
            y = jsonData.Value<int>("b") == 1;
            
            operationButton.SetValueWithoutNotify(operation, 1);
            xField.SetValueWithoutNotify(x);
            yField.SetValueWithoutNotify(y);
            
            RuntimeNode.UpdateCompareOperation(operation);
            RuntimeNode.UpdateValue(x, Which.A);
            RuntimeNode.UpdateValue(y, Which.B);

            OnOperationChanged();

            base.SetNodeData(jsonData);
        }
    }
}