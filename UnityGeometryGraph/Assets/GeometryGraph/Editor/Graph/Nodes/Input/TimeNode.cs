using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Input", "Time")]
    public class TimeNode : AbstractNode<GeometryGraph.Runtime.Graph.TimeNode> {
        protected override string Title => "Time";
        protected override NodeCategory Category => NodeCategory.Input;

        private GraphFrameworkPort smoothDeltaTimePort;
        private GraphFrameworkPort timeScalePort;
        private GraphFrameworkPort realtimeSinceStartupPort;
        private GraphFrameworkPort timeSinceLevelLoadPort;
        private GraphFrameworkPort timePort;
        private GraphFrameworkPort deltaTimePort;

        private Toggle isFixedField;
        private Toggle isUnscaledField;

        private bool isFixed;
        private bool isUnscaled;

        protected override void CreateNode() {
            smoothDeltaTimePort = GraphFrameworkPort.Create("Smooth Delta Time", Direction.Output, Port.Capacity.Multi, PortType.Float, this);
            timeScalePort = GraphFrameworkPort.Create("Time Scale", Direction.Output, Port.Capacity.Multi, PortType.Float, this);
            realtimeSinceStartupPort = GraphFrameworkPort.Create("Realtime Since Startup", Direction.Output, Port.Capacity.Multi, PortType.Float, this);
            timeSinceLevelLoadPort = GraphFrameworkPort.Create("Time Since Level Load", Direction.Output, Port.Capacity.Multi, PortType.Float, this);
            timePort = GraphFrameworkPort.Create("Time", Direction.Output, Port.Capacity.Multi, PortType.Float, this);
            deltaTimePort = GraphFrameworkPort.Create("Delta Time", Direction.Output, Port.Capacity.Multi, PortType.Float, this);

            isFixedField = new Toggle("Fixed") {
                tooltip = "Changes Time and Delta Time to the fixed variant."
            };
            isFixedField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == isFixed) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Fixed");
                isFixed = evt.newValue;
                RuntimeNode.UpdateIsFixed(isFixed);
                OnTimeTypeChanged();
            });

            isUnscaledField = new Toggle("Unscaled") {
                tooltip = "Changes Time and Delta Time to the unscaled variant."
            };
            isUnscaledField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == isUnscaled) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Unscaled");
                isUnscaled = evt.newValue;
                RuntimeNode.UpdateIsUnscaled(isUnscaled);
                OnTimeTypeChanged();
            });

            outputContainer.Add(isFixedField);
            outputContainer.Add(isUnscaledField);
            AddPort(timePort);
            AddPort(deltaTimePort);
            AddPort(smoothDeltaTimePort);
            AddPort(timeScalePort);
            AddPort(realtimeSinceStartupPort);
            AddPort(timeSinceLevelLoadPort);
        }

        private void OnTimeTypeChanged() {
            string prefix = isFixed switch {
                true when isUnscaled => "(F+U) ",
                true => "(F) ",
                false when isUnscaled => "(U) ",
                false => ""
            };

            timePort.portName = $"{prefix}Time";
            deltaTimePort.portName = $"{prefix}Delta Time";
        }

        protected override void BindPorts() {
            BindPort(smoothDeltaTimePort, RuntimeNode.SmoothDeltaTimePort);
            BindPort(timeScalePort, RuntimeNode.TimeScalePort);
            BindPort(realtimeSinceStartupPort, RuntimeNode.RealtimeSinceStartupPort);
            BindPort(timeSinceLevelLoadPort, RuntimeNode.TimeSinceLevelLoadPort);
            BindPort(timePort, RuntimeNode.TimePort);
            BindPort(deltaTimePort, RuntimeNode.DeltaTimePort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray array = new() {
                isFixed ? 1 : 0,
                isUnscaled ? 1 : 0
            };
            root["d"] = array;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;

            isFixed = array!.Value<int>(0) == 1;
            isUnscaled = array.Value<int>(1) == 1;

            isFixedField.SetValueWithoutNotify(isFixed);
            isUnscaledField.SetValueWithoutNotify(isUnscaled);

            RuntimeNode.UpdateIsFixed(isFixed);
            RuntimeNode.UpdateIsUnscaled(isUnscaled);

            OnTimeTypeChanged();

            base.Deserialize(data);
        }
    }
}