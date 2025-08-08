using UnityEngine;

namespace LegendsOfTianming.Core
{
    public class Character : MonoBehaviour
    {
        [Header("Character Info")]
        public string characterId;
        public string characterName;
        public string characterClass;
        public int level = 1;
        public long experience = 0;

        private CharacterStats stats;
        private SkillSystem skillSystem;
        private EquipmentSystem equipmentSystem;
        private CombatSystem combatSystem;

        public CharacterStats Stats => stats;
        public SkillSystem Skills => skillSystem;
        public EquipmentSystem Equipment => equipmentSystem;
        public CombatSystem Combat => combatSystem;

        private void Awake()
        {
            stats = GetComponent<CharacterStats>() ?? gameObject.AddComponent<CharacterStats>();
            skillSystem = GetComponent<SkillSystem>() ?? gameObject.AddComponent<SkillSystem>();
            equipmentSystem = GetComponent<EquipmentSystem>() ?? gameObject.AddComponent<EquipmentSystem>();
            combatSystem = GetComponent<CombatSystem>() ?? gameObject.AddComponent<CombatSystem>();
        }

        public void Initialize(CharacterData data)
        {
            characterId = data.id;
            characterName = data.name;
            characterClass = data.characterClass;
            level = data.level;
            experience = data.experience;
            
            if (data.stats != null)
            {
                stats.Initialize(data.stats);
            }
            
            transform.position = data.position;
            
            gameObject.name = $"{characterName} ({characterClass})";
            
            SetupClassSpecificFeatures();
            
            Debug.Log($"⚔️ Character initialized: {characterName} - {characterClass} Lv.{level}");
        }

        private void SetupClassSpecificFeatures()
        {
            switch (characterClass)
            {
                case "Azure Cloud Sect":
                    SetupAzureCloudSect();
                    break;
                case "Iron Bell Sect":
                    SetupIronBellSect();
                    break;
                case "Shadow Veil Sect":
                    SetupShadowVeilSect();
                    break;
                case "Wandering Spear Sect":
                    SetupWanderingSpearSect();
                    break;
                case "Spirit Lute Sect":
                    SetupSpiritLuteSect();
                    break;
                case "Stoneheart Sect":
                    SetupStoneheartSect();
                    break;
            }
        }

        private void SetupAzureCloudSect()
        {
            skillSystem.AddSkill(new Skill
            {
                id = "swift_strike",
                name = "Swift Strike",
                description = "A quick sword strike that can be chained into combos",
                damage = 120,
                cooldown = 2f,
                manaCost = 10,
                skillType = SkillType.Attack,
                targetType = TargetType.Enemy
            });

            skillSystem.AddSkill(new Skill
            {
                id = "cloud_step",
                name = "Cloud Step",
                description = "Dash forward with brief invincibility frames",
                cooldown = 8f,
                manaCost = 20,
                skillType = SkillType.Movement,
                targetType = TargetType.Self
            });

            skillSystem.AddSkill(new Skill
            {
                id = "flowing_counter",
                name = "Flowing Counter",
                description = "Parry an attack and counter with increased damage",
                damage = 180,
                cooldown = 12f,
                manaCost = 25,
                skillType = SkillType.Counter,
                targetType = TargetType.Enemy
            });

            skillSystem.AddSkill(new Skill
            {
                id = "wind_blade",
                name = "Wind Blade",
                description = "Launch a ranged sword energy projectile",
                damage = 180,
                cooldown = 6f,
                manaCost = 30,
                skillType = SkillType.Ranged,
                targetType = TargetType.Enemy
            });
        }

        private void SetupIronBellSect()
        {
            skillSystem.AddSkill(new Skill
            {
                id = "iron_sweep",
                name = "Iron Sweep",
                description = "Sweep attack that knocks down enemies in an arc",
                damage = 140,
                cooldown = 4f,
                manaCost = 15,
                skillType = SkillType.AoE,
                targetType = TargetType.Enemy
            });

            skillSystem.AddSkill(new Skill
            {
                id = "bells_resonance",
                name = "Bell's Resonance",
                description = "AoE stun that affects all nearby enemies",
                cooldown = 20f,
                manaCost = 35,
                skillType = SkillType.Control,
                targetType = TargetType.Enemy
            });

            skillSystem.AddSkill(new Skill
            {
                id = "staff_vault",
                name = "Staff Vault",
                description = "Pole vault over enemies, landing with impact damage",
                damage = 160,
                cooldown = 10f,
                manaCost = 25,
                skillType = SkillType.Movement,
                targetType = TargetType.Enemy
            });

            skillSystem.AddSkill(new Skill
            {
                id = "earth_shaker",
                name = "Earth Shaker",
                description = "Ground slam that creates damaging shockwaves",
                damage = 200,
                cooldown = 8f,
                manaCost = 40,
                skillType = SkillType.AoE,
                targetType = TargetType.Enemy
            });
        }

        private void SetupShadowVeilSect() { }
        private void SetupWanderingSpearSect() { }
        private void SetupSpiritLuteSect() { }
        private void SetupStoneheartSect() { }

        public void GainExperience(long amount)
        {
            experience += amount;
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            long requiredExp = CalculateRequiredExperience(level + 1);
            
            if (experience >= requiredExp && level < 100)
            {
                level++;
                stats.LevelUp();
                Debug.Log($"🎉 {characterName} leveled up to {level}!");
            }
        }

        private long CalculateRequiredExperience(int targetLevel)
        {
            return (long)(1000 * Mathf.Pow(targetLevel, 1.5f));
        }

        public float GetHealthPercentage()
        {
            return (float)stats.CurrentHealth / stats.MaxHealth;
        }

        public float GetManaPercentage()
        {
            return (float)stats.CurrentMana / stats.MaxMana;
        }

        public bool IsAlive()
        {
            return stats.CurrentHealth > 0;
        }
    }
}
