﻿using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Debug")]
    public class DebugNode : AbstractNode<GeometryGraph.Runtime.Graph.DebugNode> {
        private GraphFrameworkPort valuePort;
        private Label label;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Debug");
            
            valuePort = GraphFrameworkPort.Create("Value", Direction.Input, Port.Capacity.Single, PortType.Any, this);

            label = new Label("Value: [none]");
            RuntimeNode?.SetOnValueChanged(OnValueChanged);
            
            AddPort(valuePort);
            inputContainer.Add(label);
        }

        private void OnValueChanged(object obj) {
            label.text = $"Value: {obj ?? "[none]"}";
        }

        public override void BindPorts() {
            BindPort(valuePort, RuntimeNode.Port);
        }
    }
}