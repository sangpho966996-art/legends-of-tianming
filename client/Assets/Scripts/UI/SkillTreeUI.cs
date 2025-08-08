using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class SkillTreeUI : MonoBehaviour
    {
        [Header("Skill Tree Display")]
        public Transform skillNodeContainer;
        public GameObject skillNodePrefab;
        public Text availablePointsText;
        public Button resetSkillsButton;
        
        [Header("Skill Info Panel")]
        public GameObject skillInfoPanel;
        public Text skillNameText;
        public Text skillDescriptionText;
        public Text skillRankText;
        public Text skillDamageText;
        public Text skillCooldownText;
        public Text skillManaCostText;
        public Button learnSkillButton;
        
        private SkillTree skillTree;
        private Character playerCharacter;
        private Dictionary<string, SkillNodeUI> skillNodeUIs = new Dictionary<string, SkillNodeUI>();
        private SkillNode selectedSkillNode;

        private void Start()
        {
            InitializeSkillTreeUI();
            FindPlayerCharacter();
        }

        private void InitializeSkillTreeUI()
        {
            if (resetSkillsButton != null)
                resetSkillsButton.onClick.AddListener(OnResetSkills);
                
            if (learnSkillButton != null)
                learnSkillButton.onClick.AddListener(OnLearnSkill);
                
            if (skillInfoPanel != null)
                skillInfoPanel.SetActive(false);
        }

        private void FindPlayerCharacter()
        {
            var playerManager = GameManager.Instance?.GetPlayerManager();
            if (playerManager != null && playerManager.LocalPlayer != null)
            {
                playerCharacter = playerManager.LocalPlayer.GetComponent<Character>();
                if (playerCharacter != null)
                {
                    skillTree = playerCharacter.GetComponent<SkillTree>();
                    SubscribeToEvents();
                    CreateSkillNodes();
                    UpdateSkillTree();
                }
            }
        }

        private void SubscribeToEvents()
        {
            if (skillTree != null)
            {
                skillTree.OnSkillLearned += OnSkillLearned;
                skillTree.OnSkillUpgraded += OnSkillUpgraded;
                skillTree.OnSkillPointsChanged += UpdateAvailablePoints;
            }
        }

        private void CreateSkillNodes()
        {
            if (skillTree == null || skillNodeContainer == null || skillNodePrefab == null) return;
            
            var allSkills = skillTree.GetAllSkills();
            
            foreach (var skillNode in allSkills)
            {
                GameObject nodeObj = Instantiate(skillNodePrefab, skillNodeContainer);
                SkillNodeUI nodeUI = nodeObj.GetComponent<SkillNodeUI>();
                
                if (nodeUI == null)
                    nodeUI = nodeObj.AddComponent<SkillNodeUI>();
                
                nodeUI.Initialize(skillNode, this);
                skillNodeUIs[skillNode.id] = nodeUI;
                
                PositionSkillNode(nodeUI, skillNode);
            }
        }

        private void PositionSkillNode(SkillNodeUI nodeUI, SkillNode skillNode)
        {
            Vector2 position = GetSkillNodePosition(skillNode);
            nodeUI.transform.localPosition = position;
        }

        private Vector2 GetSkillNodePosition(SkillNode skillNode)
        {
            float tierSpacing = 200f;
            float nodeSpacing = 150f;
            
            Vector2 basePosition = new Vector2(-300f, 200f);
            
            switch (skillNode.tier)
            {
                case 1:
                    if (skillNode.id == "swift_strike" || skillNode.id == "iron_sweep")
                        return basePosition + new Vector2(0, 0);
                    else
                        return basePosition + new Vector2(nodeSpacing, 0);
                        
                case 2:
                    if (skillNode.id == "flowing_counter" || skillNode.id == "staff_vault")
                        return basePosition + new Vector2(tierSpacing, nodeSpacing);
                    else
                        return basePosition + new Vector2(tierSpacing, -nodeSpacing);
                        
                case 3:
                    return basePosition + new Vector2(tierSpacing * 2, 0);
                    
                default:
                    return basePosition;
            }
        }

        private void UpdateSkillTree()
        {
            if (skillTree == null) return;
            
            foreach (var kvp in skillNodeUIs)
            {
                var skillNode = skillTree.GetSkillNode(kvp.Key);
                if (skillNode != null)
                {
                    kvp.Value.UpdateNode(skillNode);
                }
            }
            
            UpdateAvailablePoints();
        }

        private void UpdateAvailablePoints()
        {
            if (skillTree == null || availablePointsText == null) return;
            
            int availablePoints = skillTree.GetAvailableSkillPoints();
            availablePointsText.text = $"Available Points: {availablePoints}";
        }

        public void OnSkillNodeClicked(SkillNode skillNode)
        {
            selectedSkillNode = skillNode;
            ShowSkillInfo(skillNode);
        }

        private void ShowSkillInfo(SkillNode skillNode)
        {
            if (skillInfoPanel == null) return;
            
            skillInfoPanel.SetActive(true);
            
            if (skillNameText != null)
                skillNameText.text = skillNode.name;
                
            if (skillDescriptionText != null)
                skillDescriptionText.text = skillNode.description;
                
            if (skillRankText != null)
                skillRankText.text = $"Rank: {skillNode.currentRank}/{skillNode.maxRank}";
                
            if (skillDamageText != null)
                skillDamageText.text = $"Damage: {skillNode.skillData.damage}";
                
            if (skillCooldownText != null)
                skillCooldownText.text = $"Cooldown: {skillNode.skillData.cooldown}s";
                
            if (skillManaCostText != null)
                skillManaCostText.text = $"Mana Cost: {skillNode.skillData.manaCost}";
                
            if (learnSkillButton != null)
            {
                bool canLearn = skillTree.CanLearnSkill(skillNode.id);
                learnSkillButton.interactable = canLearn;
                learnSkillButton.GetComponentInChildren<Text>().text = 
                    skillNode.currentRank == 0 ? "Learn" : "Upgrade";
            }
        }

        private void OnLearnSkill()
        {
            if (selectedSkillNode != null && skillTree != null)
            {
                skillTree.LearnSkill(selectedSkillNode.id);
            }
        }

        private void OnResetSkills()
        {
            if (skillTree != null)
            {
                skillTree.ResetSkills();
                UpdateSkillTree();
            }
        }

        private void OnSkillLearned(SkillNode skillNode)
        {
            UpdateSkillTree();
            if (selectedSkillNode != null && selectedSkillNode.id == skillNode.id)
            {
                ShowSkillInfo(skillNode);
            }
        }

        private void OnSkillUpgraded(SkillNode skillNode)
        {
            UpdateSkillTree();
            if (selectedSkillNode != null && selectedSkillNode.id == skillNode.id)
            {
                ShowSkillInfo(skillNode);
            }
        }

        public void SetPlayerCharacter(Character character)
        {
            playerCharacter = character;
            skillTree = character?.GetComponent<SkillTree>();
            
            if (skillTree != null)
            {
                SubscribeToEvents();
                CreateSkillNodes();
                UpdateSkillTree();
            }
        }
    }

    public class SkillNodeUI : MonoBehaviour
    {
        [Header("Node Components")]
        public Button nodeButton;
        public Image nodeIcon;
        public Text rankText;
        public Image backgroundImage;
        
        private SkillNode skillNode;
        private SkillTreeUI skillTreeUI;

        public void Initialize(SkillNode node, SkillTreeUI treeUI)
        {
            skillNode = node;
            skillTreeUI = treeUI;
            
            if (nodeButton == null)
                nodeButton = GetComponent<Button>();
                
            if (nodeIcon == null)
                nodeIcon = GetComponentInChildren<Image>();
                
            if (rankText == null)
                rankText = GetComponentInChildren<Text>();
                
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
                
            if (nodeButton != null)
                nodeButton.onClick.AddListener(() => skillTreeUI.OnSkillNodeClicked(skillNode));
        }

        public void UpdateNode(SkillNode node)
        {
            skillNode = node;
            
            if (rankText != null)
            {
                if (node.currentRank > 0)
                    rankText.text = $"{node.currentRank}/{node.maxRank}";
                else
                    rankText.text = "";
            }
            
            if (backgroundImage != null)
            {
                if (node.currentRank > 0)
                    backgroundImage.color = Color.green;
                else if (node.currentRank == 0 && CanLearnSkill(node))
                    backgroundImage.color = Color.yellow;
                else
                    backgroundImage.color = Color.gray;
            }
        }

        private bool CanLearnSkill(SkillNode node)
        {
            var skillTree = skillTreeUI.GetComponent<SkillTreeUI>();
            return skillTree != null;
        }
    }
}
