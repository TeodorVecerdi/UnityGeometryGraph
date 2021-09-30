using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.CompareIntegerNode.CompareIntegerNode_Which;
using CompareOperation = GeometryGraph.Runtime.Graph.CompareIntegerNode.CompareIntegerNode_CompareOperation;

namespace GeometryGraph.Editor {
    [Title("Integer", "Compare")]
    public class CompareIntegerNode : AbstractNode<GeometryGraph.Runtime.Graph.CompareIntegerNode> {
        
        private GraphFrameworkPort aPort;
        private GraphFrameworkPort bPort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionButton<CompareOperation> operationButton;
        private IntegerField aField;
        private IntegerField bField;

        private CompareOperation operation;
        private int a;
        private int b;

        private static readonly SelectionTree compareOperationTree = new SelectionTree(new List<object>(Enum.GetValues(typeof(CompareOperation)).Convert(o => o))) {
            new SelectionCategory("Operation", false, SelectionCategory.CategorySize.Large) {
                new SelectionEntry("a < b", 0, false),
                new SelectionEntry("a ≤ b", 1, false),
                new SelectionEntry("a > b", 2, false),
                new SelectionEntry("a ≥ b", 3, true),
                new SelectionEntry("a = b", 4, false),
                new SelectionEntry("a ≠ b", 5, false),
            }
        };

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Compare", EditorView.DefaultNodePosition);

            (aPort, aField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("A", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(a, Which.A));
            (bPort, bField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("B", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(b, Which.B));
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Boolean, edgeConnectorListener, this);

            operationButton = new EnumSelectionButton<CompareOperation>(operation, compareOperationTree);
            operationButton.RegisterCallback<ChangeEvent<CompareOperation>>(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change operation");
                operation = evt.newValue;
                RuntimeNode.UpdateCompareOperation(operation);
            });

            aField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                a = evt.newValue;
                RuntimeNode.UpdateValue(a, Which.A);
            });
            
            bField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                b = evt.newValue;
                RuntimeNode.UpdateValue(b, Which.B);
            });

            aPort.Add(aField);
            bPort.Add(bField);
            
            inputContainer.Add(operationButton);
            AddPort(aPort);
            AddPort(bPort);
            AddPort(resultPort);
            
            Refresh();
        }

        public override void BindPorts() {
            BindPort(aPort, RuntimeNode.APort);
            BindPort(bPort, RuntimeNode.BPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["o"] = (int)operation;
            root["a"] = a;
            root["b"] = b;
            
            return root;
        }
        
        public override void SetNodeData(JObject jsonData) {
            operation = (CompareOperation) jsonData.Value<int>("o");
            a = jsonData.Value<int>("a");
            b = jsonData.Value<int>("b");
            
            operationButton.SetValueWithoutNotify(operation, 1);
            aField.SetValueWithoutNotify(a);
            bField.SetValueWithoutNotify(b);
            
            RuntimeNode.UpdateCompareOperation(operation);
            RuntimeNode.UpdateValue(a, Which.A);
            RuntimeNode.UpdateValue(b, Which.B);

            base.SetNodeData(jsonData);
        }
    }
}