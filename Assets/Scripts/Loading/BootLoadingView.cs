using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed class BootLoadingView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image logoImage;
        [SerializeField] private RectTransform spinner;
        [SerializeField] private TMP_Text loadingText;

        [Header("Timing")]
        [SerializeField] private float fadeIn = 0.18f;
        [SerializeField] private float minShowTime = 0.35f;
        [SerializeField] private float fadeOut = 0.18f;

        private Tween _spin;
        private Tween _dots;

        public float MinShowTime => minShowTime;

        private void Awake()
        {
            if (!group) group = GetComponent<CanvasGroup>();
            if (!group) group = gameObject.AddComponent<CanvasGroup>();

            group.alpha = 0f;
            group.blocksRaycasts = true;
        }

        public void PlayIn()
        {
            group.DOKill();
            logoImage?.DOKill();
            spinner?.DOKill();
            loadingText?.DOKill();

            group.alpha = 0f;

            if (logoImage)
            {
                // küçük "pop" hissi
                logoImage.color = new Color(1, 1, 1, 0f);
                logoImage.transform.localScale = Vector3.one * 0.96f;
            }

            if (spinner)
            {
                _spin?.Kill();
                spinner.localRotation = Quaternion.identity;
                _spin = spinner.DORotate(new Vector3(0, 0, -360f), 1.0f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1);
            }

            if (loadingText)
            {
                loadingText.text = "Loading";
                _dots?.Kill();
                _dots = DOTween.To(() => 0, v => SetDots(v), 3, 0.9f)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1);
            }

            var seq = DOTween.Sequence();
            seq.Append(group.DOFade(1f, fadeIn));

            if (logoImage)
            {
                seq.Join(logoImage.DOFade(1f, fadeIn));
                seq.Join(logoImage.transform.DOScale(1f, 0.22f).SetEase(Ease.OutBack));
            }
        }

        public void PlayOut(System.Action onComplete)
        {
            var seq = DOTween.Sequence();
            seq.Append(group.DOFade(0f, fadeOut));
            seq.OnComplete(() =>
            {
                _spin?.Kill();
                _dots?.Kill();
                onComplete?.Invoke();
            });
        }

        private void SetDots(int v)
        {
            if (!loadingText) return;
            loadingText.text = "Loading" + new string('.', v);
        }
    }
}
