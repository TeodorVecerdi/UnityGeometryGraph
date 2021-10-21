using Unity.Jobs;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Curve.Primitive {
    public interface ICurveJob : IJobParallelFor {
        float3 Position(float t);
        float3 Tangent(float t);
        float3 Normal(float t);
        float3 Binormal(float t);
    }
}