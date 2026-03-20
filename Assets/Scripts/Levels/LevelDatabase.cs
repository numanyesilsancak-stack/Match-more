using UnityEngine;

namespace Game.Levels
{
    [CreateAssetMenu(menuName = "Game/LevelDatabase", fileName = "LevelDatabase")]
    public sealed class LevelDatabase : ScriptableObject
    {
        [SerializeField] private LevelData[] levels;

        public int Count => levels != null ? levels.Length : 0;

        public LevelData Get(int levelIndex)
        {
            if (levels == null || levels.Length == 0)
            {
                Debug.LogError("[LevelDatabase] Levels dizi null veya boş döndürüyor!");
                return null;
            }

            // 1-based → 0-based
            int clampedIndex = Mathf.Clamp(levelIndex - 1, 0, levels.Length - 1);
            LevelData data = levels[clampedIndex];

            if (data == null)
            {
                Debug.LogError($"[LevelDatabase] LevelData şu indeksde null çeviriyor: {clampedIndex}");
            }

            return data;
        }
    }
}
