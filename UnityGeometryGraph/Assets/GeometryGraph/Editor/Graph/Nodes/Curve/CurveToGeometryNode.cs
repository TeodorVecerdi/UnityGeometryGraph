using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using WhichBooleanSetting = GeometryGraph.Runtime.Graph.CurveToGeometryNode.CurveToGeometryNode_Which;

namespace GeometryGraph.Editor {
    [Title("Curve", "Curve To Geometry")]
    public class CurveToGeometryNode : AbstractNode<GeometryGraph.Runtime.Graph.CurveToGeometryNode> {
        private GraphFrameworkPort inputCurvePort;
        private GraphFrameworkPort profileCurvePort;
        private GraphFrameworkPort rotationOffsetPort;
        private GraphFrameworkPort incrementalRotationOffsetPort;
        private GraphFrameworkPort resultPort;

        private FloatField rotationOffsetField;
        private FloatField incrementalRotationOffsetField;
        private Toggle closeCapsToggle;
        private Toggle separateMaterialForCapsToggle;
        private Toggle shadeSmoothCurveToggle;
        private Toggle shadeSmoothCapsToggle;

        private float rotationOffset;
        private float incrementalRotationOffset;
        private bool closeCaps;
        private bool separateMaterialForCaps;
        private bool shadeSmoothCurve;
        private bool shadeSmoothCaps;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Curve To Geometry");
            
            inputCurvePort = GraphFrameworkPort.Create("Curve", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Curve, edgeConnectorListener, this);
            profileCurvePort = GraphFrameworkPort.Create("Profile", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Curve, edgeConnectorListener, this, onConnect: OnProfileCurveConnected, onDisconnect: OnProfileCurveDisconnected);
            (rotationOffsetPort, rotationOffsetField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Profile Rotation", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateRotationOffset(rotationOffset));
            (incrementalRotationOffsetPort, incrementalRotationOffsetField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Incremental Rotation", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateIncrementalRotationOffset(incrementalRotationOffset));
            resultPort = GraphFrameworkPort.Create("Geometry", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            rotationOffsetField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - rotationOffset) < Constants.FLOAT_TOLERANCE) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change profile rotation");
                rotationOffset = evt.newValue;
                RuntimeNode.UpdateRotationOffset(rotationOffset);
            });
            
            incrementalRotationOffsetField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - incrementalRotationOffset) < Constants.FLOAT_TOLERANCE) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change incremental rotation");
                incrementalRotationOffset = evt.newValue;
                RuntimeNode.UpdateIncrementalRotationOffset(incrementalRotationOffset);
            });

            closeCapsToggle = new Toggle("Close Caps");
            separateMaterialForCapsToggle = new Toggle("Separate Material for Caps");
            shadeSmoothCurveToggle = new Toggle("Shade Smooth");
            shadeSmoothCapsToggle = new Toggle("Shade Smooth Caps");
            
            closeCapsToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == closeCaps) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Toggle close caps");
                closeCaps = evt.newValue;
                RuntimeNode.UpdateBooleanSetting(closeCaps, WhichBooleanSetting.CloseCaps);
            });
            
            separateMaterialForCapsToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == separateMaterialForCaps) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Toggle separate material for caps");
                separateMaterialForCaps = evt.newValue;
                RuntimeNode.UpdateBooleanSetting(separateMaterialForCaps, WhichBooleanSetting.SeparateMaterialForCaps);
            });
            
            shadeSmoothCurveToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == shadeSmoothCurve) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Toggle shade smooth curve");
                shadeSmoothCurve = evt.newValue;
                RuntimeNode.UpdateBooleanSetting(shadeSmoothCurve, WhichBooleanSetting.ShadeSmoothCurve);
            });
            
            shadeSmoothCapsToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == shadeSmoothCaps) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Toggle shade smooth caps");
                shadeSmoothCaps = evt.newValue;
                RuntimeNode.UpdateBooleanSetting(shadeSmoothCaps, WhichBooleanSetting.ShadeSmoothCaps);
            });
            
            rotationOffsetPort.Add(rotationOffsetField);
            incrementalRotationOffsetPort.Add(incrementalRotationOffsetField);
            
            OnProfileCurveDisconnected(null, null);
            
            inputContainer.Add(closeCapsToggle);
            inputContainer.Add(separateMaterialForCapsToggle);
            inputContainer.Add(shadeSmoothCurveToggle);
            inputContainer.Add(shadeSmoothCapsToggle);
            
            AddPort(inputCurvePort);
            AddPort(profileCurvePort);
            AddPort(rotationOffsetPort);
            AddPort(incrementalRotationOffsetPort);
            AddPort(resultPort);
        }

        private void OnProfileCurveConnected(Edge _1, GraphFrameworkPort _2) {
            rotationOffsetPort.Show();
            incrementalRotationOffsetPort.Show();
            
            closeCapsToggle.SetEnabled(true);
            separateMaterialForCapsToggle.SetEnabled(true);
            shadeSmoothCurveToggle.SetEnabled(true);
            shadeSmoothCapsToggle.SetEnabled(true);
        }

        private void OnProfileCurveDisconnected(Edge _1, GraphFrameworkPort _2) {
            rotationOffsetPort.HideAndDisconnect();
            incrementalRotationOffsetPort.HideAndDisconnect();
            
            closeCapsToggle.SetEnabled(false);
            separateMaterialForCapsToggle.SetEnabled(false);
            shadeSmoothCurveToggle.SetEnabled(false);
            shadeSmoothCapsToggle.SetEnabled(false);
        }

        public override void BindPorts() {
            BindPort(inputCurvePort, RuntimeNode.InputCurvePort);
            BindPort(profileCurvePort, RuntimeNode.ProfileCurvePort);
            BindPort(rotationOffsetPort, RuntimeNode.RotationOffsetPort);
            BindPort(incrementalRotationOffsetPort, RuntimeNode.IncrementalRotationOffsetPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();
            var array = new JArray {
                rotationOffset,
                incrementalRotationOffset,
                closeCaps ? 1 : 0,
                separateMaterialForCaps ? 1 : 0,
                shadeSmoothCurve ? 1 : 0,
                shadeSmoothCaps ? 1 : 0
            };
            root["d"] = array;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            var array = jsonData["d"] as JArray;
            
            rotationOffset = array!.Value<float>(0);
            incrementalRotationOffset = array.Value<float>(1);
            closeCaps = array.Value<int>(2) == 1;
            separateMaterialForCaps = array.Value<int>(3) == 1;
            shadeSmoothCurve = array.Value<int>(4) == 1;
            shadeSmoothCaps = array.Value<int>(5) == 1;
            
            rotationOffsetField.SetValueWithoutNotify(rotationOffset);
            incrementalRotationOffsetField.SetValueWithoutNotify(incrementalRotationOffset);
            closeCapsToggle.SetValueWithoutNotify(closeCaps);
            separateMaterialForCapsToggle.SetValueWithoutNotify(separateMaterialForCaps);
            shadeSmoothCurveToggle.SetValueWithoutNotify(shadeSmoothCurve);
            shadeSmoothCapsToggle.SetValueWithoutNotify(shadeSmoothCaps);

            RuntimeNode.UpdateRotationOffset(rotationOffset);
            RuntimeNode.UpdateIncrementalRotationOffset(incrementalRotationOffset);
            RuntimeNode.UpdateBooleanSetting(closeCaps, WhichBooleanSetting.CloseCaps);
            RuntimeNode.UpdateBooleanSetting(separateMaterialForCaps, WhichBooleanSetting.SeparateMaterialForCaps);
            RuntimeNode.UpdateBooleanSetting(shadeSmoothCurve, WhichBooleanSetting.ShadeSmoothCurve);
            RuntimeNode.UpdateBooleanSetting(shadeSmoothCaps, WhichBooleanSetting.ShadeSmoothCaps);
            
            base.SetNodeData(jsonData);
        }
    }
}