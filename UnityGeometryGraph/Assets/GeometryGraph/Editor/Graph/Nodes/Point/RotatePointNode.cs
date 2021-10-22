using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.RotatePointNode.RotatePointNode_Which;
using RotationMode = GeometryGraph.Runtime.Graph.RotatePointNode.RotatePointNode_RotationMode;
using AxisMode = GeometryGraph.Runtime.Graph.RotatePointNode.RotatePointNode_AxisMode;
using AngleMode = GeometryGraph.Runtime.Graph.RotatePointNode.RotatePointNode_AngleMode;
using RotationType = GeometryGraph.Runtime.Graph.RotatePointNode.RotatePointNode_RotationType;

namespace GeometryGraph.Editor {
    [Title("Point", "Rotate Points")]
    public class RotatePointNode : AbstractNode<GeometryGraph.Runtime.Graph.RotatePointNode> {
        
        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort rotationPort;
        private GraphFrameworkPort axisPort;
        private GraphFrameworkPort anglePort;
        private GraphFrameworkPort rotationAttributePort;
        private GraphFrameworkPort axisAttributePort;
        private GraphFrameworkPort angleAttributePort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionDropdown<RotationMode> rotationModeDropdown;
        private EnumSelectionDropdown<AxisMode> axisModeDropdown;
        private EnumSelectionDropdown<AngleMode> angleModeDropdown;
        private EnumSelectionToggle<RotationType> rotationTypeToggle;
        
        private Vector3Field rotationField;
        private Vector3Field axisField;
        private FloatField angleField;
        private TextField rotationAttributeField;
        private TextField axisAttributeField;
        private TextField angleAttributeField;

        private float3 rotation;
        private string rotationAttribute;
        private float3 axis;
        private string axisAttribute;
        private float angle;
        private string angleAttribute;
        
        private RotationMode rotationMode;
        private AxisMode axisMode;
        private AngleMode angleMode;
        private RotationType rotationType = RotationType.Euler;

        private static readonly SelectionTree rotationTree = new SelectionTree(new List<object>(Enum.GetValues(typeof(RotationMode)).Convert(o => o))) {
            new SelectionCategory("Rotation Type", false, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("Rotate each point using a vector", 0, false),
                new SelectionEntry("Rotate each point using an attribute", 1, false)
            }
        };
        
        private static readonly SelectionTree axisTree = new SelectionTree(new List<object>(Enum.GetValues(typeof(AxisMode)).Convert(o => o))) {
            new SelectionCategory("Axis Type", false, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("Use a vector as the rotation axis", 0, false),
                new SelectionEntry("Use an attribute as the rotation axis", 1, false)
            }
        };

        private static readonly SelectionTree angleTree = new SelectionTree(new List<object>(Enum.GetValues(typeof(AngleMode)).Convert(o => o))) {
            new SelectionCategory("Angle Type", false, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("Rotate each point using a float value", 0, false),
                new SelectionEntry("Rotate each point using an attribute", 1, false)
            }
        };

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Rotate Points");

            inputPort = GraphFrameworkPort.Create("Geometry", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Geometry, edgeConnectorListener, this);
            (rotationPort, rotationField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Rotation", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateValue(rotation, Which.RotationVector));
            (axisPort, axisField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Axis", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateValue(axis, Which.AxisVector));
            (anglePort, angleField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Angle", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(angle, Which.AngleFloat));
            (rotationAttributePort, rotationAttributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Rotation", Orientation.Horizontal, PortType.String, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(rotationAttribute, Which.RotationAttribute));
            (axisAttributePort, axisAttributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Axis", Orientation.Horizontal, PortType.String, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(axisAttribute, Which.AxisAttribute));
            (angleAttributePort, angleAttributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Angle", Orientation.Horizontal, PortType.String, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(angleAttribute, Which.AngleAttribute));
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            rotationModeDropdown = new EnumSelectionDropdown<RotationMode>(rotationMode, rotationTree, "Rotation");
            rotationModeDropdown.RegisterCallback<ChangeEvent<RotationMode>>(evt => {
                if (evt.newValue == rotationMode) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change rotation type");
                rotationMode = evt.newValue;
                RuntimeNode.UpdateRotationMode(rotationMode);
                OnModeChanged();
            });
            axisModeDropdown = new EnumSelectionDropdown<AxisMode>(axisMode, axisTree, "Axis");
            axisModeDropdown.RegisterCallback<ChangeEvent<AxisMode>>(evt => {
                if (evt.newValue == axisMode) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change axis type");
                axisMode = evt.newValue;
                RuntimeNode.UpdateAxisMode(axisMode);
                OnModeChanged();
            });
            angleModeDropdown = new EnumSelectionDropdown<AngleMode>(angleMode, angleTree, "Angle");
            angleModeDropdown.RegisterCallback<ChangeEvent<AngleMode>>(evt => {
                if (evt.newValue == angleMode) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change angle type");
                angleMode = evt.newValue;
                RuntimeNode.UpdateAngleMode(angleMode);
                OnModeChanged();
            });

            rotationTypeToggle = new EnumSelectionToggle<RotationType>(rotationType);
            rotationTypeToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == rotationType) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change rotation mode");
                rotationType = evt.newValue;
                RuntimeNode.UpdateRotationType(rotationType);
                OnModeChanged();
            });
            
            rotationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change rotation vector");
                rotation = evt.newValue;
                RuntimeNode.UpdateValue(rotation, Which.RotationVector);
            });
            axisField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change axis vector");
                axis = evt.newValue;
                RuntimeNode.UpdateValue(axis, Which.AxisVector);
            });
            
            angleField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change angle value");
                angle = evt.newValue;
                RuntimeNode.UpdateValue(angle, Which.AngleFloat);
            });
            
            rotationAttributeField.RegisterValueChangedCallback(evt => {
                if (string.Equals(evt.newValue, rotationAttribute, StringComparison.InvariantCulture)) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Rotation Attribute name");
                rotationAttribute = evt.newValue;
                RuntimeNode.UpdateValue(rotationAttribute, Which.RotationAttribute);
            });

            axisAttributeField.RegisterValueChangedCallback(evt => {
                if (string.Equals(evt.newValue, axisAttribute, StringComparison.InvariantCulture)) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Axis Attribute name");
                axisAttribute = evt.newValue;
                RuntimeNode.UpdateValue(axisAttribute, Which.AxisAttribute);
            });

            angleAttributeField.RegisterValueChangedCallback(evt => {
                if (string.Equals(evt.newValue, angleAttribute, StringComparison.InvariantCulture)) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Angle Attribute name");
                angleAttribute = evt.newValue;
                RuntimeNode.UpdateValue(angleAttribute, Which.AngleAttribute);
            });

            anglePort.Add(angleField);
            rotationAttributePort.Add(rotationAttributeField);
            axisAttributePort.Add(axisAttributeField);
            angleAttributePort.Add(angleAttributeField);

            inputContainer.Add(rotationTypeToggle);
            inputContainer.Add(rotationModeDropdown);
            inputContainer.Add(axisModeDropdown);
            inputContainer.Add(angleModeDropdown);
            AddPort(inputPort);
            
            AddPort(rotationPort);
            inputContainer.Add(rotationField);
            AddPort(rotationAttributePort);

            AddPort(axisPort);
            inputContainer.Add(axisField);
            AddPort(axisAttributePort);
            
            AddPort(anglePort);
            AddPort(angleAttributePort);
            
            AddPort(resultPort);

            OnModeChanged();
            
            Refresh();
        }

        private void OnModeChanged() {
            if (rotationType == RotationType.Euler) {
                // Disconnect / hide all from AxisAngle
                axisPort.HideAndDisconnect();
                axisAttributePort.HideAndDisconnect();
                anglePort.HideAndDisconnect();
                angleAttributePort.HideAndDisconnect();
                axisModeDropdown.AddToClassList("d-none");
                angleModeDropdown.AddToClassList("d-none");
                
                rotationModeDropdown.RemoveFromClassList("d-none");
                if (rotationMode == RotationMode.Vector) {
                    rotationPort.Show();
                    rotationAttributePort.HideAndDisconnect();
                } else {
                    rotationPort.HideAndDisconnect();
                    rotationAttributePort.Show();
                }
            } else {
                // Disconnect / hide all from Euler
                rotationPort.HideAndDisconnect();
                rotationAttributePort.HideAndDisconnect();
                rotationModeDropdown.AddToClassList("d-none");
                
                axisModeDropdown.RemoveFromClassList("d-none");
                angleModeDropdown.RemoveFromClassList("d-none");
                if (axisMode == AxisMode.Vector) {
                    axisPort.Show();
                    axisAttributePort.HideAndDisconnect();
                } else {
                    axisPort.HideAndDisconnect();
                    axisAttributePort.Show();
                }
                
                if (angleMode == AngleMode.Float) {
                    anglePort.Show();
                    angleAttributePort.HideAndDisconnect();
                } else {
                    anglePort.HideAndDisconnect();
                    angleAttributePort.Show();
                }
            }
        }
        
        public override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(rotationPort, RuntimeNode.RotationPort);
            BindPort(axisPort, RuntimeNode.AxisPort);
            BindPort(anglePort, RuntimeNode.AnglePort);
            BindPort(rotationAttributePort, RuntimeNode.RotationAttributePort);
            BindPort(axisAttributePort, RuntimeNode.AxisAttributePort);
            BindPort(angleAttributePort, RuntimeNode.AngleAttributePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["0"] = JsonConvert.SerializeObject(rotation, Formatting.None, float3Converter.Converter);
            root["1"] = JsonConvert.SerializeObject(axis, Formatting.None, float3Converter.Converter);
            root["2"] = angle;
            root["3"] = rotationAttribute;
            root["4"] = axisAttribute;
            root["5"] = angleAttribute;
            root["6"] = (int)rotationMode;
            root["7"] = (int)axisMode;
            root["8"] = (int)angleMode;
            root["9"] = (int)rotationType;
            
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            rotation = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("0")!, float3Converter.Converter);
            axis = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("1")!, float3Converter.Converter);
            angle = jsonData.Value<float>("2");
            rotationAttribute = jsonData.Value<string>("3");
            axisAttribute = jsonData.Value<string>("4");
            angleAttribute = jsonData.Value<string>("5");
            rotationMode = (RotationMode) jsonData.Value<int>("6");
            axisMode = (AxisMode) jsonData.Value<int>("7");
            angleMode = (AngleMode) jsonData.Value<int>("8");
            rotationType = (RotationType) jsonData.Value<int>("9");
            
            rotationField.SetValueWithoutNotify(rotation);
            axisField.SetValueWithoutNotify(axis);
            angleField.SetValueWithoutNotify(angle);
            rotationAttributeField.SetValueWithoutNotify(rotationAttribute);
            axisAttributeField.SetValueWithoutNotify(axisAttribute);
            angleAttributeField.SetValueWithoutNotify(angleAttribute);
            rotationModeDropdown.SetValueWithoutNotify(rotationMode);
            axisModeDropdown.SetValueWithoutNotify(axisMode);
            angleModeDropdown.SetValueWithoutNotify(angleMode);
            rotationTypeToggle.SetValueWithoutNotify(rotationType);
            
            RuntimeNode.UpdateValue(rotation, Which.RotationVector);
            RuntimeNode.UpdateValue(axis, Which.AxisVector);
            RuntimeNode.UpdateValue(angle, Which.AngleFloat);
            RuntimeNode.UpdateValue(rotationAttribute, Which.RotationAttribute);
            RuntimeNode.UpdateValue(axisAttribute, Which.AxisAttribute);
            RuntimeNode.UpdateValue(angleAttribute, Which.AngleAttribute);
            RuntimeNode.UpdateRotationMode(rotationMode);
            RuntimeNode.UpdateAxisMode(axisMode);
            RuntimeNode.UpdateAngleMode(angleMode);
            RuntimeNode.UpdateRotationType(rotationType);

            OnModeChanged();

            base.SetNodeData(jsonData);
        }
    }
}