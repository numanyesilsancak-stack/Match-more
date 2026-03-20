using UnityEngine;

namespace Game.Levels
{
    [CreateAssetMenu(menuName = "Game/LevelData", fileName = "LevelData_001")]
    public class LevelData : ScriptableObject
    {
        public int levelIndex = 1;
        public int width = 8;
        public int height = 8;
        public int moveLimit = 20;
        public int tileTypes = 5;

        [Range(0f, 0.2f)] public float blockerChance = 0f; //next updates   
        [Range(0.5f, 2f)] public float rewardMultiplier = 1f; //next updates

        [Header("Goals")]
        public TileGoal[] goals;

        /*
        ── 20 LEVEL GOAL TASARIMI (devamında geliştirilebilir) ──────────────────

        Her levelde moveLimit = 20 sabit.
        tileTypes, zorluk arttıkça 4 → 5 → 6'ya çıkıyor.
        Hedefler gerçekçi cascade oranı hesaplanarak ayarlandı;
        son 4 levelde hamle satın almak neredeyse zorunlu hale gelir.

        LVL  tileTypes  Goals
        ───────────────────────────────────────────────────────
        01      4       Red:5,  Blue:5
        02      4       Red:7,  Green:6
        03      4       Blue:8, Yellow:7
        04      5       Red:6,  Blue:6,  Green:5
        05      5       Red:8,  Blue:8,  Yellow:7
        06      6       Green:10, Purple:8, Orange:7
        07      5       Red:10, Blue:10, Green:9
        08      5       Red:9,  Blue:8,  Green:8,  Yellow:7
        09      5       Red:10, Blue:10, Green:9,  Purple:8
        10      6       Red:12, Blue:11, Yellow:10, Orange:9
        11      6       Red:12, Blue:12, Green:11, Purple:10
        12      6       Red:11, Blue:10, Green:10, Yellow:9,  Purple:9
        13      6       Red:13, Blue:12, Green:11, Yellow:10, Orange:9
        14      6       Red:14, Blue:13, Green:12, Purple:11, Orange:10
        15      6       Red:15, Blue:14, Green:13, Yellow:12, Purple:11
        16      6       Red:13, Blue:12, Green:11, Yellow:11, Purple:10, Orange:10
        17      6       Red:15, Blue:14, Green:13, Yellow:12, Purple:12, Orange:11
        18      6       Red:16, Blue:15, Green:14, Yellow:13, Purple:13, Orange:12
        19      6       Red:17, Blue:16, Green:15, Yellow:14, Purple:14, Orange:13
        20      6       Red:18, Blue:17, Green:16, Yellow:15, Purple:15, Orange:14
        ────────────────────────────────────────────────────────────────────────*/
    }
}