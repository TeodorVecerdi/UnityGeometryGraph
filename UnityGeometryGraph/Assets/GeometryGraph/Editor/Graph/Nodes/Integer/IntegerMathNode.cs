using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Which;
using MathOperation = GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_MathOperation;

namespace GeometryGraph.Editor {
    [Title("Integer", "Math")]
    public class IntegerMathNode : AbstractNode<GeometryGraph.Runtime.Graph.IntegerMathNode> {
        
        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort tolerancePort;
        private GraphFrameworkPort extraPort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionDropdown<MathOperation> operationDropdown;
        private IntegerField xField;
        private IntegerField yField;
        private FloatField toleranceField;
        private IntegerField extraField;

        private MathOperation operation;
        private int x;
        private int y;
        private float tolerance;
        private int extra;

        private static readonly SelectionTree mathOperationTree = new SelectionTree(new List<object>(Enum.GetValues(typeof(MathOperation)).Convert(o => o))) {
            new SelectionCategory("Operations", false, SelectionCategory.CategorySize.Large) {
                new SelectionEntry("x + y", 0, false),
                new SelectionEntry("x - y", 1, false),
                new SelectionEntry("x * y", 2, false),
                new SelectionEntry("x / y", 3, false),
                new SelectionEntry("Floating division x/y", 7, true),
                new SelectionEntry("Base raised to the power Exponent", 4, false),
                new SelectionEntry("Logarithm of x in base Base", 5, false),
                new SelectionEntry("Square root of x", 6, false),
                new SelectionEntry("Absolute value of x", 8, false),
                new SelectionEntry("e to the power of x", 9, false),
            },
            new SelectionCategory("Rounding", true, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("The remainder of x / y", 18, false),
                new SelectionEntry("x wrapped between min and max", 19, false),
                new SelectionEntry("x snapped to the nearest multiple of increment", 20, false),
            },
            new SelectionCategory("Comparison", true, SelectionCategory.CategorySize.Medium) {
                new SelectionEntry("The minimum between x and y", 10, false),
                new SelectionEntry("The maximum between x and y", 11, false),
                new SelectionEntry("1 if x < y; 0 otherwise", 12, false),
                new SelectionEntry("1 if x > y; 0 otherwise", 13, false),
                new SelectionEntry("Sign of x", 14, false),
                new SelectionEntry("1 if x = y; 0 otherwise", 15, false),
                new SelectionEntry("The minimum between x and y with smoothing", 16, false),
                new SelectionEntry("The maximum between x and y with smoothing", 17, false),
            },
        };

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Integer Math");

            (xPort, xField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("X", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(x, Which.X));
            (yPort, yField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Y", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(y, Which.Y));
            (tolerancePort, toleranceField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Tolerance", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(tolerance, Which.Tolerance));
            (extraPort, extraField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Extra", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(extra, Which.Extra));
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Integer, edgeConnectorListener, this);

            operationDropdown = new EnumSelectionDropdown<MathOperation>(operation, mathOperationTree);
            operationDropdown.RegisterCallback<ChangeEvent<MathOperation>>(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change operation");
                operation = evt.newValue;
                RuntimeNode.UpdateOperation(operation);
                OnOperationChanged();
            });

            xField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                x = evt.newValue;
                RuntimeNode.UpdateValue(x, Which.X);
            });

            yField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                y = evt.newValue;
                RuntimeNode.UpdateValue(y, Which.Y);
            });
            
            toleranceField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change tolerance");
                tolerance = evt.newValue;
                RuntimeNode.UpdateValue(tolerance, Which.Tolerance);
            });
            
            extraField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change extra");
                extra = evt.newValue;
                RuntimeNode.UpdateValue(extra, Which.Extra);
            });
            
            xPort.Add(xField);
            yPort.Add(yField);
            tolerancePort.Add(toleranceField); 
            extraPort.Add(extraField); 
            
            inputContainer.Add(operationDropdown);
            AddPort(xPort);
            AddPort(yPort);
            AddPort(tolerancePort);
            AddPort(extraPort);
            AddPort(resultPort);
            
            OnOperationChanged();

            Refresh();
        }

        public override void BindPorts() {
            BindPort(xPort, RuntimeNode.XPort);
            BindPort(yPort, RuntimeNode.YPort);
            BindPort(tolerancePort, RuntimeNode.TolerancePort);
            BindPort(extraPort, RuntimeNode.ExtraPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["o"] = (int)operation;
            root["x"] = x;
            root["y"] = y;
            root["t"] = tolerance;
            root["v"] = extra;
            
            return root;
        }

        private void OnOperationChanged() {
            var showTolerance = operation == MathOperation.SmoothMinimum || operation == MathOperation.SmoothMaximum;
            var showExtra = operation == MathOperation.Wrap;
            var showY = !(operation == MathOperation.SquareRoot || operation == MathOperation.Absolute ||
                          operation == MathOperation.Exponent || operation == MathOperation.Sign);
            
            if (showTolerance) {
                tolerancePort.Show();
            } else {
                tolerancePort.HideAndDisconnect();
            }

            if (showExtra) {
                extraPort.Show();
            } else {
                extraPort.HideAndDisconnect();
            }

            if (showY) {
                yPort.Show();
            } else {
                yPort.HideAndDisconnect();
            }

            UpdatePortNames();
        }

        private void UpdatePortNames() {
            switch (operation) {
                case MathOperation.Add:
                case MathOperation.Subtract:
                case MathOperation.Multiply:
                case MathOperation.IntegerDivision:
                case MathOperation.FloatDivision:
                case MathOperation.Minimum:
                case MathOperation.Maximum:
                case MathOperation.LessThan:
                case MathOperation.GreaterThan:
                case MathOperation.Modulo:
                    SetPortNames("X", "Y", "", ""); break;
                
                case MathOperation.Power: 
                    SetPortNames("Base", "Exponent", "", ""); break;
                
                case MathOperation.Logarithm: 
                    SetPortNames("X", "Base", "", ""); break;
                
                case MathOperation.SquareRoot:
                case MathOperation.Absolute:
                case MathOperation.Exponent:
                case MathOperation.Sign:
                    SetPortNames("X", "", "", ""); break;
                
                case MathOperation.Compare: 
                    SetPortNames("X", "Y", "Tolerance", ""); break;
                
                case MathOperation.SmoothMinimum:
                case MathOperation.SmoothMaximum: 
                    SetPortNames("X", "Y", "Distance", ""); break;
                
                case MathOperation.Wrap:
                    SetPortNames("X", "Min", "", "Max"); break;
                
                case MathOperation.Snap:
                    SetPortNames("X", "Increment", "", ""); break;

                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void SetPortNames(string x, string y, string tolerance, string extra) {
            xPort.Label = x;
            yPort.Label = y;
            tolerancePort.Label = tolerance;
            extraPort.Label = extra;
        }

        public override void SetNodeData(JObject jsonData) {
            operation = (MathOperation) jsonData.Value<int>("o");
            x = jsonData.Value<int>("x");
            y = jsonData.Value<int>("y");
            tolerance = jsonData.Value<float>("t");
            extra = jsonData.Value<int>("v");
            
            operationDropdown.SetValueWithoutNotify(operation, 1);
            xField.SetValueWithoutNotify(x);
            yField.SetValueWithoutNotify(y);
            toleranceField.SetValueWithoutNotify(tolerance);
            extraField.SetValueWithoutNotify(extra);
            
            RuntimeNode.UpdateOperation(operation);
            RuntimeNode.UpdateValue(x, Which.X);
            RuntimeNode.UpdateValue(y, Which.Y);
            RuntimeNode.UpdateValue(tolerance, Which.Tolerance);
            RuntimeNode.UpdateValue(extra, Which.Extra);

            OnOperationChanged();

            base.SetNodeData(jsonData);
        }
    }
}