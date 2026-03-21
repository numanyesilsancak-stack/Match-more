using UnityEngine;

namespace Game.Audio
{
    public sealed class GlobalAudio : MonoBehaviour
    {
        public static GlobalAudio I { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Base Volumes")]
        [SerializeField, Range(0f, 1f)] private float baseBgmVolume = 0.8f;

        private float _bgmMul = 1f;
        private float _sfxMul = 1f;

        private void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);

            // Auto-wire sources if not assigned
            if (!bgmSource || !sfxSource)
            {
                var sources = GetComponentsInChildren<AudioSource>(true);
                if (sources.Length >= 2)
                {
                    bgmSource = sources[0];
                    sfxSource = sources[1];
                }
            }

            if (!bgmSource || !sfxSource)
            {
                Debug.LogError("GlobalAudio: bgmSource/sfxSource missing. AudioRig altında 2 AudioSource olmalı ve/veya inspector bağlanmalı.", this);
            }

            // Master
            AudioSettingsService.LoadAndApply();

            // Saved multipliers
            _bgmMul = AudioSettingsService.GetBgm(1f);
            _sfxMul = AudioSettingsService.GetSfx(1f);

            ApplyBgmVolume(_bgmMul);
            ApplySfxVolume(_sfxMul);

#if UNITY_EDITOR
            Debug.Log($"GlobalAudio ready. master={AudioListener.volume:0.00} bgmMul={_bgmMul:0.00} sfxMul={_sfxMul:0.00}", this);
#endif
        }

        public void ApplyBgmVolume(float mul01)
        {
            _bgmMul = Mathf.Clamp01(mul01);
            if (bgmSource)
                bgmSource.volume = baseBgmVolume * _bgmMul;
        }

        public void ApplySfxVolume(float mul01)
        {
            _sfxMul = Mathf.Clamp01(mul01);
        }

        public void PlayBgm(AudioClip clip)
        {
            if (!bgmSource || clip == null) return;

            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.loop = true;
            bgmSource.playOnAwake = false;

            bgmSource.clip = clip;
            bgmSource.volume = baseBgmVolume * _bgmMul;
            bgmSource.Play();
        }

        public void StopBgm()
        {
            if (bgmSource) bgmSource.Stop();
        }

        public void PlaySfx(AudioClip clip, float volume = 1f, float pitchMin = 1f, float pitchMax = 1f)
        {
            if (!sfxSource || clip == null) return;

            float pitch = (pitchMin == pitchMax) ? pitchMin : Random.Range(pitchMin, pitchMax);
            sfxSource.pitch = pitch;

            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume) * _sfxMul);

            sfxSource.pitch = 1f;
        }
    }
}