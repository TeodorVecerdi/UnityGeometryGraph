using DG.Tweening;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class PrimitiveUVSequence : VideoSequence {
        public float EnterDuration = 1.0f;
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
            tween1 = CameraPivotTransform.DOMove(new Vector3(19, 0, 0), EnterDuration).From(Vector3.zero);
        }

        public override void Play(VideoDriver driver) {
            tween2 = DOVirtual.DelayedCall(EnterDuration, () => {
                MainTitle.text = "Primitive Meshes";
                Subtitle.text = "UVs";
            });

            if (tween1 != null) {
                tween1.OnComplete(() => {
                    tween3 = CameraPivotTransform.DOMove(new Vector3(28, 0, 0), PanDuration).From(new Vector3(19, 0, 0)).OnComplete(() => {
                        driver.Finish(this);
                    });
                });
            } else {
                tween3 = CameraPivotTransform.DOMove(new Vector3(28, 0, 0), PanDuration).From(new Vector3(19, 0, 0)).SetDelay(EnterDuration).OnComplete(() => {
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