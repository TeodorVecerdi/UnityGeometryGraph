﻿using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Graph {
    public class SubdivideNode : RuntimeNode {
        private GeometryData source = GeometryData.Empty;
        private int levels = 1;
        private GeometryData result;

        public RuntimePort InputPort { get; private set; }
        public RuntimePort LevelsPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public SubdivideNode(string guid) : base(guid) {
            InputPort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
            LevelsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateLevels(int newValue) {
            if (levels == newValue) return;
            
            DebugUtility.Log("Recalculating result / levels changed");
            levels = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) {
                DebugUtility.Log("Attempting to get value for another port than the Result port");
                return null;
            }
            DebugUtility.Log("Returning result");

            if (result == null) {
                DebugUtility.Log($"Result was null: Recalculating with parameters: [source:{source};levels:{levels}]");
                CalculateResult();
            }
            
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == InputPort) {
                source = GetValue(connection, GeometryData.Empty).Clone();
                DebugUtility.Log("Input geometry changed");
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == LevelsPort) {
                var newValue = GetValue(connection, levels);
                DebugUtility.Log("Input levels changed");
                if (newValue < 0) newValue = 0;
                if (newValue != levels) {
                    levels = newValue;
                    CalculateResult();
                    NotifyPortValueChanged(ResultPort);
                }
            }
        }
        
        public override void RebindPorts() {
            InputPort = Ports[0];
            LevelsPort = Ports[1];
            ResultPort = Ports[2];
        }

        private void CalculateResult() {
            DebugUtility.Log("Calculate result");
            result = SimpleSubdivision.Subdivide(source, levels);
        }
    }
}