using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject mainMenuPanel;
        public GameObject gameplayUI;
        public GameObject characterPanel;
        public GameObject inventoryPanel;
        public GameObject skillTreePanel;
        public GameObject chatPanel;
        public GameObject pvpPanel;
        
        [Header("HUD Elements")]
        public UnityEngine.UI.Slider healthBar;
        public UnityEngine.UI.Slider manaBar;
        public UnityEngine.UI.Text playerNameText;
        public UnityEngine.UI.Text levelText;
        
        private Dictionary<string, GameObject> uiPanels = new Dictionary<string, GameObject>();
        private string currentActivePanel = "";

        private void Start()
        {
            InitializeUI();
            ShowMainMenu();
        }

        private void InitializeUI()
        {
            uiPanels["mainMenu"] = mainMenuPanel;
            uiPanels["gameplay"] = gameplayUI;
            uiPanels["character"] = characterPanel;
            uiPanels["inventory"] = inventoryPanel;
            uiPanels["skillTree"] = skillTreePanel;
            uiPanels["chat"] = chatPanel;
            uiPanels["pvp"] = pvpPanel;
            
            foreach (var panel in uiPanels.Values)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
        }

        private void Update()
        {
            HandleUIInput();
        }

        private void HandleUIInput()
        {
            if (Input.GetKeyDown(KeyCode.C))
                TogglePanel("character");
            
            if (Input.GetKeyDown(KeyCode.I))
                TogglePanel("inventory");
            
            if (Input.GetKeyDown(KeyCode.K))
                TogglePanel("skillTree");
            
            if (Input.GetKeyDown(KeyCode.Enter))
                TogglePanel("chat");
            
            if (Input.GetKeyDown(KeyCode.P))
                TogglePanel("pvp");
            
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseAllPanels();
        }

        public void ShowMainMenu()
        {
            CloseAllPanels();
            ShowPanel("mainMenu");
        }

        public void ShowGameplayUI()
        {
            CloseAllPanels();
            ShowPanel("gameplay");
            UpdatePlayerHUD();
        }

        public void ShowPanel(string panelName)
        {
            if (uiPanels.ContainsKey(panelName) && uiPanels[panelName] != null)
            {
                uiPanels[panelName].SetActive(true);
                currentActivePanel = panelName;
                Debug.Log($"📱 Opened panel: {panelName}");
            }
        }

        public void HidePanel(string panelName)
        {
            if (uiPanels.ContainsKey(panelName) && uiPanels[panelName] != null)
            {
                uiPanels[panelName].SetActive(false);
                if (currentActivePanel == panelName)
                    currentActivePanel = "";
                Debug.Log($"📱 Closed panel: {panelName}");
            }
        }

        public void TogglePanel(string panelName)
        {
            if (uiPanels.ContainsKey(panelName) && uiPanels[panelName] != null)
            {
                bool isActive = uiPanels[panelName].activeSelf;
                if (isActive)
                    HidePanel(panelName);
                else
                    ShowPanel(panelName);
            }
        }

        public void CloseAllPanels()
        {
            foreach (var panel in uiPanels)
            {
                if (panel.Value != null)
                    panel.Value.SetActive(false);
            }
            currentActivePanel = "";
        }

        public void UpdatePlayerHUD()
        {
            var playerManager = GameManager.Instance.GetPlayerManager();
            if (playerManager.LocalPlayer != null)
            {
                var character = playerManager.LocalPlayer.GetComponent<Character>();
                if (character != null)
                {
                    UpdateHealthBar(character.Stats.CurrentHealth, character.Stats.MaxHealth);
                    UpdateManaBar(character.Stats.CurrentMana, character.Stats.MaxMana);
                    UpdatePlayerInfo(character.characterName, character.level);
                }
            }
        }

        public void UpdateHealthBar(int current, int max)
        {
            if (healthBar != null)
            {
                healthBar.value = max > 0 ? (float)current / max : 0f;
            }
        }

        public void UpdateManaBar(int current, int max)
        {
            if (manaBar != null)
            {
                manaBar.value = max > 0 ? (float)current / max : 0f;
            }
        }

        public void UpdatePlayerInfo(string playerName, int level)
        {
            if (playerNameText != null)
                playerNameText.text = playerName;
            
            if (levelText != null)
                levelText.text = $"Lv. {level}";
        }

        public void HandleChatMessage(Dictionary<string, object> data)
        {
            if (data.ContainsKey("message") && data.ContainsKey("playerId"))
            {
                string message = data["message"].ToString();
                string playerId = data["playerId"].ToString();
                
                Debug.Log($"💬 Chat: {playerId}: {message}");
            }
        }

        public void HandlePvPChallenge(Dictionary<string, object> data)
        {
            if (data.ContainsKey("challengerId") && data.ContainsKey("challengerName"))
            {
                string challengerId = data["challengerId"].ToString();
                string challengerName = data["challengerName"].ToString();
                
                Debug.Log($"⚔️ PvP Challenge from: {challengerName}");
            }
        }

        public void OnStartGameClicked()
        {
            ShowGameplayUI();
        }

        public void OnExitGameClicked()
        {
            Application.Quit();
        }

        public void OnCharacterCreateClicked()
        {
            Debug.Log("🎭 Character creation clicked");
        }

        public void OnSettingsClicked()
        {
            Debug.Log("⚙️ Settings clicked");
        }
    }
}
