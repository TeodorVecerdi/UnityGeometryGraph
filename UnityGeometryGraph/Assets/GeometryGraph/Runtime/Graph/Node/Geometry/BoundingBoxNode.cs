using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    public class BoundingBoxNode : RuntimeNode {
        private GeometryData input = GeometryData.Empty;
        
        private GeometryData boundingBox;
        private float3 min;
        private float3 max;

        public RuntimePort InputPort { get; private set; }
        public RuntimePort MinPort { get; private set; }
        public RuntimePort MaxPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public BoundingBoxNode(string guid) : base(guid) {
            InputPort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
            MinPort = RuntimePort.Create(PortType.Vector, PortDirection.Output, this);
            MaxPort = RuntimePort.Create(PortType.Vector, PortDirection.Output, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port == MinPort) return min;
            if (port == MaxPort) return max;
            if (port == ResultPort) return boundingBox;
            return null;
        }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port != InputPort) return;
            input = GeometryData.Empty;
            min = float3.zero;
            max = float3.zero;
            boundingBox = GeometryData.Empty;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port != InputPort) return;

            input = GetValue(connection, input);
            
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
            NotifyPortValueChanged(MinPort);
            NotifyPortValueChanged(MaxPort);
        }
        
        public override void RebindPorts() {
            InputPort = Ports[0];
            MinPort = Ports[1];
            MaxPort = Ports[2];
            ResultPort = Ports[3];
        }

        private void CalculateResult() {
            (min, max, boundingBox) = Geometry.Geometry.BoundingBox(input);
        }
    }
}