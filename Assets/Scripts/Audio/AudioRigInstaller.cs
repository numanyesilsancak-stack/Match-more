using Game.Core;
using UnityEngine;

namespace Game.Audio
{
    public sealed class AudioRigInstaller : MonoBehaviour
    {
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (!bgmSource || !sfxSource)
            {
                var sources = GetComponentsInChildren<AudioSource>(true);
                if (sources.Length >= 2)
                {
                    bgmSource = sources[0];
                    sfxSource = sources[1];
                }
            }

            Services.Audio = new AudioService(bgmSource, sfxSource);

            // AudioSettingsService çağır
            var v = AudioSettingsService.GetVolume(1f);
            Services.Audio.SetMasterVolume(v);
        }
    }
}