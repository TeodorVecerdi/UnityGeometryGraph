using System;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Data {
    [Serializable]
    public struct InstancedTransformData {
        [SerializeField] private float4x4 matrix;
        public float4x4 Matrix => matrix;

        public InstancedTransformData(float3 translation, float3 eulerRotation, float3 scale) {
            matrix = float4x4.TRS(translation, quaternion.Euler(eulerRotation), scale);
        }
    }
}