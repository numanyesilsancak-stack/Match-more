using Game.Core;
using Game.Save;

namespace Game.Economy
{
    public interface ICurrencyService
    {
        int Gold { get; }
        void AddGold(int amount);
        bool TrySpendGold(int amount);
    }

    public sealed class CurrencyService : ICurrencyService
    {
        private readonly ISaveService _save;

        public int Gold => _save.Data.gold;

        public CurrencyService(ISaveService save)
        {
            _save = save;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            _save.Data.gold += amount;
            _save.Save();

            EventBus.RaiseGoldChanged(_save.Data.gold);
            EventBus.RaiseGoldEarned(amount, _save.Data.gold);
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0) return true;

            if (_save.Data.gold < amount)               
                return false;     
            
            _save.Data.gold -= amount;
            _save.Save();

            EventBus.RaiseGoldChanged(_save.Data.gold);
            EventBus.RaiseGoldSpent(amount, _save.Data.gold);
            return true;
        }
    }
}
