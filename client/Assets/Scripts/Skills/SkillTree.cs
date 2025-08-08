using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace LegendsOfTianming.Core
{
    public class SkillTree : MonoBehaviour
    {
        [Header("Skill Tree Settings")]
        public int maxSkillPoints = 100;
        public int skillPointsPerLevel = 2;
        
        private Dictionary<string, SkillNode> skillNodes = new Dictionary<string, SkillNode>();
        private Dictionary<string, int> spentSkillPoints = new Dictionary<string, int>();
        private int availableSkillPoints = 0;
        private Character character;

        public event System.Action<SkillNode> OnSkillLearned;
        public event System.Action<SkillNode> OnSkillUpgraded;
        public event System.Action OnSkillPointsChanged;

        private void Start()
        {
            character = GetComponent<Character>();
            InitializeSkillTree();
            CalculateAvailableSkillPoints();
        }

        private void InitializeSkillTree()
        {
            string characterClass = character.characterClass;
            
            if (characterClass == "Azure Cloud Sect")
            {
                InitializeAzureCloudSkillTree();
            }
            else if (characterClass == "Iron Bell Sect")
            {
                InitializeIronBellSkillTree();
            }
        }

        private void InitializeAzureCloudSkillTree()
        {
            skillNodes["swift_strike"] = new SkillNode
            {
                id = "swift_strike",
                name = "Swift Strike",
                description = "A quick sword strike that deals moderate damage with reduced cooldown.",
                tier = 1,
                maxRank = 5,
                currentRank = 1,
                skillPointCost = 1,
                levelRequirement = 1,
                prerequisites = new List<string>(),
                skillData = new Skill
                {
                    id = "swift_strike",
                    name = "Swift Strike",
                    damage = 80,
                    cooldown = 1.5f,
                    manaCost = 15,
                    range = 3f,
                    skillType = SkillType.Attack,
                    targetType = TargetType.Enemy,
                    damageType = DamageType.Physical
                }
            };

            skillNodes["cloud_step"] = new SkillNode
            {
                id = "cloud_step",
                name = "Cloud Step",
                description = "Dash forward quickly, avoiding enemy attacks and positioning for counterattacks.",
                tier = 1,
                maxRank = 3,
                currentRank = 1,
                skillPointCost = 1,
                levelRequirement = 3,
                prerequisites = new List<string>(),
                skillData = new Skill
                {
                    id = "cloud_step",
                    name = "Cloud Step",
                    damage = 0,
                    cooldown = 8f,
                    manaCost = 25,
                    range = 8f,
                    skillType = SkillType.Movement,
                    targetType = TargetType.None,
                    damageType = DamageType.Physical
                }
            };

            skillNodes["flowing_counter"] = new SkillNode
            {
                id = "flowing_counter",
                name = "Flowing Counter",
                description = "Counter enemy attacks with increased damage. Must be used within 2 seconds of taking damage.",
                tier = 2,
                maxRank = 4,
                currentRank = 0,
                skillPointCost = 2,
                levelRequirement = 12,
                prerequisites = new List<string> { "swift_strike" },
                skillData = new Skill
                {
                    id = "flowing_counter",
                    name = "Flowing Counter",
                    damage = 150,
                    cooldown = 6f,
                    manaCost = 30,
                    range = 3f,
                    skillType = SkillType.Counter,
                    targetType = TargetType.Enemy,
                    damageType = DamageType.Physical
                }
            };

            skillNodes["wind_blade"] = new SkillNode
            {
                id = "wind_blade",
                name = "Wind Blade",
                description = "Launch a ranged wind projectile that pierces through enemies.",
                tier = 2,
                maxRank = 4,
                currentRank = 0,
                skillPointCost = 2,
                levelRequirement = 15,
                prerequisites = new List<string> { "cloud_step" },
                skillData = new Skill
                {
                    id = "wind_blade",
                    name = "Wind Blade",
                    damage = 120,
                    cooldown = 4f,
                    manaCost = 35,
                    range = 12f,
                    skillType = SkillType.Ranged,
                    targetType = TargetType.Enemy,
                    damageType = DamageType.Physical
                }
            };

            skillNodes["azure_tempest"] = new SkillNode
            {
                id = "azure_tempest",
                name = "Azure Tempest",
                description = "Ultimate technique: Create a whirlwind of sword strikes around you, hitting all nearby enemies.",
                tier = 3,
                maxRank = 3,
                currentRank = 0,
                skillPointCost = 3,
                levelRequirement = 25,
                prerequisites = new List<string> { "flowing_counter", "wind_blade" },
                skillData = new Skill
                {
                    id = "azure_tempest",
                    name = "Azure Tempest",
                    damage = 200,
                    cooldown = 20f,
                    manaCost = 60,
                    range = 6f,
                    skillType = SkillType.AoE,
                    targetType = TargetType.Ground,
                    damageType = DamageType.Physical
                }
            };
        }

        private void InitializeIronBellSkillTree()
        {
            skillNodes["iron_sweep"] = new SkillNode
            {
                id = "iron_sweep",
                name = "Iron Sweep",
                description = "Sweep your staff in a wide arc, hitting multiple enemies in front of you.",
                tier = 1,
                maxRank = 5,
                currentRank = 1,
                skillPointCost = 1,
                levelRequirement = 1,
                prerequisites = new List<string>(),
                skillData = new Skill
                {
                    id = "iron_sweep",
                    name = "Iron Sweep",
                    damage = 90,
                    cooldown = 2f,
                    manaCost = 20,
                    range = 4f,
                    skillType = SkillType.AoE,
                    targetType = TargetType.Ground,
                    damageType = DamageType.Physical
                }
            };

            skillNodes["bells_resonance"] = new SkillNode
            {
                id = "bells_resonance",
                name = "Bell's Resonance",
                description = "Create a stunning sound wave that disorients nearby enemies.",
                tier = 1,
                maxRank = 3,
                currentRank = 1,
                skillPointCost = 1,
                levelRequirement = 3,
                prerequisites = new List<string>(),
                skillData = new Skill
                {
                    id = "bells_resonance",
                    name = "Bell's Resonance",
                    damage = 40,
                    cooldown = 10f,
                    manaCost = 30,
                    range = 5f,
                    skillType = SkillType.Control,
                    targetType = TargetType.Ground,
                    damageType = DamageType.Magical
                }
            };

            skillNodes["staff_vault"] = new SkillNode
            {
                id = "staff_vault",
                name = "Staff Vault",
                description = "Use your staff to vault over enemies, landing behind them with a powerful strike.",
                tier = 2,
                maxRank = 4,
                currentRank = 0,
                skillPointCost = 2,
                levelRequirement = 12,
                prerequisites = new List<string> { "iron_sweep" },
                skillData = new Skill
                {
                    id = "staff_vault",
                    name = "Staff Vault",
                    damage = 140,
                    cooldown = 8f,
                    manaCost = 35,
                    range = 6f,
                    skillType = SkillType.Movement,
                    targetType = TargetType.Enemy,
                    damageType = DamageType.Physical
                }
            };

            skillNodes["earth_shaker"] = new SkillNode
            {
                id = "earth_shaker",
                name = "Earth Shaker",
                description = "Strike the ground with tremendous force, creating shockwaves that knock down enemies.",
                tier = 2,
                maxRank = 4,
                currentRank = 0,
                skillPointCost = 2,
                levelRequirement = 15,
                prerequisites = new List<string> { "bells_resonance" },
                skillData = new Skill
                {
                    id = "earth_shaker",
                    name = "Earth Shaker",
                    damage = 160,
                    cooldown = 12f,
                    manaCost = 45,
                    range = 8f,
                    skillType = SkillType.AoE,
                    targetType = TargetType.Ground,
                    damageType = DamageType.Physical
                }
            };

            skillNodes["iron_fortress"] = new SkillNode
            {
                id = "iron_fortress",
                name = "Iron Fortress",
                description = "Ultimate technique: Become immovable and reflect all damage back to attackers for 10 seconds.",
                tier = 3,
                maxRank = 3,
                currentRank = 0,
                skillPointCost = 3,
                levelRequirement = 25,
                prerequisites = new List<string> { "staff_vault", "earth_shaker" },
                skillData = new Skill
                {
                    id = "iron_fortress",
                    name = "Iron Fortress",
                    damage = 0,
                    cooldown = 60f,
                    manaCost = 80,
                    range = 0f,
                    skillType = SkillType.Buff,
                    targetType = TargetType.Self,
                    damageType = DamageType.True
                }
            };
        }

        public bool LearnSkill(string skillId)
        {
            if (!skillNodes.ContainsKey(skillId))
            {
                Debug.LogWarning($"Skill not found: {skillId}");
                return false;
            }

            var skillNode = skillNodes[skillId];
            
            if (skillNode.currentRank >= skillNode.maxRank)
            {
                Debug.LogWarning($"Skill already at max rank: {skillNode.name}");
                return false;
            }

            if (character.level < skillNode.levelRequirement)
            {
                Debug.LogWarning($"Level {skillNode.levelRequirement} required for {skillNode.name}");
                return false;
            }

            if (availableSkillPoints < skillNode.skillPointCost)
            {
                Debug.LogWarning($"Not enough skill points for {skillNode.name}");
                return false;
            }

            foreach (var prereq in skillNode.prerequisites)
            {
                if (!skillNodes.ContainsKey(prereq) || skillNodes[prereq].currentRank == 0)
                {
                    Debug.LogWarning($"Prerequisite not met: {skillNodes[prereq].name}");
                    return false;
                }
            }

            if (skillNode.currentRank == 0)
            {
                skillNode.currentRank = 1;
                availableSkillPoints -= skillNode.skillPointCost;
                spentSkillPoints[skillId] = skillNode.skillPointCost;
                
                var skillSystem = character.GetComponent<SkillSystem>();
                if (skillSystem != null)
                {
                    skillSystem.AddSkill(skillNode.skillData);
                }
                
                OnSkillLearned?.Invoke(skillNode);
                Debug.Log($"📚 Learned skill: {skillNode.name}");
            }
            else
            {
                skillNode.currentRank++;
                availableSkillPoints -= skillNode.skillPointCost;
                spentSkillPoints[skillId] += skillNode.skillPointCost;
                
                UpgradeSkillStats(skillNode);
                
                OnSkillUpgraded?.Invoke(skillNode);
                Debug.Log($"⬆️ Upgraded skill: {skillNode.name} (Rank {skillNode.currentRank})");
            }

            OnSkillPointsChanged?.Invoke();
            return true;
        }

        private void UpgradeSkillStats(SkillNode skillNode)
        {
            var skill = skillNode.skillData;
            float upgradeMultiplier = 1.2f; // 20% increase per rank
            
            skill.damage = Mathf.RoundToInt(skill.damage * upgradeMultiplier);
            skill.cooldown = Mathf.Max(0.5f, skill.cooldown * 0.95f); // Reduce cooldown by 5%
            skill.range *= 1.1f; // Increase range by 10%
        }

        public void OnLevelUp(int newLevel)
        {
            int newSkillPoints = skillPointsPerLevel;
            availableSkillPoints += newSkillPoints;
            OnSkillPointsChanged?.Invoke();
            
            Debug.Log($"🎉 Gained {newSkillPoints} skill points! Total available: {availableSkillPoints}");
        }

        private void CalculateAvailableSkillPoints()
        {
            int totalEarned = character.level * skillPointsPerLevel;
            int totalSpent = spentSkillPoints.Values.Sum();
            availableSkillPoints = totalEarned - totalSpent;
        }

        public bool CanLearnSkill(string skillId)
        {
            if (!skillNodes.ContainsKey(skillId)) return false;
            
            var skillNode = skillNodes[skillId];
            
            if (skillNode.currentRank >= skillNode.maxRank) return false;
            if (character.level < skillNode.levelRequirement) return false;
            if (availableSkillPoints < skillNode.skillPointCost) return false;
            
            foreach (var prereq in skillNode.prerequisites)
            {
                if (!skillNodes.ContainsKey(prereq) || skillNodes[prereq].currentRank == 0)
                    return false;
            }
            
            return true;
        }

        public List<SkillNode> GetAvailableSkills()
        {
            return skillNodes.Values.Where(node => CanLearnSkill(node.id)).ToList();
        }

        public List<SkillNode> GetLearnedSkills()
        {
            return skillNodes.Values.Where(node => node.currentRank > 0).ToList();
        }

        public List<SkillNode> GetAllSkills()
        {
            return skillNodes.Values.ToList();
        }

        public SkillNode GetSkillNode(string skillId)
        {
            return skillNodes.ContainsKey(skillId) ? skillNodes[skillId] : null;
        }

        public int GetAvailableSkillPoints()
        {
            return availableSkillPoints;
        }

        public int GetSpentSkillPoints()
        {
            return spentSkillPoints.Values.Sum();
        }

        public bool ResetSkills()
        {
            foreach (var skillNode in skillNodes.Values)
            {
                skillNode.currentRank = skillNode.id == "swift_strike" || skillNode.id == "iron_sweep" ? 1 : 0;
            }
            
            spentSkillPoints.Clear();
            CalculateAvailableSkillPoints();
            OnSkillPointsChanged?.Invoke();
            
            Debug.Log("🔄 All skills have been reset!");
            return true;
        }
    }

    [System.Serializable]
    public class SkillNode
    {
        public string id;
        public string name;
        public string description;
        public int tier;
        public int maxRank;
        public int currentRank;
        public int skillPointCost;
        public int levelRequirement;
        public List<string> prerequisites;
        public Skill skillData;
        public Vector2 treePosition; // For UI positioning
    }
}
