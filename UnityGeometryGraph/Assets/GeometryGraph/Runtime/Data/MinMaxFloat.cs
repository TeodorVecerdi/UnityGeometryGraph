using UnityCommons;

namespace GeometryGraph.Runtime.Data {
    public struct MinMaxFloat {
        private float value;
        private float? min;
        private float? max;
        
        public float Value {
            get => Clamped();
            set => this.value = value;
        }

        public float? Min {
            get => min;
            set => min = value;
        }
        
        public float? Max {
            get => max;
            set => max = value;
        }

        public MinMaxFloat(float value, float? min = null, float? max = null) {
            this.value = value;
            this.min = min;
            this.max = max;
        }
        
        private float Clamped() {
            float val = value;
            if (min != null) val = val.MinClamped((float)min);
            if (max != null) val = val.MaxClamped((float)max);
            return val;
        }

        public static implicit operator float(MinMaxFloat minMaxFloat) {
            return minMaxFloat.Value;
        }

        public static implicit operator MinMaxFloat(float @float) {
            return new MinMaxFloat(@float);
        }
    }
}