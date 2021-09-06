using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphFramework.Editor.Vector {
    [Title("Vector", "Input")]
    public class VectorInputNode : AbstractNode {
        private float x;
        private float y;
        private float z;
        private Vector3 result;
        
        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort zPort;
        private GraphFrameworkPort resultPort;
        
        private FloatField xField;
        private FloatField yField;
        private FloatField zField;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            
            Initialize("Vector", EditorView.DefaultNodePosition);
            
            (xPort, xField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>(
                "X", Orientation.Horizontal, PortType.Float, edgeConnectorListener, onDisconnect: (edge, port) => {
                    x = xField.value;
                    NotifyPortValueChanged(resultPort);
                });
            (yPort, yField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>(
                "Y", Orientation.Horizontal, PortType.Float, edgeConnectorListener, onDisconnect: (edge, port) => {
                    y = yField.value;
                    NotifyPortValueChanged(resultPort);
                });
            (zPort, zField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>(
                "Z", Orientation.Horizontal, PortType.Float, edgeConnectorListener, onDisconnect: (edge, port) => {
                    z = zField.value;
                    NotifyPortValueChanged(resultPort);
                });
            
            xPort.Add(xField);
            yPort.Add(yField);
            zPort.Add(zField);

            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Vector, edgeConnectorListener);
            
            xField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Vector component (X)");
                x = evt.newValue;
                NotifyPortValueChanged(resultPort);
            });
            yField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Vector component (Y)");
                y = evt.newValue;
                NotifyPortValueChanged(resultPort);
            });
            zField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Vector component (Z)");
                z = evt.newValue;
                NotifyPortValueChanged(resultPort);
            });
            
            AddPort(xPort);
            AddPort(yPort);
            AddPort(zPort);
            AddPort(resultPort);
        }

        protected override void OnPortValueChanged(Edge edge, GraphFrameworkPort port) {
            if (port == xPort) x = GetValueFromEdge(edge, x);
            else if (port == yPort) y = GetValueFromEdge(edge, y);
            else if (port == zPort) z = GetValueFromEdge(edge, z);

            UpdateResult();
        }

        private void UpdateResult() {
            var newResult = new Vector3(x, y, z);
            if (newResult == result) return;

            result = newResult;
            NotifyPortValueChanged(resultPort);
        }

        public override object GetValueForPort(GraphFrameworkPort port) {
            if (port != resultPort) return null;
            result = new Vector3(x, y, z);
            return result;
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();
            root["x"] = x;
            root["y"] = y;
            root["z"] = z;
            root["xf"] = xField.enabledSelf ? x : xField.value;
            root["yf"] = yField.enabledSelf ? y : yField.value;
            root["zf"] = zField.enabledSelf ? z : zField.value;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            x = jsonData.Value<float>("x");
            y = jsonData.Value<float>("y");
            z = jsonData.Value<float>("z");
            xField.SetValueWithoutNotify(jsonData.Value<float>("xf"));
            yField.SetValueWithoutNotify(jsonData.Value<float>("yf"));
            zField.SetValueWithoutNotify(jsonData.Value<float>("zf"));
            
            base.SetNodeData(jsonData);
        }
    }
}