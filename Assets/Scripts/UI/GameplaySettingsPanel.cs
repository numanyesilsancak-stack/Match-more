using DG.Tweening;
using Game.Audio;
using Game.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed class GameplaySettingsPanel : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform window;

        [Header("Settings UI")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backToMenuButton;

        [Header("Anim")]
        [SerializeField] private float fadeTime = 0.12f;
        [SerializeField] private float popTime = 0.18f;

        [Header("Audio")]
        [SerializeField] private AudioInPanel audioInPanel; //slider çağır
        [SerializeField] private AudioClip uiClick;
        private Sequence _seq;

        private void Awake()
        {
            if (!group) group = GetComponent<CanvasGroup>();
            if (!group) group = gameObject.AddComponent<CanvasGroup>();

            group.alpha = 0f;
            group.blocksRaycasts = false;
            

            if (closeButton) closeButton.onClick.AddListener(Hide);
            if (backToMenuButton) backToMenuButton.onClick.AddListener(BackToMenu);

           
        }

        private void OnDisable()
        {
            KillTweens();
        }

        private void OnDestroy()
        {
            KillTweens();

            InputGate.SetBlocked(false); // block takılı kalmasın diye kontrol(buglar için)

        }

        public void Show()
        {
            gameObject.SetActive(true);

            InputGate.SetBlocked(true);

            audioInPanel?.Show();
            GlobalAudio.I?.PlaySfx(uiClick);

            _seq?.Kill();
            group.DOKill();
            window?.DOKill();

            group.blocksRaycasts = true;
            group.alpha = 0f;

            if (window) window.localScale = Vector3.one * 0.90f;

            _seq = DOTween.Sequence();
            _seq.Join(group.DOFade(1f, fadeTime));
            if (window) _seq.Join(window.DOScale(1f, popTime).SetEase(Ease.OutBack));
        }

        public void Hide(System.Action onComplete)
        {
            KillTweens();
            group.blocksRaycasts = false;
            GlobalAudio.I?.PlaySfx(uiClick);

            _seq = DOTween.Sequence();
            _seq.Join(group.DOFade(0f, fadeTime));
            if (window) _seq.Join(window.DOScale(0.92f, fadeTime).SetEase(Ease.InQuad));

            _seq.OnComplete(() =>
            {
                gameObject.SetActive(false);
                InputGate.SetBlocked(false); //panel kapatılınca block bitsin
                onComplete?.Invoke();
            });
        }
        public void Hide()
        {
            Hide(null);
        }

        private void KillTweens()
        {
            _seq?.Kill();
            if (group) group.DOKill();
            if (window) window.DOKill();
        }     

        private void BackToMenu()
        {
            GlobalAudio.I?.PlaySfx(uiClick);
            Hide(() =>
            {
                SceneLoader.I.Load("MainMenu");
            });
        }
    }
}