using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Oyunun temel ayarlarını (FPS, Input, Ekran kapanma vs.) yönetir.
    /// </summary>
    public static class GameSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            // 1. VSync'i KAPAT (Böylece targetFrameRate çalışabilir)
            QualitySettings.vSyncCount = 0;

            // 2. Ekranın yenileme hızını al, geçerli değilse 60 yap
            int refreshRate = (int)Screen.currentResolution.refreshRateRatio.value;
            Application.targetFrameRate = refreshRate > 0 ? refreshRate : 60;

            // 3. Oyun oynarken ekranın uyku moduna geçmesini (kararmasını) engelle
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // 4. Oyunda aynı anda birden fazla dokunmayı engelle (sadece 1 input)
            UnityEngine.Input.multiTouchEnabled = false; 

            // 5. Ekran yönelimini kesinlikle sadece Dikey (Portrait) olarak sınırla (Ters dönmeyi ve yatay dönmeyi engelle)
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;

            Debug.Log($"[GameSetup] Oyun Ayarları Otomatik Kuruldu: VSync={QualitySettings.vSyncCount}, TargetFPS={Application.targetFrameRate}");
        }
    }
}
