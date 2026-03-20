
using Game.Economy;

namespace Game.Board
{
    public sealed class MovePurchaseService
    {
        private readonly ICurrencyService _currency;

        public MovePurchaseService(ICurrencyService currency)
        {
            _currency = currency;
        }

        public int GetCost(int levelIndex) => 150 + levelIndex * 3;

        public bool TryBuyMoves(LevelSession session, int levelIndex, int addMoves = 5)
        {
            int cost = GetCost(levelIndex);
            if (!_currency.TrySpendGold(cost))
                return false;

            session.AddMoves(addMoves);
            return true;
        }
    }
}