using DG.Tweening;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class MorphSequence : VideoSequence {
        [SerializeField] private MorphPrimitive morpher;
        [SerializeField] private float morphDuration = 0.25f;
        [SerializeField] private float morphHoldDuration = 2.0f;
        private Sequence sequence;
        private Tweener cameraRotator;
        private Tween exitTween;

        public override void Enter() {
            sequence?.Kill();
            cameraRotator?.Kill();
            exitTween?.Kill();

            gameObject.SetActive(true);
        }

        public override void Play(VideoDriver driver) {
            MainTitle.text = "Primitive Meshes";
            Subtitle.text = "Cubes";
            morpher.Initialize();
            morpher.Morph(0.0f);
            sequence = DOTween.Sequence()
                   .AppendInterval(morphHoldDuration).AppendCallback(() => Subtitle.text = "Planes").Append(DOVirtual.Float(0.0f, 1.0f, morphDuration, value => morpher.Morph(value)))
                   .AppendInterval(morphHoldDuration).AppendCallback(() => Subtitle.text = "Circles").Append(DOVirtual.Float(1.0f, 2.0f, morphDuration, value => morpher.Morph(value)))
                   .AppendInterval(morphHoldDuration).AppendCallback(() => Subtitle.text = "Cylinders").Append(DOVirtual.Float(2.0f, 3.0f, morphDuration, value => morpher.Morph(value)))
                   .AppendInterval(morphHoldDuration).AppendCallback(() => Subtitle.text = "Cones").Append(DOVirtual.Float(3.0f, 4.0f, morphDuration, value => morpher.Morph(value)))
                   .AppendInterval(morphHoldDuration).AppendCallback(() => Subtitle.text = "Spheres").Append(DOVirtual.Float(4.0f, 5.0f, morphDuration, value => morpher.Morph(value)))
                   .AppendInterval(morphHoldDuration * 2.0f);
            cameraRotator = DOVirtual.Float(0.0f, 720.0f, morphDuration * 5 + morphHoldDuration * 7, value => {
                CameraPivotTransform.localEulerAngles = new Vector3(0.0f, value, 0.0f);
            }).OnComplete(() => {
                driver.Finish(this);
            });

            sequence.Play();
            cameraRotator.Play();
        }

        public override void Stop() {
            exitTween = DOVirtual.DelayedCall(3.0f, () => {
                gameObject.SetActive(false);
            });
        }

        /*[Button]
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
        }*/
    }
}