namespace GeometryGraph.Runtime.Curve {
    public readonly struct CurveToGeometrySettings {
        public bool CloseCaps { get; }
        public bool SeparateMaterialsForCaps { get; }
        
        public bool ShadeSmoothCurve { get; }
        public bool ShadeSmoothCaps { get; }

        public float RotationOffset { get; }
        public float IncrementalRotationOffset { get; }

        public CurveToGeometrySettings(bool closeCaps, bool separateMaterialsForCaps, bool shadeSmoothCurve, bool shadeSmoothCaps, float rotationOffset, float incrementalRotationOffset) {
            CloseCaps = closeCaps;
            SeparateMaterialsForCaps = separateMaterialsForCaps;
            ShadeSmoothCurve = shadeSmoothCurve;
            ShadeSmoothCaps = shadeSmoothCaps;
            RotationOffset = rotationOffset;
            IncrementalRotationOffset = incrementalRotationOffset;
        }
    }
}