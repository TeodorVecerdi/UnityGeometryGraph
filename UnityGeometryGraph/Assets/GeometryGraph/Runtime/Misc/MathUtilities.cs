using System;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime {
    public static class ExtraMath {
        public static float DistanceSqr(Vector3 a, Vector3 b) {
            var num1 = a.x - b.x;
            var num2 = a.y - b.y;
            var num3 = a.z - b.z;
            return num1 * num1 + num2 * num2 + num3 * num3;
        }

        public static float SmoothMaximum(float x, float y, float distance) {
            return SmoothMinimum(x, y, -distance);
            /*var inverseDistance = 1.0 / distance;
            var expX = Math.Exp(distance * x);
            var expY = Math.Exp(distance * y);
            return (float) Math.Log(expX + expY - 1);*/
        }

        public static float SmoothMinimum(float x, float y, float distance) {
            var h = Math.Clamp(0.5 + 0.5 * (x - y) / distance, 0.0, 1.0);
            return (float)(x * (1 - h) + y * h - distance * h * (1 - h));
        }
        
        public static float Wrap(float x, float min, float max) {
            return ((x - min) % (max - min) + (max - min)) % (max - min) + min;
        }

        public static int Mod(this int a, int n) => a % n < 0 ? a % n + n : a % n;

        public static Vector3 WrapPI(Vector3 a) {
            if (a.x < -180.0f) a.x += 360.0f;
            else if (a.x > 180.0f) a.x -= 360.0f;
            if (a.y < -180.0f) a.y += 360.0f;
            else if (a.y > 180.0f) a.y -= 360.0f;
            if (a.z < -180.0f) a.z += 360.0f;
            else if (a.z > 180.0f) a.z -= 360.0f;
            return a;
        }
    }

    //!! Utilities for Unity.Mathematics
    // ReSharper disable InconsistentNaming
    public static class math_util {
        public const float TWO_PI = 6.28318530717959f;

        public static float angle(float3 from, float3 to) {
            var num = math.sqrt(math.lengthsq(from) * (double)math.lengthsq(to));
            return num < 1.00000000362749E-15 ? 0.0f : (float)math.acos(math.clamp(math.dot(from, to) / num, -1f, 1f)) * 57.29578f;
        }

        public static float4 float4(this float3 float3, float w = 0.0f) {
            return new float4(float3, w);
        }
    }

    public static class float3_util {
        public static readonly float3 zero = float3.zero;
        public static readonly float3 one = new float3(1);
        public static readonly float3 right = new float3(1, 0, 0);
        public static readonly float3 up = new float3(0, 1, 0);
        public static readonly float3 forward = new float3(0, 0, 1);
    }

    public static class float2_util {
        public static readonly float2 zero = float2.zero;
        public static readonly float2 one = new float2(1);
        public static readonly float2 right = new float2(1, 0);
        public static readonly float2 up = new float2(0, 1);
    }
}
// ReSharper restore InconsistentNaming