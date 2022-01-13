using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Curve {
    public static partial class CurveOperations {
        public static CurveData RecalculateDirectionVectors(CurveData originalCurve, RecalculateCurveDirectionsSettings settings) {
            if (originalCurve == null || originalCurve.Points == 0) return CurveData.Empty;

            List<float3> position = originalCurve.Position.ToList();
            List<float3> tangent = originalCurve.Tangent.ToList();
            List<float3> normal = originalCurve.Normal.ToList();
            List<float3> binormal = originalCurve.Binormal.ToList();

            float tangentMultiplier = settings.FlipTangents ? -1 : 1;
            float normalMultiplier = settings.FlipNormals ? -1 : 1;
            float binormalMultiplier = settings.FlipBinormals ? -1 : 1;

            // Recalculate tangents
            for (int i = 0; i < originalCurve.Points - 1; i++) {
                tangent[i] = math.normalize(position[i + 1] - position[i]);
            }

            if (originalCurve.IsClosed) {
                tangent[^1] = math.normalize(position[0] - position[^1]);
            } else {
                tangent[^1] = tangent[^2];
            }

            // Recalculate normals
            for (int i = 0; i < originalCurve.Points; i++) {
                normal[i] = math.normalize(math.cross(binormal[i], tangent[i]));
            }

            // Recalculate binormals
            for (int i = 0; i < originalCurve.Points; i++) {
                binormal[i] = math.normalize(math.cross(tangent[i], normal[i]));
            }

            // Apply multipliers
            for (int i = 0; i < originalCurve.Points; i++) {
                tangent[i] *= tangentMultiplier;
                normal[i] *= normalMultiplier;
                binormal[i] *= binormalMultiplier;
            }

            return new CurveData(originalCurve.Type, position.Count, originalCurve.IsClosed, position, tangent, normal, binormal);
        }
    }
}