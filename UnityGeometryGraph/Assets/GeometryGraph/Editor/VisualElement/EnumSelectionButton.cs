using System;
using UnityCommons;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class EnumSelectionButton<T> : Button, INotifyValueChanged<T> where T : Enum {
        private SelectionTree tree;
        private T rawValue;

        internal EnumSelectionButton(T value, SelectionTree tree) : base() {
            SetValueWithoutNotify(value, 1);
            this.tree = tree;
            
            AddToClassList("enum-dropdown-button");
            var arrow = new VisualElement();
            arrow.AddToClassList("arrow-down");
            Add(arrow);
            
            clicked += () => {
                var pos = GUIUtility.GUIToScreenPoint(worldBound.position);
                SelectionWindow.ShowWindow(pos, worldBound.height, this.tree, selection => {
                    this.value = (T)selection;
                });
            };
        }

        public void SetValueWithoutNotify(T newValue, int scheduleNesting = 0) {
            rawValue = newValue;
            var buttonText = RandomUtilities.DisplayNameEnum(rawValue);
            text = buttonText;

            scheduleNesting = scheduleNesting.Clamped(0, 2);
            
            if (scheduleNesting == 0) {
                schedule.Execute(() => {
                    var size = MeasureTextSize(buttonText, 15, MeasureMode.Undefined, 0, MeasureMode.Undefined);
                    style.minWidth = size.x + 24f;
                });
            } else if (scheduleNesting == 1) {
                schedule.Execute(() => {
                    schedule.Execute(() => {
                        var size = MeasureTextSize(buttonText, 15, MeasureMode.Undefined, 0, MeasureMode.Undefined);
                        style.minWidth = size.x + 24f;
                    });
                });
            } else {
                schedule.Execute(() => {
                    schedule.Execute(() => {
                        schedule.Execute(() => {
                            var size = MeasureTextSize(buttonText, 15, MeasureMode.Undefined, 0, MeasureMode.Undefined);
                            style.minWidth = size.x + 24f;
                        });
                    });
                });
            }
                
        }

        void INotifyValueChanged<T>.SetValueWithoutNotify(T newValue) {
            SetValueWithoutNotify(newValue);
        }

        public T value {
            get => rawValue;
            set {
                using var pooled = ChangeEvent<T>.GetPooled(rawValue, value);
                pooled.target = this;
                SetValueWithoutNotify(value);
                SendEvent(pooled);
            }
        }
    }
}