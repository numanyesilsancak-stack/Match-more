using Game.Audio;
using Game.Economy;
using Game.Save;

namespace Game.Core
{
    public static class Services
    {
        public static ISaveService Save { get; private set; }
        public static ICurrencyService Currency { get; private set; }

        public static AudioService Audio { get; set; }
        public static void Init()
        {
            Save = new SaveService();
            Save.Load();

            Currency = new CurrencyService(Save);
        }
    }
}
