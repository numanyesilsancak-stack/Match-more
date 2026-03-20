using System.Collections.Generic;
using Game.Core;
using Game.Economy;
using Game.Levels;
using Game.Board;

namespace Game.Board
{
    public sealed class LevelSession
    {
        public int MovesLeft { get; private set; }
        public int Score { get; private set; }
        public int ComboStreak { get; private set; }
        public int ComboPointsTotal { get; private set; }

        private readonly LevelData _level;
        private readonly ICurrencyService _currency;

        // Goal takip: her TileType için kaç tane toplandı
        private readonly Dictionary<TileType, int> _collected = new();
        // Tamamlanan goal'ları tekrar event fırlatmamak için
        private readonly HashSet<TileType> _completedGoals = new();

        public LevelSession(LevelData level, ICurrencyService currency)
        {
            _level = level ?? throw new System.ArgumentNullException(nameof(level));
            _currency = currency ?? throw new System.ArgumentNullException(nameof(currency));

            MovesLeft = System.Math.Max(1, _level.moveLimit);
            Score = 0;
            ComboStreak = 0;
            ComboPointsTotal = 0;

            // Goal dictionary'yi sıfırla
            if (_level.goals != null)
            {
                foreach (var g in _level.goals)
                {
                    _collected[g.tileType] = 0;
                }
            }

            EventBus.RaiseMovesChanged(MovesLeft);
            EventBus.RaiseComboChanged(ComboStreak);

            // UI'a başlangıç goal durumlarını bildir
            BroadcastAllGoals();
        }

        public bool CanMakeMove => MovesLeft > 0;

        public void ConsumeMove()
        {
            if (MovesLeft <= 0) return;
            MovesLeft--;
            EventBus.RaiseMovesChanged(MovesLeft);
        }

        public void AddMoves(int amount)
        {
            if (amount <= 0) return;
            MovesLeft += amount;
            EventBus.RaiseMovesChanged(MovesLeft);
        }

        /// <summary>
        /// BoardResolver her tile patladığında çağırır.
        /// </summary>
        public void CollectTile(TileType type)
        {
            if (_level.goals == null) return;

            // Bu type bir goal'da mı?
            int required = -1;
            foreach (var g in _level.goals)
            {
                if (g.tileType == type) { required = g.requiredCount; break; }
            }
            if (required < 0) return; // bu renk bu levelin görevi değil

            _collected.TryGetValue(type, out int current);
            if (current >= required) return; // zaten dolmuş, sayma

            current++;
            _collected[type] = current;

            EventBus.RaiseGoalProgressChanged(type, current, required);

            // Tam doldu mu?
            if (current >= required && !_completedGoals.Contains(type))
            {
                _completedGoals.Add(type);
                EventBus.RaiseGoalCompleted(type);
            }
        }

        public void AddMatchScore(int clearedCount)
        {
            if (clearedCount <= 0) return;

            int baseScore = clearedCount * 60;

            ComboStreak++;
            EventBus.RaiseComboChanged(ComboStreak);

            int comboBonusScore = (ComboStreak - 1) * 80;
            int total = baseScore + comboBonusScore;

            Score += total;
            EventBus.RaiseScoreChanged(Score);

            int comboGold = (ComboStreak * clearedCount) / 2;
            ComboPointsTotal += comboGold;
        }

        public void ResetCombo()
        {
            if (ComboStreak == 0) return;
            ComboStreak = 0;
            EventBus.RaiseComboChanged(ComboStreak);
        }

        /// <summary>
        /// Tüm goal'lar tamamlandıysa kazanıldı.
        /// Goal tanımlanmamışsa kazanılamaz (tasarım güvencesi).
        /// </summary>
        public bool IsWin()
        {
            if (_level.goals == null || _level.goals.Length == 0) return false;

            foreach (var g in _level.goals)
            {
                _collected.TryGetValue(g.tileType, out int current);
                if (current < g.requiredCount) return false;
            }
            return true;
        }

        public bool IsLose() => MovesLeft <= 0 && !IsWin();

        public int CalculateWinGoldReward(int levelIndex)
        {
            if (levelIndex < 1) levelIndex = 1;

            float mult = _level.rewardMultiplier;
            if (mult <= 0f) mult = 1f;

            int baseGold = (int)((80 + levelIndex * 5) * mult);
            int movesLeftBonus = System.Math.Max(0, MovesLeft) * 8;
            int comboBonus = System.Math.Max(0, ComboPointsTotal);

            return System.Math.Max(0, baseGold + movesLeftBonus + comboBonus);
        }

        // ── Yardımcılar ──────────────────────────────────────────────────────

        /// <summary>
        /// Level yüklenince ya da yeni UI bağlanınca mevcut durumu broadcast et.
        /// </summary>
        public void BroadcastAllGoals()
        {
            if (_level.goals == null) return;
            foreach (var g in _level.goals)
            {
                _collected.TryGetValue(g.tileType, out int current);
                EventBus.RaiseGoalProgressChanged(g.tileType, current, g.requiredCount);
            }
        }

        /// <summary>
        /// GoalBarUI'ın başlangıçta slot oluşturması için level goals'a erişim.
        /// </summary>
        public Game.Levels.TileGoal[] GetGoals() => _level.goals;
    }
}