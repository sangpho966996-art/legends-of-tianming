using UnityEngine;
using UnityEngine.UI;

namespace LegendsOfTianming.Core
{
    public class CharacterPanelUI : MonoBehaviour
    {
        [Header("Character Info")]
        public Text characterNameText;
        public Text characterClassText;
        public Text characterLevelText;
        public Text experienceText;
        
        [Header("Stats Display")]
        public Text strengthText;
        public Text agilityText;
        public Text intelligenceText;
        public Text vitalityText;
        
        [Header("Combat Stats")]
        public Text attackPowerText;
        public Text defenseText;
        public Text criticalChanceText;
        public Text attackSpeedText;
        
        [Header("Equipment Slots")]
        public Button weaponSlot;
        public Button armorSlot;
        public Button helmetSlot;
        public Button bootsSlot;
        public Button glovesSlot;
        public Button beltSlot;
        public Button ring1Slot;
        public Button ring2Slot;
        public Button necklaceSlot;
        
        [Header("Equipment Icons")]
        public Image weaponIcon;
        public Image armorIcon;
        public Image helmetIcon;
        public Image bootsIcon;
        public Image glovesIcon;
        public Image beltIcon;
        public Image ring1Icon;
        public Image ring2Icon;
        public Image necklaceIcon;
        
        private Character playerCharacter;
        private EquipmentSystem equipmentSystem;

        private void Start()
        {
            InitializePanel();
            FindPlayerCharacter();
        }

        private void InitializePanel()
        {
            if (weaponSlot != null)
                weaponSlot.onClick.AddListener(() => OnEquipmentSlotClicked("weapon"));
            if (armorSlot != null)
                armorSlot.onClick.AddListener(() => OnEquipmentSlotClicked("armor"));
            if (helmetSlot != null)
                helmetSlot.onClick.AddListener(() => OnEquipmentSlotClicked("helmet"));
            if (bootsSlot != null)
                bootsSlot.onClick.AddListener(() => OnEquipmentSlotClicked("boots"));
            if (glovesSlot != null)
                glovesSlot.onClick.AddListener(() => OnEquipmentSlotClicked("gloves"));
            if (beltSlot != null)
                beltSlot.onClick.AddListener(() => OnEquipmentSlotClicked("belt"));
            if (ring1Slot != null)
                ring1Slot.onClick.AddListener(() => OnEquipmentSlotClicked("ring1"));
            if (ring2Slot != null)
                ring2Slot.onClick.AddListener(() => OnEquipmentSlotClicked("ring2"));
            if (necklaceSlot != null)
                necklaceSlot.onClick.AddListener(() => OnEquipmentSlotClicked("necklace"));
        }

        private void FindPlayerCharacter()
        {
            var playerManager = GameManager.Instance?.GetPlayerManager();
            if (playerManager != null && playerManager.LocalPlayer != null)
            {
                playerCharacter = playerManager.LocalPlayer.GetComponent<Character>();
                if (playerCharacter != null)
                {
                    equipmentSystem = playerCharacter.GetComponent<EquipmentSystem>();
                    SubscribeToEvents();
                    UpdateCharacterInfo();
                }
            }
        }

        private void SubscribeToEvents()
        {
            if (playerCharacter != null)
            {
                playerCharacter.Stats.OnStatsChanged += UpdateCharacterInfo;
            }
        }

        private void UpdateCharacterInfo()
        {
            if (playerCharacter == null) return;
            
            UpdateBasicInfo();
            UpdateStats();
            UpdateCombatStats();
            UpdateEquipmentSlots();
        }

        private void UpdateBasicInfo()
        {
            if (characterNameText != null)
                characterNameText.text = playerCharacter.characterName;
                
            if (characterClassText != null)
                characterClassText.text = playerCharacter.characterClass;
                
            if (characterLevelText != null)
                characterLevelText.text = $"Level {playerCharacter.level}";
                
            if (experienceText != null)
            {
                long currentExp = playerCharacter.experience;
                long requiredExp = CalculateRequiredExperience(playerCharacter.level + 1);
                experienceText.text = $"EXP: {currentExp}/{requiredExp}";
            }
        }

        private void UpdateStats()
        {
            var stats = playerCharacter.Stats;
            
            if (strengthText != null)
                strengthText.text = $"Strength: {stats.Strength}";
                
            if (agilityText != null)
                agilityText.text = $"Agility: {stats.Agility}";
                
            if (intelligenceText != null)
                intelligenceText.text = $"Intelligence: {stats.Intelligence}";
                
            if (vitalityText != null)
                vitalityText.text = $"Vitality: {stats.Vitality}";
        }

        private void UpdateCombatStats()
        {
            var stats = playerCharacter.Stats;
            
            if (attackPowerText != null)
                attackPowerText.text = $"Attack Power: {stats.GetAttackPower()}";
                
            if (defenseText != null)
                defenseText.text = $"Defense: {stats.GetDefense()}";
                
            if (criticalChanceText != null)
                criticalChanceText.text = $"Critical Chance: {(stats.GetCriticalChance() * 100):F1}%";
                
            if (attackSpeedText != null)
                attackSpeedText.text = $"Attack Speed: {stats.GetAttackSpeed():F2}";
        }

        private void UpdateEquipmentSlots()
        {
            if (equipmentSystem == null) return;
            
            UpdateEquipmentIcon(weaponIcon, "weapon");
            UpdateEquipmentIcon(armorIcon, "armor");
            UpdateEquipmentIcon(helmetIcon, "helmet");
            UpdateEquipmentIcon(bootsIcon, "boots");
            UpdateEquipmentIcon(glovesIcon, "gloves");
            UpdateEquipmentIcon(beltIcon, "belt");
            UpdateEquipmentIcon(ring1Icon, "ring1");
            UpdateEquipmentIcon(ring2Icon, "ring2");
            UpdateEquipmentIcon(necklaceIcon, "necklace");
        }

        private void UpdateEquipmentIcon(Image icon, string slotType)
        {
            if (icon == null) return;
            
            icon.color = Color.gray;
            icon.enabled = true;
        }

        private void OnEquipmentSlotClicked(string slotType)
        {
            Debug.Log($"🎒 Equipment slot clicked: {slotType}");
        }

        private long CalculateRequiredExperience(int targetLevel)
        {
            return (long)(1000 * Mathf.Pow(targetLevel, 1.5f));
        }

        public void SetPlayerCharacter(Character character)
        {
            playerCharacter = character;
            equipmentSystem = character?.GetComponent<EquipmentSystem>();
            
            if (playerCharacter != null)
            {
                SubscribeToEvents();
                UpdateCharacterInfo();
            }
        }
    }
}
