using System.Collections.Generic;

namespace Game.Board
{
    public static class MatchMetrics
    {
        // 3/4/5 match ses tespiti — blok (2x2, 2x3, 3x2) → match4 ses çalar
        public static int GetMaxLineMatchLen(BoardModel model, List<int> matches)
        {
            // Önce blok var mı bak (2x2 yeterli, daha büyük blok da buradan geçer)
            if (HasBlock(model, matches)) return 4;

            int maxLen = 3;

            for (int i = 0; i < matches.Count; i++)
            {
                MatchKey.Decode(matches[i], out int x, out int y);
                var t = model.types[x, y];

                int h = 1;
                for (int xx = x - 1; xx >= 0 && model.types[xx, y] == t; xx--) h++;
                for (int xx = x + 1; xx < model.w && model.types[xx, y] == t; xx++) h++;

                int v = 1;
                for (int yy = y - 1; yy >= 0 && model.types[x, yy] == t; yy--) v++;
                for (int yy = y + 1; yy < model.h && model.types[x, yy] == t; yy++) v++;

                int localMax = (h > v) ? h : v;
                if (localMax > maxLen) maxLen = localMax;
                if (maxLen >= 5) return 5;
            }

            return maxLen >= 5 ? 5 : (maxLen == 4 ? 4 : 3);
        }

        // Match listesinde 2x2 veya daha büyük blok köşesi var mı?
        private static bool HasBlock(BoardModel model, List<int> matches)
        {
            var set = new HashSet<int>(matches);
            int w = model.w, h = model.h;

            foreach (var p in matches)
            {
                MatchKey.Decode(p, out int x, out int y);

                // Bu tile sol üst köşe kabul edersek 2x2 tüm köşeleri sette mi?
                if (x + 1 < w && y + 1 < h &&
                    set.Contains(MatchKey.Encode(x + 1, y)) &&
                    set.Contains(MatchKey.Encode(x, y + 1)) &&
                    set.Contains(MatchKey.Encode(x + 1, y + 1)))
                    return true;
            }

            return false;
        }
    }
}