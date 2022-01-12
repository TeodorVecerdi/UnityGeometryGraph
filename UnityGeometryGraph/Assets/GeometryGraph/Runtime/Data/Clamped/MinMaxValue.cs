using System;
using UnityCommons;

namespace GeometryGraph.Runtime.Data {
    public struct MinMaxValue<T> where T : struct, IComparable<T> {
        private T value;
        private T? min;
        private T? max;

        public T Value {
            get => Clamped();
            set => this.value = value;
        }

        public T? Min {
            get => min;
            set => min = value;
        }

        public T? Max {
            get => max;
            set => max = value;
        }

        public MinMaxValue(T value, T? min = null, T? max = null) {
            this.value = value;
            this.min = min;
            this.max = max;
        }

        private T Clamped() {
            T val = value;
            if (min != null) val = val.MinClamped((T)min);
            if (max != null) val = val.MaxClamped((T)max);
            return val;
        }

        public static implicit operator T(MinMaxValue<T> minMax) {
            return minMax.Value;
        }

        public static implicit operator MinMaxValue<T>(T value) {
            return new MinMaxValue<T>(value);
        }
    }
}