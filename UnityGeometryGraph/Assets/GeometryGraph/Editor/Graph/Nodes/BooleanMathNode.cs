using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Operation = GeometryGraph.Runtime.Graph.BooleanMathNode.BooleanMathNode_Operation;

namespace GeometryGraph.Editor {
    [Title("Boolean", "Math")]
    public class BooleanMathNode : AbstractNode<GeometryGraph.Runtime.Graph.BooleanMathNode> {
        protected override string Title => "Math (Boolean)";
        protected override NodeCategory Category => NodeCategory.Boolean;

        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionDropdown<Operation> operationDropdown;
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

        protected override void CreateNode() {
            (xPort, xField) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>("X", PortType.Boolean, this, onDisconnect: (_, _) => RuntimeNode.UpdateA(x));
            (yPort, yField) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>("Y", PortType.Boolean, this, onDisconnect: (_, _) => RuntimeNode.UpdateB(y));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Boolean, this);

            operationDropdown = new EnumSelectionDropdown<Operation>(operation, compareOperationTree);
            operationDropdown.RegisterCallback<ChangeEvent<Operation>>(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change operation");
                operation = evt.newValue;
                RuntimeNode.UpdateOperation(operation);
                OnOperationChanged();
            });

            xField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                x = evt.newValue;
                RuntimeNode.UpdateA(x);
            });
            
            yField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                y = evt.newValue;
                RuntimeNode.UpdateB(y);
            });

            xPort.Add(xField);
            yPort.Add(yField);
            
            inputContainer.Add(operationDropdown);
            AddPort(xPort);
            AddPort(yPort);
            AddPort(resultPort);
            
            OnOperationChanged();

            Refresh();
        }

        protected override void BindPorts() {
            BindPort(xPort, RuntimeNode.APort);
            BindPort(yPort, RuntimeNode.BPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();

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

        protected internal override void Deserialize(JObject data) {
            operation = (Operation) data.Value<int>("o");
            x = data.Value<int>("a") == 1;
            y = data.Value<int>("b") == 1;
            
            operationDropdown.SetValueWithoutNotify(operation, 1);
            xField.SetValueWithoutNotify(x);
            yField.SetValueWithoutNotify(y);
            
            RuntimeNode.UpdateOperation(operation);
            RuntimeNode.UpdateA(x);
            RuntimeNode.UpdateB(y);

            OnOperationChanged();

            base.Deserialize(data);
        }
    }
}