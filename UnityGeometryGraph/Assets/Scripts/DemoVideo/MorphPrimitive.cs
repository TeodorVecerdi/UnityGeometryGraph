using System.Collections;
using System.Linq;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace GeometryGraph.Runtime.Testing {
    public class MorphPrimitive : MonoBehaviour {
        [SerializeField, Range(0.0f, 5.0f), OnValueChanged(nameof(OnMorphChanged))] private float morphAmount = 0.0f;
        [SerializeField] private bool autoMorph = false;
        [Space]
        [SerializeField] private GeometryData cubeReference;
        [SerializeField] private GeometryExporter exporter;
        [SerializeField] private GeometryData morphedGeometry;
        [SerializeField] private Mesh morphedMesh;
        [SerializeField] private MeshFilter meshFilter;
        [Space]
        [SerializeField] private float cubeSize = 1.0f;
        [SerializeField] private float planeSize = 1.0f;
        [SerializeField] private float circleRadius = 1.0f;
        [SerializeField] private float cylinderRadius = 1.0f;
        [SerializeField] private float cylinderHeight = 1.0f;
        [SerializeField] private float coneRadius = 1.0f;
        [SerializeField] private float coneHeight = 1.0f;
        [SerializeField] private float sphereRadius = 1.0f;

        [SerializeField, HideInInspector] private BoolAttribute shadeSmooth;
        [SerializeField, HideInInspector] private BoolAttribute shadeFlat;
        [SerializeField, HideInInspector] private Vector3Attribute normalsUp;

        [Button]
        private void Initialize() {
            cubeReference = SimpleSubdivision.Subdivide(GeometryPrimitive.Cube(float3_ext.one), 3);
            InitializeAttributes();

            exporter = new GeometryExporter();
            morphedMesh = new Mesh { indexFormat = IndexFormat.UInt32 };
            morphedMesh.MarkDynamic();

            if (Application.isPlaying) {
                if (meshFilter.mesh != null) {
                    Destroy(meshFilter.mesh);
                }
                meshFilter.sharedMesh = morphedMesh;
            } else {
                if (meshFilter.sharedMesh != null) {
                    DestroyImmediate(meshFilter.sharedMesh);
                }
                meshFilter.sharedMesh = morphedMesh;
            }
        }

        private void InitializeAttributes() {
            shadeSmooth = Enumerable.Repeat(true, cubeReference.Faces.Count).Into<BoolAttribute>(AttributeId.ShadeSmooth, AttributeDomain.Face);
            shadeFlat = Enumerable.Repeat(false, cubeReference.Faces.Count).Into<BoolAttribute>(AttributeId.ShadeSmooth, AttributeDomain.Face);
            normalsUp = Enumerable.Repeat(float3_ext.up, cubeReference.Faces.Count).Into<Vector3Attribute>(AttributeId.Normal, AttributeDomain.Face);
        }

        [Button]
        private void Morph() {
            Morph(morphAmount);
        }

        public void Morph(float amount) {
            if (cubeReference == null) Initialize();
            if (shadeSmooth == null || shadeFlat == null || normalsUp == null) InitializeAttributes();

            if (amount <= 1.0f) {
                // Morph cube to plane
                MorphCubeToPlane(amount - 0.0f);
            } else if (amount <= 2.0f) {
                // Morph plane to circle
                MorphPlaneToCircle(amount - 1.0f);
            } else if (amount <= 3.0f) {
                // Morph circle to cylinder
                MorphCircleToCylinder(amount - 2.0f);
            } else if (amount <= 4.0f) {
                // Morph cylinder to cone
                MorphCylinderToCone(amount - 3.0f);
            } else if (amount <= 5.0f) {
                // Morph cone to sphere
                MorphConeToSphere(amount - 4.0f);
            }

            Export();
        }

        [Button]
        private void Export() {
            exporter.Export(morphedGeometry, morphedMesh);
        }

        private void MorphCubeToPlane(float amount) {
            morphedGeometry = cubeReference.Clone();
            morphedGeometry.StoreAttribute(morphedGeometry.GetAttribute<Vector3Attribute>(AttributeId.Position)!.Yield(vert => {
                vert.y = math.lerp(vert.y, 0.0f, amount);
                return vert;
            }).Into<Vector3Attribute>(AttributeId.Position, AttributeDomain.Vertex));
        }

        private void MorphPlaneToCircle(float amount) {
            morphedGeometry = cubeReference.Clone();
            morphedGeometry.StoreAttribute(normalsUp);
            morphedGeometry.StoreAttribute(morphedGeometry.GetAttribute<Vector3Attribute>(AttributeId.Position)!.Yield(vert => {
                float3 from = vert;
                from.y = 0.0f;
                float3 to = math.normalize(vert) * circleRadius;
                to.y = 0.0f;
                return math.lerp(from, to, amount);
            }).Into<Vector3Attribute>(AttributeId.Position, AttributeDomain.Vertex));
        }

        private void MorphCircleToCylinder(float amount) {
            morphedGeometry = cubeReference.Clone();
            int index = 0;
            Vector3Attribute normals = ((IEnumerable)normalsUp).Into<Vector3Attribute>(AttributeId.Normal, AttributeDomain.Face);
            BoolAttribute shade = ((IEnumerable)shadeFlat).Into<BoolAttribute>(AttributeId.ShadeSmooth, AttributeDomain.Face);
            morphedGeometry.StoreAttribute(morphedGeometry.GetAttribute<Vector3Attribute>(AttributeId.Position)!.Yield(vert => {
                float3 normVert = math.normalize(vert);
                float3 from = normVert * circleRadius;
                from.y = 0.0f;
                float3 to = normVert * cylinderRadius;
                to.y = (vert.y < 0.0f ? -cylinderHeight : cylinderHeight) * 0.5f;
                if (vert.y is > -0.01f and < 0.01f) {
                    foreach (int face in morphedGeometry.Vertices[index].Faces) {
                        normals[face] = normVert;
                        shade[face] = true;
                    }
                }
                index++;
                return math.lerp(from, to, amount);
            }).Into<Vector3Attribute>(AttributeId.Position, AttributeDomain.Vertex));
            morphedGeometry.StoreAttribute(normals);
            morphedGeometry.StoreAttribute(shade);
        }

        private void MorphCylinderToCone(float amount) {
            morphedGeometry = cubeReference.Clone();
            int index = 0;
            Vector3Attribute normals = ((IEnumerable)normalsUp).Into<Vector3Attribute>(AttributeId.Normal, AttributeDomain.Face);
            morphedGeometry.StoreAttribute(morphedGeometry.GetAttribute<Vector3Attribute>(AttributeId.Position)!.Yield(vert => {
                float3 normVert = math.normalize(vert);
                float3 from = normVert * cylinderRadius;
                from.y = (vert.y < 0.0f ? -cylinderHeight : cylinderHeight) * 0.5f;

                float3 to = normVert * coneRadius;
                if (vert.y > -0.01f) {
                    to = float3.zero;
                }

                to.y = (vert.y < 0.0f ? -coneHeight : coneHeight) * 0.5f;
                if (vert.y is > -0.01f and < 0.01f) {
                    foreach (int face in morphedGeometry.Vertices[index].Faces) {
                        normals[face] = normVert;
                    }
                }
                index++;
                return math.lerp(from, to, amount);
            }).Into<Vector3Attribute>(AttributeId.Position, AttributeDomain.Vertex));
            morphedGeometry.StoreAttribute(normals);
            morphedGeometry.StoreAttribute(shadeSmooth);
        }

        private void MorphConeToSphere(float amount) {
            morphedGeometry = cubeReference.Clone();
            int index = 0;
            Vector3Attribute normals = ((IEnumerable)normalsUp).Into<Vector3Attribute>(AttributeId.Normal, AttributeDomain.Face);
            morphedGeometry.StoreAttribute(morphedGeometry.GetAttribute<Vector3Attribute>(AttributeId.Position)!.Yield(vert => {
                float3 normVert = math.normalize(vert);
                float3 from = normVert * coneRadius;
                if (vert.y > -0.01f) {
                    from = float3.zero;
                }
                from.y = (vert.y < 0.0f ? -coneHeight : coneHeight) * 0.5f;

                float3 to = normVert * sphereRadius;

                foreach (int face in morphedGeometry.Vertices[index].Faces) {
                    normals[face] = normVert;
                }
                index++;
                return math.lerp(from, to, amount);
            }).Into<Vector3Attribute>(AttributeId.Position, AttributeDomain.Vertex));
            morphedGeometry.StoreAttribute(normals);
            morphedGeometry.StoreAttribute(shadeSmooth);
        }

        private void OnMorphChanged() {
            if (autoMorph) Morph();
        }
    }
}