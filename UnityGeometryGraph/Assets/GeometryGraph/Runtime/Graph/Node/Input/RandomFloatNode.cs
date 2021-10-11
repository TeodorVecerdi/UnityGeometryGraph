using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    public class RandomFloatNode : RuntimeNode {
        private int seed;

        public RuntimePort ValuePort { get; private set; }
        public RuntimePort SeedPort { get; private set; }

        public RandomFloatNode(string guid) : base(guid) {
            SeedPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ValuePort = RuntimePort.Create(PortType.Float, PortDirection.Output, this);
        }

        public void UpdateSeed(int newSeed) {
            seed = newSeed;
            NotifyPortValueChanged(ValuePort);
        }

        public override object GetValueForPort(RuntimePort port) {
            return port == ValuePort ? Rand.FloatSeeded(seed) : 0.0f;
        }

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ValuePort) yield break;
            if (count < 0) yield break;
            Rand.PushState(seed);
            for (var i = 0; i < count; i++) {
                yield return Rand.Float;
            }
            Rand.PopState();
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == SeedPort) {
                var newSeed = GetValue(SeedPort, seed);
                if (newSeed != seed) {
                    seed = newSeed;
                    NotifyPortValueChanged(ValuePort);
                }
            }
        }
        
        public override void RebindPorts() {
            SeedPort = Ports[0];
            ValuePort = Ports[1];
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["s"] = seed
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            var data = JObject.Parse(json);
            seed = data.Value<int>("s");
            NotifyPortValueChanged(ValuePort);
        }
    }
}