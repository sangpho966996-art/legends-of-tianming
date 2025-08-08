using UnityEngine;
using UnityEngine.UI;

namespace LegendsOfTianming.Core
{
    public class GameplayHUD : MonoBehaviour
    {
        [Header("Player Info")]
        public Text playerNameText;
        public Text playerLevelText;
        public Text playerClassText;
        
        [Header("Health & Mana")]
        public Slider healthBar;
        public Slider manaBar;
        public Text healthText;
        public Text manaText;
        
        [Header("Skill Hotbar")]
        public Button[] skillButtons = new Button[8];
        public Image[] skillIcons = new Image[8];
        public Text[] skillCooldownTexts = new Text[8];
        
        [Header("Experience")]
        public Slider experienceBar;
        public Text experienceText;
        
        [Header("Mini Map")]
        public RawImage miniMapImage;
        public Text regionNameText;
        
        private Character playerCharacter;
        private SkillSystem playerSkills;

        private void Start()
        {
            InitializeHUD();
            FindPlayerCharacter();
        }

        private void Update()
        {
            UpdateSkillCooldowns();
        }

        private void InitializeHUD()
        {
            for (int i = 0; i < skillButtons.Length; i++)
            {
                int index = i;
                if (skillButtons[i] != null)
                {
                    skillButtons[i].onClick.AddListener(() => OnSkillButtonClicked(index));
                }
            }
        }

        private void FindPlayerCharacter()
        {
            var playerManager = GameManager.Instance?.GetPlayerManager();
            if (playerManager != null && playerManager.LocalPlayer != null)
            {
                playerCharacter = playerManager.LocalPlayer.GetComponent<Character>();
                if (playerCharacter != null)
                {
                    playerSkills = playerCharacter.GetComponent<SkillSystem>();
                    SubscribeToEvents();
                    UpdatePlayerInfo();
                }
            }
        }

        private void SubscribeToEvents()
        {
            if (playerCharacter != null)
            {
                playerCharacter.Stats.OnHealthChanged += UpdateHealthBar;
                playerCharacter.Stats.OnManaChanged += UpdateManaBar;
            }
        }

        private void UpdatePlayerInfo()
        {
            if (playerCharacter == null) return;
            
            if (playerNameText != null)
                playerNameText.text = playerCharacter.characterName;
                
            if (playerLevelText != null)
                playerLevelText.text = $"Lv.{playerCharacter.level}";
                
            if (playerClassText != null)
                playerClassText.text = playerCharacter.characterClass;
                
            if (regionNameText != null)
                regionNameText.text = "Qingze Plains";
                
            UpdateHealthBar(playerCharacter.Stats.CurrentHealth, playerCharacter.Stats.MaxHealth);
            UpdateManaBar(playerCharacter.Stats.CurrentMana, playerCharacter.Stats.MaxMana);
            UpdateExperienceBar();
            UpdateSkillHotbar();
        }

        private void UpdateHealthBar(int current, int max)
        {
            if (healthBar != null)
            {
                healthBar.value = (float)current / max;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{current}/{max}";
            }
        }

        private void UpdateManaBar(int current, int max)
        {
            if (manaBar != null)
            {
                manaBar.value = (float)current / max;
            }
            
            if (manaText != null)
            {
                manaText.text = $"{current}/{max}";
            }
        }

        private void UpdateExperienceBar()
        {
            if (playerCharacter == null || experienceBar == null) return;
            
            long currentExp = playerCharacter.experience;
            long requiredExp = CalculateRequiredExperience(playerCharacter.level + 1);
            long previousLevelExp = playerCharacter.level > 1 ? CalculateRequiredExperience(playerCharacter.level) : 0;
            
            float progress = (float)(currentExp - previousLevelExp) / (requiredExp - previousLevelExp);
            experienceBar.value = progress;
            
            if (experienceText != null)
            {
                experienceText.text = $"{currentExp - previousLevelExp}/{requiredExp - previousLevelExp}";
            }
        }

        private long CalculateRequiredExperience(int targetLevel)
        {
            return (long)(1000 * Mathf.Pow(targetLevel, 1.5f));
        }

        private void UpdateSkillHotbar()
        {
            if (playerSkills == null) return;
            
            var equippedSkills = playerSkills.GetEquippedSkills();
            
            for (int i = 0; i < skillButtons.Length; i++)
            {
                if (i < equippedSkills.Count && equippedSkills[i] != null)
                {
                    if (skillButtons[i] != null)
                        skillButtons[i].interactable = true;
                        
                    if (skillIcons[i] != null)
                    {
                        skillIcons[i].enabled = true;
                        skillIcons[i].color = Color.white;
                    }
                }
                else
                {
                    if (skillButtons[i] != null)
                        skillButtons[i].interactable = false;
                        
                    if (skillIcons[i] != null)
                    {
                        skillIcons[i].enabled = false;
                    }
                }
            }
        }

        private void UpdateSkillCooldowns()
        {
            if (playerSkills == null) return;
            
            var equippedSkills = playerSkills.GetEquippedSkills();
            
            for (int i = 0; i < skillCooldownTexts.Length && i < equippedSkills.Count; i++)
            {
                if (equippedSkills[i] != null && skillCooldownTexts[i] != null)
                {
                    float cooldown = playerSkills.GetSkillCooldown(equippedSkills[i].id);
                    
                    if (cooldown > 0)
                    {
                        skillCooldownTexts[i].text = cooldown.ToString("F1");
                        skillCooldownTexts[i].gameObject.SetActive(true);
                        
                        if (skillButtons[i] != null)
                            skillButtons[i].interactable = false;
                    }
                    else
                    {
                        skillCooldownTexts[i].gameObject.SetActive(false);
                        
                        if (skillButtons[i] != null)
                            skillButtons[i].interactable = true;
                    }
                }
            }
        }

        private void OnSkillButtonClicked(int index)
        {
            if (playerSkills == null) return;
            
            var equippedSkills = playerSkills.GetEquippedSkills();
            if (index < equippedSkills.Count && equippedSkills[index] != null)
            {
                playerSkills.CastSkill(equippedSkills[index].id);
            }
        }

        public void SetPlayerCharacter(Character character)
        {
            playerCharacter = character;
            playerSkills = character?.GetComponent<SkillSystem>();
            
            if (playerCharacter != null)
            {
                SubscribeToEvents();
                UpdatePlayerInfo();
            }
        }
    }
}
