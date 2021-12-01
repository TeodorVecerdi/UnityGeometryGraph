using System.Collections.Generic;
using UnityEngine;

namespace GeometryGraph.Runtime {
    public class BakedInstancedGeometry {
        private readonly List<Mesh> meshes;
        private readonly List<Matrix4x4[]> matrices;

        public List<Mesh> Meshes => meshes;
        public List<Matrix4x4[]> Matrices => matrices;

        public BakedInstancedGeometry() {
            meshes = new List<Mesh>();
            matrices = new List<Matrix4x4[]>();
        }
    }
}