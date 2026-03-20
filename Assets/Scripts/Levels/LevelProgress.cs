using Game.Core;
using Game.Economy;
using Game.Save;

namespace Game.Levels
{
    public sealed class ProgressService
    {
        private const int MaxLevel = 20;
        private const int MinLevel = 1;

        private readonly ISaveService _save;
        private readonly ICurrencyService _currency;

        public ProgressService(ISaveService save, ICurrencyService currency)
        {
            _save = save;
            _currency = currency;

            // Save bozuksa açılışta düzelt
            NormalizeAndFixIfNeeded();
        }

        public int CurrentLevel => Normalize(_save.Data.currentLevel);

        public void CompleteLevel(int earnedGold)
        {
            if (earnedGold > 0)
                _currency.AddGold(earnedGold);

            int next = Normalize(_save.Data.currentLevel + 1);
            if (next != _save.Data.currentLevel)
            {
                _save.Data.currentLevel = next;
                _save.Save();
            }

            EventBus.RaiseLevelChanged(_save.Data.currentLevel);
        }

        private void NormalizeAndFixIfNeeded()
        {
            int normalized = Normalize(_save.Data.currentLevel);
            if (normalized != _save.Data.currentLevel)
            {
                _save.Data.currentLevel = normalized;
                _save.Save();
            }
        }

        private static int Normalize(int level)
        {
            if (level < MinLevel) return MinLevel;
            if (level > MaxLevel) return MaxLevel;
            return level;
        }
    }
}
