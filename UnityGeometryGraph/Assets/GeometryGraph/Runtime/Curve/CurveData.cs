using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace GeometryGraph.Runtime.Curve {
    [Serializable]
    public sealed class CurveData : ICloneable {
        [SerializeField] private CurveType type;
        [SerializeField] private int points;
        [SerializeField] private bool isClosed;
        [SerializeField] private List<float3> position;
        [SerializeField] private List<float3> tangent;
        [SerializeField] private List<float3> normal;
        [SerializeField] private List<float3> binormal;

        public CurveType Type => type;
        public int Points => points;
        public bool IsClosed => isClosed;
        public IReadOnlyList<float3> Position => position;
        public IReadOnlyList<float3> Tangent => tangent;
        public IReadOnlyList<float3> Normal => normal;
        public IReadOnlyList<float3> Binormal => binormal;

        internal CurveData(CurveType type, int points, bool isClosed, List<float3> position, List<float3> tangent, List<float3> normal, List<float3> binormal) {
            this.type = type;
            this.isClosed = isClosed;
            this.points = points;
            this.position = position;
            this.tangent = tangent;
            this.normal = normal;
            this.binormal = binormal;
            
            Assert.AreEqual(points, position.Count);
            Assert.AreEqual(points, tangent.Count);
            Assert.AreEqual(points, normal.Count);
            Assert.AreEqual(points, binormal.Count);
        }

        object ICloneable.Clone() {
            return Clone();
        }

        internal CurveData Clone() {
            return new CurveData(type, points, isClosed, position, tangent, normal, binormal);
        }

        internal static CurveData Empty => new (CurveType.None, 0, false, new List<float3>(), new List<float3>(), new List<float3>(), new List<float3>());
    }
}