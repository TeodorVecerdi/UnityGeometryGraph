using System.Collections.Generic;

namespace GeometryGraph.Runtime.AttributeSystem {
    public static class AttributeId {
        public const string Position = "position";
        public const string Normal = "normal";
        public const string Material = "material_index";
        public const string ShadeSmooth = "shade_smooth";
        public const string Crease = "crease";
        public const string UV = "uv";
        
        public static readonly HashSet<string> BuiltinIds = new() {
            Position,
            Normal,
            Material,
            ShadeSmooth,
            Crease,
            UV
        };
    }
}