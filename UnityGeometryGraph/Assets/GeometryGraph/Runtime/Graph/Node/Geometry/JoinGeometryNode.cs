using System.Linq;
using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Graph {
    public class JoinGeometryNode : RuntimeNode {
        private GeometryData result;

        public RuntimePort APort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public JoinGeometryNode(string guid) : base(guid) {
            APort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            CalculateResult();
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            NotifyPortValueChanged(ResultPort);
        }
        
        public override void RebindPorts() {
            APort = Ports[0];
            ResultPort = Ports[1];
        }

        private void CalculateResult() {
            var values = GetValues(APort, GeometryData.Empty).ToList();
            result = GeometryData.Empty;
            foreach (var geometryData in values) {
                result.MergeWith(geometryData);
            }
        }
    }
}