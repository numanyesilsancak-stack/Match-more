using System.Collections.Generic;
using UnityEngine;
using Game.Levels;

namespace Game.Board
{
    public static class BoardInitializer
    {
        public static void Init(BoardModel model, BoardView view, LevelData level, List<int> matches)
        {
            view.Init(model.w, model.h);
            view.ClearAll();

            for (int y = 0; y < model.h; y++)
                for (int x = 0; x < model.w; x++)
                {
                    model.types[x, y] = RandomTypeAvoidingStartMatch(model, level, x, y);
                    view.SetTile(x, y, model.types[x, y]);
                }

            while (MatchFinder.FindAllMatches(model, matches) > 0)
                RerollMatches(model, view, level, matches);
        }

        private static TileType RandomTypeAvoidingStartMatch(BoardModel model, LevelData level, int x, int y)
        {
            int types = level.tileTypes;

            for (int attempt = 0; attempt < 20; attempt++)
            {
                var t = (TileType)Random.Range(0, types);

                if (x >= 2 && model.types[x - 1, y] == t && model.types[x - 2, y] == t) continue;
                if (y >= 2 && model.types[x, y - 1] == t && model.types[x, y - 2] == t) continue;

                return t;
            }

            return (TileType)Random.Range(0, types);
        }

        private static void RerollMatches(BoardModel model, BoardView view, LevelData level, List<int> matches)
        {
            foreach (var p in matches)
            {
                MatchKey.Decode(p, out int x, out int y);
                model.types[x, y] = (TileType)Random.Range(0, level.tileTypes);
                view.SetTile(x, y, model.types[x, y]);
            }
        }
    }
}