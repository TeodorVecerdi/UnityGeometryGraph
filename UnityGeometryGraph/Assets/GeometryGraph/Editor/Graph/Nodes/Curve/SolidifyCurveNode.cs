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
    [Title("Curve", "Solidify Curve")]
    public class SolidifyCurveNode : AbstractNode<GeometryGraph.Runtime.Graph.SolidifyCurveNode> {
        protected override string Title => "Solidify Curve";
        protected override NodeCategory Category => NodeCategory.Curve;

        private GraphFrameworkPort curvePort;
        private GraphFrameworkPort thicknessPort;
        private GraphFrameworkPort resolutionPort;
        private GraphFrameworkPort geometryPort;

        private ClampedFloatField thicknessField;
        private ClampedIntegerField resolutionField;
        private BooleanToggle closeCapsToggle;
        private BooleanToggle separateMaterialForCapsToggle;
        private BooleanToggle shadeSmoothCurveToggle;
        private BooleanToggle shadeSmoothCapsToggle;
        private EnumSelectionDropdown<CurveToGeometrySettings.CapUVType> capUVTypeToggle;

        private float thickness = 0.1f;
        private int resolution = 4;
        private bool closeCaps;
        private bool separateMaterialForCaps;
        private bool shadeSmoothCurve;
        private bool shadeSmoothCaps;
        private CurveToGeometrySettings.CapUVType capUVType = CurveToGeometrySettings.CapUVType.WorldSpace;

        private static readonly SelectionTree capUVTypeTree = new(new List<object>(Enum.GetValues(typeof(CurveToGeometrySettings.CapUVType)).Convert(o => o))) {
            new SelectionCategory("Cap UV Type", false, SelectionCategory.CategorySize.Medium) {
                new("Generate cap UVs in local space (relative to the cap)", 0, false),
                new("Generate cap UVs in world space", 1, false),
                new("Same as World Space but offset to start at 0,0", 2, false),
            }
        };

        protected override void CreateNode() {
            curvePort = GraphFrameworkPort.Create("Curve", Direction.Input, Port.Capacity.Single, PortType.Curve , this);
            (thicknessPort, thicknessField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Thickness", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateThickness(thickness));
            (resolutionPort, resolutionField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Resolution", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateResolution(resolution));
            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            thicknessField.Min = Constants.MIN_CIRCULAR_CURVE_RADIUS;
            resolutionField.Min = Constants.MIN_CIRCLE_CURVE_RESOLUTION;
            resolutionField.Max = Constants.MAX_SOLIDIFY_CURVE_RESOLUTION;

            closeCapsToggle = new BooleanToggle(false, "Close Caps");
            separateMaterialForCapsToggle = new BooleanToggle(false, "Separate Material For Caps");
            shadeSmoothCurveToggle = new BooleanToggle(false, "Shade Smooth");
            shadeSmoothCapsToggle = new BooleanToggle(false, "Shade Smooth Caps");
            capUVTypeToggle = new EnumSelectionDropdown<CurveToGeometrySettings.CapUVType>(capUVType, capUVTypeTree, "Cap UVs");

            closeCapsToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == closeCaps) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Close Caps");
                closeCaps = evt.newValue;
                RuntimeNode.UpdateCloseCaps(closeCaps);
            });

            separateMaterialForCapsToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == separateMaterialForCaps) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Separate Material For Caps");
                separateMaterialForCaps = evt.newValue;
                RuntimeNode.UpdateSeparateMaterialForCaps(separateMaterialForCaps);
            });

            shadeSmoothCurveToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == shadeSmoothCurve) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Shade Smooth Curve");
                shadeSmoothCurve = evt.newValue;
                RuntimeNode.UpdateShadeSmoothCurve(shadeSmoothCurve);
            });

            shadeSmoothCapsToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == shadeSmoothCaps) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Shade Smooth Caps");
                shadeSmoothCaps = evt.newValue;
                RuntimeNode.UpdateShadeSmoothCaps(shadeSmoothCaps);
            });

            capUVTypeToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == capUVType) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Cap UV Type");
                capUVType = evt.newValue;
                RuntimeNode.UpdateCapUVType(capUVType);
            });

            thicknessField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - thickness) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Thickness");
                thickness = evt.newValue;
                RuntimeNode.UpdateThickness(thickness);
            });

            resolutionField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == resolution) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Resolution");
                resolution = evt.newValue;
                RuntimeNode.UpdateResolution(resolution);
            });

            thicknessField.SetValueWithoutNotify(thickness);
            resolutionField.SetValueWithoutNotify(resolution);

            thicknessPort.Add(thicknessField);
            resolutionPort.Add(resolutionField);

            inputContainer.Add(closeCapsToggle);
            inputContainer.Add(separateMaterialForCapsToggle);
            inputContainer.Add(shadeSmoothCurveToggle);
            inputContainer.Add(shadeSmoothCapsToggle);
            inputContainer.Add(capUVTypeToggle);

            AddPort(curvePort);
            AddPort(thicknessPort);
            AddPort(resolutionPort);
            AddPort(geometryPort);
        }

        protected override void BindPorts() {
            BindPort(curvePort, RuntimeNode.CurvePort);
            BindPort(thicknessPort, RuntimeNode.ThicknessPort);
            BindPort(resolutionPort, RuntimeNode.ResolutionPort);
            BindPort(geometryPort, RuntimeNode.GeometryPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray data = new() {
                thickness,
                resolution,
                closeCaps ? 1 : 0,
                separateMaterialForCaps ? 1 : 0,
                shadeSmoothCurve ? 1 : 0,
                shadeSmoothCaps ? 1 : 0,
                (int)capUVType
            };

            root["d"] = data;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;

            thickness = array!.Value<float>(0);
            resolution = array.Value<int>(1);
            closeCaps = array.Value<int>(2) == 1;
            separateMaterialForCaps = array.Value<int>(3) == 1;
            shadeSmoothCurve = array.Value<int>(4) == 1;
            shadeSmoothCaps = array.Value<int>(5) == 1;
            capUVType = (CurveToGeometrySettings.CapUVType)array.Value<int>(6);

            thicknessField.SetValueWithoutNotify(thickness);
            resolutionField.SetValueWithoutNotify(resolution);
            closeCapsToggle.SetValueWithoutNotify(closeCaps);
            separateMaterialForCapsToggle.SetValueWithoutNotify(separateMaterialForCaps);
            shadeSmoothCurveToggle.SetValueWithoutNotify(shadeSmoothCurve);
            shadeSmoothCapsToggle.SetValueWithoutNotify(shadeSmoothCaps);
            capUVTypeToggle.SetValueWithoutNotify(capUVType);

            RuntimeNode.UpdateThickness(thickness);
            RuntimeNode.UpdateResolution(resolution);
            RuntimeNode.UpdateCloseCaps(closeCaps);
            RuntimeNode.UpdateSeparateMaterialForCaps(separateMaterialForCaps);
            RuntimeNode.UpdateShadeSmoothCurve(shadeSmoothCurve);
            RuntimeNode.UpdateShadeSmoothCaps(shadeSmoothCaps);
            RuntimeNode.UpdateCapUVType(capUVType);

            base.Deserialize(data);
        }
    }
}