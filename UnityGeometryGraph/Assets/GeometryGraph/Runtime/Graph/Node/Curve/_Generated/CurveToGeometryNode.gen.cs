// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GeometryGraph Code Generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace GeometryGraph.Runtime.Graph {
    [SourceClass("GeometryGraph.Runtime::GeometryGraph.Runtime.Graph::CurveToGeometryNode")]
    public partial class CurveToGeometryNode : RuntimeNode {
        public RuntimePort SourcePort { get; }
        public RuntimePort ProfilePort { get; }
        public RuntimePort RotationOffsetPort { get; }
        public RuntimePort IncrementalRotationOffsetPort { get; }
        public RuntimePort ResultPort { get; }

        public CurveToGeometryNode(string guid) : base(guid) {
            SourcePort = RuntimePort.Create(PortType.Curve, PortDirection.Input, this);
            ProfilePort = RuntimePort.Create(PortType.Curve, PortDirection.Input, this);
            RotationOffsetPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            IncrementalRotationOffsetPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateRotationOffset(float newValue) {
            if(Math.Abs(RotationOffset - newValue) < Constants.FLOAT_TOLERANCE) return;
            RotationOffset = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateIncrementalRotationOffset(float newValue) {
            if(Math.Abs(IncrementalRotationOffset - newValue) < Constants.FLOAT_TOLERANCE) return;
            IncrementalRotationOffset = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateCloseCaps(bool newValue) {
            if(CloseCaps == newValue) return;
            CloseCaps = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateSeparateMaterialForCaps(bool newValue) {
            if(SeparateMaterialForCaps == newValue) return;
            SeparateMaterialForCaps = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateShadeSmoothCurve(bool newValue) {
            if(ShadeSmoothCurve == newValue) return;
            ShadeSmoothCurve = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateShadeSmoothCaps(bool newValue) {
            if(ShadeSmoothCaps == newValue) return;
            ShadeSmoothCaps = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateCapUVType(CurveToGeometrySettings.CapUVType newValue) {
            if(CapUVType == newValue) return;
            CapUVType = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port == ResultPort) {
                if (Result == null) CalculateResult();
                return Result;
            }
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == SourcePort) {
                Source = GetValue(connection, CurveData.Empty).Clone();
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == ProfilePort) {
                Profile = GetValue(connection, CurveData.Empty).Clone();
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == RotationOffsetPort) {
                var newValue = GetValue(connection, RotationOffset);
                if(Math.Abs(RotationOffset - newValue) < Constants.FLOAT_TOLERANCE) return;
                RotationOffset = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == IncrementalRotationOffsetPort) {
                var newValue = GetValue(connection, IncrementalRotationOffset);
                if(Math.Abs(IncrementalRotationOffset - newValue) < Constants.FLOAT_TOLERANCE) return;
                IncrementalRotationOffset = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            }
        }

        public override string GetCustomData() {
            return new JArray {
                RotationOffset,
                IncrementalRotationOffset,
                CloseCaps ? 1 : 0,
                SeparateMaterialForCaps ? 1 : 0,
                ShadeSmoothCurve ? 1 : 0,
                ShadeSmoothCaps ? 1 : 0,
                (int)CapUVType,
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string data) {
            JArray array = JArray.Parse(data);
            RotationOffset = array.Value<float>(0);
            IncrementalRotationOffset = array.Value<float>(1);
            CloseCaps = array.Value<int>(2) == 1;
            SeparateMaterialForCaps = array.Value<int>(3) == 1;
            ShadeSmoothCurve = array.Value<int>(4) == 1;
            ShadeSmoothCaps = array.Value<int>(5) == 1;
            CapUVType = (CurveToGeometrySettings.CapUVType) array.Value<int>(6);

            NotifyPortValueChanged(ResultPort);
        }
    }
}