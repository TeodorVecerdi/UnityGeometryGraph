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
using Which = GeometryGraph.Runtime.Graph.RotateVectorNode.RotateVectorNode_Which;
using RotationType = GeometryGraph.Runtime.Graph.RotateVectorNode.RotateVectorNode_Type;

namespace GeometryGraph.Editor {
    [Title("Vector", "Rotate")]
    public class RotateVectorNode : AbstractNode<GeometryGraph.Runtime.Graph.RotateVectorNode> {
        private GraphFrameworkPort vectorPort;
        private GraphFrameworkPort centerPort;
        private GraphFrameworkPort axisPort;
        private GraphFrameworkPort eulerAnglesPort;
        private GraphFrameworkPort anglePort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionButton<RotationType> rotationTypeButton;
        private Vector3Field vectorField;
        private Vector3Field centerField;
        private Vector3Field axisField;
        private Vector3Field eulerAnglesField;
        private FloatField angleField;

        private RotationType rotationType;
        private float3 vector;
        private float3 center;
        private float3 axis;
        private float3 eulerAngles;
        private float angle;

        private static readonly SelectionTree compareOperationTree = new SelectionTree(new List<object>(Enum.GetValues(typeof(RotationType)).Convert(o => o))) {
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
            Initialize("Rotate", EditorView.DefaultNodePosition);

            (vectorPort, vectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Vector", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateValue(vector, Which.Vector));
            (centerPort, centerField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Center", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateValue(center, Which.Center));
            (axisPort, axisField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Axis", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateValue(axis, Which.Axis));
            (eulerAnglesPort, eulerAnglesField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Euler", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateValue(eulerAngles, Which.Euler));
            (anglePort, angleField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Angle", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(angle, Which.Angle));
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Vector, edgeConnectorListener, this);

            rotationTypeButton = new EnumSelectionButton<RotationType>(rotationType, compareOperationTree);
            rotationTypeButton.RegisterCallback<ChangeEvent<RotationType>>(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change operation");
                rotationType = evt.newValue;
                RuntimeNode.UpdateType(rotationType);
                OnRotationTypeChanged();
            });

            vectorField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vector value");
                vector = evt.newValue;
                RuntimeNode.UpdateValue(vector, Which.Vector);
            });
            
            centerField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change center value");
                center = evt.newValue;
                RuntimeNode.UpdateValue(center, Which.Center);
            });
            
            axisField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change axis value");
                axis = evt.newValue;
                RuntimeNode.UpdateValue(axis, Which.Axis);
            });
            
            eulerAnglesField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change euler angles value");
                eulerAngles = evt.newValue;
                RuntimeNode.UpdateValue(eulerAngles, Which.Euler);
            });
            
            anglePort.Add(angleField);
            
            inputContainer.Add(rotationTypeButton);
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
            switch (rotationType) {
                case RotationType.AxisAngle:
                    axisPort.Show();
                    anglePort.Show();
                    eulerAnglesPort.HideAndDisconnect();
                    break;
                case RotationType.Euler:
                    eulerAnglesPort.Show();
                    axisPort.HideAndDisconnect();
                    anglePort.HideAndDisconnect();
                    break;
                case RotationType.X_Axis:
                case RotationType.Y_Axis:
                case RotationType.Z_Axis:
                    anglePort.Show();
                    axisPort.HideAndDisconnect();
                    eulerAnglesPort.HideAndDisconnect();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rotationType), rotationType, null);
            }
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["t"] = (int)rotationType;
            root["v"] = JsonConvert.SerializeObject(vector, float3Converter.Converter);
            root["c"] = JsonConvert.SerializeObject(center, float3Converter.Converter);
            root["x"] = JsonConvert.SerializeObject(axis, float3Converter.Converter);
            root["e"] = JsonConvert.SerializeObject(eulerAngles, float3Converter.Converter);
            root["a"] = angle;
            
            return root;
        }
        
        public override void SetNodeData(JObject jsonData) {
            rotationType = (RotationType)jsonData.Value<int>("t");
            vector = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("v"), float3Converter.Converter);
            center = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("c"), float3Converter.Converter);
            axis = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("x"), float3Converter.Converter);
            eulerAngles = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("e"), float3Converter.Converter);
            angle = jsonData.Value<float>("a");
            
            rotationTypeButton.SetValueWithoutNotify(rotationType, 1);
            vectorField.SetValueWithoutNotify(vector);
            centerField.SetValueWithoutNotify(center);
            axisField.SetValueWithoutNotify(axis);
            eulerAnglesField.SetValueWithoutNotify(eulerAngles);
            angleField.SetValueWithoutNotify(angle);
            
            RuntimeNode.UpdateType(rotationType);
            RuntimeNode.UpdateValue(vector, Which.Vector);
            RuntimeNode.UpdateValue(center, Which.Center);
            RuntimeNode.UpdateValue(axis, Which.Axis);
            RuntimeNode.UpdateValue(eulerAngles, Which.Euler);
            RuntimeNode.UpdateValue(angle, Which.Angle);
            
            OnRotationTypeChanged();
            
            base.SetNodeData(jsonData);
        }
    }
}