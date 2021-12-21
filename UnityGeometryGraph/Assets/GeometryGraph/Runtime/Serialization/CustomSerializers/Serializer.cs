using Newtonsoft.Json.Linq;
using UnityEngine;
using AnimationCurve = GeometryGraph.Runtime.Data.AnimationCurve;
using Gradient = GeometryGraph.Runtime.Data.Gradient;

namespace GeometryGraph.Runtime.Serialization {
    public static class Serializer {
        public static JObject AnimationCurve(AnimationCurve curve) {
            JObject data = new() {
                ["p"] = (int)curve.PreWrapMode,
                ["P"] = (int)curve.PostWrapMode
            };
            JArray keys = new();
            foreach (Keyframe key in curve.Keyframes) {
                JObject keyData = new() {
                    ["t"] = key.time,
                    ["v"] = key.value,
                    ["i"] = key.inTangent,
                    ["o"] = key.outTangent,
                    ["I"] = key.inWeight,
                    ["O"] = key.outWeight,
                    ["w"] = (int)key.weightedMode
                };
                keys.Add(keyData);
            }
            data["k"] = keys;
            return data;
        }

        public static JArray Color(Color color) {
            return new JArray { color.r, color.g, color.b, color.a };
        }

        public static JObject Gradient(Gradient gradient) {
            JObject data = new() {
                ["m"] = (int)gradient.Mode
            };
            JArray colorKeys = new();
            foreach (GradientColorKey colorKey in gradient.ColorKeys) {
                JObject keyData = new() {
                    ["t"] = colorKey.time,
                    ["c"] = Color(colorKey.color),
                };
                colorKeys.Add(keyData);
            }
            JArray alphaKeys = new();
            foreach (GradientAlphaKey alphaKey in gradient.AlphaKeys) {
                JObject keyData = new() {
                    ["t"] = alphaKey.time,
                    ["a"] = alphaKey.alpha
                };
                alphaKeys.Add(keyData);
            }
            data["c"] = colorKeys;
            data["a"] = alphaKeys;
            return data;
        }
    }
}