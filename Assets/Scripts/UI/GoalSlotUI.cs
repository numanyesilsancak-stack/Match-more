using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Tek bir görev slot'u.
    /// Hierarchy: GoalSlot → [Icon(Image), ProgressText(TMP), TickRoot(GameObject) → TickIcon(Image)]
    /// </summary>
    public sealed class GoalSlotUI : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private GameObject tickRoot;      // tik container (başta kapalı)
        [SerializeField] private RectTransform tickRect;   // tik animasyon hedefi
        [SerializeField] private Image tickBg;             // opsiyonel: yeşil daire bg

        private Game.Board.TileType _type;
        private int _required;
        private bool _completed;

        // ── Init ─────────────────────────────────────────────────────────────

        public void Setup(Game.Board.TileType type, Sprite icon, int current, int required)
        {
            _type = type;
            _required = required;
            _completed = false;

            if (iconImage) iconImage.sprite = icon;
            if (tickRoot) tickRoot.SetActive(false);

            UpdateText(current, required);
        }

        // ── Progress güncellemesi ─────────────────────────────────────────────

        public void OnProgressChanged(Game.Board.TileType type, int current, int required)
        {
            if (type != _type || _completed) return;
            UpdateText(current, required);
        }

        // ── Goal tamamlandı ───────────────────────────────────────────────────

        public void OnGoalCompleted(Game.Board.TileType type)
        {
            if (type != _type || _completed) return;
            _completed = true;

            UpdateText(_required, _required); // metni "5/5" yap

            PlayTickAnimation();
        }

        // ── Yardımcılar ───────────────────────────────────────────────────────

        private void UpdateText(int current, int required)
        {
            if (!progressText) return;
            int clamped = Mathf.Clamp(current, 0, required);
            progressText.text = $"{clamped}/{required}";

            // Dolmaya yakınsa rengi sıcaklaştır
            progressText.color = (clamped >= required)
                ? new Color(0.2f, 0.85f, 0.3f)   // yeşil — tamamlandı
                : Color.white;
        }

        private void PlayTickAnimation()
        {
            if (!tickRoot) return;

            tickRoot.SetActive(true);

            if (tickRect)
            {
                tickRect.localScale = Vector3.zero;

                // 1) springe çıkar
                tickRect.DOScale(1.25f, 0.18f).SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        // 2) normale otur
                        tickRect.DOScale(1f, 0.10f).SetEase(Ease.InOutQuad);
                    });
            }

            // Arka plan varsa yeşile dön
            if (tickBg)
            {
                tickBg.color = new Color(0.2f, 0.85f, 0.3f, 0f);
                tickBg.DOFade(1f, 0.20f);
            }

            // Slot'un kendisi de hafif "hop" yapsın
            transform.DOPunchScale(Vector3.one * 0.12f, 0.25f, 5, 0.5f);
        }
    }
}