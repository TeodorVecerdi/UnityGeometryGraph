using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class MorphSequence : MonoBehaviour {
        [SerializeField] private MorphPrimitive morpher;
        [SerializeField] private float morphDuration = 0.25f;
        [SerializeField] private float morphHoldDuration = 2.0f;
        [SerializeField] private int stageCount = 5;
        [Space]
        [SerializeField] private bool enable;
        [SerializeField] private float time;
        [SerializeField] private bool isMorphing;
        [SerializeField] private bool isHolding;
        [SerializeField] private int stage;

        [Button]
        private void StartSequence() {
            isMorphing = false;
            isHolding = true;
            time = 0.0f;
            stage = -1;
            enable = true;
            morpher.Morph(0.0f);
        }

        [Button]
        private void StopSequence() {
            enable = false;
        }

        private void Update() {
            if (!enable) return;

            if (isMorphing) {
                time += Time.deltaTime;
                morpher.Morph(Mathf.Lerp(0.0f, 1.0f, time / morphDuration) + stage);
                if (time >= morphDuration) {
                    morpher.Morph(stage + 1);
                    isMorphing = false;
                    isHolding = true;
                    time = 0.0f;
                }
            }

            if (isHolding) {
                time += Time.deltaTime;
                if (time >= morphHoldDuration) {
                    isHolding = false;
                    isMorphing = true;
                    time = 0.0f;
                    stage++;

                    if (stage >= stageCount) {
                        enable = false;
                        stage = 0;
                    }
                }
            }
        }
    }
}