using Game.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed class AudioInPanel : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        private bool _wired;

        private void OnEnable()
        {
            WireOnce();
            RefreshFromPrefs(); //  panel açılınca otomatik güncelle
        }

        private void OnDestroy()
        {
            if (masterSlider) masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
            if (bgmSlider) bgmSlider.onValueChanged.RemoveListener(OnBgmChanged);
            if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
        }

        private void WireOnce()
        {
            if (_wired) return;
            _wired = true;

            if (masterSlider) masterSlider.onValueChanged.AddListener(OnMasterChanged);
            if (bgmSlider) bgmSlider.onValueChanged.AddListener(OnBgmChanged);
            if (sfxSlider) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }

        public void RefreshFromPrefs()
        {
            if (masterSlider)
                masterSlider.SetValueWithoutNotify(AudioSettingsService.GetVolume(1f));

            if (bgmSlider)
                bgmSlider.SetValueWithoutNotify(AudioSettingsService.GetBgm(1f));

            if (sfxSlider)
                sfxSlider.SetValueWithoutNotify(AudioSettingsService.GetSfx(1f));

            // İsteğe bağlı: panel açıldığında da hemen uygula (slider oynamadan)
            GlobalAudio.I?.ApplyBgmVolume(AudioSettingsService.GetBgm(1f));
            GlobalAudio.I?.ApplySfxVolume(AudioSettingsService.GetSfx(1f));
            AudioSettingsService.ApplyMaster(AudioSettingsService.GetVolume(1f));
        }

        // Show u silip hataya sebep olmuştum yine show adıyla çağırmak istiyorum
        public void Show() => RefreshFromPrefs();

        private void OnMasterChanged(float v)
        {
            AudioSettingsService.SetVolume(v);
        }

        private void OnBgmChanged(float v)
        {
            AudioSettingsService.SetBgm(v);
            GlobalAudio.I?.ApplyBgmVolume(v);
        }

        private void OnSfxChanged(float v)
        {
            AudioSettingsService.SetSfx(v);
            GlobalAudio.I?.ApplySfxVolume(v);
        }

    }
}