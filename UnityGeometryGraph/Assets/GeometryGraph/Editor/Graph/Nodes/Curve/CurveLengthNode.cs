using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Curve", "Curve Length")]
    public class CurveLengthNode : AbstractNode<GeometryGraph.Runtime.Graph.CurveLengthNode> {
        private GraphFrameworkPort curvePort;
        private GraphFrameworkPort lengthPort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Curve Length");
            
            curvePort = GraphFrameworkPort.Create("Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);
            lengthPort = GraphFrameworkPort.Create("Length", Direction.Output, Port.Capacity.Multi, PortType.Float, this);
            
            AddPort(curvePort);
            AddPort(lengthPort);
        }

        public override void BindPorts() {
            BindPort(curvePort, RuntimeNode.CurvePort);
            BindPort(lengthPort, RuntimeNode.LengthPort);
        }
    }
}