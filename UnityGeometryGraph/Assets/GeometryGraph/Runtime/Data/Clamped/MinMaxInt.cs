using UnityCommons;

namespace GeometryGraph.Runtime.Data {
    public struct MinMaxInt {
        private int value;
        private int? min;
        private int? max;

        public int Value {
            get => Clamped();
            set => this.value = value;
        }

        public int? Min {
            get => min;
            set => min = value;
        }

        public int? Max {
            get => max;
            set => max = value;
        }

        public MinMaxInt(int value, int? min = null, int? max = null) {
            this.value = value;
            this.min = min;
            this.max = max;
        }

        private int Clamped() {
            int val = value;
            if (min != null) val = val.MinClamped((int)min);
            if (max != null) val = val.MaxClamped((int)max);
            return val;
        }

        public static implicit operator int(MinMaxInt minMaxInt) {
            return minMaxInt.Value;
        }

        public static implicit operator MinMaxInt(int @int) {
            return new MinMaxInt(@int);
        }
    }
}