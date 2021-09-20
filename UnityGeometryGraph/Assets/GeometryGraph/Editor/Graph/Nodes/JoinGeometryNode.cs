using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Join Geometry")]
    public class JoinGeometryNode : AbstractNode<GeometryGraph.Runtime.Graph.JoinGeometryNode> {
        private GeometryData a = GeometryData.Empty;
        private GeometryData b = GeometryData.Empty;
        private GeometryData result;
        
        private GraphFrameworkPort aPort;
        private GraphFrameworkPort bPort;
        private GraphFrameworkPort resultPort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Join Geometry", EditorView.DefaultNodePosition);


            aPort = GraphFrameworkPort.Create("A", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Geometry, edgeConnectorListener);
            bPort = GraphFrameworkPort.Create("B", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Geometry, edgeConnectorListener);
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener);
            
            AddPort(aPort);
            AddPort(bPort);
            AddPort(resultPort);
            
            RefreshExpandedState();
        }
        
        public override void BindPorts() {
            BindPort(aPort, RuntimeNode.APort);
            BindPort(bPort, RuntimeNode.BPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override void OnPortValueChanged(Edge edge, GraphFrameworkPort port) {
            if (port == aPort) {
                a = (GeometryData)GetValueFromEdge(edge, a).Clone();
                RuntimeNode.NotifyPortValueChanged(RuntimePortDictionary[aPort]);
            } else if (port == bPort) {
                b = (GeometryData)GetValueFromEdge(edge, b).Clone();
                RuntimeNode.NotifyPortValueChanged(RuntimePortDictionary[bPort]);
            }

            UpdateResult();
        }

        private void UpdateResult() {
            CalculateResult();
            NotifyPortValueChanged(resultPort);
        }

        public override object GetValueForPort(GraphFrameworkPort port) {
            if (port != resultPort) return null;

            CalculateResult();
            return result;
        }

        private void CalculateResult() {
            result = (GeometryData)a.Clone();
            result.MergeWith(b);
        }

        public override void SetNodeData(JObject jsonData) {
            a = GeometryData.Empty;
            b = GeometryData.Empty;

            base.SetNodeData(jsonData); 
        }
    }
}