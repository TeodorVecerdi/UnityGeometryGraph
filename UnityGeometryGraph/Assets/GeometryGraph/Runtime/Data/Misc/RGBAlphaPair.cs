using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Data {
    public readonly struct RGBAlphaPair {
        private readonly Color color;

        public float3 RGB => new(color.r, color.g, color.b);
        public float Alpha => color.a;

        public RGBAlphaPair(Color color) {
            this.color = color;
        }

        public static explicit operator RGBAlphaPair(Color color) {
            return new RGBAlphaPair(color);
        }
    }
}