using System;
using UnityEngine;

namespace GeometryGraph.Editor {
    public static class GUILayoutHelper {
        public static void BeginCenterVertically() {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
        }

        public static void EndCenterVertically() {
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        public static void CenterVertically(Action drawGUI) {
            BeginCenterVertically();
            drawGUI();
            EndCenterVertically();
        }
    }
}