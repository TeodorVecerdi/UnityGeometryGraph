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

namespace GeometryGraph.Editor {
    [Title("Vector", "Rotate")]
    public class RotateVectorNode : AbstractNode<GeometryGraph.Runtime.Graph.RotateVectorNode> {
        private GraphFrameworkPort vectorPort;
        private GraphFrameworkPort centerPort;
        private GraphFrameworkPort axisPort;
        private GraphFrameworkPort eulerAnglesPort;
        private GraphFrameworkPort anglePort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionDropdown<Runtime.Graph.RotateVectorNode.RotateVectorNode_Mode> rotationTypeDropdown;
        private Vector3Field vectorField;
        private Vector3Field centerField;
        private Vector3Field axisField;
        private Vector3Field eulerAnglesField;
        private FloatField angleField;

        private Runtime.Graph.RotateVectorNode.RotateVectorNode_Mode rotationMode;
        private float3 vector;
        private float3 center;
        private float3 axis;
        private float3 eulerAngles;
        private float angle;

        private static readonly SelectionTree compareOperationTree = new SelectionTree(new List<object>(Enum.GetValues(typeof(Runtime.Graph.RotateVectorNode.RotateVectorNode_Mode)).Convert(o => o))) {
            new SelectionCategory("Rotation Type", false, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("", 0, false),
                new SelectionEntry("", 1, true),
                new SelectionEntry("", 2, false),
                new SelectionEntry("", 3, false),
                new SelectionEntry("", 4, false),
            }
        };

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Rotate");

            (vectorPort, vectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Vector", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateVector(vector));
            (centerPort, centerField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Center", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateCenter(center));
            (axisPort, axisField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Axis", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateAxis(axis));
            (eulerAnglesPort, eulerAnglesField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Euler", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateEulerAngles(eulerAngles));
            (anglePort, angleField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Angle", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateAngle(angle));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);

            rotationTypeDropdown = new EnumSelectionDropdown<Runtime.Graph.RotateVectorNode.RotateVectorNode_Mode>(rotationMode, compareOperationTree);
            rotationTypeDropdown.RegisterCallback<ChangeEvent<Runtime.Graph.RotateVectorNode.RotateVectorNode_Mode>>(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change operation");
                rotationMode = evt.newValue;
                RuntimeNode.UpdateMode(rotationMode);
                OnRotationTypeChanged();
            });

            vectorField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vector value");
                vector = evt.newValue;
                RuntimeNode.UpdateVector(vector);
            });
            
            centerField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change center value");
                center = evt.newValue;
                RuntimeNode.UpdateCenter(center);
            });
            
            axisField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change axis value");
                axis = evt.newValue;
                RuntimeNode.UpdateAxis(axis);
            });
            
            eulerAnglesField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change euler angles value");
                eulerAngles = evt.newValue;
                RuntimeNode.UpdateEulerAngles(eulerAngles);
            });
            
            anglePort.Add(angleField);
            
            inputContainer.Add(rotationTypeDropdown);
            AddPort(vectorPort);
            inputContainer.Add(vectorField);
            AddPort(centerPort);
            inputContainer.Add(centerField);
            AddPort(axisPort);
            inputContainer.Add(axisField);
            AddPort(eulerAnglesPort);
            inputContainer.Add(eulerAnglesField);
            AddPort(anglePort);
            AddPort(resultPort);
            
            OnRotationTypeChanged();

            Refresh();
        }

        public override void BindPorts() {
            BindPort(vectorPort, RuntimeNode.VectorPort);
            BindPort(centerPort, RuntimeNode.CenterPort);
            BindPort(axisPort, RuntimeNode.AxisPort);
            BindPort(eulerAnglesPort, RuntimeNode.EulerAnglesPort);
            BindPort(anglePort, RuntimeNode.AnglePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        private void OnRotationTypeChanged() {
            switch (rotationMode) {
                case GeometryGraph.Runtime.Graph.RotateVectorNode.RotateVectorNode_Mode.AxisAngle:
                    axisPort.Show();
                    anglePort.Show();
                    eulerAnglesPort.HideAndDisconnect();
                    break;
                case GeometryGraph.Runtime.Graph.RotateVectorNode.RotateVectorNode_Mode.Euler:
                    eulerAnglesPort.Show();
                    axisPort.HideAndDisconnect();
                    anglePort.HideAndDisconnect();
                    break;
                case GeometryGraph.Runtime.Graph.RotateVectorNode.RotateVectorNode_Mode.X_Axis:
                case GeometryGraph.Runtime.Graph.RotateVectorNode.RotateVectorNode_Mode.Y_Axis:
                case GeometryGraph.Runtime.Graph.RotateVectorNode.RotateVectorNode_Mode.Z_Axis:
                    anglePort.Show();
                    axisPort.HideAndDisconnect();
                    eulerAnglesPort.HideAndDisconnect();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rotationMode), rotationMode, null);
            }
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["t"] = (int)rotationMode;
            root["v"] = JsonConvert.SerializeObject(vector, float3Converter.Converter);
            root["c"] = JsonConvert.SerializeObject(center, float3Converter.Converter);
            root["x"] = JsonConvert.SerializeObject(axis, float3Converter.Converter);
            root["e"] = JsonConvert.SerializeObject(eulerAngles, float3Converter.Converter);
            root["a"] = angle;
            
            return root;
        }
        
        public override void SetNodeData(JObject jsonData) {
            rotationMode = (Runtime.Graph.RotateVectorNode.RotateVectorNode_Mode)jsonData.Value<int>("t");
            vector = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("v")!, float3Converter.Converter);
            center = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("c")!, float3Converter.Converter);
            axis = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("x")!, float3Converter.Converter);
            eulerAngles = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("e")!, float3Converter.Converter);
            angle = jsonData.Value<float>("a");
            
            rotationTypeDropdown.SetValueWithoutNotify(rotationMode, 1);
            vectorField.SetValueWithoutNotify(vector);
            centerField.SetValueWithoutNotify(center);
            axisField.SetValueWithoutNotify(axis);
            eulerAnglesField.SetValueWithoutNotify(eulerAngles);
            angleField.SetValueWithoutNotify(angle);
            
            RuntimeNode.UpdateMode(rotationMode);
            RuntimeNode.UpdateVector(vector);
            RuntimeNode.UpdateCenter(center);
            RuntimeNode.UpdateAxis(axis);
            RuntimeNode.UpdateEulerAngles(eulerAngles);
            RuntimeNode.UpdateAngle(angle);
            
            OnRotationTypeChanged();
            
            base.SetNodeData(jsonData);
        }
    }
}