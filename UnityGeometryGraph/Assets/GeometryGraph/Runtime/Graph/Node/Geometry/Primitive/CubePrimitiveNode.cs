using GeometryGraph.Runtime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    public class CubePrimitiveNode : RuntimeNode {
        private float3 size = float3_util.one;

        private GeometryData result;

        public RuntimePort SizePort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public CubePrimitiveNode(string guid) : base(guid) {
            SizePort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateSize(float3 newSize) {
            size = newSize;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port != SizePort) return;
            var newSize = GetValue(connection, size);
            if (newSize.Equals(size)) return;

            size = newSize;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public override void RebindPorts() {
            SizePort = Ports[0];
            ResultPort = Ports[1];
        }

        private void CalculateResult() {
            result = Primitive.Cube(size);
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["w"] = size.x,
                ["h"] = size.y,
                ["d"] = size.z
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if (string.IsNullOrEmpty(json)) return;

            var data = JObject.Parse(json);
            size.x = data.Value<float>("w");
            size.y = data.Value<float>("h");
            size.z = data.Value<float>("d");
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
    }
}