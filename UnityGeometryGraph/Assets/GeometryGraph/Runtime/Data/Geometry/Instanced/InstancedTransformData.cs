using System;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Data {
    [Serializable]
    public struct InstancedTransformData {
        [SerializeField] private float3 translation;
        [SerializeField] private float3 eulerRotation;
        [SerializeField] private float3 scale;
        
        public float3 Translation => translation;
        public float3 EulerRotation => eulerRotation;
        public float3 Scale => scale;

        public InstancedTransformData(float3 translation, float3 eulerRotation, float3 scale) {
            this.translation = translation;
            this.eulerRotation = eulerRotation;
            this.scale = scale;
        }
    }
}