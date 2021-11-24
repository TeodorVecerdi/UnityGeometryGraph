namespace GeometryGraph.Runtime {
    // https://github.com/dotnet/runtime/blob/2c554312e10f3a6348b3edd727f2a2270e91c4a2/src/libraries/System.Private.CoreLib/src/System/Numerics/Hashing/HashHelpers.cs
    internal static class HashHelpers {
        public static int Combine(int h1, int h2) {
            // RyuJIT optimizes this to use the ROL instruction
            // Related GitHub pull request: https://github.com/dotnet/coreclr/pull/1830
            uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)rol5 + h1) ^ h2;
        }
    }
}