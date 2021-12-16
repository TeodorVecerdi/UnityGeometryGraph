using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Integer", "Math")]
    public class IntegerMathNode : AbstractNode<GeometryGraph.Runtime.Graph.IntegerMathNode> {
        protected override string Title => "Math (Integer)";
        protected override NodeCategory Category => NodeCategory.Integer;

        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort tolerancePort;
        private GraphFrameworkPort extraPort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionDropdown<Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation> operationDropdown;
        private IntegerField xField;
        private IntegerField yField;
        private FloatField toleranceField;
        private IntegerField extraField;

        private Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation operation;
        private int x;
        private int y;
        private float tolerance;
        private int extra;

        private static readonly SelectionTree mathOperationTree = new SelectionTree(new List<object>(Enum.GetValues(typeof(Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation)).Convert(o => o))) {
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

        protected override void CreateNode() {
            (xPort, xField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("X", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateX(x));
            (yPort, yField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Y", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateY(y));
            (tolerancePort, toleranceField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Tolerance", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateTolerance(tolerance));
            (extraPort, extraField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Extra", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateExtra(extra));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Integer, this);

            operationDropdown = new EnumSelectionDropdown<Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation>(operation, mathOperationTree);
            operationDropdown.RegisterCallback<ChangeEvent<Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation>>(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change operation");
                operation = evt.newValue;
                RuntimeNode.UpdateOperation(operation);
                OnOperationChanged();
            });

            xField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                x = evt.newValue;
                RuntimeNode.UpdateX(x);
            });

            yField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                y = evt.newValue;
                RuntimeNode.UpdateY(y);
            });
            
            toleranceField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change tolerance");
                tolerance = evt.newValue;
                RuntimeNode.UpdateTolerance(tolerance);
            });
            
            extraField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change extra");
                extra = evt.newValue;
                RuntimeNode.UpdateExtra(extra);
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

        protected override void BindPorts() {
            BindPort(xPort, RuntimeNode.XPort);
            BindPort(yPort, RuntimeNode.YPort);
            BindPort(tolerancePort, RuntimeNode.TolerancePort);
            BindPort(extraPort, RuntimeNode.ExtraPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject GetNodeData() {
            JObject root = base.GetNodeData();

            root["o"] = (int)operation;
            root["x"] = x;
            root["y"] = y;
            root["t"] = tolerance;
            root["v"] = extra;
            
            return root;
        }

        private void OnOperationChanged() {
            bool showTolerance = operation == GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.SmoothMinimum || operation == GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.SmoothMaximum;
            bool showExtra = operation == GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Wrap;
            bool showY = !(operation == GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.SquareRoot || operation == GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Absolute ||
                           operation == GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Exponent || operation == GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Sign);
            
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
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Add:
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Subtract:
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Multiply:
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.IntegerDivision:
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.FloatDivision:
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Minimum:
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Maximum:
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.LessThan:
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.GreaterThan:
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Modulo:
                    SetPortNames("X", "Y", "", ""); break;
                
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Power: 
                    SetPortNames("Base", "Exponent", "", ""); break;
                
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Logarithm: 
                    SetPortNames("X", "Base", "", ""); break;
                
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.SquareRoot:
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Absolute:
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Exponent:
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Sign:
                    SetPortNames("X", "", "", ""); break;
                
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Compare: 
                    SetPortNames("X", "Y", "Tolerance", ""); break;
                
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.SmoothMinimum:
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.SmoothMaximum: 
                    SetPortNames("X", "Y", "Distance", ""); break;
                
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Wrap:
                    SetPortNames("X", "Min", "", "Max"); break;
                
                case GeometryGraph.Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation.Snap:
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

        protected internal override void SetNodeData(JObject jsonData) {
            operation = (Runtime.Graph.IntegerMathNode.IntegerMathNode_Operation) jsonData.Value<int>("o");
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
            RuntimeNode.UpdateX(x);
            RuntimeNode.UpdateY(y);
            RuntimeNode.UpdateTolerance(tolerance);
            RuntimeNode.UpdateExtra(extra);

            OnOperationChanged();

            base.SetNodeData(jsonData);
        }
    }
}