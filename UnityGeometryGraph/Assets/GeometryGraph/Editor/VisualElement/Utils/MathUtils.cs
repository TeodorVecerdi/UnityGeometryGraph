using System;
using UnityEngine;

namespace GeometryGraph.Editor.Utils {
    public class MathUtils {
        private const int kMaxDecimals = 15;

        internal static float ClampToFloat(double value) {
            if (double.IsPositiveInfinity(value))
                return float.PositiveInfinity;
            if (double.IsNegativeInfinity(value))
                return float.NegativeInfinity;
            if (value < -3.40282346638529E+38)
                return float.MinValue;
            return value > 3.40282346638529E+38 ? float.MaxValue : (float)value;
        }

        internal static int ClampToInt(long value) {
            if (value < int.MinValue)
                return int.MinValue;
            return value > int.MaxValue ? int.MaxValue : (int)value;
        }

        internal static float RoundToMultipleOf(float value, float roundingValue) {
            return roundingValue == 0.0 ? value : Mathf.Round(value / roundingValue) * roundingValue;
        }

        internal static float GetClosestPowerOfTen(float positiveNumber) {
            return positiveNumber <= 0.0 ? 1f : Mathf.Pow(10f, Mathf.RoundToInt(Mathf.Log10(positiveNumber)));
        }

        internal static int GetNumberOfDecimalsForMinimumDifference(float minDifference) {
            return Mathf.Clamp(-Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(minDifference))), 0, 15);
        }

        internal static int GetNumberOfDecimalsForMinimumDifference(double minDifference) {
            return (int)Math.Max(0.0, -Math.Floor(Math.Log10(Math.Abs(minDifference))));
        }

        internal static float RoundBasedOnMinimumDifference(float valueToRound, float minDifference) {
            return minDifference == 0.0
                ? DiscardLeastSignificantDecimal(valueToRound)
                : (float)Math.Round(valueToRound, GetNumberOfDecimalsForMinimumDifference(minDifference), MidpointRounding.AwayFromZero);
        }

        internal static double RoundBasedOnMinimumDifference(double valueToRound, double minDifference) {
            return minDifference == 0.0
                ? DiscardLeastSignificantDecimal(valueToRound)
                : Math.Round(valueToRound, GetNumberOfDecimalsForMinimumDifference(minDifference), MidpointRounding.AwayFromZero);
        }

        internal static float DiscardLeastSignificantDecimal(float v) {
            var digits = Mathf.Clamp((int)(5.0 - Mathf.Log10(Mathf.Abs(v))), 0, 15);
            return (float)Math.Round(v, digits, MidpointRounding.AwayFromZero);
        }

        internal static double DiscardLeastSignificantDecimal(double v) {
            var digits = Math.Max(0, (int)(5.0 - Math.Log10(Math.Abs(v))));
            try {
                return Math.Round(v, digits);
            } catch (ArgumentOutOfRangeException ex) {
                return 0.0;
            }
        }

        public static float GetQuatLength(Quaternion q) {
            return Mathf.Sqrt((float)(q.x * (double)q.x + q.y * (double)q.y + q.z * (double)q.z + q.w * (double)q.w));
        }

        public static Quaternion GetQuatConjugate(Quaternion q) {
            return new Quaternion(-q.x, -q.y, -q.z, q.w);
        }

        public static Matrix4x4 OrthogonalizeMatrix(Matrix4x4 m) {
            var identity = Matrix4x4.identity;
            Vector3 column1 = m.GetColumn(0);
            Vector3 column2 = m.GetColumn(1);
            Vector3 vector3 = m.GetColumn(2);
            vector3 = vector3.normalized;
            var normalized1 = Vector3.Cross(column2, vector3).normalized;
            var normalized2 = Vector3.Cross(vector3, normalized1).normalized;
            identity.SetColumn(0, normalized1);
            identity.SetColumn(1, normalized2);
            identity.SetColumn(2, vector3);
            return identity;
        }

        public static void QuaternionNormalize(ref Quaternion q) {
            var num = 1f / Mathf.Sqrt((float)(q.x * (double)q.x + q.y * (double)q.y + q.z * (double)q.z + q.w * (double)q.w));
            q.x *= num;
            q.y *= num;
            q.z *= num;
            q.w *= num;
        }

        public static Quaternion QuaternionFromMatrix(Matrix4x4 m) {
            var q = new Quaternion();
            q.w = Mathf.Sqrt(Mathf.Max(0.0f, 1f + m[0, 0] + m[1, 1] + m[2, 2])) / 2f;
            q.x = Mathf.Sqrt(Mathf.Max(0.0f, 1f + m[0, 0] - m[1, 1] - m[2, 2])) / 2f;
            q.y = Mathf.Sqrt(Mathf.Max(0.0f, 1f - m[0, 0] + m[1, 1] - m[2, 2])) / 2f;
            q.z = Mathf.Sqrt(Mathf.Max(0.0f, 1f - m[0, 0] - m[1, 1] + m[2, 2])) / 2f;
            q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
            q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
            q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
            QuaternionNormalize(ref q);
            return q;
        }

        public static Quaternion GetQuatLog(Quaternion q) {
            var quaternion = q;
            quaternion.w = 0.0f;
            if (Mathf.Abs(q.w) < 1.0) {
                var f1 = Mathf.Acos(q.w);
                var f2 = Mathf.Sin(f1);
                if (Mathf.Abs(f2) > 0.0001) {
                    var num = f1 / f2;
                    quaternion.x = q.x * num;
                    quaternion.y = q.y * num;
                    quaternion.z = q.z * num;
                }
            }

            return quaternion;
        }

        public static Quaternion GetQuatExp(Quaternion q) {
            var quaternion = q;
            var f1 = Mathf.Sqrt((float)(q.x * (double)q.x + q.y * (double)q.y + q.z * (double)q.z));
            var f2 = Mathf.Sin(f1);
            quaternion.w = Mathf.Cos(f1);
            if (Mathf.Abs(f2) > 0.0001) {
                var num = f2 / f1;
                quaternion.x = num * q.x;
                quaternion.y = num * q.y;
                quaternion.z = num * q.z;
            }

            return quaternion;
        }

        public static Quaternion GetQuatSquad(float t, Quaternion q0, Quaternion q1, Quaternion a0, Quaternion a1) {
            var t1 = (float)(2.0 * t * (1.0 - t));
            var quaternion = Slerp(Slerp(q0, q1, t), Slerp(a0, a1, t), t1);
            var num = Mathf.Sqrt((float)(quaternion.x * (double)quaternion.x + quaternion.y * (double)quaternion.y + quaternion.z * (double)quaternion.z +
                                         quaternion.w * (double)quaternion.w));
            quaternion.x /= num;
            quaternion.y /= num;
            quaternion.z /= num;
            quaternion.w /= num;
            return quaternion;
        }

        public static Quaternion GetSquadIntermediate(Quaternion q0, Quaternion q1, Quaternion q2) {
            var quatConjugate = GetQuatConjugate(q1);
            var quatLog1 = GetQuatLog(quatConjugate * q0);
            var quatLog2 = GetQuatLog(quatConjugate * q2);
            var q = new Quaternion((float)(-0.25 * (quatLog1.x + (double)quatLog2.x)), (float)(-0.25 * (quatLog1.y + (double)quatLog2.y)),
                                   (float)(-0.25 * (quatLog1.z + (double)quatLog2.z)), (float)(-0.25 * (quatLog1.w + (double)quatLog2.w)));
            return q1 * GetQuatExp(q);
        }

        public static float Ease(float t, float k1, float k2) {
            var num = (float)(k1 * 2.0 / 3.14159274101257 + k2 - k1 + (1.0 - k2) * 2.0 / 3.14159274101257);
            return (t >= (double)k1
                ? t >= (double)k2
                    ? (float)(2.0 * k1 / 3.14159274101257 + k2 - k1 + (1.0 - k2) * 0.636619746685028 *
                        Mathf.Sin((float)((t - (double)k2) / (1.0 - k2) * 3.14159274101257 / 2.0)))
                    : (float)(2.0 * k1 / 3.14159274101257) + t - k1
                : (float)(k1 * 0.636619746685028 * (Mathf.Sin((float)(t / (double)k1 * 3.14159274101257 / 2.0 - 1.57079637050629)) + 1.0))) / num;
        }

        public static Quaternion Slerp(Quaternion p, Quaternion q, float t) {
            var f1 = Quaternion.Dot(p, q);
            Quaternion quaternion;
            if (1.0 + f1 > 1E-05) {
                float num1;
                float num2;
                if (1.0 - f1 > 1E-05) {
                    var f2 = Mathf.Acos(f1);
                    var num3 = 1f / Mathf.Sin(f2);
                    num1 = Mathf.Sin((1f - t) * f2) * num3;
                    num2 = Mathf.Sin(t * f2) * num3;
                } else {
                    num1 = 1f - t;
                    num2 = t;
                }

                quaternion.x = (float)(num1 * (double)p.x + num2 * (double)q.x);
                quaternion.y = (float)(num1 * (double)p.y + num2 * (double)q.y);
                quaternion.z = (float)(num1 * (double)p.z + num2 * (double)q.z);
                quaternion.w = (float)(num1 * (double)p.w + num2 * (double)q.w);
            } else {
                var num4 = Mathf.Sin((float)((1.0 - t) * 3.14159274101257 * 0.5));
                var num5 = Mathf.Sin((float)(t * 3.14159274101257 * 0.5));
                quaternion.x = (float)(num4 * (double)p.x - num5 * (double)p.y);
                quaternion.y = (float)(num4 * (double)p.y + num5 * (double)p.x);
                quaternion.z = (float)(num4 * (double)p.z - num5 * (double)p.w);
                quaternion.w = p.z;
            }

            return quaternion;
        }

        public static object IntersectRayTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, bool bidirectional) {
            var lhs = v1 - v0;
            var vector3_1 = v2 - v0;
            var rhs1 = Vector3.Cross(lhs, vector3_1);
            var num1 = Vector3.Dot(-ray.direction, rhs1);
            if (num1 <= 0.0)
                return null;
            var vector3_2 = ray.origin - v0;
            var num2 = Vector3.Dot(vector3_2, rhs1);
            if (num2 < 0.0 && !bidirectional)
                return null;
            var rhs2 = Vector3.Cross(-ray.direction, vector3_2);
            var num3 = Vector3.Dot(vector3_1, rhs2);
            if (num3 < 0.0 || num3 > (double)num1)
                return null;
            var num4 = -Vector3.Dot(lhs, rhs2);
            if (num4 < 0.0 || num3 + (double)num4 > num1)
                return null;
            var num5 = 1f / num1;
            var num6 = num2 * num5;
            var y = num3 * num5;
            var z = num4 * num5;
            var x = 1f - y - z;
            return new RaycastHit {
                point = ray.origin + num6 * ray.direction,
                distance = num6,
                barycentricCoordinate = new Vector3(x, y, z),
                normal = Vector3.Normalize(rhs1)
            };
        }

        public static Vector3 ClosestPtSegmentRay(Vector3 p1, Vector3 q1, Ray ray, out float squaredDist, out float s, out Vector3 closestRay) {
            var origin = ray.origin;
            var point = ray.GetPoint(10000f);
            var vector3_1 = q1 - p1;
            var vector3_2 = point - origin;
            var rhs = p1 - origin;
            var num1 = Vector3.Dot(vector3_1, vector3_1);
            var num2 = Vector3.Dot(vector3_2, vector3_2);
            var num3 = Vector3.Dot(vector3_2, rhs);
            if (num1 <= (double)Mathf.Epsilon && num2 <= (double)Mathf.Epsilon) {
                squaredDist = Vector3.Dot(p1 - origin, p1 - origin);
                s = 0.0f;
                closestRay = origin;
                return p1;
            }

            float num4;
            if (num1 <= (double)Mathf.Epsilon) {
                s = 0.0f;
                num4 = Mathf.Clamp(num3 / num2, 0.0f, 1f);
            } else {
                var num5 = Vector3.Dot(vector3_1, rhs);
                if (num2 <= (double)Mathf.Epsilon) {
                    num4 = 0.0f;
                    s = Mathf.Clamp(-num5 / num1, 0.0f, 1f);
                } else {
                    var num6 = Vector3.Dot(vector3_1, vector3_2);
                    var num7 = (float)(num1 * (double)num2 - num6 * (double)num6);
                    s = num7 == 0.0 ? 0.0f : Mathf.Clamp((float)(num6 * (double)num3 - num5 * (double)num2) / num7, 0.0f, 1f);
                    num4 = (num6 * s + num3) / num2;
                    if (num4 < 0.0) {
                        num4 = 0.0f;
                        s = Mathf.Clamp(-num5 / num1, 0.0f, 1f);
                    } else if (num4 > 1.0) {
                        num4 = 1f;
                        s = Mathf.Clamp((num6 - num5) / num1, 0.0f, 1f);
                    }
                }
            }

            var vector3_3 = p1 + vector3_1 * s;
            var vector3_4 = origin + vector3_2 * num4;
            squaredDist = Vector3.Dot(vector3_3 - vector3_4, vector3_3 - vector3_4);
            closestRay = vector3_4;
            return vector3_3;
        }

        public static bool IntersectRaySphere(Ray ray, Vector3 sphereOrigin, float sphereRadius, ref float t, ref Vector3 q) {
            var vector3 = ray.origin - sphereOrigin;
            var num1 = Vector3.Dot(vector3, ray.direction);
            var num2 = Vector3.Dot(vector3, vector3) - sphereRadius * sphereRadius;
            if (num2 > 0.0 && num1 > 0.0)
                return false;
            var f = num1 * num1 - num2;
            if (f < 0.0)
                return false;
            t = -num1 - Mathf.Sqrt(f);
            if (t < 0.0)
                t = 0.0f;
            q = ray.origin + t * ray.direction;
            return true;
        }

        public static bool ClosestPtRaySphere(Ray ray, Vector3 sphereOrigin, float sphereRadius, ref float t, ref Vector3 q) {
            var vector3 = ray.origin - sphereOrigin;
            var num1 = Vector3.Dot(vector3, ray.direction);
            var num2 = Vector3.Dot(vector3, vector3) - sphereRadius * sphereRadius;
            if (num2 > 0.0 && num1 > 0.0) {
                t = 0.0f;
                q = ray.origin;
                return true;
            }

            var f = num1 * num1 - num2;
            if (f < 0.0)
                f = 0.0f;
            t = -num1 - Mathf.Sqrt(f);
            if (t < 0.0)
                t = 0.0f;
            q = ray.origin + t * ray.direction;
            return true;
        }
    }
}