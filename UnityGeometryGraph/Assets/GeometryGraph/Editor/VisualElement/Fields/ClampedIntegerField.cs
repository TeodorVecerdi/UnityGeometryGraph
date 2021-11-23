using System;
using System.Globalization;
using GeometryGraph.Editor.Utils;
using UnityCommons;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class ClampedIntegerField : TextValueField<int> {
        private int? max;
        private int? min;

        public ClampedIntegerField() : this(null) {
        }

        public ClampedIntegerField(string label, int? min = null, int? max = null, int maxLength = -1)
            : base(label, maxLength, new Input()) {
            this.min = min;
            this.max = max;

            AddToClassList(IntegerField.ussClassName);
            AddToClassList("clamped-field");
            labelElement.AddToClassList(IntegerField.labelUssClassName);
            IntegerInput.AddToClassList(IntegerField.inputUssClassName);
            AddLabelDragger<int>();

            RegisterCallback<BlurEvent>(_ => ClampWithoutNotify());
        }

        public int? Min {
            get => min;
            set {
                min = value;
                if (value == null) return;
                if (this.value < min) this.value = (int)value;
            }
        }

        public int? Max {
            get => max;
            set {
                max = value;
                if (value == null) return;
                if (this.value > max) this.value = (int)value;
            }
        }

        private Input IntegerInput => (Input)textInputBase;

        private void ClampWithoutNotify() {
            int val = value;
            if (min != null) val = val.MinClamped((int)min);
            if (max != null) val = val.MaxClamped((int)max);
            SetValueWithoutNotify(val);
        }

        private void Clamp() {
            int val = value;
            if (min != null) val = val.MinClamped((int)min);
            if (max != null) val = val.MaxClamped((int)max);
            if (val != value) value = val;
        }

        protected override string ValueToString(int v) {
            return v.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        protected override int StringToValue(string str) {
            if (long.TryParse(str, out long num)) return MathUtils.ClampToInt(num);

            return default;
        }

        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue) {
            IntegerInput.ApplyInputDeviceDelta(delta, speed, startValue);
        }

        public new class UxmlFactory : UxmlFactory<ClampedIntegerField, UxmlTraits> {
        }

        public new class UxmlTraits : TextValueFieldTraits<int, UxmlIntAttributeDescription> {
        }

        private class Input : TextValueInput {
            internal Input() {
                formatString = "#######0";
            }

            private ClampedIntegerField ParentField => (ClampedIntegerField)parent;

            protected override string allowedCharacters => "0123456789-*/+%^()cosintaqrtelfundxvRL,=pPI#";

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue) {
                double dragSensitivity = NumericFieldDraggerUtility.CalculateIntDragSensitivity(startValue);
                float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
                long num = StringToValue(text) + (long)Math.Round(NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * dragSensitivity);

                if (ParentField.min != null) num = num.MinClamped((long)ParentField.min);
                if (ParentField.max != null) num = num.MaxClamped((long)ParentField.max);

                if (ParentField.isDelayed) text = ValueToString(MathUtils.ClampToInt(num));
                else ParentField.value = MathUtils.ClampToInt(num);
            }

            protected override string ValueToString(int v) {
                return v.ToString(formatString);
            }

            protected override int StringToValue(string str) {
                if (long.TryParse(str, out long num)) return MathUtils.ClampToInt(num);

                return default;
            }
        }
    }
}