using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Curve", "Recalculate Directions")]
    public class RecalculateCurveDirectionsNode : AbstractNode<GeometryGraph.Runtime.Graph.RecalculateCurveDirectionsNode> {
        protected override string Title => "Recalculate Directions";
        protected override NodeCategory Category => NodeCategory.Curve;

        private GraphFrameworkPort curvePort;
        private GraphFrameworkPort resultPort;

        private bool flipTangents;
        private bool flipNormals;
        private bool flipBinormals;

        private BooleanToggle flipTangentsToggle;
        private BooleanToggle flipNormalsToggle;
        private BooleanToggle flipBinormalsToggle;

        protected override void CreateNode() {
            curvePort = GraphFrameworkPort.Create("Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);
            resultPort = GraphFrameworkPort.Create("Curve", Direction.Output, Port.Capacity.Multi, PortType.Curve, this);

            flipTangentsToggle = new BooleanToggle(false, "Flip Tangents (Yes)", "Flip Tangents (No)");
            flipNormalsToggle = new BooleanToggle(false, "Flip Normals (Yes)", "Flip Normals (No)");
            flipBinormalsToggle = new BooleanToggle(false, "Flip Binormals (Yes)", "Flip Binormals (No)");

            flipTangentsToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == flipTangents) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Flip Tangents");
                flipTangents = evt.newValue;
                RuntimeNode.UpdateFlipTangents(flipTangents);
            });

            flipNormalsToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == flipNormals) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Flip Normals");
                flipNormals = evt.newValue;
                RuntimeNode.UpdateFlipNormals(flipNormals);
            });

            flipBinormalsToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == flipBinormals) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Flip Binormals");
                flipBinormals = evt.newValue;
                RuntimeNode.UpdateFlipBinormals(flipBinormals);
            });

            inputContainer.Add(flipTangentsToggle);
            inputContainer.Add(flipNormalsToggle);
            inputContainer.Add(flipBinormalsToggle);
            AddPort(curvePort);
            AddPort(resultPort);
        }

        protected override void BindPorts() {
            BindPort(curvePort, RuntimeNode.CurvePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray data = new() {
                flipTangents ? 1 : 0,
                flipNormals ? 1 : 0,
                flipBinormals ? 1 : 0
            };

            root["d"] = data;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;

            flipTangents = array!.Value<int>(0) == 1;
            flipNormals = array.Value<int>(1) == 1;
            flipBinormals = array.Value<int>(2) == 1;

            flipTangentsToggle.SetValueWithoutNotify(flipTangents);
            flipNormalsToggle.SetValueWithoutNotify(flipNormals);
            flipBinormalsToggle.SetValueWithoutNotify(flipBinormals);

            RuntimeNode.UpdateFlipTangents(flipTangents);
            RuntimeNode.UpdateFlipNormals(flipNormals);
            RuntimeNode.UpdateFlipBinormals(flipBinormals);

            base.Deserialize(data);
        }
    }
}