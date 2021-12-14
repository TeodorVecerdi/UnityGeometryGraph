using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Graph {
    public class OutputNode : RuntimeNode {
        public RuntimePort GeometryPort { get; private set; }
        public RuntimePort CurvePort { get; private set; }
        public RuntimePort InstancedGeometryPort { get; private set; }

        public OutputNode(string guid) : base(guid) {
            GeometryPort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
            CurvePort = RuntimePort.Create(PortType.Curve, PortDirection.Input, this);
            InstancedGeometryPort = RuntimePort.Create(PortType.Instances, PortDirection.Input, this);
        }

        protected override object GetValueForPort(RuntimePort port) {
            return null;
        }

        internal GeometryData GetGeometryData() {
            DebugUtility.Log("Evaluating Graph");
            GeometryData value = GetValue(GeometryPort, (GeometryData)null);

            if (value == null) {
                DebugUtility.Log("Return value was null");
            }

            return value;
        }

        internal CurveData GetCurveData() {
            DebugUtility.Log("Getting curve data");
            CurveData curve = GetValue(CurvePort, (CurveData) null);
            if (curve == null) {
                DebugUtility.Log("Curve was null");
            }
            return curve;
        }

        internal InstancedGeometryData GetInstancedGeometryData() {
            DebugUtility.Log("Getting instanced geometry data");
            InstancedGeometryData instancedGeometry = GetValue(InstancedGeometryPort, (InstancedGeometryData) null);
            if (instancedGeometry == null) {
                DebugUtility.Log("Instanced geometry was null");
            }
            return instancedGeometry;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {}
    }
}