using DG.Tweening;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class InstancedRenderingSequence : VideoSequence {
        public float EnterDuration = 1.5f;
        public float PanDuration = 5.0f;
        private Tween tween1;
        private Tween tween2;
        private Tween tween3;
        private Tween tween4;

        public override void Enter() {
            tween1?.Kill();
            tween2?.Kill();
            tween3?.Kill();
            tween4?.Kill();

            gameObject.SetActive(true);
            tween1 = CameraPivotTransform.DOMove(new Vector3(39, -0.61f, -3), EnterDuration).From(new Vector3(33, 0, 0));
        }

        public override void Play(VideoDriver driver) {
            tween2 = DOVirtual.DelayedCall(0.0f, () => {
                MainTitle.text = "Instanced Rendering";
                Subtitle.text = "";
            });

            if (tween1 != null) {
                tween1.OnComplete(() => {
                    tween3 = CameraPivotTransform.DOMove(new Vector3(51, -0.61f, -3), PanDuration).From(new Vector3(39, -0.61f, -3)).OnComplete(() => {
                        driver.Finish(this);
                    });
                });
            } else {
                tween3 = CameraPivotTransform.DOMove(new Vector3(51, -0.61f, -3), PanDuration).From(new Vector3(39, -0.61f, -3)).SetDelay(EnterDuration).OnComplete(() => {
                    driver.Finish(this);
                });
            }
        }

        public override void Stop() {
            tween4 = DOVirtual.DelayedCall(3.0f, () => {
                gameObject.SetActive(false);
            });
        }
    }
}