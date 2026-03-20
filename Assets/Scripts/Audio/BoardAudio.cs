using UnityEngine;
using Game.Audio;
using Game.Core;

namespace Game.Board
{
    public sealed class BoardAudio : MonoBehaviour
    {
        [Header("BGM")]
        [SerializeField] private AudioClip gameplayBgm;

        [Header("UI Click")]
        [SerializeField] private AudioClip uiClick;

        [Header("SFX - Swap")]
        [SerializeField] private AudioClip swapSfx;

        [Header("SFX - Match (3/4/5+)")]
        [SerializeField] private AudioClip sfxMatch3;
        [SerializeField] private AudioClip sfxMatch4;
        [SerializeField] private AudioClip sfxMatch5;

        [Header("SFX - Goal")]
        [SerializeField] private AudioClip sfxGoalComplete;

        [Header("Pitch Random")]
        [SerializeField] private bool randomPitch = true;
        [SerializeField] private float pitchMin = 0.98f;
        [SerializeField] private float pitchMax = 1.02f;

        private void Start()
        {
            if (GlobalAudio.I == null)
                Debug.LogError("GlobalAudio yok. Boot sahnesinde AudioRig/GlobalAudio var mı?", this);
            else if (gameplayBgm == null)
                Debug.LogWarning("BoardController: gameplayBgm Inspector'da boş.", this);
            else
                GlobalAudio.I.PlayBgm(gameplayBgm);

            GlobalAudio.I.ApplyBgmVolume(AudioSettingsService.GetBgm(1f));
            GlobalAudio.I.ApplySfxVolume(AudioSettingsService.GetSfx(1f));
            AudioSettingsService.ApplyMaster(AudioSettingsService.GetVolume(1f));
        }

        public void PlaySwap()
        {
            if (!swapSfx || GlobalAudio.I == null) return;
            if (randomPitch) GlobalAudio.I.PlaySfx(swapSfx, pitchMin, pitchMax);
            else GlobalAudio.I.PlaySfx(swapSfx, 1f, 1f);
        }

        public void PlayMatchSfx(int matchLen, int cascadeIndex)
        {
            float pMin = 0.98f + cascadeIndex * 0.02f;
            float pMax = 1.02f + cascadeIndex * 0.02f;

            switch (matchLen)
            {
                case 5:
                    GlobalAudio.I?.PlaySfx(sfxMatch5, 1f, pMin, pMax);
                    break;
                case 4:
                    GlobalAudio.I?.PlaySfx(sfxMatch4, 1f, pMin, pMax);
                    break;
                default:
                    GlobalAudio.I?.PlaySfx(sfxMatch3, 0.95f, pMin, pMax);
                    break;
            }
        }

        /// <summary>
        /// Bir goal tamamlandığında çağrılır.
        /// Inspector'da sfxGoalComplete slotuna uygun bir ses bağlayın.
        /// </summary>
        public void PlayGoalComplete()
        {
            if (!sfxGoalComplete || GlobalAudio.I == null) return;
            GlobalAudio.I.PlaySfx(sfxGoalComplete, 1f, 1f, 1f);
        }

        public void ClickSound()
        {
            GlobalAudio.I?.PlaySfx(uiClick);
        }
    }
}