using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace LegendsOfTianming.Core
{
    public class QuestSystem : MonoBehaviour
    {
        [Header("Quest Settings")]
        public int maxActiveQuests = 10;
        
        private Dictionary<string, Quest> availableQuests = new Dictionary<string, Quest>();
        private Dictionary<string, Quest> activeQuests = new Dictionary<string, Quest>();
        private Dictionary<string, Quest> completedQuests = new Dictionary<string, Quest>();
        private Character character;

        public event System.Action<Quest> OnQuestAccepted;
        public event System.Action<Quest> OnQuestCompleted;
        public event System.Action<Quest> OnQuestFailed;
        public event System.Action<Quest> OnQuestProgressUpdated;

        private void Start()
        {
            character = GetComponent<Character>();
            InitializeQuests();
        }

        private void InitializeQuests()
        {
            var starterQuests = new List<Quest>
            {
                new Quest
                {
                    id = "welcome_to_tianming",
                    title = "Welcome to Tianming",
                    description = "Learn the basics of martial arts combat. Defeat 5 Wild Rabbits to prove your skills.",
                    questType = QuestType.Kill,
                    level = 1,
                    region = "Qingze Plains",
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective
                        {
                            type = ObjectiveType.Kill,
                            targetId = "wild_rabbit",
                            targetName = "Wild Rabbit",
                            currentCount = 0,
                            requiredCount = 5,
                            description = "Defeat Wild Rabbits"
                        }
                    },
                    rewards = new QuestRewards
                    {
                        experience = 100,
                        gold = 50,
                        items = new List<string> { "health_potion_small" }
                    }
                },
                new Quest
                {
                    id = "first_equipment",
                    title = "Your First Equipment",
                    description = "Visit the Equipment Master and equip a weapon suitable for your class.",
                    questType = QuestType.Interact,
                    level = 1,
                    region = "Qingze Plains",
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective
                        {
                            type = ObjectiveType.Interact,
                            targetId = "equipment_master",
                            targetName = "Equipment Master",
                            currentCount = 0,
                            requiredCount = 1,
                            description = "Talk to Equipment Master"
                        },
                        new QuestObjective
                        {
                            type = ObjectiveType.Equip,
                            targetId = "starter_weapon",
                            targetName = "Starter Weapon",
                            currentCount = 0,
                            requiredCount = 1,
                            description = "Equip a weapon"
                        }
                    },
                    rewards = new QuestRewards
                    {
                        experience = 75,
                        gold = 25,
                        items = new List<string> { "starter_sword", "starter_staff" }
                    }
                },
                new Quest
                {
                    id = "skill_training",
                    title = "Skill Training",
                    description = "Practice your martial arts by using skills 10 times in combat.",
                    questType = QuestType.UseSkill,
                    level = 2,
                    region = "Qingze Plains",
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective
                        {
                            type = ObjectiveType.UseSkill,
                            targetId = "any_skill",
                            targetName = "Any Skill",
                            currentCount = 0,
                            requiredCount = 10,
                            description = "Use skills in combat"
                        }
                    },
                    rewards = new QuestRewards
                    {
                        experience = 150,
                        gold = 75,
                        items = new List<string> { "mana_potion_small" }
                    }
                }
            };

            foreach (var quest in starterQuests)
            {
                availableQuests[quest.id] = quest;
            }
        }

        public bool AcceptQuest(string questId)
        {
            if (!availableQuests.ContainsKey(questId))
            {
                Debug.LogWarning($"Quest not found: {questId}");
                return false;
            }

            if (activeQuests.Count >= maxActiveQuests)
            {
                Debug.LogWarning("Quest log is full!");
                return false;
            }

            if (activeQuests.ContainsKey(questId) || completedQuests.ContainsKey(questId))
            {
                Debug.LogWarning($"Quest already accepted or completed: {questId}");
                return false;
            }

            var quest = availableQuests[questId];
            
            if (character.level < quest.level)
            {
                Debug.LogWarning($"Level {quest.level} required for quest: {quest.title}");
                return false;
            }

            activeQuests[questId] = quest;
            quest.status = QuestStatus.Active;
            quest.acceptedTime = System.DateTime.Now;

            OnQuestAccepted?.Invoke(quest);
            Debug.Log($"📜 Quest accepted: {quest.title}");
            
            return true;
        }

        public void UpdateQuestProgress(ObjectiveType type, string targetId, int amount = 1)
        {
            foreach (var quest in activeQuests.Values)
            {
                bool questUpdated = false;
                
                foreach (var objective in quest.objectives)
                {
                    if (objective.type == type && 
                        (objective.targetId == targetId || objective.targetId == "any_skill" || objective.targetId == "any"))
                    {
                        if (objective.currentCount < objective.requiredCount)
                        {
                            objective.currentCount = Mathf.Min(objective.currentCount + amount, objective.requiredCount);
                            questUpdated = true;
                            
                            Debug.Log($"📈 Quest progress: {quest.title} - {objective.description} ({objective.currentCount}/{objective.requiredCount})");
                        }
                    }
                }
                
                if (questUpdated)
                {
                    OnQuestProgressUpdated?.Invoke(quest);
                    
                    if (IsQuestComplete(quest))
                    {
                        CompleteQuest(quest.id);
                    }
                }
            }
        }

        private bool IsQuestComplete(Quest quest)
        {
            return quest.objectives.All(obj => obj.currentCount >= obj.requiredCount);
        }

        public bool CompleteQuest(string questId)
        {
            if (!activeQuests.ContainsKey(questId))
            {
                Debug.LogWarning($"Quest not active: {questId}");
                return false;
            }

            var quest = activeQuests[questId];
            
            if (!IsQuestComplete(quest))
            {
                Debug.LogWarning($"Quest not complete: {quest.title}");
                return false;
            }

            AwardQuestRewards(quest);
            
            activeQuests.Remove(questId);
            completedQuests[questId] = quest;
            quest.status = QuestStatus.Completed;
            quest.completedTime = System.DateTime.Now;

            OnQuestCompleted?.Invoke(quest);
            Debug.Log($"✅ Quest completed: {quest.title}");
            
            return true;
        }

        private void AwardQuestRewards(Quest quest)
        {
            var rewards = quest.rewards;
            
            if (rewards.experience > 0)
            {
                Debug.Log($"🎉 Gained {rewards.experience} experience");
            }
            
            if (rewards.gold > 0)
            {
                character.Stats.AddGold(rewards.gold);
            }
            
            if (rewards.silver > 0)
            {
                character.Stats.AddSilver(rewards.silver);
            }
            
            var inventory = character.GetComponent<InventorySystem>();
            if (inventory != null && rewards.items != null)
            {
                foreach (var itemId in rewards.items)
                {
                    Debug.Log($"📦 Received item: {itemId}");
                }
            }
        }

        public bool AbandonQuest(string questId)
        {
            if (!activeQuests.ContainsKey(questId))
            {
                Debug.LogWarning($"Quest not active: {questId}");
                return false;
            }

            var quest = activeQuests[questId];
            activeQuests.Remove(questId);
            quest.status = QuestStatus.Abandoned;

            Debug.Log($"❌ Quest abandoned: {quest.title}");
            return true;
        }

        public List<Quest> GetAvailableQuests()
        {
            return availableQuests.Values
                .Where(q => !activeQuests.ContainsKey(q.id) && !completedQuests.ContainsKey(q.id))
                .Where(q => character.level >= q.level)
                .ToList();
        }

        public List<Quest> GetActiveQuests()
        {
            return activeQuests.Values.ToList();
        }

        public List<Quest> GetCompletedQuests()
        {
            return completedQuests.Values.ToList();
        }

        public Quest GetQuest(string questId)
        {
            if (activeQuests.ContainsKey(questId))
                return activeQuests[questId];
            
            if (completedQuests.ContainsKey(questId))
                return completedQuests[questId];
            
            if (availableQuests.ContainsKey(questId))
                return availableQuests[questId];
            
            return null;
        }

        public void OnMonsterKilled(string monsterId)
        {
            UpdateQuestProgress(ObjectiveType.Kill, monsterId);
        }

        public void OnNPCInteracted(string npcId)
        {
            UpdateQuestProgress(ObjectiveType.Interact, npcId);
        }

        public void OnItemEquipped(string itemId)
        {
            UpdateQuestProgress(ObjectiveType.Equip, itemId);
        }

        public void OnSkillUsed(string skillId)
        {
            UpdateQuestProgress(ObjectiveType.UseSkill, skillId);
            UpdateQuestProgress(ObjectiveType.UseSkill, "any_skill");
        }

        public void OnItemCollected(string itemId, int amount)
        {
            UpdateQuestProgress(ObjectiveType.Collect, itemId, amount);
        }
    }

    [System.Serializable]
    public class Quest
    {
        public string id;
        public string title;
        public string description;
        public QuestType questType;
        public int level;
        public string region;
        public QuestStatus status = QuestStatus.Available;
        public List<QuestObjective> objectives = new List<QuestObjective>();
        public QuestRewards rewards;
        public System.DateTime acceptedTime;
        public System.DateTime completedTime;
    }

    [System.Serializable]
    public class QuestObjective
    {
        public ObjectiveType type;
        public string targetId;
        public string targetName;
        public string description;
        public int currentCount;
        public int requiredCount;
    }

    [System.Serializable]
    public class QuestRewards
    {
        public int experience;
        public int gold;
        public int silver;
        public List<string> items;
    }

    public enum QuestType
    {
        Kill,
        Collect,
        Interact,
        UseSkill,
        Escort,
        Delivery
    }

    public enum QuestStatus
    {
        Available,
        Active,
        Completed,
        Failed,
        Abandoned
    }

    public enum ObjectiveType
    {
        Kill,
        Collect,
        Interact,
        UseSkill,
        Equip,
        Reach
    }
}
