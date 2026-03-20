using DG.Tweening;
using Game.Core;
using UnityEngine;

namespace Game.UI
{
    public sealed class SettingsPanelView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform window;

        [Header("Anim")]
        [SerializeField] private float fadeTime = 0.12f;
        [SerializeField] private float popTime = 0.18f;

        private Sequence _seq;

        private void Awake()
        {
            if (!group) group = GetComponent<CanvasGroup>();
            if (!group) group = gameObject.AddComponent<CanvasGroup>();

            // Başlangıç güvenliği
            group.alpha = 0f;
            group.blocksRaycasts = false;
            
        }

        public void Show()
        {
            gameObject.SetActive(true);

            InputGate.SetBlocked(true);

            _seq?.Kill();
            group.blocksRaycasts = true;

            group.alpha = 0f;

            if (window)
                window.localScale = Vector3.one * 0.90f;

            _seq = DOTween.Sequence();
            _seq.Join(group.DOFade(1f, fadeTime));

            if (window)
                _seq.Join(window.DOScale(1f, popTime).SetEase(Ease.OutBack));
        }

        public void Hide()
        {
            _seq?.Kill();
            group.blocksRaycasts = false;

            _seq = DOTween.Sequence();
            _seq.Join(group.DOFade(0f, fadeTime));

            if (window)
                _seq.Join(window.DOScale(0.92f, fadeTime).SetEase(Ease.InQuad));

            _seq.OnComplete(() =>
            {
                gameObject.SetActive(false);
                InputGate.SetBlocked(false);
            });
        }
        private void OnDisable()
        {
            // panel sahneden kalkarsa kilit kalmasın
            InputGate.SetBlocked(false);

            // tween temizliği olası bug için
            _seq?.Kill();
            group?.DOKill();
            window?.DOKill();
        }
    }
}
