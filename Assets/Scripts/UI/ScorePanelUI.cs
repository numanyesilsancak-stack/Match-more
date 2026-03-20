using TMPro;
using UnityEngine;
using EventBus = Game.Core.EventBus;

namespace Game.UI
{
    // Şu an kullanılmıyor — GameObject Inspector'da deaktif bırakın.
    public sealed class ScorePanelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;

        private int _score;

        private void OnEnable() => EventBus.ScoreChanged += OnScoreChanged;
        private void OnDisable() => EventBus.ScoreChanged -= OnScoreChanged;

        private void OnScoreChanged(int score)
        {
            _score = score;
            if (scoreText) scoreText.text = $"Score: {_score}";
        }
    }
}