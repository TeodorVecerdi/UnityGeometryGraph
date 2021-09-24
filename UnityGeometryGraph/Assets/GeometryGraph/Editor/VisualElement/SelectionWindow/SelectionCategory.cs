﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    internal class SelectionCategory : IEnumerable<SelectionEntry> {
        public enum CategorySize {Normal = 0, Medium = 1, Large = 2, ExtraLarge = 3}
        private const float k_BaseCategoryHeight = 24.0f + 8.0f;
        private readonly string title;
        private readonly bool isStacked;
        private readonly CategorySize size;
        private readonly List<SelectionEntry> entries;

        public bool IsStacked => isStacked;
        public CategorySize Size => size;

        public SelectionCategory(string title, bool isStacked, CategorySize size = CategorySize.Normal) {
            this.title = title;
            this.isStacked = isStacked;
            this.size = size;
            entries = new List<SelectionEntry>();
        }

        public float GetHeight() {
            var height = k_BaseCategoryHeight;
            foreach(var entry in entries) {
                height += entry.GetHeight();
            }
            return height;
        }

        public VisualElement CreateElement(List<object> valueProvider, CategorySize realSize, Action<object> onSelect, EditorWindow window) {
            var selectionCategory = new VisualElement();
            selectionCategory.AddToClassList("category");
            switch (realSize) {
                case CategorySize.Medium: selectionCategory.AddToClassList("md"); break;
                case CategorySize.Large: selectionCategory.AddToClassList("lg"); break;
                case CategorySize.ExtraLarge: selectionCategory.AddToClassList("xl"); break;
            }
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("category-title");
            selectionCategory.Add(titleLabel);

            var entriesContainer = new VisualElement();
            entriesContainer.AddToClassList("entries-container");
            selectionCategory.Add(entriesContainer);

            foreach (var entry in entries) {
                entriesContainer.Add(entry.CreateElement(valueProvider, onSelect, window));
            }

            if (!isStacked) {
                selectionCategory.AddToClassList("full-column");
            }

            return selectionCategory;
        }

        public IEnumerator<SelectionEntry> GetEnumerator() {
            return entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(SelectionEntry entry) {
            entries.Add(entry);
        }

        public void AddRange(IEnumerable<SelectionEntry> entries) {
            this.entries.AddRange(entries);
        }
    }
}