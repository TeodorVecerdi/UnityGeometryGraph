using GeometryGraph.Runtime.AttributeSystem;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Geometry {
    public static class Geometry {
        public static (float3 min, float3 max, GeometryData boundingBox) BoundingBox(GeometryData data) {
            Vector3Attribute positionAttribute = data.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);

            if (positionAttribute == null) {
                return (float3.zero, float3.zero, GeometryData.Empty);
            }

            float3 min = float3_ext.one * float.MaxValue;
            float3 max = float3_ext.one * float.MinValue;

            foreach (float3 position in positionAttribute) {
                min = math.min(min, position);
                max = math.max(max, position);
            }

            float3 size = max - min;
            float3 center = (min + max) * 0.5f;
            GeometryData boundingBox = GeometryPrimitive.Cube(size);
            Vector3Attribute bbPosition = boundingBox.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            bbPosition!.Yield(position => position + center).Into(bbPosition);

            return (min, max, boundingBox);
        }
    }
}