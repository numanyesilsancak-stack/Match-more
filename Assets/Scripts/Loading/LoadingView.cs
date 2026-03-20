using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed class LoadingView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image progressFill;         // opsiyonel
        [SerializeField] private RectTransform spinner;      // opsiyonel

        [Header("Timing")]
        [SerializeField] private float fadeIn = 0.12f;
        [SerializeField] private float fadeOut = 0.12f;

        private Tween _spin;

        private void Awake()
        {
            if (!group) group = GetComponent<CanvasGroup>();
            if (!group) group = gameObject.AddComponent<CanvasGroup>();

            group.alpha = 0f;
            group.blocksRaycasts = false;

            if (progressFill) progressFill.fillAmount = 0f;
        }

        public void Show()
        {
            gameObject.SetActive(true);

            group.DOKill();
            group.blocksRaycasts = true;
            group.alpha = 0f;

            if (progressFill) progressFill.fillAmount = 0f;

            // spinner rotate
            _spin?.Kill();
            if (spinner)
            {
                spinner.localRotation = Quaternion.identity;
                _spin = spinner.DORotate(new Vector3(0, 0, -360f), 1.0f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1);
            }

            group.DOFade(1f, fadeIn);
        }

        public void SetProgress01(float p)
        {
            if (progressFill) progressFill.fillAmount = Mathf.Clamp01(p);
        }

        public void Hide()
        {
            group.DOKill();
            group.blocksRaycasts = false;

            group.DOFade(0f, fadeOut).OnComplete(() =>
            {
                _spin?.Kill();
                gameObject.SetActive(false);
            });
        }
    }
}
