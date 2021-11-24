using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace GeometryGraph.Runtime.Data {
    public struct AnimationCurve {
        private UnityEngine.AnimationCurve unityCurve;
        private readonly WrapMode preWrapMode;
        private readonly WrapMode postWrapMode;
        private readonly List<Keyframe> keyframes;

        public UnityEngine.AnimationCurve UnityCurve => unityCurve;
        public WrapMode PreWrapMode => preWrapMode;
        public WrapMode PostWrapMode => postWrapMode;
        public ReadOnlyCollection<Keyframe> Keyframes => keyframes.AsReadOnly();

        public AnimationCurve(UnityEngine.AnimationCurve unityAnimationCurve) {
            unityCurve = unityAnimationCurve;
            preWrapMode = unityAnimationCurve.preWrapMode;
            postWrapMode = unityAnimationCurve.postWrapMode;
            keyframes = new List<Keyframe>(unityAnimationCurve.keys);
        }

        public AnimationCurve(WrapMode preWrapMode, WrapMode postWrapMode, List<Keyframe> keyframes) {
            this.keyframes = keyframes;
            this.preWrapMode = preWrapMode;
            this.postWrapMode = postWrapMode;
            unityCurve = new UnityEngine.AnimationCurve(keyframes.ToArray()) {preWrapMode = preWrapMode, postWrapMode = postWrapMode};
        }

        public float Evaluate(float time) {
            unityCurve ??= new UnityEngine.AnimationCurve(keyframes.ToArray()) { preWrapMode = preWrapMode, postWrapMode = postWrapMode };
            return unityCurve.Evaluate(time);
        }

        public static explicit operator AnimationCurve(UnityEngine.AnimationCurve unityAnimationCurve) {
            return new AnimationCurve(unityAnimationCurve);
        }
    }
}