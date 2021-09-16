using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public abstract class AbstractNode : Node {
        public SerializedNode Owner { get; set; }
        public Dictionary<GraphFrameworkPort, RuntimePort> RuntimePortDictionary;
        
        private string guid;
        
        public string GUID {
            get => guid;
            set {
                guid = value;
                OnNodeGuidChanged();
            }
        }
        
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

        public virtual void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            EdgeConnectorListener = edgeConnectorListener;
        }

        public void Refresh() {
            RefreshPorts();
            RefreshExpandedState();
        }

        public void SetExpandedWithoutNotify(bool value) {
            base.expanded = value;
        }
        
        public abstract object GetValueForPort(GraphFrameworkPort port);
        public abstract void BindPorts();

        protected internal virtual void OnPortValueChanged(Edge edge, GraphFrameworkPort port) {}
        protected internal virtual void OnEdgeConnected(Edge edge, GraphFrameworkPort port) {}
        protected internal virtual void OnEdgeDisconnected(Edge edge, GraphFrameworkPort port) {}
        protected virtual void OnNodeGuidChanged() { }
        public virtual void NotifyEdgeConnected(Edge edge, GraphFrameworkPort port) { }
        public virtual void NotifyEdgeDisconnected(Edge edge, GraphFrameworkPort port) { }
        public virtual void OnNodeSerialized() { }
        public virtual void OnNodeDeserialized() { }
        public virtual void SetNodeData(JObject jsonData) { }
        public virtual JObject GetNodeData() => new JObject();
        public abstract void NotifyRuntimeNodeRemoved();
    }
    
    public abstract class AbstractNode<TRuntimeNode> : AbstractNode where TRuntimeNode : RuntimeNode {
        private static readonly Type runtimeNodeType = typeof(TRuntimeNode);

        protected TRuntimeNode RuntimeNode;

        protected void Initialize(string nodeTitle, Rect nodePosition) {
            base.title = nodeTitle;
            base.SetPosition(nodePosition);
            
            var guid = Guid.NewGuid().ToString();
            if(EdgeConnectorListener != null)
                RuntimeNode = (TRuntimeNode) Activator.CreateInstance(runtimeNodeType, guid);
            GUID = guid;
            viewDataKey = guid;
            RuntimePortDictionary = new Dictionary<GraphFrameworkPort, RuntimePort>();
            
            this.AddStyleSheet("Styles/Node/Node");
            InjectCustomStyle();
        }
        
        public abstract override object GetValueForPort(GraphFrameworkPort port);
        public abstract override void BindPorts();

        public override void NotifyRuntimeNodeRemoved() {
            RuntimeNode.OnNodeRemoved();
        }

        protected void BindPort(GraphFrameworkPort graphPort, RuntimePort runtimePort) {
            RuntimePortDictionary[graphPort] = runtimePort;
            runtimePort.Guid = graphPort.GUID;
        }
        
        protected virtual void InjectCustomStyle() {
            var border = this.Q("node-border");
            var overflowStyle = border.style.overflow;
            overflowStyle.value = Overflow.Visible;
            border.style.overflow = overflowStyle;

            var selectionBorder = this.Q("selection-border");
            selectionBorder.SendToBack();
        }
        
        protected void AddPort(GraphFrameworkPort port, bool alsoAddToHierarchy = true) {
            Ports.Add(port);
            
            if(!alsoAddToHierarchy) return;
            var isInput = port.direction == Direction.Input;
            if (isInput) {
                inputContainer.Add(port);
            } else {
                outputContainer.Add(port);
            }
        }

        protected sealed override void OnNodeGuidChanged() {
            if (RuntimeNode != null) RuntimeNode.Guid = GUID;
        }

        // Only input ports
        public sealed override void NotifyEdgeConnected(Edge edge, GraphFrameworkPort port) {
            if (port.direction != Direction.Input) return;
            OnEdgeConnected(edge, port);
            OnPortValueChanged(edge, port);
        }

        // Both input & output ports
        public sealed override void NotifyEdgeDisconnected(Edge edge, GraphFrameworkPort port) {
            OnEdgeDisconnected(edge, port);
        }

        // Only output ports
        protected void NotifyPortValueChanged(GraphFrameworkPort port) {
            if (port.direction != Direction.Output) return;
            
            foreach (var edge in port.connections) {
                (edge.input.node as AbstractNode)!.OnPortValueChanged(edge, edge.input as GraphFrameworkPort);
            }
        }
        
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
    }
}