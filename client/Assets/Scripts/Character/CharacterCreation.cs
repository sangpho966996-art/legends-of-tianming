using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class CharacterCreation : MonoBehaviour
    {
        [Header("UI References")]
        public InputField characterNameInput;
        public Dropdown classDropdown;
        public Button createButton;
        public Button backButton;
        public Text classDescriptionText;
        public Image classPreviewImage;
        
        [Header("Class Preview")]
        public GameObject[] classPreviewModels;
        public Transform previewSpawnPoint;
        
        private Dictionary<string, CharacterClassData> availableClasses;
        private GameObject currentPreviewModel;
        private string selectedClass = "";

        private void Start()
        {
            InitializeClasses();
            SetupUI();
        }

        private void InitializeClasses()
        {
            availableClasses = new Dictionary<string, CharacterClassData>
            {
                ["Azure Cloud Sect"] = new CharacterClassData
                {
                    className = "Azure Cloud Sect",
                    description = "Masters of swift swordplay and evasive techniques. Azure Cloud disciples excel in speed and counterattacks, flowing like wind through battle.",
                    baseStats = new CharacterStats { strength = 12, agility = 16, intelligence = 10, vitality = 12 },
                    startingSkills = new List<string> { "swift_strike", "cloud_step", "flowing_counter", "wind_blade" },
                    weaponType = "Sword",
                    playstyle = "Fast, evasive, counterattack-focused"
                },
                ["Iron Bell Sect"] = new CharacterClassData
                {
                    className = "Iron Bell Sect",
                    description = "Wielders of heavy staffs and masters of crowd control. Iron Bell monks use powerful strikes and stunning techniques to dominate the battlefield.",
                    baseStats = new CharacterStats { strength = 14, agility = 10, intelligence = 12, vitality = 14 },
                    startingSkills = new List<string> { "iron_sweep", "bells_resonance", "staff_vault", "earth_shaker" },
                    weaponType = "Staff",
                    playstyle = "Strong, defensive, crowd control"
                }
            };
        }

        private void SetupUI()
        {
            if (classDropdown != null)
            {
                classDropdown.ClearOptions();
                List<string> classNames = new List<string>(availableClasses.Keys);
                classDropdown.AddOptions(classNames);
                classDropdown.onValueChanged.AddListener(OnClassSelected);
            }

            if (createButton != null)
                createButton.onClick.AddListener(OnCreateCharacter);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            if (characterNameInput != null)
                characterNameInput.onValueChanged.AddListener(OnNameChanged);

            OnClassSelected(0);
        }

        private void OnClassSelected(int classIndex)
        {
            if (classIndex < 0 || classIndex >= availableClasses.Count) return;

            string[] classNames = new string[availableClasses.Count];
            availableClasses.Keys.CopyTo(classNames, 0);
            selectedClass = classNames[classIndex];

            var classData = availableClasses[selectedClass];
            
            if (classDescriptionText != null)
            {
                classDescriptionText.text = $"{classData.description}\n\n" +
                    $"Weapon: {classData.weaponType}\n" +
                    $"Playstyle: {classData.playstyle}\n\n" +
                    $"Base Stats:\n" +
                    $"Strength: {classData.baseStats.strength}\n" +
                    $"Agility: {classData.baseStats.agility}\n" +
                    $"Intelligence: {classData.baseStats.intelligence}\n" +
                    $"Vitality: {classData.baseStats.vitality}";
            }

            UpdateClassPreview(classIndex);
            ValidateForm();
        }

        private void UpdateClassPreview(int classIndex)
        {
            if (currentPreviewModel != null)
            {
                Destroy(currentPreviewModel);
            }

            if (classPreviewModels != null && classIndex < classPreviewModels.Length && classPreviewModels[classIndex] != null)
            {
                Vector3 spawnPos = previewSpawnPoint != null ? previewSpawnPoint.position : Vector3.zero;
                Quaternion spawnRot = previewSpawnPoint != null ? previewSpawnPoint.rotation : Quaternion.identity;
                
                currentPreviewModel = Instantiate(classPreviewModels[classIndex], spawnPos, spawnRot);
                
                var rotator = currentPreviewModel.AddComponent<PreviewModelRotator>();
                rotator.rotationSpeed = 30f;
            }
        }

        private void OnNameChanged(string newName)
        {
            ValidateForm();
        }

        private void ValidateForm()
        {
            bool isValid = !string.IsNullOrEmpty(characterNameInput?.text) && 
                          !string.IsNullOrEmpty(selectedClass) &&
                          IsValidCharacterName(characterNameInput?.text);

            if (createButton != null)
                createButton.interactable = isValid;
        }

        private bool IsValidCharacterName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            if (name.Length < 3 || name.Length > 16) return false;
            
            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }
            
            return true;
        }

        private void OnCreateCharacter()
        {
            if (!ValidateForm()) return;

            string characterName = characterNameInput.text.Trim();
            
            if (availableClasses.ContainsKey(selectedClass))
            {
                var classData = availableClasses[selectedClass];
                
                CharacterData newCharacter = new CharacterData
                {
                    id = System.Guid.NewGuid().ToString(),
                    name = characterName,
                    characterClass = selectedClass,
                    level = 1,
                    experience = 0,
                    stats = classData.baseStats,
                    position = Vector3.zero,
                    region = "Qingze Plains"
                };

                CreateCharacterOnServer(newCharacter, classData);
            }
        }

        private void CreateCharacterOnServer(CharacterData characterData, CharacterClassData classData)
        {
            var networkManager = GameManager.Instance.GetNetworkManager();
            
            if (networkManager.IsConnected)
            {
                var createData = new
                {
                    character = characterData,
                    startingSkills = classData.startingSkills
                };
                
                Debug.Log($"🎭 Creating character: {characterData.name} ({characterData.characterClass})");
                
                StartCharacterCreationProcess(characterData);
            }
            else
            {
                Debug.LogError("❌ Not connected to server. Cannot create character.");
                ShowErrorMessage("Connection error. Please try again.");
            }
        }

        private void StartCharacterCreationProcess(CharacterData characterData)
        {
            var playerManager = GameManager.Instance.GetPlayerManager();
            playerManager.SpawnLocalPlayer(characterData.id, characterData);
            
            var uiManager = GameManager.Instance.GetUIManager();
            if (uiManager != null)
            {
                uiManager.ShowGameplayUI();
            }
            
            gameObject.SetActive(false);
            
            Debug.Log($"✅ Character created successfully: {characterData.name}");
        }

        private void ShowErrorMessage(string message)
        {
            Debug.LogError($"❌ Character Creation Error: {message}");
        }

        private void OnBackClicked()
        {
            var uiManager = GameManager.Instance.GetUIManager();
            if (uiManager != null)
            {
                uiManager.ShowMainMenu();
            }
            
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (currentPreviewModel != null)
            {
                Destroy(currentPreviewModel);
            }
        }
    }

    [System.Serializable]
    public class CharacterClassData
    {
        public string className;
        public string description;
        public CharacterStats baseStats;
        public List<string> startingSkills;
        public string weaponType;
        public string playstyle;
    }

    public class PreviewModelRotator : MonoBehaviour
    {
        public float rotationSpeed = 30f;
        
        private void Update()
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
}
