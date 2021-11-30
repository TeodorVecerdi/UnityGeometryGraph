using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Curve", "Reverse Curve")]
    public class ReverseCurveNode : AbstractNode<GeometryGraph.Runtime.Graph.ReverseCurveNode> {
        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort resultPort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Reverse Curve", NodeCategory.Curve);
            
            inputPort = GraphFrameworkPort.Create("Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Curve, this);
            
            AddPort(inputPort);
            AddPort(resultPort);
        }

        public override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }
    }
}