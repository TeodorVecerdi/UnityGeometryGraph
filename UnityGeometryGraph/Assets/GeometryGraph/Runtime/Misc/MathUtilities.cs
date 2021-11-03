using Unity.Burst;
using Unity.Mathematics;

namespace GeometryGraph.Runtime {
    //!! Utilities for Unity.Mathematics
    // ReSharper disable InconsistentNaming
    public static class math_ext {
        public const float TWO_PI = 6.28318530717959f;

        public static float angle(float3 from, float3 to) {
            var num = math.sqrt(math.lengthsq(from) * (double)math.lengthsq(to));
            return num < 1.00000000362749E-15 ? 0.0f : (float)math.acos(math.clamp(math.dot(from, to) / num, -1f, 1f)) * 57.29578f;
        }
        
        public static float smooth_max(float x, float y, float distance) {
            return smooth_min(x, y, -distance);
        }

        public static float smooth_min(float x, float y, float distance) {
            float h = math.clamp(0.5f + 0.5f * (x - y) / distance, 0.0f, 1.0f);
            return x * (1.0f - h) + y * h - distance * h * (1.0f - h);
        }
        
        public static float wrap(float x, float min, float max) {
            return ((x - min) % (max - min) + (max - min)) % (max - min) + min;
        }

        public static int mod(this int a, int n) {
            return (a % n + n) % n;
        }

        public static float3 wrap(float3 a, float min, float max) {
            return new float3(
                wrap(a.x, min, max),
                wrap(a.y, min, max),
                wrap(a.z, min, max)
            );
        }

        public static float4 float4(this float3 float3, float w = 0.0f) {
            return new float4(float3, w);
        }
    }

    public static class float3_ext {
        public static readonly float3 zero = float3.zero;
        public static readonly float3 one = new float3(1);
        public static readonly float3 right = new float3(1, 0, 0);
        public static readonly float3 up = new float3(0, 1, 0);
        public static readonly float3 forward = new float3(0, 0, 1);
    }

    public static class float2_ext {
        public static readonly float2 zero = float2.zero;
        public static readonly float2 one = new float2(1);
        public static readonly float2 right = new float2(1, 0);
        public static readonly float2 up = new float2(0, 1);
    }
}
// ReSharper restore InconsistentNaming