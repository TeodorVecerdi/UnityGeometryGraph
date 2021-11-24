using GeometryGraph.Runtime.Geometry;
using UnityEngine;

namespace GeometryGraph.Runtime.Data {
    public interface IGeometryProvider {
        GeometryData Geometry { get; }
        Matrix4x4 LocalToWorldMatrix { get; }
    }
}