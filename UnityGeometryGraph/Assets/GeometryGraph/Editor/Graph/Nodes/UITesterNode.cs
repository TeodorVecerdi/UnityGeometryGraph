using UnityEngine;

namespace GeometryGraph.Editor {
    [Title("UI Tester")]
    public class UITesterNode : AbstractNode<GeometryGraph.Runtime.Graph.UITesterNode> {
        protected override string Title => "UI Tester";
        protected override NodeCategory Category => NodeCategory.None;

        private float fromMin = 0.0f;
        private float fromMax = 1.0f;
        private float toMin = 0.0f;
        private float toMax = 1.0f;

        protected override void CreateNode() {
            FloatMapRangeField floatMapRangeField = new FloatMapRangeField("Label", fromMin, fromMax, toMin, toMax);
            floatMapRangeField.RegisterFromMinValueChanged(evt => {
                Debug.Log("FromMinValueChanged: " + evt.newValue);
                fromMin = evt.newValue;
            });
            floatMapRangeField.RegisterFromMaxValueChanged(evt => {
                Debug.Log("FromMaxValueChanged: " + evt.newValue);
                fromMax = evt.newValue;
            });
            floatMapRangeField.RegisterToMinValueChanged(evt => {
                Debug.Log("ToMinValueChanged: " + evt.newValue);
                toMin = evt.newValue;
            });
            floatMapRangeField.RegisterToMaxValueChanged(evt => {
                Debug.Log("ToMaxValueChanged: " + evt.newValue);
                toMax = evt.newValue;
            });

            inputContainer.Add(floatMapRangeField);

            BooleanSelectionToggle booleanSelectionToggleA = new (true);
            BooleanSelectionToggle booleanSelectionToggleB = new (false, "Yes", "No");
            BooleanSelectionToggle booleanSelectionToggleC = new (true, "Yes", "No", "Label");
            inputContainer.Add(booleanSelectionToggleA);
            inputContainer.Add(booleanSelectionToggleB);
            inputContainer.Add(booleanSelectionToggleC);

            BooleanToggle booleanToggleA = new (true, "Label (On)", "Label (Off)");
            BooleanToggle booleanToggleB = new (false, "Label");
            inputContainer.Add(booleanToggleA);
            inputContainer.Add(booleanToggleB);
        }

        protected override void BindPorts() {}
    }
}