using System;
using Game.Board;

namespace Game.Core
{
    public static class EventBus
    {
        public static event Action<int> GoldChanged;
        public static event Action<int, int> GoldEarned;
        public static event Action<int, int> GoldSpent;

        public static event Action<int> LevelChanged;
        public static event Action<int> ScoreChanged;

        public static event Action<int> MovesChanged;
        public static event Action<int, int> MovesPurchased;

        public static event Action<int> ComboChanged;

        public static event Action<string> Toast;

        // Goal sistemi
        public static event Action<TileType, int, int> GoalProgressChanged; // type, current, required
        public static event Action<TileType> GoalCompleted;                  // type

        public static void RaiseGoldChanged(int value) => GoldChanged?.Invoke(value);
        public static void RaiseGoldEarned(int amount, int newTotal) => GoldEarned?.Invoke(amount, newTotal);
        public static void RaiseGoldSpent(int amount, int newTotal) => GoldSpent?.Invoke(amount, newTotal);

        public static void RaiseLevelChanged(int value) => LevelChanged?.Invoke(value);
        public static void RaiseComboChanged(int value) => ComboChanged?.Invoke(value);
        public static void RaiseMovesChanged(int newTotal) => MovesChanged?.Invoke(newTotal);
        public static void RaiseMovesPurchased(int addedMoves, int newTotalMoves) => MovesPurchased?.Invoke(addedMoves, newTotalMoves);
        public static void RaiseToast(string msg) => Toast?.Invoke(msg);
        public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);

        public static void RaiseGoalProgressChanged(TileType type, int current, int required)
            => GoalProgressChanged?.Invoke(type, current, required);

        public static void RaiseGoalCompleted(TileType type)
            => GoalCompleted?.Invoke(type);
    }
}