using GeometryGraph.Runtime.Attribute;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Geometry {
    public static class Geometry {
        public static (float3 min, float3 max, GeometryData boundingBox) BoundingBox(GeometryData data) {
            var positionAttribute = data.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);

            if (positionAttribute == null) {
                return (float3.zero, float3.zero, GeometryData.Empty);
            }

            var min = float3_ext.one * float.MaxValue;
            var max = float3_ext.one * float.MinValue;
            
            foreach (var position in positionAttribute) {
                min = math.min(min, position);
                max = math.max(max, position);
            }

            var size = max - min;
            var center = (min + max) * 0.5f;
            var boundingBox = GeometryPrimitive.Cube(size);
            var bbPosition = boundingBox.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            bbPosition!.Yield(position => position + center).Into(bbPosition);

            return (min, max, boundingBox);
        }
    }
}