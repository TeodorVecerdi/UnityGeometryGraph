using System.Collections.Generic;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Curve.Primitive {
    public abstract class Curve {
        protected internal abstract CurveType Type { get; }

        protected int Resolution;
        protected internal bool IsClosed;
        protected internal List<float3> Points;
        protected internal List<float3> Tangents;
        protected internal List<float3> Normals;
        protected internal List<float3> Binormals;

        protected internal bool IsInitialized;

        public Curve(int resolution) {
            Resolution = resolution;
        }

        protected internal abstract int PointCount();
        internal abstract void Generate();
    }

    public static class CurveExtensions {
        internal static CurveData ToCurveData(this Curve curve) {
            if (!curve.IsInitialized) curve.Generate();
            return new CurveData(curve.Type, curve.PointCount(), curve.IsClosed, curve.Points, curve.Tangents, curve.Normals, curve.Binormals);
        }
    }
}