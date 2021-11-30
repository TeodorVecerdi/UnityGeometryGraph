using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Curve", "Curve Info")]
    public class CurveInfoNode : AbstractNode<GeometryGraph.Runtime.Graph.CurveInfoNode> {
        private GraphFrameworkPort inputCurvePort;
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort isClosedPort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Curve Info", NodeCategory.Curve);
            
            inputCurvePort = GraphFrameworkPort.Create("Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);
            pointsPort = GraphFrameworkPort.Create("Points", Direction.Output, Port.Capacity.Multi, PortType.Integer, this);
            isClosedPort = GraphFrameworkPort.Create("Is Closed", Direction.Output, Port.Capacity.Multi, PortType.Boolean, this);
            
            AddPort(inputCurvePort);
            AddPort(pointsPort);
            AddPort(isClosedPort);
        }

        public override void BindPorts() {
            BindPort(inputCurvePort, RuntimeNode.CurvePort);
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(isClosedPort, RuntimeNode.IsClosedPort);
        }
    }
}