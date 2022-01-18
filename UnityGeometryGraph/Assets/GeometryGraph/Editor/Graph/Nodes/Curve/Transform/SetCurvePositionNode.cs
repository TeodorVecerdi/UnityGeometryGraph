using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using IsClosedMode = GeometryGraph.Runtime.Graph.TransformCurveNode.TransformCurveNode_IsClosedMode;

namespace GeometryGraph.Editor {
    [Title("Curve", "Set Curve Position")]
    public class SetCurvePositionNode : AbstractNode<GeometryGraph.Runtime.Graph.SetCurvePositionNode> {
        protected override string Title => "Set Curve Position";
        protected override NodeCategory Category => NodeCategory.Curve;

        private float3 position;
        private Vector3Field positionField;

        private GraphFrameworkPort inputCurvePort;
        private GraphFrameworkPort positionPort;
        private GraphFrameworkPort resultCurvePort;

        protected override void CreateNode() {
            inputCurvePort = GraphFrameworkPort.Create("Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);
            (positionPort, positionField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "Position", PortType.Vector, this, showLabelOnField: false,
                onDisconnect: (_, _) => RuntimeNode.UpdatePosition(position)
            );
            resultCurvePort = GraphFrameworkPort.Create("Curve", Direction.Output, Port.Capacity.Multi, PortType.Curve, this);

            positionField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change curve position");
                position = evt.newValue;
                RuntimeNode.UpdatePosition(position);
            });

            AddPort(inputCurvePort);
            AddPort(positionPort);
            inputContainer.Add(positionField);
            AddPort(resultCurvePort);
        }


        protected override void BindPorts() {
            BindPort(inputCurvePort, RuntimeNode.InputPort);
            BindPort(positionPort, RuntimeNode.PositionPort);
            BindPort(resultCurvePort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray array = new() {
                JsonConvert.SerializeObject(position, float3Converter.Converter),
            };
            root["d"] = array;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;

            position = JsonConvert.DeserializeObject<float3>(array!.Value<string>(0), float3Converter.Converter);
            positionField.SetValueWithoutNotify(position);
            RuntimeNode.UpdatePosition(position);

            base.Deserialize(data);
        }
    }
}