using System.Collections.Generic;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public static class PrimitiveHelper {
        private static readonly Dictionary<PrimitiveType, Mesh> primitiveMeshes = new();

        public static GameObject CreatePrimitive(PrimitiveType type, bool withCollider) {
            if (withCollider) {
                return GameObject.CreatePrimitive(type);
            }

            GameObject gameObject = new(type.ToString());
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = GetPrimitiveMesh(type);
            gameObject.AddComponent<MeshRenderer>();

            return gameObject;
        }

        public static Mesh GetPrimitiveMesh(PrimitiveType type) {
            if (!primitiveMeshes.ContainsKey(type)) {
                CreatePrimitiveMesh(type);
            }

            return primitiveMeshes[type];
        }

        private static Mesh CreatePrimitiveMesh(PrimitiveType type) {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            GameObject.Destroy(gameObject);

            primitiveMeshes[type] = mesh;
            return mesh;
        }
    }
}