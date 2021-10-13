using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    public class SampleCollectionNode : RuntimeNode {
        private GeometryData result;

        public RuntimePort CollectionPort { get; private set; }
        public RuntimePort IndexPort { get; private set; }
        public RuntimePort SeedPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        private SampleCollectionNode_SampleType sampleType;
        private List<GeometryData> collection = new List<GeometryData>();
        private int index;
        private int seed;

        public SampleCollectionNode(string guid) : base(guid) {
            CollectionPort = RuntimePort.Create(PortType.Collection, PortDirection.Input, this);
            IndexPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            SeedPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateValue(int newValue, SampleCollectionNode_Which which) {
            switch (which) {
                case SampleCollectionNode_Which.Index: index = newValue; break;
                case SampleCollectionNode_Which.Seed: seed = newValue; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateSampleType(SampleCollectionNode_SampleType newSampleType) {
            sampleType = newSampleType;
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            
            var geometry = GetValue(CollectionPort, Enumerable.Empty<GeometryData>());
            collection = new List<GeometryData>(geometry);
            CalculateResult();
            
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == IndexPort) {
                var newValue = GetValue(connection, index);
                if (newValue != index) {
                    index = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == SeedPort) {
                var newValue = GetValue(connection, seed);
                if (newValue != seed) {
                    seed = newValue;
                    NotifyPortValueChanged(ResultPort);
                }
            } else if (port == CollectionPort) {
                var geometry = GetValue(connection, Enumerable.Empty<GeometryData>());
                collection = new List<GeometryData>(geometry);
                NotifyPortValueChanged(ResultPort);
            }
        }
        
        public override void RebindPorts() {
            CollectionPort = Ports[0];
            IndexPort = Ports[1];
            SeedPort = Ports[2];
            ResultPort = Ports[3];
        }

        private void CalculateResult() {
            if (collection == null || collection.Count == 0) {
                result = GeometryData.Empty;
                return;
            } 
            
            switch (sampleType) {
                case SampleCollectionNode_SampleType.AtIndex:
                    result = collection[index.Mod(collection.Count)];
                    break;
                case SampleCollectionNode_SampleType.Random:
                    Rand.PushState(seed);
                    result = Rand.ListItem(collection);
                    Rand.PopState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["i"] = index,
                ["s"] = seed,
                ["m"] = (int)sampleType
            };

            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;

            var data = JObject.Parse(json);
            index = data.Value<int>("i");
            seed = data.Value<int>("s");
            sampleType = (SampleCollectionNode_SampleType) data.Value<int>("m");
            
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public enum SampleCollectionNode_SampleType {AtIndex = 0, Random = 1}
        public enum SampleCollectionNode_Which {Index = 0, Seed = 1}
    }
}