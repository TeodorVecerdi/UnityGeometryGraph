namespace GeometryGraph.Runtime.Curve {
    public readonly struct CurveToGeometrySettings {
        public bool CloseCaps { get; }
        public CapUVType CapUvType { get; }
        public bool SeparateMaterialForCaps { get; }
        
        public bool ShadeSmoothCurve { get; }
        public bool ShadeSmoothCaps { get; }

        public float RotationOffset { get; }
        public float IncrementalRotationOffset { get; }

        public CurveToGeometrySettings(bool closeCaps, bool separateMaterialForCaps, bool shadeSmoothCurve, bool shadeSmoothCaps, float rotationOffset, float incrementalRotationOffset, CapUVType capUVType) {
            CloseCaps = closeCaps;
            SeparateMaterialForCaps = separateMaterialForCaps;
            ShadeSmoothCurve = shadeSmoothCurve;
            ShadeSmoothCaps = shadeSmoothCaps;
            RotationOffset = rotationOffset;
            IncrementalRotationOffset = incrementalRotationOffset;
            CapUvType = capUVType;
        }
        
        public enum CapUVType {
            /// <summary>Generate cap UVs in local space (relative to the cap)</summary>
            LocalSpace = 0,
            
            /// <summary>Generate cap UVs in world space</summary>
            WorldSpace = 1,
            
            /// <summary>Same as <see cref="WorldSpace"/> but offset to start at 0,0</summary> 
            WorldSpaceAligned = 2,
        }
    }
}