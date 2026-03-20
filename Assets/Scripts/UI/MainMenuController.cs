using Game.Core;
using Game.Save;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Audio;

namespace Game.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Main Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;


        [Header("Texts")]
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text levelText;

        [Header("Settings UI")]
        [SerializeField] private Button settingsButton;     
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private SettingsPanelView settingsPanelView;

              
        [SerializeField] private Button panelExitButton;    
        [SerializeField] private Button quitButton;         

        [Header("Audio")]
        [SerializeField] private AudioClip uiClick;
        
        [SerializeField] private AudioInPanel audioInPanel; //slider çağır


        private ISaveService _save;

        public void Construct(ISaveService save) => _save = save;

        private void Awake()
        {
            if (_save == null)
                _save = Services.Save;

            // Panel başlangıçta kapalı kalsın garantisi
            if (settingsPanel) settingsPanel.SetActive(false);

            
            
        }

        private void Start()
        {
            // Main
            startButton.onClick.AddListener(OnStart);
            continueButton.onClick.AddListener(OnContinue);

            // Settings
            if (settingsButton) settingsButton.onClick.AddListener(OpenSettings);
            if (panelExitButton) panelExitButton.onClick.AddListener(CloseSettings);
            if (quitButton) quitButton.onClick.AddListener(OnQuit);

           

            RefreshUI();

            EventBus.GoldChanged += OnGoldChanged;
            EventBus.LevelChanged += OnLevelChanged;
        }

        private void OnDestroy()
        {
            EventBus.GoldChanged -= OnGoldChanged;
            EventBus.LevelChanged -= OnLevelChanged;

            // Listener temizliği 
            
        }

        private void OnGoldChanged(int newGold)
        {
            if (goldText) goldText.text = newGold.ToString();
        }

        private void OnLevelChanged(int lvl)
        {
            if (levelText) levelText.text = $"Level {lvl}";
            if (continueButton) continueButton.interactable = lvl > 1;
        }

        private void RefreshUI()
        {
            goldText.text = _save.Data.gold.ToString();
            levelText.text = $"Level {_save.Data.currentLevel}";
            continueButton.interactable = _save.Data.currentLevel > 1;
        }

        private void OnStart()
        {
            GlobalAudio.I?.PlaySfx(uiClick, 1f, 0.98f, 1.02f);
            // prefab - Sceneloader


            _save.Data.currentLevel = 1;
            _save.Save();
            SceneLoader.I.Load("Gameplay");
        }

        private void OnContinue()
        {
            GlobalAudio.I?.PlaySfx(uiClick, 1f, 0.98f, 1.02f);

            SceneLoader.I.Load("Gameplay");
        }

        //  Settings Panel
        private void OpenSettings()
        {
            audioInPanel?.Show();
            GlobalAudio.I?.PlaySfx(uiClick);
            settingsPanelView?.Show();
        }

        private void CloseSettings()
        {
            GlobalAudio.I?.PlaySfx(uiClick);
            settingsPanelView?.Hide();
        }

        // Quit
        private void OnQuit()
        {
            GlobalAudio.I?.PlaySfx(uiClick);


#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
