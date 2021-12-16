using UnityEngine;

namespace GeometryGraph.Editor {
    [Title("UI Tester")]
    public class UITesterNode : AbstractNode<GeometryGraph.Runtime.Graph.UITesterNode> {
        private float fromMin = 0.0f;
        private float fromMax = 1.0f;
        private float toMin = 0.0f;
        private float toMax = 1.0f;

        private FloatMapRangeField floatMapRangeField;
        
        public override void CreateNode() {
            Initialize("UI Tester", NodeCategory.None);
            
            floatMapRangeField = new FloatMapRangeField("Label", fromMin, fromMax, toMin, toMax);
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
        }

        public override void BindPorts() {}
    }
}