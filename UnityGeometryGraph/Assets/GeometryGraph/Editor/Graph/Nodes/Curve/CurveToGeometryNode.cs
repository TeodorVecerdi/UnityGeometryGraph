using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Curve", "Curve To Geometry")]
    public class CurveToGeometryNode : AbstractNode<GeometryGraph.Runtime.Graph.CurveToGeometryNode> {
        protected override string Title => "Curve To Geometry";
        protected override NodeCategory Category => NodeCategory.Curve;

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
        private EnumSelectionDropdown<CurveToGeometrySettings.CapUVType> capUVTypeDropdown;

        private static readonly SelectionTree capUVTypeTree = new(new List<object>(Enum.GetValues(typeof(CurveToGeometrySettings.CapUVType)).Convert(o => o))) {
            new SelectionCategory("Cap UV Type", false, SelectionCategory.CategorySize.Medium) {
                new("Generate cap UVs in local space (relative to the cap)", 0, false),
                new("Generate cap UVs in world space", 1, false),
                new("Same as World Space but offset to start at 0,0", 2, false),
            }
        };

        private float rotationOffset;
        private float incrementalRotationOffset;
        private bool closeCaps;
        private bool separateMaterialForCaps;
        private bool shadeSmoothCurve;
        private bool shadeSmoothCaps;
        private CurveToGeometrySettings.CapUVType capUVType = CurveToGeometrySettings.CapUVType.WorldSpace;

        protected override void CreateNode() {
            inputCurvePort = GraphFrameworkPort.Create("Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);
            profileCurvePort = GraphFrameworkPort.Create("Profile", Direction.Input, Port.Capacity.Single, PortType.Curve, this, onConnect: OnProfileCurveConnected, onDisconnect: OnProfileCurveDisconnected);
            (rotationOffsetPort, rotationOffsetField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Profile Rotation", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateRotationOffset(rotationOffset));
            (incrementalRotationOffsetPort, incrementalRotationOffsetField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Incremental Rotation", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateIncrementalRotationOffset(incrementalRotationOffset));
            resultPort = GraphFrameworkPort.Create("Geometry", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

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
            capUVTypeDropdown = new EnumSelectionDropdown<CurveToGeometrySettings.CapUVType>(capUVType, capUVTypeTree, "Cap UVs");

            closeCapsToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == closeCaps) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Toggle close caps");
                closeCaps = evt.newValue;
                RuntimeNode.UpdateCloseCaps(closeCaps);
            });

            separateMaterialForCapsToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == separateMaterialForCaps) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Toggle separate material for caps");
                separateMaterialForCaps = evt.newValue;
                RuntimeNode.UpdateSeparateMaterialForCaps(separateMaterialForCaps);
            });

            shadeSmoothCurveToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == shadeSmoothCurve) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Toggle shade smooth curve");
                shadeSmoothCurve = evt.newValue;
                RuntimeNode.UpdateShadeSmoothCurve(shadeSmoothCurve);
            });

            shadeSmoothCapsToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == shadeSmoothCaps) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Toggle shade smooth caps");
                shadeSmoothCaps = evt.newValue;
                RuntimeNode.UpdateShadeSmoothCaps(shadeSmoothCaps);
            });

            capUVTypeDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == capUVType) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change cap UV type");
                capUVType = evt.newValue;
                RuntimeNode.UpdateCapUVType(capUVType);
            });

            rotationOffsetPort.Add(rotationOffsetField);
            incrementalRotationOffsetPort.Add(incrementalRotationOffsetField);

            OnProfileCurveDisconnected(null, null);

            inputContainer.Add(closeCapsToggle);
            inputContainer.Add(separateMaterialForCapsToggle);
            inputContainer.Add(shadeSmoothCurveToggle);
            inputContainer.Add(shadeSmoothCapsToggle);
            inputContainer.Add(capUVTypeDropdown);

            AddPort(inputCurvePort);
            AddPort(profileCurvePort);
            AddPort(rotationOffsetPort);
            AddPort(incrementalRotationOffsetPort);
            AddPort(resultPort);
        }

        private void OnProfileCurveConnected(Edge _1, GraphFrameworkPort _2) {
            rotationOffsetPort.Show();
            incrementalRotationOffsetPort.Show();

            closeCapsToggle.RemoveFromClassList("d-none");
            separateMaterialForCapsToggle.RemoveFromClassList("d-none");
            shadeSmoothCurveToggle.RemoveFromClassList("d-none");
            shadeSmoothCapsToggle.RemoveFromClassList("d-none");
            capUVTypeDropdown.RemoveFromClassList("d-none");
        }

        private void OnProfileCurveDisconnected(Edge _1, GraphFrameworkPort _2) {
            rotationOffsetPort.HideAndDisconnect();
            incrementalRotationOffsetPort.HideAndDisconnect();

            closeCapsToggle.AddToClassList("d-none");
            separateMaterialForCapsToggle.AddToClassList("d-none");
            shadeSmoothCurveToggle.AddToClassList("d-none");
            shadeSmoothCapsToggle.AddToClassList("d-none");
            capUVTypeDropdown.AddToClassList("d-none");
        }

        protected override void BindPorts() {
            BindPort(inputCurvePort, RuntimeNode.SourcePort);
            BindPort(profileCurvePort, RuntimeNode.ProfilePort);
            BindPort(rotationOffsetPort, RuntimeNode.RotationOffsetPort);
            BindPort(incrementalRotationOffsetPort, RuntimeNode.IncrementalRotationOffsetPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray array = new() {
                rotationOffset,
                incrementalRotationOffset,
                closeCaps ? 1 : 0,
                separateMaterialForCaps ? 1 : 0,
                shadeSmoothCurve ? 1 : 0,
                shadeSmoothCaps ? 1 : 0,
                (int)capUVType
            };
            root["d"] = array;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;

            rotationOffset = array!.Value<float>(0);
            incrementalRotationOffset = array.Value<float>(1);
            closeCaps = array.Value<int>(2) == 1;
            separateMaterialForCaps = array.Value<int>(3) == 1;
            shadeSmoothCurve = array.Value<int>(4) == 1;
            shadeSmoothCaps = array.Value<int>(5) == 1;
            capUVType = (CurveToGeometrySettings.CapUVType) array.Value<int>(6);

            rotationOffsetField.SetValueWithoutNotify(rotationOffset);
            incrementalRotationOffsetField.SetValueWithoutNotify(incrementalRotationOffset);
            closeCapsToggle.SetValueWithoutNotify(closeCaps);
            separateMaterialForCapsToggle.SetValueWithoutNotify(separateMaterialForCaps);
            shadeSmoothCurveToggle.SetValueWithoutNotify(shadeSmoothCurve);
            shadeSmoothCapsToggle.SetValueWithoutNotify(shadeSmoothCaps);
            capUVTypeDropdown.SetValueWithoutNotify(capUVType);

            RuntimeNode.UpdateRotationOffset(rotationOffset);
            RuntimeNode.UpdateIncrementalRotationOffset(incrementalRotationOffset);
            RuntimeNode.UpdateCloseCaps(closeCaps);
            RuntimeNode.UpdateSeparateMaterialForCaps(separateMaterialForCaps);
            RuntimeNode.UpdateShadeSmoothCurve(shadeSmoothCurve);
            RuntimeNode.UpdateShadeSmoothCaps(shadeSmoothCaps);
            RuntimeNode.UpdateCapUVType(capUVType);

            base.Deserialize(data);
        }
    }
}