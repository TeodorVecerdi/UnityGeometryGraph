using System;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.Graph {
    public class CurveToGeometryNode : RuntimeNode {
        private CurveData source;
        private CurveData profile;
        private float rotationOffset;
        private float incrementalRotationOffset;
        private GeometryData result;

        private bool closeCaps;
        private bool separateMaterialForCaps;
        private bool shadeSmoothCurve;
        private bool shadeSmoothCaps;
        private CurveToGeometrySettings.CapUVType capUVType = CurveToGeometrySettings.CapUVType.WorldSpace;

        public RuntimePort InputCurvePort { get; private set; }
        public RuntimePort ProfileCurvePort { get; private set; }
        public RuntimePort RotationOffsetPort { get; private set; }
        public RuntimePort IncrementalRotationOffsetPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public CurveToGeometryNode(string guid) : base(guid) {
            InputCurvePort = RuntimePort.Create(PortType.Curve, PortDirection.Input, this);
            ProfileCurvePort = RuntimePort.Create(PortType.Curve, PortDirection.Input, this);
            RotationOffsetPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            IncrementalRotationOffsetPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateRotationOffset(float newValue) {
            if (Math.Abs(newValue - rotationOffset) < Constants.FLOAT_TOLERANCE) return;
            rotationOffset = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateIncrementalRotationOffset(float newValue) {
            if (Math.Abs(newValue - incrementalRotationOffset) < Constants.FLOAT_TOLERANCE) return;
            incrementalRotationOffset = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateBooleanSetting(bool newValue, CurveToGeometryNode_Which which) {
            switch (which) {
                case CurveToGeometryNode_Which.CloseCaps:
                    if (closeCaps == newValue) return;
                    closeCaps = newValue;
                    break;
                case CurveToGeometryNode_Which.SeparateMaterialForCaps:
                    if(separateMaterialForCaps == newValue) return;
                    separateMaterialForCaps = newValue;
                    break;
                case CurveToGeometryNode_Which.ShadeSmoothCurve:
                    if (shadeSmoothCurve == newValue) return;
                    shadeSmoothCurve = newValue;
                    break;
                case CurveToGeometryNode_Which.ShadeSmoothCaps:
                    if (shadeSmoothCaps == newValue) return;
                    shadeSmoothCaps = newValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }
            
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
        
        public void UpdateCapUVType(CurveToGeometrySettings.CapUVType newValue) {
            if (capUVType == newValue) return;
            capUVType = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port == InputCurvePort) {
                source = null;
                result = GeometryData.Empty;
            } else if (port == ProfileCurvePort) {
                profile = null;
            }
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            if (result == null) {
                CalculateResult();
            }
            
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == InputCurvePort) {
                source = GetValue(connection, CurveData.Empty).Clone();
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == ProfileCurvePort) {
                profile = GetValue(connection, CurveData.Empty).Clone();
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == RotationOffsetPort) {
                var newValue = GetValue(connection, rotationOffset);
                if (Math.Abs(newValue - rotationOffset) < Constants.FLOAT_TOLERANCE) return;
                rotationOffset = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == IncrementalRotationOffsetPort) {
                var newValue = GetValue(connection, incrementalRotationOffset);
                if (Math.Abs(newValue - incrementalRotationOffset) < Constants.FLOAT_TOLERANCE) return;
                incrementalRotationOffset = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            }
        }

        private void CalculateResult() {
            if (source is not { Type: not CurveType.None }) {
                DebugUtility.Log("Source curve was null");
                result = GeometryData.Empty;
                return;
            }

            if (profile is not { Type: not CurveType.None }) {
                DebugUtility.Log("Profile curve was null");
                result = CurveToGeometry.WithoutProfile(source);
                return;
            }

            
            if (RuntimeGraphObjectData.IsDuringSerialization) {
                DebugUtility.Log("Attempting to generate geometry from curve during serialization. Aborting.");
                result = null;
                return;
            }
            DebugUtility.Log("Generated mesh with profile");
            
            result = CurveToGeometry.WithProfile(source, profile, new CurveToGeometrySettings(closeCaps, separateMaterialForCaps, shadeSmoothCurve, shadeSmoothCaps, rotationOffset, incrementalRotationOffset, capUVType));
        }

        public override string GetCustomData() {
            return new JArray {
                rotationOffset,
                incrementalRotationOffset,
                closeCaps ? 1 : 0,
                separateMaterialForCaps ? 1 : 0,
                shadeSmoothCurve ? 1 : 0,
                shadeSmoothCaps ? 1 : 0,
                (int)capUVType
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            JArray array = JArray.Parse(json);
            rotationOffset = array.Value<float>(0);
            incrementalRotationOffset = array.Value<float>(1);
            closeCaps = array.Value<int>(2) == 1;
            separateMaterialForCaps = array.Value<int>(3) == 1;
            shadeSmoothCurve = array.Value<int>(4) == 1;
            shadeSmoothCaps = array.Value<int>(5) == 1;
            capUVType = (CurveToGeometrySettings.CapUVType)array.Value<int>(6);
            
            NotifyPortValueChanged(ResultPort);
        }

        public enum CurveToGeometryNode_Which {
            CloseCaps = 0,
            SeparateMaterialForCaps = 1,
            ShadeSmoothCurve = 2,
            ShadeSmoothCaps = 3,
        }
    }
}
