using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime {
    public static class MathUtilities {
        public static float DistanceSqr(Vector3 a, Vector3 b) {
            var num1 = a.x - b.x;
            var num2 = a.y - b.y;
            var num3 = a.z - b.z;
            return num1 * num1 + num2 * num2 + num3 * num3;
        }

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
        public static float angle(float3 from, float3 to) {
            var num = math.sqrt(math.lengthsq(from) * (double)math.lengthsq(to));
            return num < 1.00000000362749E-15 ? 0.0f : (float) math.acos(math.clamp(math.dot(from, to) / num, -1f, 1f)) * 57.29578f;
        }
    }

    public static class float3_util {
        public static readonly float3 one = new float3(1);
    }
    public static class float2_util {
        public static readonly float2 one = new float2(1);
    }
}
// ReSharper restore InconsistentNaming
