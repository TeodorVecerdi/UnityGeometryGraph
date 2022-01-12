using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using CompareOperation = GeometryGraph.Runtime.Graph.CompareIntegerNode.CompareIntegerNode_CompareOperation;

namespace GeometryGraph.Editor {
    [Title("Integer", "Compare")]
    public class CompareIntegerNode : AbstractNode<GeometryGraph.Runtime.Graph.CompareIntegerNode> {
        protected override string Title => "Compare (Integer)";
        protected override NodeCategory Category => NodeCategory.Integer;

        private GraphFrameworkPort aPort;
        private GraphFrameworkPort bPort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionDropdown<CompareOperation> operationDropdown;
        private IntegerField aField;
        private IntegerField bField;

        private CompareOperation operation;
        private int a;
        private int b;

        private static readonly SelectionTree compareOperationTree = new(new List<object>(Enum.GetValues(typeof(CompareOperation)).Convert(o => o))) {
            new SelectionCategory("Operation", false, SelectionCategory.CategorySize.Large) {
                new("a < b", 0, false),
                new("a ≤ b", 1, false),
                new("a > b", 2, false),
                new("a ≥ b", 3, true),
                new("a = b", 4, false),
                new("a ≠ b", 5, false),
            }
        };

        protected override void CreateNode() {
            (aPort, aField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("A", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateA(a));
            (bPort, bField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("B", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateB(b));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Boolean, this);

            operationDropdown = new EnumSelectionDropdown<CompareOperation>(operation, compareOperationTree);
            operationDropdown.RegisterCallback<ChangeEvent<CompareOperation>>(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change operation");
                operation = evt.newValue;
                RuntimeNode.UpdateOperation(operation);
            });

            aField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                a = evt.newValue;
                RuntimeNode.UpdateA(a);
            });
            
            bField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                b = evt.newValue;
                RuntimeNode.UpdateB(b);
            });

            aPort.Add(aField);
            bPort.Add(bField);
            
            inputContainer.Add(operationDropdown);
            AddPort(aPort);
            AddPort(bPort);
            AddPort(resultPort);
            
            Refresh();
        }

        protected override void BindPorts() {
            BindPort(aPort, RuntimeNode.APort);
            BindPort(bPort, RuntimeNode.BPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();

            root["o"] = (int)operation;
            root["a"] = a;
            root["b"] = b;
            
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            operation = (CompareOperation) data.Value<int>("o");
            a = data.Value<int>("a");
            b = data.Value<int>("b");
            
            operationDropdown.SetValueWithoutNotify(operation, 1);
            aField.SetValueWithoutNotify(a);
            bField.SetValueWithoutNotify(b);
            
            RuntimeNode.UpdateOperation(operation);
            RuntimeNode.UpdateA(a);
            RuntimeNode.UpdateB(b);

            base.Deserialize(data);
        }
    }
}