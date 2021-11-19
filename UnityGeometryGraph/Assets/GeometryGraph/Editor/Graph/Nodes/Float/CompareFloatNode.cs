using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using CompareOperation = GeometryGraph.Runtime.Graph.CompareFloatNode.CompareFloatNode_CompareOperation;

namespace GeometryGraph.Editor {
    [Title("Float", "Compare")]
    public class CompareFloatNode : AbstractNode<GeometryGraph.Runtime.Graph.CompareFloatNode> {
        
        private GraphFrameworkPort tolerancePort;
        private GraphFrameworkPort aPort;
        private GraphFrameworkPort bPort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionDropdown<CompareOperation> operationDropdown;
        private FloatField toleranceField;
        private FloatField aField;
        private FloatField bField;

        private CompareOperation operation;
        private float tolerance = 1e-6f;
        private float a;
        private float b;

        private static readonly SelectionTree compareOperationTree = new SelectionTree(new List<object>(Enum.GetValues(typeof(CompareOperation)).Convert(o => o))) {
            new SelectionCategory("Operation", false, SelectionCategory.CategorySize.Large) {
                new SelectionEntry("a < b", 0, false),
                new SelectionEntry("a ≤ b", 1, false),
                new SelectionEntry("a > b", 2, false),
                new SelectionEntry("a ≥ b", 3, true),
                new SelectionEntry("a = b, within tolerance", 4, false),
                new SelectionEntry("a ≠ b, within tolerance", 5, false),
            }
        };

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Compare");

            (tolerancePort, toleranceField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Tolerance", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateTolerance(tolerance));
            (aPort, aField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("A", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateA(a));
            (bPort, bField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("B", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateB(b));
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Boolean, edgeConnectorListener, this);

            operationDropdown = new EnumSelectionDropdown<CompareOperation>(operation, compareOperationTree);
            operationDropdown.RegisterCallback<ChangeEvent<CompareOperation>>(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change operation");
                operation = evt.newValue;
                RuntimeNode.UpdateOperation(operation);
                OnOperationChanged();
            });

            toleranceField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change tolerance");
                tolerance = evt.newValue;
                RuntimeNode.UpdateTolerance(tolerance);
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

            tolerancePort.Add(toleranceField); 
            aPort.Add(aField);
            bPort.Add(bField);
            
            inputContainer.Add(operationDropdown);
            AddPort(tolerancePort);
            AddPort(aPort);
            AddPort(bPort);
            AddPort(resultPort);
            
            OnOperationChanged();

            Refresh();
        }

        public override void BindPorts() {
            BindPort(tolerancePort, RuntimeNode.TolerancePort);
            BindPort(aPort, RuntimeNode.APort);
            BindPort(bPort, RuntimeNode.BPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["o"] = (int)operation;
            root["t"] = tolerance;
            root["a"] = a;
            root["b"] = b;
            
            return root;
        }

        private void OnOperationChanged() {
            var showTolerance = operation == CompareOperation.Equal || operation == CompareOperation.NotEqual;
            
            if (showTolerance) {
                tolerancePort.Show();
            } else {
                tolerancePort.HideAndDisconnect();
            }
        }

        public override void SetNodeData(JObject jsonData) {
            operation = (CompareOperation) jsonData.Value<int>("o");
            tolerance = jsonData.Value<float>("t");
            a = jsonData.Value<float>("a");
            b = jsonData.Value<float>("b");
            
            operationDropdown.SetValueWithoutNotify(operation, 1);
            toleranceField.SetValueWithoutNotify(tolerance);
            aField.SetValueWithoutNotify(a);
            bField.SetValueWithoutNotify(b);
            
            RuntimeNode.UpdateOperation(operation);
            RuntimeNode.UpdateTolerance(tolerance);
            RuntimeNode.UpdateA(a);
            RuntimeNode.UpdateB(b);

            OnOperationChanged();

            base.SetNodeData(jsonData);
        }
    }
}