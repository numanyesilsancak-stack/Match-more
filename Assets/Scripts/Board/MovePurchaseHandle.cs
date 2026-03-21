using Game.Core;
using Game.Economy;
using Game.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Board
{
    /// <summary>
    /// Hamle satın alma: tamamen BoardController dışına taşındı.
    /// UI butonundan tetiklenir, cost UI günceller, yetersiz gold -> toast.
    /// </summary>
    public sealed class MovePurchaseHandler : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private BoardController board;
        [SerializeField] private Button buyButton;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text goldText;

        [Header("Config")]
        [SerializeField] private int addMoves = 5;

        private ISaveService _save;
        private ICurrencyService _currency;
        private MovePurchaseService _service;

        private void Awake()
        {
            _save = Services.Save;
            _currency = Services.Currency;
            _service = new MovePurchaseService(_currency);

            if (!buyButton) buyButton = GetComponent<Button>();
            if (buyButton) buyButton.onClick.AddListener(BuyMoves);
        }

        private void OnEnable()
        {
            EventBus.LevelChanged += OnLevelChanged;
            EventBus.GoldChanged += OnGoldChanged;

            RefreshUI();
        }

        private void OnDisable()
        {
            EventBus.LevelChanged -= OnLevelChanged;
            EventBus.GoldChanged -= OnGoldChanged;
        }

        private void OnDestroy()
        {
            if (buyButton)
                buyButton.onClick.RemoveListener(BuyMoves);
        }

        private void OnLevelChanged(int lvl) => RefreshUI();

        private void OnGoldChanged(int gold)
        {
            if (goldText) goldText.text = gold.ToString();
            RefreshInteractable();
        }

        private int GetCost()
        {
            int lvl = Mathf.Clamp(_save.Data.currentLevel, 1, 20);
            return _service.GetCost(lvl);
        }

        public void RefreshUI()
        {
            if (costText) costText.text = GetCost().ToString();
            if (goldText) goldText.text = _save.Data.gold.ToString();
            RefreshInteractable();
        }

        private void RefreshInteractable()
        {
            if (!buyButton) return;

            int cost = GetCost();
            int gold = _save.Data.gold;

            buyButton.interactable = (board != null && board.Session != null && gold >= cost);
        }

        public void BuyMoves()
        {
            if (board == null || board.Session == null)
                return;

            int lvl = Mathf.Clamp(_save.Data.currentLevel, 1, 20);

            bool ok = _service.TryBuyMoves(board.Session, lvl, addMoves);
            if (!ok)
            {
                EventBus.RaiseToast("Not enough gold!");
                return;
            }

            int newMovesTotal = board.Session.MovesLeft;
            EventBus.RaiseMovesChanged(newMovesTotal);
            EventBus.RaiseMovesPurchased(addMoves, newMovesTotal);

            RefreshUI();
        }
    }
}