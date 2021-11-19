// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GeometryGraph Code Generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [SourceClass("GeometryGraph.Runtime::GeometryGraph.Runtime.Graph::IcospherePrimitiveNode")]
    public partial class IcospherePrimitiveNode : RuntimeNode {
        public RuntimePort RadiusPort { get; }
        public RuntimePort SubdivisionsPort { get; }
        public RuntimePort ResultPort { get; }

        public IcospherePrimitiveNode(string guid) : base(guid) {
            RadiusPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            SubdivisionsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateRadius(float newValue) {
            if(Math.Abs(Radius - newValue) < Constants.FLOAT_TOLERANCE) return;
            Radius = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateSubdivisions(int newValue) {
            if(Subdivisions == newValue) return;
            Subdivisions = newValue;
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
            if (port == RadiusPort) {
                var newValue = GetValue(connection, Radius);
                newValue = newValue.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);
                if(Math.Abs(Radius - newValue) < Constants.FLOAT_TOLERANCE) return;
                Radius = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == SubdivisionsPort) {
                var newValue = GetValue(connection, Subdivisions);
                newValue = newValue.Clamped(0, Constants.MAX_ICOSPHERE_SUBDIVISIONS);
                if(Subdivisions == newValue) return;
                Subdivisions = newValue;
                GeometryDirty = true;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            }
        }

        public override string GetCustomData() {
            return new JArray {
                Radius,
                Subdivisions,
                GeometryDirty ? 1 : 0,
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string data) {
            JArray array = JArray.Parse(data);
            Radius = array.Value<float>(0);
            Subdivisions = array.Value<int>(1);
            GeometryDirty = array.Value<int>(2) == 1 || Result == null;

            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
    }
}