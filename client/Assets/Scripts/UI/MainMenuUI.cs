using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace LegendsOfTianming.Core
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        public Button startGameButton;
        public Button characterCreationButton;
        public Button settingsButton;
        public Button exitButton;
        public GameObject settingsPanel;
        public Text versionText;
        
        [Header("Settings")]
        public Slider volumeSlider;
        public Dropdown qualityDropdown;
        public Toggle fullscreenToggle;
        public Button settingsBackButton;

        private void Start()
        {
            InitializeUI();
            LoadSettings();
        }

        private void InitializeUI()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGame);
                
            if (characterCreationButton != null)
                characterCreationButton.onClick.AddListener(OnCharacterCreation);
                
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettings);
                
            if (exitButton != null)
                exitButton.onClick.AddListener(OnExit);
                
            if (settingsBackButton != null)
                settingsBackButton.onClick.AddListener(OnSettingsBack);
                
            if (volumeSlider != null)
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
                
            if (qualityDropdown != null)
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
                
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
                
            if (versionText != null)
                versionText.text = "Version 1.0.0 - Step 7 Demo";
                
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        private void LoadSettings()
        {
            if (volumeSlider != null)
            {
                volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
                AudioListener.volume = volumeSlider.value;
            }
            
            if (qualityDropdown != null)
            {
                qualityDropdown.value = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
                QualitySettings.SetQualityLevel(qualityDropdown.value);
            }
            
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
            }
        }

        private void OnStartGame()
        {
            Debug.Log("🎮 Starting game...");
            SceneManager.LoadScene("GameWorld");
        }

        private void OnCharacterCreation()
        {
            Debug.Log("🎭 Opening character creation...");
            SceneManager.LoadScene("CharacterCreation");
        }

        private void OnSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        private void OnSettingsBack()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        private void OnExit()
        {
            Debug.Log("👋 Exiting game...");
            Application.Quit();
        }

        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat("Volume", value);
        }

        private void OnQualityChanged(int value)
        {
            QualitySettings.SetQualityLevel(value);
            PlayerPrefs.SetInt("Quality", value);
        }

        private void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }
    }
}
