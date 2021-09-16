using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class DisplayValueNode : RuntimeNode {
        private RuntimePort input;
        
        public DisplayValueNode(string guid) : base(guid) {
        }

        public override object GetValueForPort(RuntimePort port) {
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port != input) return;

            Debug.Log(GetValue(connection, (object)null).ToString());
        }
    }
}