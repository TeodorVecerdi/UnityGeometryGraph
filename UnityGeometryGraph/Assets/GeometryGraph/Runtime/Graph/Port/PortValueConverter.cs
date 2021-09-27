namespace GeometryGraph.Runtime.Graph {
    public static class PortValueConverter {
        public static object Convert(object value, PortType sourceType, PortType targetType) {
            if (targetType == PortType.Float) {
                switch (sourceType) {
                    case PortType.Integer: return (float)(int)value;
                    case PortType.Boolean: return (bool)value ? 1.0f : 0.0f;
                }
            } else if (targetType == PortType.Integer) {
                switch (sourceType) {
                    case PortType.Float: return (int)(float)value;
                    case PortType.Boolean: return (bool)value ? 1 : 0;
                }
            } else if (targetType == PortType.Boolean) {
                switch (sourceType) {
                    case PortType.Integer: return (int)value != 0;
                    case PortType.Float: return (float)value != 0.0f;
                }
            }

            return value;
        }
    }
}