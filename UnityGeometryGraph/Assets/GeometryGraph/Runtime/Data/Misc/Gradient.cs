using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace GeometryGraph.Runtime.Data {
    public struct Gradient {
        private UnityEngine.Gradient unityGradient;
        private readonly GradientMode mode;
        private readonly List<GradientColorKey> colorKeys;
        private readonly List<GradientAlphaKey> alphaKeys;

        public UnityEngine.Gradient UnityGradient => unityGradient;
        public GradientMode Mode => mode;
        public ReadOnlyCollection<GradientColorKey> ColorKeys => colorKeys.AsReadOnly();
        public ReadOnlyCollection<GradientAlphaKey> AlphaKeys => alphaKeys.AsReadOnly();

        public Gradient(UnityEngine.Gradient unityGradient) {
            this.unityGradient = unityGradient;
            try {
                mode = unityGradient.mode;
            } catch (UnityException) {
                //Exception: get_mode is not allowed to be called during serialization
                mode = GradientMode.Blend;
            }
            colorKeys = new List<GradientColorKey>(unityGradient.colorKeys);
            alphaKeys = new List<GradientAlphaKey>(unityGradient.alphaKeys);
        }

        public Gradient(GradientMode mode, List<GradientColorKey> colorKeys, List<GradientAlphaKey> alphaKeys) {
            this.mode = mode;
            this.colorKeys = colorKeys;
            this.alphaKeys = alphaKeys;
            try {
                unityGradient = new UnityEngine.Gradient{mode = mode, colorKeys = colorKeys.ToArray(), alphaKeys = alphaKeys.ToArray()};
            } catch (UnityException) {
                //Exception: set_mode is not allowed to be called during serialization
                unityGradient = new UnityEngine.Gradient{colorKeys = colorKeys.ToArray(), alphaKeys = alphaKeys.ToArray()};
            }
        }

        public RGBAlphaPair Evaluate(float time) {
            try {
                unityGradient ??= new UnityEngine.Gradient{mode = mode, colorKeys = colorKeys.ToArray(), alphaKeys = alphaKeys.ToArray()};
            } catch (UnityException) {
                //Exception: set_mode is not allowed to be called during serialization
                unityGradient ??= new UnityEngine.Gradient{colorKeys = colorKeys.ToArray(), alphaKeys = alphaKeys.ToArray()};
            }
            return (RGBAlphaPair)unityGradient.Evaluate(time);
        }

        public static explicit operator Gradient (UnityEngine.Gradient unityGradient) {
            return new Gradient(unityGradient);
        }

        public static explicit operator UnityEngine.Gradient (Gradient gradient) {
            return gradient.unityGradient;
        }
    }
}