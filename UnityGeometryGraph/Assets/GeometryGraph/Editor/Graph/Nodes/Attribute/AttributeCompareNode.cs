using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using CompareOperation = GeometryGraph.Runtime.Graph.AttributeCompareNode.AttributeCompareNode_Operation;
using CompareType = GeometryGraph.Runtime.Graph.AttributeCompareNode.AttributeCompareNode_Type;

namespace GeometryGraph.Editor {
    [Title("Attribute", "Attribute Compare")]
    public class AttributeCompareNode : AbstractNode<GeometryGraph.Runtime.Graph.AttributeCompareNode> {
        protected override string Title => "Attribute Compare";
        protected override NodeCategory Category => NodeCategory.Attribute;

        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort attributeXPort;
        private GraphFrameworkPort attributeYPort;
        private GraphFrameworkPort floatXPort;
        private GraphFrameworkPort floatYPort;
        private GraphFrameworkPort tolerancePort;
        private GraphFrameworkPort resultAttributePort;
        private GraphFrameworkPort resultPort;

        private TextField attributeXField;
        private TextField attributeYField;
        private FloatField toleranceField;
        private FloatField floatXField;
        private FloatField floatYField;
        private TextField resultAttributeField;
        private EnumSelectionDropdown<CompareOperation> operationDropdown;
        private EnumSelectionDropdown<CompareType> typeXDropdown;
        private EnumSelectionDropdown<CompareType> typeYDropdown;

        private string attributeX;
        private string attributeY;
        private string resultAttribute;
        private float tolerance = 0.01f;
        private float floatX;
        private float floatY;
        private CompareOperation operation = CompareOperation.LessThan;
        private CompareType typeX = CompareType.Attribute;
        private CompareType typeY = CompareType.Attribute;

        private static readonly SelectionTree compareOperationTree = new(new List<object>(Enum.GetValues(typeof(CompareOperation)).Convert(o => o))) {
            new SelectionCategory("Operation", false) {
                new("a < b", 0, false),
                new("a ≤ b", 1, false),
                new("a > b", 2, false),
                new("a ≥ b", 3, true),
                new("a = b, within tolerance", 4, false),
                new("a ≠ b, within tolerance", 5, false),
            }
        };

        // types are Attribute and Float
        private static readonly SelectionTree compareTypeTree = new(new List<object>(Enum.GetValues(typeof(CompareType)).Convert(o => o))) {
            new SelectionCategory("Type", false) {
                new("Uses an attribute to compare", 0, false),
                new("Uses a float value to compare", 1, false),
            }
        };

        protected override void CreateNode() {
            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            (attributeXPort, attributeXField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("X", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateAttributeX(attributeX));
            (attributeYPort, attributeYField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Y", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateAttributeY(attributeY));
            (floatXPort, floatXField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("X", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateFloatX(floatX));
            (floatYPort, floatYField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Y", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateFloatY(floatY));
            (tolerancePort, toleranceField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Tolerance", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateTolerance(tolerance));
            (resultAttributePort, resultAttributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Result", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateResultAttribute(resultAttribute));
            resultPort = GraphFrameworkPort.Create("Geometry", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            operationDropdown = new EnumSelectionDropdown<CompareOperation>(operation, compareOperationTree);
            typeXDropdown = new EnumSelectionDropdown<CompareType>(typeX, compareTypeTree, "Type X");
            typeYDropdown = new EnumSelectionDropdown<CompareType>(typeY, compareTypeTree, "Type Y");

            operationDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == operation) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change operation");
                operation = evt.newValue;
                RuntimeNode.UpdateOperation(operation);
                OnOperationChanged();
            });

            typeXDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == typeX) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change X type");
                typeX = evt.newValue;
                RuntimeNode.UpdateTypeX(typeX);
                OnTypeChanged();
            });

            typeYDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == typeY) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Y type");
                typeY = evt.newValue;
                RuntimeNode.UpdateTypeY(typeY);
                OnTypeChanged();
            });

            attributeXField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == attributeX) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change X attribute name");
                attributeX = evt.newValue;
                RuntimeNode.UpdateAttributeX(attributeX);
            });

            attributeYField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == attributeY) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Y attribute name");
                attributeY = evt.newValue;
                RuntimeNode.UpdateAttributeY(attributeY);
            });

            floatXField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - floatX) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change X float value");
                floatX = evt.newValue;
                RuntimeNode.UpdateFloatX(floatX);
            });

            floatYField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - floatY) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Y float value");
                floatY = evt.newValue;
                RuntimeNode.UpdateFloatY(floatY);
            });

            toleranceField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - tolerance) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change tolerance");
                tolerance = evt.newValue;
                RuntimeNode.UpdateTolerance(tolerance);
            });

            resultAttributeField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == resultAttribute) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change result attribute name");
                resultAttribute = evt.newValue;
                RuntimeNode.UpdateResultAttribute(resultAttribute);
            });

            operationDropdown.SetValueWithoutNotify(operation);
            typeXDropdown.SetValueWithoutNotify(typeX);
            typeYDropdown.SetValueWithoutNotify(typeY);
            toleranceField.SetValueWithoutNotify(tolerance);

            attributeXPort.Add(attributeXField);
            attributeYPort.Add(attributeYField);
            tolerancePort.Add(toleranceField);
            floatXPort.Add(floatXField);
            floatYPort.Add(floatYField);
            resultAttributePort.Add(resultAttributeField);

            inputContainer.Add(operationDropdown);
            inputContainer.Add(typeXDropdown);
            inputContainer.Add(typeYDropdown);

            AddPort(geometryPort);
            AddPort(attributeXPort);
            AddPort(attributeYPort);
            AddPort(floatXPort);
            AddPort(floatYPort);
            AddPort(tolerancePort);
            AddPort(resultAttributePort);
            AddPort(resultPort);

            OnOperationChanged();
            OnTypeChanged();
        }

        private void OnOperationChanged() {
            if (operation is CompareOperation.Equal or CompareOperation.NotEqual) {
                tolerancePort.Show();
            } else {
                tolerancePort.HideAndDisconnect();
            }
        }

        private void OnTypeChanged() {
            if (typeX == CompareType.Attribute) {
                attributeXPort.Show();
                floatXPort.HideAndDisconnect();
            } else {
                attributeXPort.HideAndDisconnect();
                floatXPort.Show();
            }

            if (typeY == CompareType.Attribute) {
                attributeYPort.Show();
                floatYPort.HideAndDisconnect();
            } else {
                attributeYPort.HideAndDisconnect();
                floatYPort.Show();
            }
        }

        protected override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(attributeXPort, RuntimeNode.AttributeXPort);
            BindPort(attributeYPort, RuntimeNode.AttributeYPort);
            BindPort(floatXPort, RuntimeNode.FloatXPort);
            BindPort(floatYPort, RuntimeNode.FloatYPort);
            BindPort(tolerancePort, RuntimeNode.TolerancePort);
            BindPort(resultAttributePort, RuntimeNode.ResultAttributePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray array = new() {
                attributeX,
                attributeY,
                floatX,
                floatY,
                tolerance,
                resultAttribute,
                (int) operation,
                (int) typeX,
                (int) typeY
            };
            root["d"] = array;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;
            attributeX = array!.Value<string>(0);
            attributeY = array.Value<string>(1);
            floatX = array.Value<float>(2);
            floatY = array.Value<float>(3);
            tolerance = array.Value<float>(4);
            resultAttribute = array.Value<string>(5);
            operation = (CompareOperation) array.Value<int>(6);
            typeX = (CompareType) array.Value<int>(7);
            typeY = (CompareType) array.Value<int>(8);

            attributeXField.SetValueWithoutNotify(attributeX);
            attributeYField.SetValueWithoutNotify(attributeY);
            floatXField.SetValueWithoutNotify(floatX);
            floatYField.SetValueWithoutNotify(floatY);
            toleranceField.SetValueWithoutNotify(tolerance);
            resultAttributeField.SetValueWithoutNotify(resultAttribute);
            operationDropdown.SetValueWithoutNotify(operation);
            typeXDropdown.SetValueWithoutNotify(typeX);
            typeYDropdown.SetValueWithoutNotify(typeY);

            RuntimeNode.UpdateAttributeX(attributeX);
            RuntimeNode.UpdateAttributeY(attributeY);
            RuntimeNode.UpdateFloatX(floatX);
            RuntimeNode.UpdateFloatY(floatY);
            RuntimeNode.UpdateTolerance(tolerance);
            RuntimeNode.UpdateResultAttribute(resultAttribute);
            RuntimeNode.UpdateOperation(operation);
            RuntimeNode.UpdateTypeX(typeX);
            RuntimeNode.UpdateTypeY(typeY);

            OnOperationChanged();
            OnTypeChanged();

            base.Deserialize(data);
        }
    }
}