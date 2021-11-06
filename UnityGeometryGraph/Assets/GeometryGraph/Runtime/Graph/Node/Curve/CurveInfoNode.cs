using GeometryGraph.Runtime.Curve;

namespace GeometryGraph.Runtime.Graph {
    public class CurveInfoNode : RuntimeNode {
        public RuntimePort InputCurvePort { get; private set; }
        public RuntimePort PointsPort { get; private set; }
        public RuntimePort IsClosedPort { get; private set; }

        private CurveData curve;
        
        public CurveInfoNode(string guid) : base(guid) {
            InputCurvePort = RuntimePort.Create(PortType.Curve, PortDirection.Input, this);
            PointsPort = RuntimePort.Create(PortType.Integer, PortDirection.Output, this);
            IsClosedPort = RuntimePort.Create(PortType.Boolean, PortDirection.Output, this);
        }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port == InputCurvePort) {
                curve = null;
            }
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port == PointsPort) {
                return curve?.Points ?? 0;
            } 
            
            if (port == IsClosedPort) {
                return curve?.IsClosed ?? false;
            } 
         
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == InputCurvePort) {
                curve = GetValue(connection, (CurveData) null);
                NotifyPortValueChanged(PointsPort);
                NotifyPortValueChanged(IsClosedPort);
            }
        }

        public override void RebindPorts() {
            throw new System.NotImplementedException();
        }
    }
}