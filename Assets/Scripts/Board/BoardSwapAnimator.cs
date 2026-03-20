using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Game.Board
{
    /// <summary>
    /// Sadece "swap" animasyonlarını yönet 
    /// Model / dictionary swap işine karışmaz
    /// </summary>
    public sealed class BoardSwapAnimator
    {
        private readonly BoardView _view;

        private readonly Ease _moveEase;
        private readonly float _punchScale;
        private readonly int _punchVibrato;
        private readonly float _punchElasticity;

        public BoardSwapAnimator(
            BoardView view,
            Ease moveEase = Ease.OutQuad,
            float punchScale = 0.08f,
            int punchVibrato = 6,
            float punchElasticity = 0.6f)
        {
            _view = view;
            _moveEase = moveEase;
            _punchScale = punchScale;
            _punchVibrato = punchVibrato;
            _punchElasticity = punchElasticity;
        }

        public IEnumerator Play(TileUI a, TileUI b, int ax, int ay, int bx, int by, float duration)
        {
            if (_view == null || a == null || b == null)
                yield break;

            // Beklenen başlangıç pozisyonlarına "snap" (drift/yarım piksel birikmesin)
            var aStart = _view.CellToLocal(ax, ay);
            var bStart = _view.CellToLocal(bx, by);
            a.Rt.anchoredPosition = aStart;
            b.Rt.anchoredPosition = bStart;

            var aTarget = bStart;
            var bTarget = aStart;

            // Üst üste tween birikmesini engelle
            a.Rt.DOKill(false);
            b.Rt.DOKill(false);
            a.transform.DOKill(false);
            b.transform.DOKill(false);

            var seq = DOTween.Sequence();
            seq.SetUpdate(isIndependentUpdate: false);

            seq.Join(a.Rt.DOAnchorPos(aTarget, duration).SetEase(_moveEase));
            seq.Join(b.Rt.DOAnchorPos(bTarget, duration).SetEase(_moveEase));

            // Hafif "punch" (istersen kapatırız, ama match-3 hissi iyi)
            if (_punchScale > 0f)
            {
                seq.Join(a.transform.DOPunchScale(Vector3.one * _punchScale, duration, _punchVibrato, _punchElasticity));
                seq.Join(b.transform.DOPunchScale(Vector3.one * _punchScale, duration, _punchVibrato, _punchElasticity));
            }

            yield return seq.WaitForCompletion();

            // Final snap
            a.Rt.anchoredPosition = aTarget;
            b.Rt.anchoredPosition = bTarget;
        }
    }
}