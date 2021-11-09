﻿using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Curve", "Curve Info")]
    public class CurveInfoNode : AbstractNode<GeometryGraph.Runtime.Graph.CurveInfoNode> {
        private GraphFrameworkPort inputCurvePort;
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort isClosedPort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Curve Info");
            
            inputCurvePort = GraphFrameworkPort.Create("Curve", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Curve, edgeConnectorListener, this);
            pointsPort = GraphFrameworkPort.Create("Points", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Integer, edgeConnectorListener, this);
            isClosedPort = GraphFrameworkPort.Create("Is Closed", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Boolean, edgeConnectorListener, this);
            
            AddPort(inputCurvePort);
            AddPort(pointsPort);
            AddPort(isClosedPort);
        }

        public override void BindPorts() {
            BindPort(inputCurvePort, RuntimeNode.InputCurvePort);
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(isClosedPort, RuntimeNode.IsClosedPort);
        }
    }
}