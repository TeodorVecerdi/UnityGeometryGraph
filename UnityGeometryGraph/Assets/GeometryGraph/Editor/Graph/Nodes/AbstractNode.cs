using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public abstract class AbstractNode : Node {
        public SerializedNode Owner { get; set; }
        public string GUID;
        public readonly List<GraphFrameworkPort> Ports = new List<GraphFrameworkPort>();

        protected EdgeConnectorListener EdgeConnectorListener;

        public override bool expanded {
            get => base.expanded;
            set {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Expanded state changed");
                base.expanded = value;
                Owner.DrawState.Expanded = value;
            }
        }

        protected void AddPort(GraphFrameworkPort port, bool alsoAddToHierarchy = true) {
            Ports.Add(port);
            
            if(!alsoAddToHierarchy) return;
            var isInput = port.direction == Direction.Input;
            if (isInput) {
                base.inputContainer.Add(port);
            } else {
                base.outputContainer.Add(port);
            }

        }

        public virtual void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            EdgeConnectorListener = edgeConnectorListener;
        }

        protected void Initialize(string nodeTitle, Rect nodePosition) {
            base.title = nodeTitle;
            base.SetPosition(nodePosition);
            GUID = Guid.NewGuid().ToString();
            viewDataKey = GUID;
            this.AddStyleSheet("Styles/Node/Node");
            InjectCustomStyle();
        }

        // Only input ports
        public void NotifyEdgeConnected(Edge edge, GraphFrameworkPort port) {
            if (port.direction != Direction.Input) return;
            OnEdgeConnected(edge, port);
            OnPortValueChanged(edge, port);
        }

        // Both input & output ports
        public void NotifyEdgeDisconnected(Edge edge, GraphFrameworkPort port) {
            OnEdgeDisconnected(edge, port);
        }

        // Only output ports
        protected void NotifyPortValueChanged(GraphFrameworkPort port) {
            if (port.direction != Direction.Output) return;
            
            foreach (var edge in port.connections) {
                (edge.input.node as AbstractNode)!.OnPortValueChanged(edge, edge.input as GraphFrameworkPort);
            }
        }

        protected virtual void OnPortValueChanged(Edge edge, GraphFrameworkPort port) {}
        protected virtual void OnEdgeConnected(Edge edge, GraphFrameworkPort port) {}
        protected virtual void OnEdgeDisconnected(Edge edge, GraphFrameworkPort port) {}

        protected virtual void InjectCustomStyle() {
            var border = this.Q("node-border");
            var overflowStyle = border.style.overflow;
            overflowStyle.value = Overflow.Visible;
            border.style.overflow = overflowStyle;

            var selectionBorder = this.Q("selection-border");
            selectionBorder.SendToBack();
        }

        public void Refresh() {
            RefreshPorts();
            RefreshExpandedState();
        }

        public void SetExpandedWithoutNotify(bool value) {
            base.expanded = value;
        }

        public abstract object GetValueForPort(GraphFrameworkPort port);

        protected T GetValueFromEdge<T>(Edge edge, T defaultValue) {
            var outputPort = edge.output as GraphFrameworkPort;
            if (outputPort == null) return defaultValue;

            return (T)outputPort.node.GetValueForPort(outputPort);
        }

        protected T GetValue<T>(GraphFrameworkPort port, T defaultValue) {
            var firstConnection = port.connections.FirstOrDefault();
            if (firstConnection == null) return defaultValue;

            Debug.Log($"Found connection for port {port.portName}");

            var sourcePort = firstConnection.output as GraphFrameworkPort;
            return (T)sourcePort!.node!.GetValueForPort(sourcePort);
        }

        public virtual void SetNodeData(JObject jsonData) { }

        public virtual JObject GetNodeData() {
            var root = new JObject();
            return root;
        }

        public virtual void OnNodeSerialized() { }
        public virtual void OnNodeDeserialized() { }
    }
}