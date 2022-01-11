using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime {
    //* Utilities for Unity.Mathematics
    // ReSharper disable InconsistentNaming
    public static class math_ext {
        public const float TWO_PI = 6.28318530717959f;

        public static float angle(float3 from, float3 to) {
            double num = math.sqrt(math.lengthsq(from) * (double)math.lengthsq(to));
            return num < 1.00000000362749E-15 ? 0.0f : (float)math.acos(math.clamp(math.dot(from, to) / num, -1f, 1f)) * 57.29578f;
        }
        
        public static float smooth_max(float x, float y, float distance) {
            return smooth_min(x, y, -distance);
        }

        public static float smooth_min(float x, float y, float distance) {
            float h = math.clamp(0.5f + 0.5f * (y - x) / distance, 0.0f, 1.0f);
            return math.lerp(y, x, h) - distance * h * (1.0f - h);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float wrap(float x, float min, float max) {
            return fmod(x - min, max - min) + min;
        }

        public static float3 wrap(float3 a, float min, float max) {
            return new float3(
                wrap(a.x, min, max),
                wrap(a.y, min, max),
                wrap(a.z, min, max)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int mod(this int x, int y) {
            return (x % y + y) % y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float fmod(this float x, float y) {
            return (x % y + y) % y;
        }

        public static float4 float4(this float3 float3, float w = 0.0f) {
            return new float4(float3, w);
        }

        public static float inverse_lerp(float3 a, float3 b, float3 x) {
            float3 d = b - a;
            return math.dot(x - a, d) / math.dot(d, d);
        }

        public static float inverse_lerp_clamped(float3 a, float3 b, float3 x) {
            float inv = inverse_lerp(a, b, x);
            return inv.Clamped01();
        }
    }

    public static class quat_ext {
        public static float compute_euler_x(quaternion quat)
        {
            float sinr_cosp = 2 * (quat.value.w * quat.value.x + quat.value.y * quat.value.z);
            float cosr_cosp = 1 - 2 * (quat.value.x * quat.value.x + quat.value.y * quat.value.y);
            return math.atan2(sinr_cosp, cosr_cosp);
        }

        public static float compute_euler_y(quaternion quat)
        {
            float sinp = 2 * (quat.value.w * quat.value.y - quat.value.z * quat.value.x);
            if (math.abs(sinp) >= 1)
                return math.PI / 2 * math.sign(sinp); // use 90 degrees if out of range
            return math.asin(sinp);
        }

        public static float compute_euler_z(quaternion quat)
        {
            float siny_cosp = 2 * (quat.value.w * quat.value.z + quat.value.x * quat.value.y);
            float cosy_cosp = 1 - 2 * (quat.value.y * quat.value.y + quat.value.z * quat.value.z);
            return math.atan2(siny_cosp, cosy_cosp);
        }

        public static float3 to_euler(quaternion quat)
        {
            return new float3(compute_euler_x(quat), compute_euler_y(quat), compute_euler_z(quat));
        }

        public static quaternion from_euler(float3 eulerAngles)
        {

            float cy = math.cos(eulerAngles.z * 0.5f);
            float sy = math.sin(eulerAngles.z * 0.5f);
            float cp = math.cos(eulerAngles.y * 0.5f);
            float sp = math.sin(eulerAngles.y * 0.5f);
            float cr = math.cos(eulerAngles.x * 0.5f);
            float sr = math.sin(eulerAngles.x * 0.5f);

            float4 q;
            q.w = cr * cp * cy + sr * sp * sy;
            q.x = sr * cp * cy - cr * sp * sy;
            q.y = cr * sp * cy + sr * cp * sy;
            q.z = cr * cp * sy - sr * sp * cy;

            return q;

        }
    }

    public static class float3_ext {
        public static readonly float3 zero = float3.zero;
        public static readonly float3 one = new(1);
        public static readonly float3 right = new(1, 0, 0);
        public static readonly float3 up = new(0, 1, 0);
        public static readonly float3 forward = new(0, 0, 1);
    }

    public static class float2_ext {
        public static readonly float2 zero = float2.zero;
        public static readonly float2 one = new(1);
        public static readonly float2 right = new(1, 0);
        public static readonly float2 up = new(0, 1);
    }
}
// ReSharper restore InconsistentNaming