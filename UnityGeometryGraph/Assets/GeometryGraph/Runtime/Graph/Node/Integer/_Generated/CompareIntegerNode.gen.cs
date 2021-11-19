// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GeometryGraph Code Generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using GeometryGraph.Runtime.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace GeometryGraph.Runtime.Graph {
    [SourceClass("GeometryGraph.Runtime::GeometryGraph.Runtime.Graph::CompareIntegerNode")]
    public partial class CompareIntegerNode : RuntimeNode {
        public RuntimePort APort { get; }
        public RuntimePort BPort { get; }
        public RuntimePort ResultPort { get; }

        public CompareIntegerNode(string guid) : base(guid) {
            APort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            BPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Boolean, PortDirection.Output, this);
        }

        public void UpdateA(int newValue) {
            if(A == newValue) return;
            A = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateB(int newValue) {
            if(B == newValue) return;
            B = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateOperation(CompareIntegerNode_CompareOperation newValue) {
            if(Operation == newValue) return;
            Operation = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port == ResultPort) {
                return Operation switch {
                    CompareIntegerNode_CompareOperation.LessThan => A < B,
                    CompareIntegerNode_CompareOperation.LessThanOrEqual => A <= B,
                    CompareIntegerNode_CompareOperation.GreaterThan => A > B,
                    CompareIntegerNode_CompareOperation.GreaterThanOrEqual => A >= B,
                    CompareIntegerNode_CompareOperation.Equal => A == B,
                    CompareIntegerNode_CompareOperation.NotEqual => A != B,
                    _ => throw new ArgumentOutOfRangeException(nameof(Operation), Operation, null)
                };
            }
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == APort) {
                var newValue = GetValue(connection, A);
                if(A == newValue) return;
                A = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == BPort) {
                var newValue = GetValue(connection, B);
                if(B == newValue) return;
                B = newValue;
                NotifyPortValueChanged(ResultPort);
            }
        }

        public override string GetCustomData() {
            return new JArray {
                A,
                B,
                (int)Operation,
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string data) {
            JArray array = JArray.Parse(data);
            A = array.Value<int>(0);
            B = array.Value<int>(1);
            Operation = (CompareIntegerNode_CompareOperation) array.Value<int>(2);

            NotifyPortValueChanged(ResultPort);
        }
    }
}