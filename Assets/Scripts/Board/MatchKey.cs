namespace Game.Board
{
    public static class MatchKey
    {
        // MatchFinder projede x + y*100 üretiyor
        private const int K = 100;

        public static int Encode(int x, int y) => x + y * K;

        public static void Decode(int p, out int x, out int y)
        {
            x = p % K;
            y = p / K;
        }
    }
}