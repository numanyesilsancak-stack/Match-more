using Game.Core;
using UnityEngine;
using TMPro;

namespace Game.UI
{
    public sealed class GameplayHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text movesText;
        [SerializeField] private TMP_Text goldText;

        private void OnEnable()
        {
            EventBus.LevelChanged += OnLevel;
            EventBus.MovesChanged += OnMoves;
            EventBus.GoldChanged += OnGold;
        }

        private void OnDisable()
        {
            EventBus.LevelChanged -= OnLevel;
            EventBus.MovesChanged -= OnMoves;
            EventBus.GoldChanged -= OnGold;
        }

        private void Start()
        {
            // ilk değerler
            OnGold(Services.Currency.Gold);
            OnLevel(Services.Save.Data.currentLevel);
        }

        private void OnLevel(int v) => levelText.text = $"{v}";
        private void OnMoves(int v) => movesText.text = $"{v}";
        private void OnGold(int v) => goldText.text = v.ToString();
    }
}
