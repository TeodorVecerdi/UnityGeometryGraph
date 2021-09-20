using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Graph {
    public class JoinGeometryNode : RuntimeNode {
        private GeometryData result;

        public RuntimePort APort { get; private set; }
        public RuntimePort BPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public JoinGeometryNode(string guid) : base(guid) {
            APort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
            BPort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            CalculateResult();
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            NotifyPortValueChanged(ResultPort);
        }
        
        public override void RebindPorts() {
            APort = Ports[0];
            BPort = Ports[1];
            ResultPort = Ports[2];
        }

        private void CalculateResult() {
            result = (GeometryData)GetValue(APort, GeometryData.Empty).Clone();
            result.MergeWith(GetValue(BPort, GeometryData.Empty));
        }
    }
}