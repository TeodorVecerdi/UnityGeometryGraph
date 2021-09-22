using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Join Geometry")]
    public class JoinGeometryNode : AbstractNode<GeometryGraph.Runtime.Graph.JoinGeometryNode> {
        private GeometryData a = GeometryData.Empty;
        private GeometryData result;
        
        private GraphFrameworkPort aPort;
        private GraphFrameworkPort resultPort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Join Geometry", EditorView.DefaultNodePosition);

            aPort = GraphFrameworkPort.Create("Values", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener);
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener);
            
            AddPort(aPort);
            AddPort(resultPort);
            
            RefreshExpandedState();
        }
        
        public override void BindPorts() {
            BindPort(aPort, RuntimeNode.APort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override void OnPortValueChanged(Edge edge, GraphFrameworkPort port) {
            // TODO: change this to account for multiple values
            if (port == aPort) {
                a = (GeometryData)GetValueFromEdge(edge, a).Clone();
                RuntimeNode.NotifyPortValueChanged(RuntimePortDictionary[aPort]);
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
        }

        public override void SetNodeData(JObject jsonData) {
            a = GeometryData.Empty;

            base.SetNodeData(jsonData); 
        }
    }
}