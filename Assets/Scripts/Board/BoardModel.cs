using System.Collections.Generic;

namespace Game.Board
{
    public sealed class BoardModel
    {
        public readonly int w, h;
        public TileType[,] types;
        public bool[,] blockers; // basit blocker

        public BoardModel(int w, int h)
        {
            this.w = w; this.h = h;
            types = new TileType[w, h];
            blockers = new bool[w, h];
        }
    }

    public static class MatchFinder
    {
        
        public static int FindAllMatches(BoardModel m, List<int> outMatches)
        {
            outMatches.Clear();
            int w = m.w, h = m.h;

            // Horizontal
            for (int y = 0; y < h; y++)
            {
                int run = 1;
                for (int x = 1; x < w; x++)
                {
                    if (m.blockers[x, y] || m.blockers[x - 1, y]) { run = 1; continue; }
                    if (m.types[x, y] == m.types[x - 1, y]) run++;
                    else
                    {
                        if (run >= 3)
                        {
                            for (int k = 0; k < run; k++)
                                outMatches.Add((x - 1 - k) + y * 100);
                        }
                        run = 1;
                    }
                }
                if (run >= 3)
                {
                    for (int k = 0; k < run; k++)
                        outMatches.Add((w - 1 - k) + y * 100);
                }
            }

            // Vertical
            for (int x = 0; x < w; x++)
            {
                int run = 1;
                for (int y = 1; y < h; y++)
                {
                    if (m.blockers[x, y] || m.blockers[x, y - 1]) { run = 1; continue; }
                    if (m.types[x, y] == m.types[x, y - 1]) run++;
                    else
                    {
                        if (run >= 3)
                        {
                            for (int k = 0; k < run; k++)
                                outMatches.Add(x + (y - 1 - k) * 100);
                        }
                        run = 1;
                    }
                }
                if (run >= 3)
                {
                    for (int k = 0; k < run; k++)
                        outMatches.Add(x + (h - 1 - k) * 100);
                }
            }

            // --- BLOK TESPITI ---

            // 2x2
            for (int y = 0; y < h - 1; y++)
                for (int x = 0; x < w - 1; x++)
                {
                    if (m.blockers[x, y] || m.blockers[x + 1, y] ||
                        m.blockers[x, y + 1] || m.blockers[x + 1, y + 1]) continue;

                    var t = m.types[x, y];
                    if (m.types[x + 1, y] == t && m.types[x, y + 1] == t && m.types[x + 1, y + 1] == t)
                    {
                        outMatches.Add(x + y * 100);
                        outMatches.Add((x + 1) + y * 100);
                        outMatches.Add(x + (y + 1) * 100);
                        outMatches.Add((x + 1) + (y + 1) * 100);
                    }
                }

            // 2x3 (2 satır, 3 sütun)
            for (int y = 0; y < h - 1; y++)
                for (int x = 0; x < w - 2; x++)
                {
                    if (m.blockers[x, y] || m.blockers[x + 1, y] || m.blockers[x + 2, y] ||
                        m.blockers[x, y + 1] || m.blockers[x + 1, y + 1] || m.blockers[x + 2, y + 1]) continue;

                    var t = m.types[x, y];
                    if (m.types[x + 1, y] == t && m.types[x + 2, y] == t &&
                        m.types[x, y + 1] == t && m.types[x + 1, y + 1] == t && m.types[x + 2, y + 1] == t)
                    {
                        outMatches.Add(x + y * 100);
                        outMatches.Add((x + 1) + y * 100);
                        outMatches.Add((x + 2) + y * 100);
                        outMatches.Add(x + (y + 1) * 100);
                        outMatches.Add((x + 1) + (y + 1) * 100);
                        outMatches.Add((x + 2) + (y + 1) * 100);
                    }
                }

            // 3x2 (3 satır, 2 sütun)
            for (int y = 0; y < h - 2; y++)
                for (int x = 0; x < w - 1; x++)
                {
                    if (m.blockers[x, y] || m.blockers[x + 1, y] ||
                        m.blockers[x, y + 1] || m.blockers[x + 1, y + 1] ||
                        m.blockers[x, y + 2] || m.blockers[x + 1, y + 2]) continue;

                    var t = m.types[x, y];
                    if (m.types[x + 1, y] == t &&
                        m.types[x, y + 1] == t && m.types[x + 1, y + 1] == t &&
                        m.types[x, y + 2] == t && m.types[x + 1, y + 2] == t)
                    {
                        outMatches.Add(x + y * 100);
                        outMatches.Add((x + 1) + y * 100);
                        outMatches.Add(x + (y + 1) * 100);
                        outMatches.Add((x + 1) + (y + 1) * 100);
                        outMatches.Add(x + (y + 2) * 100);
                        outMatches.Add((x + 1) + (y + 2) * 100);
                    }
                }

            // Dedup 
            var set = new HashSet<int>(outMatches);
            outMatches.Clear();
            outMatches.AddRange(set);
            return outMatches.Count;
        }
    }
}