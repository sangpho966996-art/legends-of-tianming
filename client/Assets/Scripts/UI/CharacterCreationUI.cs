using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class CharacterCreationUI : MonoBehaviour
    {
        [Header("Character Info")]
        public InputField characterNameInput;
        public Text characterNameError;
        
        [Header("Class Selection")]
        public Transform classButtonContainer;
        public GameObject classButtonPrefab;
        public Text classDescriptionText;
        public Text classStatsText;
        
        [Header("Character Preview")]
        public Transform characterPreviewPoint;
        public Camera previewCamera;
        
        [Header("Stats Allocation")]
        public Text availablePointsText;
        public Button[] statIncreaseButtons;
        public Button[] statDecreaseButtons;
        public Text[] statValueTexts;
        public string[] statNames = { "Strength", "Agility", "Intelligence", "Vitality" };
        
        [Header("Action Buttons")]
        public Button createCharacterButton;
        public Button backToMenuButton;
        
        private List<ClassSelectionButton> classButtons = new List<ClassSelectionButton>();
        private CharacterCreationData currentCharacterData;
        private GameObject currentPreviewModel;
        private int availableStatPoints = 10;
        private int[] baseStats = { 10, 10, 10, 10 };
        private int[] allocatedStats = { 0, 0, 0, 0 };

        private void Start()
        {
            InitializeCharacterCreation();
            CreateClassButtons();
            SetupStatAllocation();
        }

        private void InitializeCharacterCreation()
        {
            currentCharacterData = new CharacterCreationData();
            
            if (characterNameInput != null)
            {
                characterNameInput.onValueChanged.AddListener(OnCharacterNameChanged);
                characterNameInput.text = "NewHero";
                currentCharacterData.characterName = "NewHero";
            }
            
            if (createCharacterButton != null)
                createCharacterButton.onClick.AddListener(OnCreateCharacter);
                
            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(OnBackToMenu);
                
            if (characterNameError != null)
                characterNameError.gameObject.SetActive(false);
                
            UpdateAvailablePointsDisplay();
        }

        private void CreateClassButtons()
        {
            if (classButtonContainer == null || classButtonPrefab == null) return;
            
            string[] classNames = {
                "Azure Cloud Sect",
                "Iron Bell Sect",
                "Shadow Veil Sect",
                "Wandering Spear Sect",
                "Spirit Lute Sect",
                "Stoneheart Sect"
            };
            
            string[] classDescriptions = {
                "Masters of swift sword techniques and wind-based abilities. Balanced offense and mobility.",
                "Defensive specialists wielding heavy staffs. High defense and area control abilities.",
                "Agile assassins using stealth and poison. High critical chance and mobility.",
                "Versatile warriors with long-range spear attacks. Balanced stats with good reach.",
                "Support specialists using music-based magic. Healing and buff abilities.",
                "Tank specialists with earth-based powers. Highest defense and crowd control."
            };
            
            for (int i = 0; i < classNames.Length; i++)
            {
                GameObject buttonObj = Instantiate(classButtonPrefab, classButtonContainer);
                ClassSelectionButton classButton = buttonObj.GetComponent<ClassSelectionButton>();
                
                if (classButton == null)
                    classButton = buttonObj.AddComponent<ClassSelectionButton>();
                
                classButton.Initialize(classNames[i], classDescriptions[i], this);
                classButtons.Add(classButton);
                
                if (i == 0)
                {
                    SelectClass(classNames[i], classDescriptions[i]);
                }
            }
        }

        private void SetupStatAllocation()
        {
            for (int i = 0; i < statIncreaseButtons.Length && i < statNames.Length; i++)
            {
                int index = i;
                if (statIncreaseButtons[i] != null)
                    statIncreaseButtons[i].onClick.AddListener(() => IncreaseStat(index));
                    
                if (statDecreaseButtons[i] != null)
                    statDecreaseButtons[i].onClick.AddListener(() => DecreaseStat(index));
            }
            
            UpdateStatDisplay();
        }

        public void SelectClass(string className, string description)
        {
            currentCharacterData.characterClass = className;
            
            if (classDescriptionText != null)
                classDescriptionText.text = description;
                
            UpdateClassStats(className);
            UpdateCharacterPreview(className);
            
            foreach (var button in classButtons)
            {
                button.SetSelected(button.ClassName == className);
            }
        }

        private void UpdateClassStats(string className)
        {
            if (classStatsText == null) return;
            
            string statsInfo = GetClassStatsInfo(className);
            classStatsText.text = statsInfo;
        }

        private string GetClassStatsInfo(string className)
        {
            switch (className)
            {
                case "Azure Cloud Sect":
                    return "Primary: Agility, Intelligence\nWeapon: Sword\nRole: DPS/Mobility";
                case "Iron Bell Sect":
                    return "Primary: Vitality, Strength\nWeapon: Staff\nRole: Tank/Support";
                case "Shadow Veil Sect":
                    return "Primary: Agility, Intelligence\nWeapon: Daggers\nRole: Assassin";
                case "Wandering Spear Sect":
                    return "Primary: Strength, Agility\nWeapon: Spear\nRole: DPS/Range";
                case "Spirit Lute Sect":
                    return "Primary: Intelligence, Vitality\nWeapon: Lute\nRole: Support/Healer";
                case "Stoneheart Sect":
                    return "Primary: Vitality, Strength\nWeapon: Hammer\nRole: Tank/Control";
                default:
                    return "Unknown class";
            }
        }

        private void UpdateCharacterPreview(string className)
        {
            if (currentPreviewModel != null)
            {
                Destroy(currentPreviewModel);
            }
            
            if (characterPreviewPoint != null)
            {
                currentPreviewModel = CreatePreviewModel(className);
                if (currentPreviewModel != null)
                {
                    currentPreviewModel.transform.SetParent(characterPreviewPoint);
                    currentPreviewModel.transform.localPosition = Vector3.zero;
                    currentPreviewModel.transform.localRotation = Quaternion.identity;
                }
            }
        }

        private GameObject CreatePreviewModel(string className)
        {
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            model.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            
            Renderer renderer = model.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = GetClassColor(className);
                renderer.material = material;
            }
            
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(model.transform);
            head.transform.localPosition = new Vector3(0, 0.8f, 0);
            head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            
            return model;
        }

        private Color GetClassColor(string className)
        {
            switch (className)
            {
                case "Azure Cloud Sect": return Color.cyan;
                case "Iron Bell Sect": return Color.gray;
                case "Shadow Veil Sect": return Color.black;
                case "Wandering Spear Sect": return Color.red;
                case "Spirit Lute Sect": return Color.magenta;
                case "Stoneheart Sect": return Color.yellow;
                default: return Color.white;
            }
        }

        private void OnCharacterNameChanged(string newName)
        {
            currentCharacterData.characterName = newName;
            ValidateCharacterName(newName);
        }

        private void ValidateCharacterName(string name)
        {
            bool isValid = !string.IsNullOrEmpty(name) && name.Length >= 3 && name.Length <= 20;
            
            if (characterNameError != null)
            {
                characterNameError.gameObject.SetActive(!isValid);
                if (!isValid)
                {
                    characterNameError.text = "Name must be 3-20 characters";
                }
            }
            
            if (createCharacterButton != null)
                createCharacterButton.interactable = isValid;
        }

        private void IncreaseStat(int statIndex)
        {
            if (availableStatPoints > 0 && allocatedStats[statIndex] < 10)
            {
                allocatedStats[statIndex]++;
                availableStatPoints--;
                UpdateStatDisplay();
                UpdateAvailablePointsDisplay();
            }
        }

        private void DecreaseStat(int statIndex)
        {
            if (allocatedStats[statIndex] > 0)
            {
                allocatedStats[statIndex]--;
                availableStatPoints++;
                UpdateStatDisplay();
                UpdateAvailablePointsDisplay();
            }
        }

        private void UpdateStatDisplay()
        {
            for (int i = 0; i < statValueTexts.Length && i < statNames.Length; i++)
            {
                if (statValueTexts[i] != null)
                {
                    int totalStat = baseStats[i] + allocatedStats[i];
                    statValueTexts[i].text = $"{statNames[i]}: {totalStat}";
                }
                
                if (statIncreaseButtons[i] != null)
                    statIncreaseButtons[i].interactable = availableStatPoints > 0 && allocatedStats[i] < 10;
                    
                if (statDecreaseButtons[i] != null)
                    statDecreaseButtons[i].interactable = allocatedStats[i] > 0;
            }
        }

        private void UpdateAvailablePointsDisplay()
        {
            if (availablePointsText != null)
                availablePointsText.text = $"Available Points: {availableStatPoints}";
        }

        private void OnCreateCharacter()
        {
            if (string.IsNullOrEmpty(currentCharacterData.characterName) || 
                string.IsNullOrEmpty(currentCharacterData.characterClass))
            {
                Debug.LogWarning("Character name or class not selected");
                return;
            }
            
            currentCharacterData.strength = baseStats[0] + allocatedStats[0];
            currentCharacterData.agility = baseStats[1] + allocatedStats[1];
            currentCharacterData.intelligence = baseStats[2] + allocatedStats[2];
            currentCharacterData.vitality = baseStats[3] + allocatedStats[3];
            
            Debug.Log($"🎭 Creating character: {currentCharacterData.characterName} ({currentCharacterData.characterClass})");
            
            var transitionManager = SceneTransitionManager.Instance;
            if (transitionManager != null)
            {
                transitionManager.LoadSceneWithCharacterData("GameWorld", currentCharacterData);
            }
        }

        private void OnBackToMenu()
        {
            var transitionManager = SceneTransitionManager.Instance;
            if (transitionManager != null)
            {
                transitionManager.LoadScene("MainMenu");
            }
        }
    }

    public class ClassSelectionButton : MonoBehaviour
    {
        [Header("Button Components")]
        public Button button;
        public Text classNameText;
        public Image backgroundImage;
        
        private string className;
        private string classDescription;
        private CharacterCreationUI creationUI;
        private bool isSelected = false;

        public string ClassName => className;

        public void Initialize(string name, string description, CharacterCreationUI ui)
        {
            className = name;
            classDescription = description;
            creationUI = ui;
            
            if (button == null)
                button = GetComponent<Button>();
                
            if (classNameText == null)
                classNameText = GetComponentInChildren<Text>();
                
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
                
            if (classNameText != null)
                classNameText.text = className;
                
            if (button != null)
                button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (creationUI != null)
            {
                creationUI.SelectClass(className, classDescription);
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            
            if (backgroundImage != null)
            {
                backgroundImage.color = selected ? Color.yellow : Color.white;
            }
        }
    }
}
