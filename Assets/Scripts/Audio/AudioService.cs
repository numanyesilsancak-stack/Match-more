using UnityEngine;

namespace Game.Audio
{
    public sealed class AudioService
    {
        private readonly AudioSource _bgm;
        private readonly AudioSource _sfx;

        public AudioService(AudioSource bgm, AudioSource sfx)
        {
            _bgm = bgm;
            _sfx = sfx;

            _bgm.loop = true;
            _bgm.playOnAwake = false;

            _sfx.loop = false;
            _sfx.playOnAwake = false;
        }

        public void SetMasterVolume(float v)
        {
            AudioListener.volume = Mathf.Clamp01(v);
        }

        public void PlayBgm(AudioClip clip, float volume = 1f)
        {
            if (!_bgm || clip == null) return;
            if (_bgm.clip == clip && _bgm.isPlaying) return;

            _bgm.clip = clip;
            _bgm.volume = Mathf.Clamp01(volume);
            _bgm.Play();
        }

        public void StopBgm()
        {
            if (_bgm) _bgm.Stop();
        }

        public void PlaySfx(AudioClip clip, float volume = 1f, float pitchMin = 1f, float pitchMax = 1f)
        {
            if (!_sfx || clip == null) return;

            _sfx.pitch = Random.Range(pitchMin, pitchMax);
            _sfx.PlayOneShot(clip, Mathf.Clamp01(volume));
            _sfx.pitch = 1f; // geri al
        }
    }
}