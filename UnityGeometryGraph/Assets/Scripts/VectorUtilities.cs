
using UnityEngine;

public static class VectorUtilities {
    public static float DistanceSqr(Vector3 a, Vector3 b) {
        var num1 = a.x - b.x;
        var num2 = a.y - b.y;
        var num3 = a.z - b.z;
        return num1 * num1 + num2 * num2 + num3 * num3;
    }
}