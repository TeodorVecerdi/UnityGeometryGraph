using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Which;
using MathOperation = GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_MathOperation;

namespace GeometryGraph.Editor {
    [Title("Float", "Math")]
    public class FloatMathNode : AbstractNode<GeometryGraph.Runtime.Graph.FloatMathNode> {
        
        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort tolerancePort;
        private GraphFrameworkPort extraPort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionButton<MathOperation> operationButton;
        private FloatField xField;
        private FloatField yField;
        private FloatField toleranceField;
        private FloatField extraField;

        private MathOperation operation;
        private float x;
        private float y;
        private float tolerance = 1e-6f;
        private float extra;

        private static readonly SelectionTree mathOperationTree = new SelectionTree(new List<object>(Enum.GetValues(typeof(MathOperation)).Convert(o => o))) {
            new SelectionCategory("Operations", false, SelectionCategory.CategorySize.Large) {
                new SelectionEntry("x + y", 0, false),
                new SelectionEntry("x - y", 1, false),
                new SelectionEntry("x * y", 2, false),
                new SelectionEntry("x / y", 3, true),
                new SelectionEntry("Base raised to the power Exponent", 4, false),
                new SelectionEntry("Logarithm of x in base Base", 5, false),
                new SelectionEntry("Square root of x", 6, false),
                new SelectionEntry("1 / Square root of x", 7, false),
                new SelectionEntry("Absolute value of x", 8, false),
                new SelectionEntry("e to the power of x", 9, false),
            },
            new SelectionCategory("Rounding", false, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("x rounded to the nearest integer", 18, false),
                new SelectionEntry("The largest integer smaller than x", 19, false),
                new SelectionEntry("The smallest integer larger than x", 20, false),
                new SelectionEntry("The integral part of x", 21, true),
                new SelectionEntry("The fractional part of x", 22, false),
                new SelectionEntry("The remainder of x / y", 23, false),
                new SelectionEntry("x wrapped between min and max", 24, false),
                new SelectionEntry("x snapped to the nearest multiple of increment", 25, false),
            },
            new SelectionCategory("Comparison", false, SelectionCategory.CategorySize.Medium) {
                new SelectionEntry("The minimum between x and y", 10, false),
                new SelectionEntry("The maximum between x and y", 11, false),
                new SelectionEntry("1 if x < y; 0 otherwise", 12, false),
                new SelectionEntry("1 if x > y; 0 otherwise", 13, false),
                new SelectionEntry("Sign of x", 14, false),
                new SelectionEntry("1 if the distance between x and y is less than tolerance; 0 otherwise", 15, false),
                new SelectionEntry("The minimum between x and y with smoothing", 16, false),
                new SelectionEntry("The maximum between x and y with smoothing", 17, false),
            },
            new SelectionCategory("Conversion", true, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("Converts degrees to radians", 33, false),
                new SelectionEntry("Converts radians to degrees", 34, false),
            },
            new SelectionCategory("Trigonometry", true, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("Sine of x (in radians)", 26, false),
                new SelectionEntry("Cosine of x (in radians)", 27, false),
                new SelectionEntry("Tangent of x (in radians)", 28, false),
                new SelectionEntry("Inverse sine of x (in radians)", 29, true),
                new SelectionEntry("Inverse cosine of x (in radians)", 30, false),
                new SelectionEntry("Inverse tangent of x (in radians)", 31, false),
                new SelectionEntry("Inverse tangent of x / y (in radians)", 32, false),
            }
        };

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Float Math", EditorView.DefaultNodePosition);

            (xPort, xField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("X", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this);
            (yPort, yField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Y", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this);
            (tolerancePort, toleranceField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Tolerance", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this);
            (extraPort, extraField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Max", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this);
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Float, edgeConnectorListener, this);

            operationButton = new EnumSelectionButton<MathOperation>(operation, mathOperationTree);
            operationButton.RegisterCallback<ChangeEvent<MathOperation>>(evt => {
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
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                extra = evt.newValue;
                RuntimeNode.UpdateValue(extra, Which.Extra);
            });

            xPort.Add(xField);
            yPort.Add(yField);
            tolerancePort.Add(toleranceField); 
            extraPort.Add(extraField);
            
            inputContainer.Add(operationButton);
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
            var showTolerance = operation == MathOperation.Compare || operation == MathOperation.SmoothMinimum || operation == MathOperation.SmoothMaximum;
            var showExtra = operation == MathOperation.Wrap;
            var showY = !(operation == MathOperation.SquareRoot || operation == MathOperation.InverseSquareRoot || operation == MathOperation.Absolute ||
                          operation == MathOperation.Exponent || operation == MathOperation.Sign || operation == MathOperation.Round ||
                          operation == MathOperation.Floor || operation == MathOperation.Ceil || operation == MathOperation.Truncate ||
                          operation == MathOperation.Fraction || operation == MathOperation.Sine || operation == MathOperation.Cosine ||
                          operation == MathOperation.Tangent || operation == MathOperation.Arcsine || operation == MathOperation.Arccosine ||
                          operation == MathOperation.Arctangent || operation == MathOperation.ToRadians || operation == MathOperation.ToDegrees);
            
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
                case MathOperation.Divide:
                case MathOperation.Minimum:
                case MathOperation.Maximum:
                case MathOperation.LessThan:
                case MathOperation.GreaterThan:
                case MathOperation.Modulo:
                case MathOperation.Atan2:
                    SetPortNames("X", "Y", "", ""); break;
                
                case MathOperation.Power: 
                    SetPortNames("Base", "Exponent", "", ""); break;
                
                case MathOperation.Logarithm: 
                    SetPortNames("X", "Base", "", ""); break;
                
                case MathOperation.SquareRoot:
                case MathOperation.InverseSquareRoot:
                case MathOperation.Absolute:
                case MathOperation.Exponent:
                case MathOperation.Sign: 
                case MathOperation.Round:
                case MathOperation.Floor:
                case MathOperation.Ceil:
                case MathOperation.Truncate:
                case MathOperation.Fraction:
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
                
                case MathOperation.Sine:
                case MathOperation.Cosine:
                case MathOperation.Tangent:
                case MathOperation.Arcsine:
                case MathOperation.Arccosine:
                case MathOperation.Arctangent:
                case MathOperation.ToDegrees:
                    SetPortNames("Radians", "", "", ""); break;
                case MathOperation.ToRadians:
                    SetPortNames("Degrees", "", "", ""); break;
                
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
            x = jsonData.Value<float>("x");
            y = jsonData.Value<float>("y");
            tolerance = jsonData.Value<float>("t");
            extra = jsonData.Value<float>("v");
            
            operationButton.SetValueWithoutNotify(operation, 1);
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