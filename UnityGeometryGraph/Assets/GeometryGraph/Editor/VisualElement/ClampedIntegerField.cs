using System;
using System.Globalization;
using GeometryGraph.Editor.Utils;
using UnityCommons;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class ClampedIntegerField : TextValueField<int> {
        private int? min;
        private int? max;

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

        public ClampedIntegerField() : this(null) {
        }

        public ClampedIntegerField(string label, int? min = null, int? max = null, int maxLength = -1)
            : base(label, maxLength, new IntegerInput()) {
            this.min = min;
            this.max = max;
            
            AddToClassList(IntegerField.ussClassName);
            AddToClassList("clamped-field");
            labelElement.AddToClassList(IntegerField.labelUssClassName);
            integerInput.AddToClassList(IntegerField.inputUssClassName);
            AddLabelDragger<int>();
            
            RegisterCallback<BlurEvent>(_ => ClampWithoutNotify());
        }

        private IntegerInput integerInput => (IntegerInput)textInputBase;

        private void ClampWithoutNotify() {
            var val = value;
            if (min != null) val = val.Min((int)min);
            if (max != null) val = val.Max((int)max);
            SetValueWithoutNotify(val);
        }

        private void Clamp() {
            var val = value;
            if (min != null) val = val.Min((int)min);
            if (max != null) val = val.Max((int)max);
            if (val != value) value = val;
        }
        
        protected override string ValueToString(int v) {
            return v.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }
        
        protected override int StringToValue(string str) {
            if (long.TryParse(str, out var num)) {
                return (int) num.Clamped(int.MinValue, int.MaxValue);
            }

            return default;
        }

        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue) {
            integerInput.ApplyInputDeviceDelta(delta, speed, startValue);
        }


        public new class UxmlFactory : UxmlFactory<ClampedIntegerField, ClampedIntegerField.UxmlTraits> {
        }
        
        public new class UxmlTraits : TextValueFieldTraits<int, UxmlIntAttributeDescription> {
        }

        private class IntegerInput : TextValueInput {
            internal IntegerInput() {
                formatString = "#######0";
            }

            private ClampedIntegerField parentIntegerField => (ClampedIntegerField)parent;

            protected override string allowedCharacters => "0123456789-*/+%^()cosintaqrtelfundxvRL,=pPI#";

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue) {
                double intDragSensitivity = NumericFieldDraggerUtility.CalculateIntDragSensitivity(startValue);
                var acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
                var num = StringToValue(text) + (long)Math.Round(NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * intDragSensitivity);

                if (parentIntegerField.min != null) {
                    num = num.Min((long)parentIntegerField.min);
                }

                if (parentIntegerField.max != null) {
                    num = num.Max((long)parentIntegerField.max);
                }
                
                if (parentIntegerField.isDelayed) text = ValueToString((int) num.Clamped(int.MinValue, int.MaxValue));
                else parentIntegerField.value = (int) num.Clamped(int.MinValue, int.MaxValue);
            }

            protected override string ValueToString(int v) {
                return v.ToString(formatString);
            }

            protected override int StringToValue(string str) {
                if (long.TryParse(str, out var num)) {
                    return (int) num.Clamped(int.MinValue, int.MaxValue);
                }

                return default;
            }
        }
    }
}