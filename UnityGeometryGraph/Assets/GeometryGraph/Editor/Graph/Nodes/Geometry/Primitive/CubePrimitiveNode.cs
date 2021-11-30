using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Primitive", "Cube")]
    public class CubePrimitiveNode : AbstractNode<GeometryGraph.Runtime.Graph.CubePrimitiveNode> {
        private GraphFrameworkPort sizePort;
        private GraphFrameworkPort resultPort;

        private Vector3Field sizeField;

        private float3 size = float3_ext.one;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Cube Primitive", NodeCategory.Geometry);

            (sizePort, sizeField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Size", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateSize(size));
            resultPort = GraphFrameworkPort.Create("Cube", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            sizeField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                size = evt.newValue;
                RuntimeNode.UpdateSize(size);
            });

            sizeField.SetValueWithoutNotify(size);

            AddPort(sizePort);
            inputContainer.Add(sizeField);
            AddPort(resultPort);

            Refresh();
        }

        public override void BindPorts() {
            BindPort(sizePort, RuntimeNode.SizePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            JObject root = base.GetNodeData();

            root["w"] = size.x;
            root["h"] = size.y;
            root["d"] = size.z;

            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            size.x = jsonData.Value<float>("w");
            size.y = jsonData.Value<float>("h");
            size.z = jsonData.Value<float>("d");

            sizeField.SetValueWithoutNotify(size);
            RuntimeNode.UpdateSize(size);
            
            base.SetNodeData(jsonData);
        }
    }
}