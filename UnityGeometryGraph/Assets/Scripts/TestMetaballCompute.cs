using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace GeometryGraph.Runtime.Curve.TEMP {
    public class TestMetaballCompute : MonoBehaviour {
        public ComputeShader ComputeShader;
        public RawImage Image;
        public int Resolution = 1024;

        [OnValueChanged(nameof(Run))] public bool DoSmooth;
        [OnValueChanged(nameof(Run))] public bool DoSmoothMin;
        [OnValueChanged(nameof(Run))] public float SmoothDistance;
        [OnValueChanged(nameof(Run)), Range(0, 2)] public int SmoothMinType;
        [Space]
        [OnValueChanged(nameof(Run))] public Color CircleColor;
        [OnValueChanged(nameof(Run))] public Color BackgroundColor;
        [OnValueChanged(nameof(Run))] public float CirclePower = 1.0f;
        [OnValueChanged(nameof(Run)), Range(0.0f, 1.0f)] public float CircleThreshold = 0.5f;
        [Space]
        [OnValueChanged(nameof(Run))] public float CircleARadius = 100.0f;
        [OnValueChanged(nameof(Run))] public float2 CircleACenter = new float2(256.0f, 256.0f);
        [OnValueChanged(nameof(Run))] public float CircleBRadius = 150.0f;
        [OnValueChanged(nameof(Run))] public float2 CircleBCenter = new float2(768.0f, 768.0f);

        private RenderTexture renderTexture;

        [Button] private void Initialize() {
            if (renderTexture != null)
                renderTexture.Release();

            renderTexture = new RenderTexture(Resolution, Resolution, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
            
            Image.texture = renderTexture;
        }

        [Button]
        private void Cleanup() {
            if (renderTexture != null) {
                renderTexture.Release();
                Image.texture = null;
            }
        }

        [Button]
        private void Run() {
            if (renderTexture == null)
                Initialize();
            
            var kernel = ComputeShader.FindKernel("CSMain");
            ComputeShader.SetTexture(kernel, "Result", renderTexture);
            ComputeShader.SetFloat("Resolution", Resolution);
            
            ComputeShader.SetBool("DoSmooth", DoSmooth);
            ComputeShader.SetBool("DoSmoothMin", DoSmoothMin);
            ComputeShader.SetFloat("SmoothDistance", SmoothDistance);
            ComputeShader.SetInt("SmoothMinType", SmoothMinType);
            
            ComputeShader.SetVector("CircleColor", CircleColor);
            ComputeShader.SetVector("BackgroundColor", BackgroundColor);
            ComputeShader.SetFloat("CirclePower", CirclePower);
            ComputeShader.SetFloat("CircleThreshold", CircleThreshold);
            
            ComputeShader.SetFloat("CircleARadius", CircleARadius);
            ComputeShader.SetFloat("CircleACenterX", CircleACenter.x);
            ComputeShader.SetFloat("CircleACenterY", CircleACenter.y);
            
            ComputeShader.SetFloat("CircleBRadius", CircleBRadius);
            ComputeShader.SetFloat("CircleBCenterX", CircleBCenter.x);
            ComputeShader.SetFloat("CircleBCenterY", CircleBCenter.y);
            
            ComputeShader.Dispatch(kernel, Resolution / 8, Resolution / 8, 1);
        }
    }
}