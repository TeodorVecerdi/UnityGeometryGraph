using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using AnimationCurve = GeometryGraph.Runtime.Data.AnimationCurve;
using Gradient = GeometryGraph.Runtime.Data.Gradient;

namespace GeometryGraph.Runtime.Serialization {
    public static class Serializer {
        public static JObject AnimationCurve(AnimationCurve curve) {
            JObject data = new JObject {
                ["p"] = (int)curve.PreWrapMode,
                ["P"] = (int)curve.PostWrapMode
            };
            JArray keys = new JArray();
            foreach (Keyframe key in curve.Keyframes) {
                JObject keyData = new JObject {
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
            JObject data = new JObject {
                ["m"] = (int)gradient.Mode
            };
            JArray colorKeys = new JArray();
            foreach (GradientColorKey colorKey in gradient.ColorKeys) {
                JObject keyData = new JObject {
                    ["t"] = colorKey.time,
                    ["c"] = Color(colorKey.color),
                };
                colorKeys.Add(keyData);
            }
            JArray alphaKeys = new JArray();
            foreach (GradientAlphaKey alphaKey in gradient.AlphaKeys) {
                JObject keyData = new JObject {
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