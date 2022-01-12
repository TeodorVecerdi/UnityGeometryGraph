using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;

namespace GeometryGraph.Runtime {
    [BurstCompile(CompileSynchronously = true)]
    public static class Noise {
        [BurstCompile(CompileSynchronously = true)]
        public static float Simplex3(
            ref float3 position,
            float scale = 1.0f,
            [AssumeRange(1, Constants.MAX_NOISE_OCTAVES)] int octaves = 1,
            float frequency = 2.0f,
            float persistence = 0.5f)
        {
            Hint.Assume(octaves > 0);
            Hint.Assume(octaves <= Constants.MAX_NOISE_OCTAVES);
            Hint.Assume(frequency >= 0.001f);
            Hint.Assume(persistence >= 0.001f);

            Hint.Unlikely(scale <= 0.0f);
            Hint.Unlikely(frequency <= 1.0f);
            Hint.Unlikely(persistence >= 1.0f);

            if (octaves == 1) return noise.snoise(position * scale);

            float total = 0.0f;
            float currentFrequency = 1.0f;
            float amplitude = 1.0f;
            float maxValue = 0.0f;

            for (int i = 0; i < octaves; i++) {
                total += noise.snoise(position * scale * currentFrequency) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                currentFrequency *= frequency;
            }

            return total / maxValue;
        }

        // Totally random offsets
        private const float offset_3d_x = 84032.7825f;
        private const float offset_3d_y = 36672.8438f;
        private const float offset_3d_z = 54892.5638f;

        [BurstCompile]
        public static void Simplex3X3(
            ref float3 position,
            out float3 noise,
            float scale = 1.0f,
            [AssumeRange(1, Constants.MAX_NOISE_OCTAVES)] int octaves = 1,
            float frequency = 2.0f,
            float persistence = 0.5f)
        {
            Hint.Assume(octaves > 0);
            Hint.Assume(octaves <= Constants.MAX_NOISE_OCTAVES);
            Hint.Assume(frequency >= 0.001f);
            Hint.Assume(persistence >= 0.001f);

            Hint.Unlikely(scale <= 0.0f);
            Hint.Unlikely(frequency <= 1.0f);
            Hint.Unlikely(persistence >= 1.0f);

            float3 pX = position + new float3(offset_3d_x, 0, 0);
            float3 pY = position + new float3(0, offset_3d_y, 0);
            float3 pZ = position + new float3(0, 0, offset_3d_z);

            float x = Simplex3(ref pX, scale, octaves, frequency, persistence);
            float y = Simplex3(ref pY, scale, octaves, frequency, persistence);
            float z = Simplex3(ref pZ, scale, octaves, frequency, persistence);

            noise = new float3(x, y, z);
        }
    }
}