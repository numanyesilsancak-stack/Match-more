namespace Game.Core
{
    // board input açık mı kapalı mı bilgisini tutar
    public static class InputGate
    {
        public static bool Blocked { get; private set; }
        public static void SetBlocked(bool blocked) => Blocked = blocked;
    }
}