using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using AnimationCurve = GeometryGraph.Runtime.Data.AnimationCurve;
using Gradient = GeometryGraph.Runtime.Data.Gradient;

namespace GeometryGraph.Runtime.Serialization {
    public static class Deserializer {
        public static AnimationCurve AnimationCurve(JObject data) {
            WrapMode preWrapMode = (WrapMode)data["p"].Value<int>();
            WrapMode postWrapMode = (WrapMode)data["P"].Value<int>();
            List<Keyframe> keyframes = new();
            foreach (JObject keyData in data["k"] as JArray) {
                Debug.Assert(keyData != null);
                Keyframe keyframe = new() {
                    time = keyData["t"].Value<float>(),
                    value = keyData["v"].Value<float>(),
                    inTangent = keyData["i"].Value<float>(),
                    outTangent = keyData["o"].Value<float>(),
                    inWeight = keyData["I"].Value<float>(),
                    outWeight = keyData["O"].Value<float>(),
                    weightedMode = (WeightedMode)keyData["w"].Value<int>()
                };
                keyframes.Add(keyframe);
            }
            return new AnimationCurve(preWrapMode, postWrapMode, keyframes);
        }

        public static Color Color(JArray data) {
            return new Color(data.Value<float>(0), data.Value<float>(1), data.Value<float>(2), data.Value<float>(3));
        }

        public static Gradient Gradient(JObject data) {
            GradientMode mode = (GradientMode)data["m"].Value<int>();
            List<GradientColorKey> colorKeys = new();
            List<GradientAlphaKey> alphaKeys = new();

            foreach (JObject keyData in data["c"] as JArray) {
                Debug.Assert(keyData != null);
                float time = keyData["t"].Value<float>();
                Color color = Color(keyData["c"].Value<JArray>());
                colorKeys.Add(new GradientColorKey(color, time));
            }

            foreach (JObject keyData in data["a"] as JArray) {
                Debug.Assert(keyData != null);
                float time = keyData["t"].Value<float>();
                float alpha = keyData["a"].Value<float>();
                alphaKeys.Add(new GradientAlphaKey(alpha, time));
            }

            return new Gradient(mode, colorKeys, alphaKeys);
        }
    }
}