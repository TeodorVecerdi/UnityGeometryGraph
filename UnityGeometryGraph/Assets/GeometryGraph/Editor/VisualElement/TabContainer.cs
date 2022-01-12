using System;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public class TabContainer : VisualElement {
        private readonly VisualElement tabButtonsContainer;
        private readonly VisualElement tabContentContainer;
        private int selectedIndex = -1;
        private Action<int> onTabSelected;

        public TabContainer(Action<int> onTabSelected = null) {
            this.onTabSelected = onTabSelected;
            AddToClassList("tab-container");

            tabButtonsContainer = new VisualElement();
            tabButtonsContainer.AddToClassList("tab-buttons-container");
            tabContentContainer = new VisualElement();
            tabContentContainer.AddToClassList("tab-content-container");

            Add(tabButtonsContainer);
            Add(tabContentContainer);
        }

        public VisualElement CreateTab(string label) {
            int index = tabButtonsContainer.childCount;

            VisualElement tabContent = new();
            tabContent.AddToClassList("tab-content");
            if (index == 0) {
                tabContent.AddToClassList("tab-content__first");
            }
            tabContent.userData = index;
            tabContentContainer.Add(tabContent);

            Button tabButton = new (() => SetActive(index)) {text = label};
            tabButton.AddToClassList("tab-button");
            if (index == 0) {
                tabButton.AddToClassList("tab-button__first");
            }
            tabButton.userData = index;
            tabButtonsContainer.Add(tabButton);

            return tabContent;
        }

        public void SetActive(int index) {
            if (selectedIndex == index) return;

            onTabSelected?.Invoke(index);

            if (selectedIndex != -1) {
                tabContentContainer[selectedIndex].RemoveFromClassList("tab-content__active");
                tabButtonsContainer[selectedIndex].RemoveFromClassList("tab-button__active");
            }
            selectedIndex = index;
            tabContentContainer[selectedIndex].AddToClassList("tab-content__active");
            tabButtonsContainer[selectedIndex].AddToClassList("tab-button__active");
        }
    }
}