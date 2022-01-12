using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Float", "Math")]
    public class FloatMathNode : AbstractNode<GeometryGraph.Runtime.Graph.FloatMathNode> {
        protected override string Title => "Math (Float)";
        protected override NodeCategory Category => NodeCategory.Float;
        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort tolerancePort;
        private GraphFrameworkPort extraPort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionDropdown<Runtime.Graph.FloatMathNode.FloatMathNode_Operation> operationDropdown;
        private FloatField xField;
        private FloatField yField;
        private FloatField toleranceField;
        private FloatField extraField;

        private Runtime.Graph.FloatMathNode.FloatMathNode_Operation operation;
        private float x;
        private float y;
        private float tolerance = 1e-6f;
        private float extra;

        private static readonly SelectionTree mathOperationTree = new(new List<object>(Enum.GetValues(typeof(Runtime.Graph.FloatMathNode.FloatMathNode_Operation)).Convert(o => o))) {
            new SelectionCategory("Operations", false, SelectionCategory.CategorySize.Large) {
                new("x + y", 0, false),
                new("x - y", 1, false),
                new("x * y", 2, false),
                new("x / y", 3, true),
                new("Base raised to the power Exponent", 4, false),
                new("Logarithm of x in base Base", 5, false),
                new("Square root of x", 6, false),
                new("1 / Square root of x", 7, false),
                new("Absolute value of x", 8, false),
                new("e to the power of x", 9, false),
                new("Linear interpolation between x and y using t", 35, false),
            },
            new SelectionCategory("Rounding", false, SelectionCategory.CategorySize.Normal) {
                new("x rounded to the nearest integer", 18, false),
                new("The largest integer smaller than x", 19, false),
                new("The smallest integer larger than x", 20, false),
                new("The integral part of x", 21, true),
                new("The fractional part of x", 22, false),
                new("The remainder of x / y", 23, false),
                new("x wrapped between min and max", 24, false),
                new("x snapped to the nearest multiple of increment", 25, false),
            },
            new SelectionCategory("Comparison", false, SelectionCategory.CategorySize.Medium) {
                new("The minimum between x and y", 10, false),
                new("The maximum between x and y", 11, false),
                new("1 if x < y; 0 otherwise", 12, false),
                new("1 if x > y; 0 otherwise", 13, false),
                new("Sign of x", 14, false),
                new("1 if the distance between x and y is less than tolerance; 0 otherwise", 15, false),
                new("The minimum between x and y with smoothing", 16, false),
                new("The maximum between x and y with smoothing", 17, false),
            },
            new SelectionCategory("Conversion", true, SelectionCategory.CategorySize.Normal) {
                new("Converts degrees to radians", 33, false),
                new("Converts radians to degrees", 34, false),
            },
            new SelectionCategory("Trigonometry", true, SelectionCategory.CategorySize.Normal) {
                new("Sine of x (in radians)", 26, false),
                new("Cosine of x (in radians)", 27, false),
                new("Tangent of x (in radians)", 28, false),
                new("Inverse sine of x (in radians)", 29, true),
                new("Inverse cosine of x (in radians)", 30, false),
                new("Inverse tangent of x (in radians)", 31, false),
                new("Inverse tangent of x / y (in radians)", 32, false),
            }
        };

        protected override void CreateNode() {
            (xPort, xField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("X", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateX(x));
            (yPort, yField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Y", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateY(y));
            (tolerancePort, toleranceField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Tolerance", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateTolerance(tolerance));
            (extraPort, extraField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Max", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateExtra(extra));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Float, this);

            operationDropdown = new EnumSelectionDropdown<Runtime.Graph.FloatMathNode.FloatMathNode_Operation>(operation, mathOperationTree);
            operationDropdown.RegisterCallback<ChangeEvent<Runtime.Graph.FloatMathNode.FloatMathNode_Operation>>(evt => {
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
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
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

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();

            root["o"] = (int)operation;
            root["x"] = x;
            root["y"] = y;
            root["t"] = tolerance;
            root["v"] = extra;
            
            return root;
        }

        private void OnOperationChanged() {
            bool showTolerance = operation is GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Compare or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.SmoothMinimum or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.SmoothMaximum;
            bool showExtra = operation is GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Wrap or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Lerp;
            bool showY = !(operation is GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.SquareRoot or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.InverseSquareRoot or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Absolute or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Exponent or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Sign or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Round or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Floor or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Ceil or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Truncate or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Fraction or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Sine or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Cosine or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Tangent or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Arcsine or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Arccosine or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Arctangent or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.ToRadians or GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.ToDegrees);
            
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
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Add:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Subtract:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Multiply:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Divide:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Minimum:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Maximum:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.LessThan:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.GreaterThan:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Modulo:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Atan2:
                    SetPortNames("X", "Y", "", ""); break;

                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Power:
                    SetPortNames("Base", "Exponent", "", ""); break;

                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Logarithm:
                    SetPortNames("X", "Base", "", ""); break;

                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.SquareRoot:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.InverseSquareRoot:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Absolute:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Exponent:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Sign:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Round:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Floor:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Ceil:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Truncate:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Fraction:
                    SetPortNames("X", "", "", ""); break;

                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Compare:
                    SetPortNames("X", "Y", "Tolerance", ""); break;

                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.SmoothMinimum:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.SmoothMaximum:
                    SetPortNames("X", "Y", "Distance", ""); break;

                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Wrap:
                    SetPortNames("X", "Min", "", "Max"); break;

                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Snap:
                    SetPortNames("X", "Increment", "", ""); break;

                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Sine:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Cosine:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Tangent:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Arcsine:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Arccosine:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Arctangent:
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.ToDegrees:
                    SetPortNames("Radians", "", "", ""); break;
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.ToRadians:
                    SetPortNames("Degrees", "", "", ""); break;
                case GeometryGraph.Runtime.Graph.FloatMathNode.FloatMathNode_Operation.Lerp:
                    SetPortNames("X", "Y", "", "T"); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void SetPortNames(string x, string y, string tolerance, string extra) {
            xPort.Label = x;
            yPort.Label = y;
            tolerancePort.Label = tolerance;
            extraPort.Label = extra;
        }

        protected internal override void Deserialize(JObject data) {
            operation = (Runtime.Graph.FloatMathNode.FloatMathNode_Operation) data.Value<int>("o");
            x = data.Value<float>("x");
            y = data.Value<float>("y");
            tolerance = data.Value<float>("t");
            extra = data.Value<float>("v");
            
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

            base.Deserialize(data);
        }
    }
}