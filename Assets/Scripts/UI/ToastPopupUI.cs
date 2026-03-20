using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core;

namespace Game.UI
{
    public sealed class ToastPopupUI : MonoBehaviour, IPointerDownHandler
    {
        [Header("Refs")]
        [SerializeField] private CanvasGroup cg;
        [SerializeField] private RectTransform panel;
        [SerializeField] private TMP_Text label;

        [Header("Anim")]
        [SerializeField] private float inDur = 0.18f;
        [SerializeField] private float outDur = 0.14f;
        [SerializeField] private Ease inEase = Ease.OutBack;
        [SerializeField] private Ease outEase = Ease.InQuad;

        [Header("Options")]
        [SerializeField] private bool autoClose = false;
        [SerializeField] private float autoCloseDelay = 5;

        [Header("Gold Anim Spawn")]
        [Tooltip("Coin lerin çıkacağı nokta - board ın alt-ortasına konumlandır")]
        [SerializeField] private RectTransform goldSpawnPoint;

        private Sequence _seq;
        private Tween _auto;
        private bool _open;

        void Awake()
        {
            if (!cg) cg = GetComponent<CanvasGroup>();
            if (!panel) panel = transform.Find("Panel") as RectTransform;

            ForceHidden();
        }

        void OnEnable()
        {
            EventBus.Toast += Show;
            EventBus.GoldEarned += OnGoldEarned;
        }

        void OnDisable()
        {
            EventBus.Toast -= Show;
            EventBus.GoldEarned -= OnGoldEarned;
        }

        private void OnGoldEarned(int amount, int newTotal)
        {
            ShowGoldReward(amount);
        }

        // gold animasyonunu toast açılırken tetikle 
        public void ShowGoldReward(int amount)
        {
            Show($"{amount} Gold");

            Vector2 spawnPos = goldSpawnPoint != null
                ? goldSpawnPoint.anchoredPosition
                : new Vector2(0f, -300f);

            GoldFlyAnimation.Instance?.Play(amount, spawnPos);
        }
        public void Show(string msg)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            label.text = msg;

            _seq?.Kill();
            _auto?.Kill();

            _open = true;

            cg.alpha = 0f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            panel.localScale = Vector3.one * 0.92f;

            _seq = DOTween.Sequence()
                .Join(cg.DOFade(1f, inDur).SetEase(Ease.OutQuad))
                .Join(panel.DOScale(1f, inDur).SetEase(inEase));
                  
            if (autoClose)
                _auto = DOVirtual.DelayedCall(autoCloseDelay, Close, false);
        }
        
        public void Close()
        {
            if (!_open) { ForceHidden(); return; }
            _open = false;

            _seq?.Kill();
            _auto?.Kill();

            cg.interactable = false;
            cg.blocksRaycasts = false;

            _seq = DOTween.Sequence()
                .Join(cg.DOFade(0f, outDur).SetEase(outEase))
                .Join(panel.DOScale(0.96f, outDur).SetEase(outEase))
                .OnComplete(ForceHidden);
        }

        //  panele dokununca kapanır 
        public void OnPointerDown(PointerEventData eventData) => Close();

        private void ForceHidden()
        {
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        // "+100 gold", 150gold gibi formatları parse eder
        // sadece "+" ile başlayıp içinde sayı olan mesajları eşleşir
         /*private static bool TryParseGoldReward(string msg, out int amount)
        {
            amount = 0;
            if(string.IsNullOrEmpty(msg) || msg[0] != '+') return false;

            // '+' sonrasındaki sayıyı oku (boşluk veya harf gelene kadar)
            int i = 1;
            while (i < msg.Length && char.IsDigit(msg[i])) i++;

            return i > 1 && int.TryParse(msg.Substring(1, i - 1), out amount);
        }önceki kurulumda parse edip gold animasyonu tetikletiyorduk
         fakat bu hamledeki ve diğer metotlarda tetikleneceğini düşünüp iptal ettim.*/
    }
}