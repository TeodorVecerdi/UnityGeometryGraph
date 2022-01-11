using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Geometry {
    public static class GeometryGrid {
        public static GeometryData Make(float2 size, int pointsX, int pointsY) {
            size = math.max(size, new float2(0.01f, 0.01f));
            pointsX = pointsX.MinClamped(2);
            pointsY = pointsY.MinClamped(2);

            float2 cellSize = size / new float2(pointsX, pointsY);
            List<float3> position = new();
            for (int x = 0; x < pointsX; x++) {
                for (int y = 0; y < pointsY; y++) {
                    position.Add(new float3(x * cellSize.x, 0, y * cellSize.y));
                }
            }

            return new GeometryData(
                    Enumerable.Empty<GeometryData.Edge>(), Enumerable.Empty<GeometryData.Face>(), Enumerable.Empty<GeometryData.FaceCorner>(),
                    1, position, Enumerable.Empty<float3>(), Enumerable.Empty<int>(),
                    Enumerable.Empty<bool>(), Enumerable.Empty<float2>())
                ;
        }
    }
}