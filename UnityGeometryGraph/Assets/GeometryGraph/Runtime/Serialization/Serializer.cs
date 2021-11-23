using Newtonsoft.Json.Linq;
using UnityEngine;
using AnimationCurve = GeometryGraph.Runtime.Data.AnimationCurve;

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
    }
}