using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor.Curve {
    [Title("Curve", "Curve To Geometry")]
    public class CurveToGeometryNode : AbstractNode<GeometryGraph.Runtime.Graph.CurveToGeometryNode> {
        private GraphFrameworkPort inputCurvePort;
        private GraphFrameworkPort profileCurvePort;
        private GraphFrameworkPort rotationOffsetPort;
        private GraphFrameworkPort closeCapsPort;
        private GraphFrameworkPort resultPort;

        private FloatField rotationOffsetField;
        private Toggle closeCapsToggle;

        private bool closeCaps;
        private float rotationOffset;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Curve To Geometry");
            
            inputCurvePort = GraphFrameworkPort.Create("Curve", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Curve, edgeConnectorListener, this);
            profileCurvePort = GraphFrameworkPort.Create("Profile", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Curve, edgeConnectorListener, this, onConnect: OnProfileCurveConnected, onDisconnect: OnProfileCurveDisconnected);
            (rotationOffsetPort, rotationOffsetField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Profile Rotation", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateRotationOffset(rotationOffset));
            (closeCapsPort, closeCapsToggle) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>("Close Caps", Orientation.Horizontal, PortType.Boolean, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateCloseCaps(closeCaps));
            resultPort = GraphFrameworkPort.Create("Geometry", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            rotationOffsetField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - rotationOffset) < Constants.FLOAT_TOLERANCE) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change profile rotation");
                rotationOffset = evt.newValue;
                RuntimeNode.UpdateRotationOffset(rotationOffset);
            });
            
            closeCapsToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == closeCaps) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Toggle close caps");
                closeCaps = evt.newValue;
                RuntimeNode.UpdateCloseCaps(closeCaps);
            });
            
            rotationOffsetPort.Add(rotationOffsetField);
            closeCapsPort.Add(closeCapsToggle);
            
            OnProfileCurveDisconnected(null, null);
            
            AddPort(inputCurvePort);
            AddPort(profileCurvePort);
            AddPort(rotationOffsetPort);
            AddPort(closeCapsPort);
            AddPort(resultPort);
        }

        private void OnProfileCurveConnected(Edge _1, GraphFrameworkPort _2) {
            rotationOffsetPort.Show();
            closeCapsPort.Show();
        }

        private void OnProfileCurveDisconnected(Edge _1, GraphFrameworkPort _2) {
            rotationOffsetPort.HideAndDisconnect();
            closeCapsPort.HideAndDisconnect();
        }

        public override void BindPorts() {
            BindPort(inputCurvePort, RuntimeNode.InputCurvePort);
            BindPort(profileCurvePort, RuntimeNode.ProfileCurvePort);
            BindPort(rotationOffsetPort, RuntimeNode.RotationOffsetPort);
            BindPort(closeCapsPort, RuntimeNode.CloseCapsPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();
            var array = new JArray {
                rotationOffset,
                closeCaps ? 1 : 0,
            };
            root["d"] = array;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            var array = jsonData["d"] as JArray;
            
            rotationOffset = array!.Value<float>(0);
            closeCaps = array!.Value<int>(1) == 1;
            
            rotationOffsetField.SetValueWithoutNotify(rotationOffset);
            closeCapsToggle.SetValueWithoutNotify(closeCaps);
            
            RuntimeNode.UpdateRotationOffset(rotationOffset);
            RuntimeNode.UpdateCloseCaps(closeCaps);
            
            base.SetNodeData(jsonData);
        }
    }
}