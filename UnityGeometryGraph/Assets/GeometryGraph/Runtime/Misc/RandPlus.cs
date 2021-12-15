using System;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime {
    public static class RandPlus {
        public static float3 Range(float3 min, float3 max) {
            return new float3(Rand.Range(min.x, max.x), Rand.Range(min.y, max.y), Rand.Range(min.z, max.z));
        }

        [MustUseReturnValue("RandPlus.PushState returns an IDisposable which must be disposed in order to restore the Rand state")]
        public static IDisposable PushState(int replacementSeed) {
            return new RandStateDisposable(replacementSeed);
        }

        private class RandStateDisposable : IDisposable {
            private bool isDisposed;

            public RandStateDisposable(int replacementSeed) {
                Rand.PushState(replacementSeed);
            }

            public void Dispose() {
                if (isDisposed) return;
                isDisposed = true;
                Rand.PopState();
            }
        }
    }
}