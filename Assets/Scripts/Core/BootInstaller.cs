using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.UI;
using Game.Audio;

namespace Game.Core
{
    public sealed class BootInstaller : MonoBehaviour
    {
        [SerializeField] private BootLoadingView bootView;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);

            // (opsiyonel) kapasite
            DOTween.SetTweensCapacity(500, 50);

            // UI show
            if (bootView) bootView.PlayIn();

            // altyapı init
            Services.Init();

            //ses servisi
            AudioSettingsService.LoadAndApply();

            // minimum ekranda kalma süresi
            float wait = bootView ? bootView.MinShowTime : 0.2f;
            yield return new WaitForSeconds(wait);

            // fade out + scene load
            bool done = false;
            if (bootView)
            {
                bootView.PlayOut(() => done = true);
                while (!done) yield return null;
            }

            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
