using UnityEngine;

namespace Game.Audio
{
    public static class AudioSettingsService
    {
        // Master (senin mevcut)
        public const string VolumeKey = "main_volume";

        // Yeni
        public const string BgmKey = "bgm_volume";
        public const string SfxKey = "sfx_volume";

        public static float GetVolume(float defaultValue = 1f)
            => PlayerPrefs.GetFloat(VolumeKey, defaultValue);

        public static void SetVolume(float value)
        {
            value = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(VolumeKey, value);
            PlayerPrefs.Save();
            ApplyMaster(value);
        }

        public static void ApplyMaster(float value)
        {
            AudioListener.volume = Mathf.Clamp01(value);
        }

        public static float GetBgm(float defaultValue = 1f)
            => PlayerPrefs.GetFloat(BgmKey, defaultValue);

        public static void SetBgm(float value)
        {
            value = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(BgmKey, value);
            PlayerPrefs.Save();
        }

        public static float GetSfx(float defaultValue = 1f)
            => PlayerPrefs.GetFloat(SfxKey, defaultValue);

        public static void SetSfx(float value)
        {
            value = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxKey, value);
            PlayerPrefs.Save();
        }

        public static void LoadAndApply(float defaultMaster = 1f)
        {
            ApplyMaster(GetVolume(defaultMaster));
           
        }
    }
}