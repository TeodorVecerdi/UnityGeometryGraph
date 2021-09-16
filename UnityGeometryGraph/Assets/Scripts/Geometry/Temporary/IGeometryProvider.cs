using UnityEngine;

namespace Geometry {
    public interface IGeometryProvider {
        GeometryData Geometry { get; }
        Matrix4x4 LocalToWorldMatrix { get; }
    }
}