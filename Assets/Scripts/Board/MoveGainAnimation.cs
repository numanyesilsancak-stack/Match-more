using DG.Tweening;
using System;
using UnityEngine;

namespace Game.UI
{
    public sealed class MoveGainAnimation : MonoBehaviour
    {
        [SerializeField] private RectTransform fxRect;
        [SerializeField] private CanvasGroup fxCanvasGroup;

        private Sequence _seq;
        private Vector2 _startPos;

        private void Awake()
        {
            if (fxRect != null)
                _startPos = fxRect.anchoredPosition;

            HideImmediate();
        }

        private void OnEnable()
        { 
            Game.Core.EventBus.MovesPurchased += OnMovesPurchased;
        }

        private void OnDisable()
        {
            Game.Core.EventBus.MovesPurchased -= OnMovesPurchased;
            _seq?.Kill();
        }

        private void OnMovesPurchased(int addedMoves, int newTotalMoves)
        {
            Play();
        }
        public void Play()
        {
            if (fxRect == null || fxCanvasGroup == null) return;

            _seq?.Kill();

            fxRect.gameObject.SetActive(true);
            fxRect.anchoredPosition = _startPos;
            fxRect.localScale = Vector3.one * 0.75f;
            fxCanvasGroup.alpha = 1f;

            _seq = DOTween.Sequence();

            _seq.Append(fxRect.DOScale(1.15f, 0.14f).SetEase(Ease.OutBack));
            _seq.Join(fxRect.DOAnchorPosY(_startPos.y + 18f, 0.16f).SetEase(Ease.OutQuad));

            _seq.Append(fxRect.DOScale(0.95f, 0.10f).SetEase(Ease.InOutQuad));

            _seq.AppendInterval(0.04f);

            _seq.Append(fxCanvasGroup.DOFade(0f, 0.18f));
            _seq.Join(fxRect.DOAnchorPosY(_startPos.y + 28f, 0.18f).SetEase(Ease.InQuad));
            _seq.Join(fxRect.DOScale(0.8f, 0.18f).SetEase(Ease.InQuad));

            _seq.OnComplete(HideImmediate);
        }

        private void HideImmediate()
        {
            if (fxCanvasGroup != null) fxCanvasGroup.alpha = 0f;

            if (fxRect != null)
            {
                fxRect.anchoredPosition = _startPos;
                fxRect.localScale = Vector3.one;
            }
        }
    }
}