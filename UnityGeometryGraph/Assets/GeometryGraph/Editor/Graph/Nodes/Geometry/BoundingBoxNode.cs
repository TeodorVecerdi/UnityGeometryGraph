﻿using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Bounding Box")]
    public class BoundingBoxNode : AbstractNode<GeometryGraph.Runtime.Graph.BoundingBoxNode> {
        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort minPort;
        private GraphFrameworkPort maxPort;
        private GraphFrameworkPort boundingBoxPort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Bounding Box");

            inputPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            
            minPort = GraphFrameworkPort.Create("Min", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);
            maxPort = GraphFrameworkPort.Create("Max", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);
            boundingBoxPort = GraphFrameworkPort.Create("Box", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);
            
            AddPort(inputPort);
            AddPort(minPort);
            AddPort(maxPort);
            AddPort(boundingBoxPort);
            
            Refresh();
        }
        
        public override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(minPort, RuntimeNode.MinPort);
            BindPort(maxPort, RuntimeNode.MaxPort);
            BindPort(boundingBoxPort, RuntimeNode.BoundingBoxPort);
        }
    }
}