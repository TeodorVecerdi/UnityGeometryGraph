﻿using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Vector", "Split")]
    public class SplitVectorNode : AbstractNode<GeometryGraph.Runtime.Graph.SplitVectorNode> {
        
        private GraphFrameworkPort vectorPort;
        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort zPort;

        private Vector3Field vectorField;

        private float3 vector;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Split", EditorView.DefaultNodePosition);

            (vectorPort, vectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Vector", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false);
            xPort = GraphFrameworkPort.Create("X", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Float, edgeConnectorListener, this);
            yPort = GraphFrameworkPort.Create("Y", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Float, edgeConnectorListener, this);
            zPort = GraphFrameworkPort.Create("Z", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Float, edgeConnectorListener, this);

            vectorField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vector value");
                vector = evt.newValue;
                RuntimeNode.UpdateValue(vector);
            });

            
            AddPort(vectorPort);
            inputContainer.Add(vectorField);
            AddPort(xPort);
            AddPort(yPort);
            AddPort(zPort);
            
            Refresh();
        }
        
        public override void BindPorts() {
            BindPort(vectorPort, RuntimeNode.VectorPort);
            BindPort(xPort, RuntimeNode.XPort);
            BindPort(yPort, RuntimeNode.YPort);
            BindPort(zPort, RuntimeNode.ZPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();
            root["v"] = JsonConvert.SerializeObject(vector, float3Converter.Converter);
            
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            vector = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("v"), float3Converter.Converter);
            
            vectorField.SetValueWithoutNotify(vector);
            RuntimeNode.UpdateValue(vector);

            base.SetNodeData(jsonData);
        }
    }
}