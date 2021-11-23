using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using AnimationCurve = GeometryGraph.Runtime.Data.AnimationCurve;

namespace GeometryGraph.Runtime.Serialization {
    public static class Deserializer {
        public static AnimationCurve AnimationCurve(JObject data) {
            WrapMode preWrapMode = (WrapMode)data["p"].Value<int>();
            WrapMode postWrapMode = (WrapMode)data["P"].Value<int>();
            List<Keyframe> keyframes = new List<Keyframe>();
            foreach (JObject keyData in data["k"] as JArray) {
                Debug.Assert(keyData != null);
                Keyframe keyframe = new Keyframe {
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

    }
}