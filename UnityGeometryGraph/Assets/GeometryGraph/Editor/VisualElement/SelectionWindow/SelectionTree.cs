using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityCommons;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    internal class SelectionTree : IEnumerable<SelectionCategory> {
        private const float k_ColumnWidth = 128.0f;
        private const float k_ColumnWidthMd = 128.0f + 16.0f;
        private const float k_ColumnWidthLg = 128.0f + 32.0f;
        private const float k_ColumnWidthXl = 128.0f + 48.0f;

        private readonly List<object> valueProvider;
        private readonly List<SelectionCategory> categories;

        public SelectionTree(List<object> valueProvider = null) {
            this.valueProvider = valueProvider;
            categories = new List<SelectionCategory>();
        }

        public float GetWidth() {
            var size = 0.0f;
            var currentSize = 0.0f;
            foreach (var category in categories) {
                if (!category.IsStacked) {
                    size += currentSize;
                    currentSize = 0.0f;
                }

                currentSize = Mathf.Max(currentSize, CategorySizeToSize(category.Size));
            }

            size += currentSize;
            return size;
        }

        public float GetHeight() {
            var maxHeight = 0.0f;
            var currentHeight = 0.0f;
            foreach (var category in categories) {
                if (!category.IsStacked) {
                    maxHeight = Mathf.Max(maxHeight, currentHeight);
                    currentHeight = 0.0f;
                }

                currentHeight += category.GetHeight();
            }

            maxHeight = Mathf.Max(maxHeight, currentHeight);
            return maxHeight;
        }

        public VisualElement CreateElement(EditorWindow window, Action<object> onSelect) {
            var root = new VisualElement();
            root.AddToClassList("tree-root");

            var actualSizes = CalculateActualSizes();

            for (int i = 0; i < categories.Count; i++) {
                var selectionCategory = categories[i];
                
                if (!selectionCategory.IsStacked && i > 0) {
                    var verticalSeparator = new VisualElement();
                    verticalSeparator.AddToClassList("tree-vertical-separator");
                    root.Add(verticalSeparator);
                }
                
                root.Add(selectionCategory.CreateElement(valueProvider, (SelectionCategory.CategorySize)actualSizes[i], onSelect, window));

            }

            return root;
        }

        private List<int> CalculateActualSizes() {
            var actualSizes = new List<int>(categories.Select(_ => -1));
            var currentSize = (int)categories[0].Size;
            for (var i = 0; i < categories.Count; i++) {
                var category = categories[i];
                
                if (!category.IsStacked) {
                    for (var i1 = i - 1; i1 >= 0; i1--) {
                        if (actualSizes[i1] != -1) break;
                        actualSizes[i1] = currentSize;
                    }

                    currentSize = (int)category.Size;
                }

                currentSize = Math.Max(currentSize, (int)category.Size);
            }

            for (var i = categories.Count - 1; i >= 0; i--) {
                if (actualSizes[i] != -1) break;
                actualSizes[i] = currentSize;
            }

            return actualSizes;
        }

        public IEnumerator<SelectionCategory> GetEnumerator() {
            return categories.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(SelectionCategory SelectionCategory) {
            categories.Add(SelectionCategory);
        }

        private static float CategorySizeToSize(SelectionCategory.CategorySize categorySize) {
            return categorySize switch {
                SelectionCategory.CategorySize.Normal => k_ColumnWidth,
                SelectionCategory.CategorySize.Medium => k_ColumnWidthMd,
                SelectionCategory.CategorySize.Large => k_ColumnWidthLg,
                SelectionCategory.CategorySize.ExtraLarge => k_ColumnWidthXl,
                _ => throw new ArgumentOutOfRangeException(nameof(categorySize), categorySize, null)
            };
        }

        public static SelectionCategory.CategorySize SizeToCategorySize(float size) {
            return size switch {
                k_ColumnWidthMd => SelectionCategory.CategorySize.Medium,
                k_ColumnWidthLg => SelectionCategory.CategorySize.Large,
                k_ColumnWidthXl => SelectionCategory.CategorySize.ExtraLarge,
                _ => SelectionCategory.CategorySize.Normal,
            };
        }
    }
}