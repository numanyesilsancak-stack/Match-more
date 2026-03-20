#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Game.Board;

namespace Game.Levels.EditorTools
{
    public static class LevelGenerator
    {
        private const string SpritePath = "Assets/Sprites/tile_images.png";

        [MenuItem("Game/Generate 20 Levels")]
        public static void Generate()
        {
            // Sprite'ları önceden yükle
            var spriteRed = LoadSprite("tile_root_red");
            var spriteBlue = LoadSprite("tile_root_blue");
            var spriteGreen = LoadSprite("tile_root_green");
            var spriteYellow = LoadSprite("tile_root_yellow");
            var spritePurple = LoadSprite("tile_root_purple");
            var spriteOrange = LoadSprite("tile_root_orange");

            const string folder = "Assets/ScriptableObjects/Levels";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Levels");

            for (int i = 1; i <= 20; i++)
            {
                var data = ScriptableObject.CreateInstance<LevelData>();
                data.levelIndex = i;
                data.width = 8;
                data.height = 8;
                data.moveLimit = 20;
                data.tileTypes = (i <= 3) ? 4 : (i <= 10) ? 5 : 6;
                data.blockerChance = Mathf.Clamp01((i - 10) / 100f) * 0.12f;
                data.rewardMultiplier = 1f + (i / 100f) * 0.8f;
                data.goals = BuildGoals(i,
                    spriteRed, spriteBlue, spriteGreen,
                    spriteYellow, spritePurple, spriteOrange);

                string path = $"{folder}/LevelData_{i:000}.asset";
                AssetDatabase.CreateAsset(data, path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated 20 LevelData assets (goal-based, sprites assigned).");
        }

        // ── Sprite yükleyici ──────────────────────────────────────────────────
        private static Sprite LoadSprite(string spriteName)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(SpritePath);
            foreach (var obj in all)
                if (obj is Sprite s && s.name == spriteName)
                    return s;

            Debug.LogWarning($"[LevelGenerator] Sprite bulunamadı: {spriteName} ({SpritePath})");
            return null;
        }

        // ── Goal tablosu ──────────────────────────────────────────────────────
        private static TileGoal[] BuildGoals(int lvl,
            Sprite red, Sprite blue, Sprite green,
            Sprite yellow, Sprite purple, Sprite orange)
        {
            switch (lvl)
            {
                case 1: return G(R(5, red), B(5, blue));
                case 2: return G(R(7, red), Gr(6, green));
                case 3: return G(B(8, blue), Y(7, yellow));
                case 4: return G(R(6, red), B(6, blue), Gr(5, green));
                case 5: return G(R(8, red), B(8, blue), Y(7, yellow));
                case 6: return G(Gr(10, green), P(8, purple), O(7, orange));
                case 7: return G(R(10, red), B(10, blue), Gr(9, green));
                case 8: return G(R(9, red), B(8, blue), Gr(8, green), Y(7, yellow));
                case 9: return G(R(10, red), B(10, blue), Gr(9, green), P(8, purple));
                case 10: return G(R(12, red), B(11, blue), Y(10, yellow), O(9, orange));
                case 11: return G(R(12, red), B(12, blue), Gr(11, green), P(10, purple));
                case 12: return G(R(11, red), B(10, blue), Gr(10, green), Y(9, yellow), P(9, purple));
                case 13: return G(R(13, red), B(12, blue), Gr(11, green), Y(10, yellow), O(9, orange));
                case 14: return G(R(14, red), B(13, blue), Gr(12, green), P(11, purple), O(10, orange));
                case 15: return G(R(15, red), B(14, blue), Gr(13, green), Y(12, yellow), P(11, purple));
                case 16: return G(R(13, red), B(12, blue), Gr(11, green), Y(11, yellow), P(10, purple), O(10, orange));
                case 17: return G(R(15, red), B(14, blue), Gr(13, green), Y(12, yellow), P(12, purple), O(11, orange));
                case 18: return G(R(16, red), B(15, blue), Gr(14, green), Y(13, yellow), P(13, purple), O(12, orange));
                case 19: return G(R(17, red), B(16, blue), Gr(15, green), Y(14, yellow), P(14, purple), O(13, orange));
                case 20: return G(R(18, red), B(17, blue), Gr(16, green), Y(15, yellow), P(15, purple), O(14, orange));
                default: return G(R(5, red), B(5, blue));
            }
        }

        // ── Kısaltma yardımcıları ─────────────────────────────────────────────
        static TileGoal[] G(params TileGoal[] goals) => goals;
        static TileGoal R(int n, Sprite s) => new TileGoal { tileType = TileType.RootRed, requiredCount = n, icon = s };
        static TileGoal B(int n, Sprite s) => new TileGoal { tileType = TileType.RootBlue, requiredCount = n, icon = s };
        static TileGoal Gr(int n, Sprite s) => new TileGoal { tileType = TileType.RootGreen, requiredCount = n, icon = s };
        static TileGoal Y(int n, Sprite s) => new TileGoal { tileType = TileType.RootYellow, requiredCount = n, icon = s };
        static TileGoal P(int n, Sprite s) => new TileGoal { tileType = TileType.RootPurple, requiredCount = n, icon = s };
        static TileGoal O(int n, Sprite s) => new TileGoal { tileType = TileType.RootOrange, requiredCount = n, icon = s };
    }
}
#endif