using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Curve", "Close Curve")]
    public class CloseCurveNode : AbstractNode<GeometryGraph.Runtime.Graph.CloseCurveNode> {
        protected override string Title => "Close Curve";
        protected override NodeCategory Category => NodeCategory.Curve;

        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort resultPort;
        private BooleanSelectionToggle closeToggle;

        private bool close = false;

        protected override void CreateNode() {
            inputPort = GraphFrameworkPort.Create("Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Curve, this);

            closeToggle = new BooleanSelectionToggle(close, "Close", "Open");
            closeToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == close) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Close Curve");
                close = evt.newValue;
                RuntimeNode.UpdateClose(close);
            });

            inputContainer.Add(closeToggle);
            AddPort(inputPort);
            AddPort(resultPort);
        }

        protected override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject data = base.Serialize();
            JArray array = new() {
                close ? 1 : 0
            };

            data["d"] = array;
            return data;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;

            close = array!.Value<int>(0) == 1;

            closeToggle.SetValueWithoutNotify(close);

            RuntimeNode.UpdateClose(close);

            base.Deserialize(data);
        }
    }
}