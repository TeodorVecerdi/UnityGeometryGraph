using System;
using UnityEngine;

namespace GeometryGraph.Editor.Utils {
    internal class NumericFieldDraggerUtility {
        private static bool s_UseYSign;

        internal static float Acceleration(bool shiftPressed, bool altPressed) {
            return (float)((shiftPressed ? 4.0 : 1.0) * (altPressed ? 0.25 : 1.0));
        }

        internal static float NiceDelta(Vector2 deviceDelta, float acceleration) {
            deviceDelta.y = -deviceDelta.y;
            if (Mathf.Abs(Mathf.Abs(deviceDelta.x) - Mathf.Abs(deviceDelta.y)) / (double)Mathf.Max(Mathf.Abs(deviceDelta.x), Mathf.Abs(deviceDelta.y)) > 0.100000001490116)
                s_UseYSign = Mathf.Abs(deviceDelta.x) <= (double)Mathf.Abs(deviceDelta.y);
            return s_UseYSign
                ? Mathf.Sign(deviceDelta.y) * deviceDelta.magnitude * acceleration
                : Mathf.Sign(deviceDelta.x) * deviceDelta.magnitude * acceleration;
        }

        internal static double CalculateFloatDragSensitivity(double value) {
            return double.IsInfinity(value) || double.IsNaN(value) ? 0.0 : Math.Max(1.0, Math.Pow(Math.Abs(value), 0.5)) * 0.0299999993294477;
        }

        internal static long CalculateIntDragSensitivity(long value) {
            return (long)Math.Max(1.0, Math.Pow(Math.Abs((double)value), 0.5) * 0.0299999993294477);
        }
    }
}