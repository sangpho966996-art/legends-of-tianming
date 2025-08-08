using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace LegendsOfTianming.Core
{
    public class SkillSystem : MonoBehaviour
    {
        [Header("Skill Settings")]
        public int maxSkillSlots = 8;
        public KeyCode[] skillHotkeys = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I };
        
        private Dictionary<string, Skill> availableSkills = new Dictionary<string, Skill>();
        private Dictionary<string, float> skillCooldowns = new Dictionary<string, float>();
        private List<string> equippedSkills = new List<string>();
        private Character character;

        private void Start()
        {
            character = GetComponent<Character>();
        }

        private void Update()
        {
            UpdateCooldowns();
            HandleSkillInput();
        }

        private void UpdateCooldowns()
        {
            var keys = skillCooldowns.Keys.ToList();
            foreach (var skillId in keys)
            {
                if (skillCooldowns[skillId] > 0)
                {
                    skillCooldowns[skillId] -= Time.deltaTime;
                    if (skillCooldowns[skillId] <= 0)
                    {
                        skillCooldowns[skillId] = 0;
                        Debug.Log($"✅ Skill {skillId} is ready");
                    }
                }
            }
        }

        private void HandleSkillInput()
        {
            if (character == null || !character.GetComponent<PlayerController>().IsLocalPlayer) return;
            
            for (int i = 0; i < skillHotkeys.Length && i < equippedSkills.Count; i++)
            {
                if (Input.GetKeyDown(skillHotkeys[i]))
                {
                    CastSkill(equippedSkills[i]);
                }
            }
        }

        public void AddSkill(Skill skill)
        {
            availableSkills[skill.id] = skill;
            skillCooldowns[skill.id] = 0f;
            
            if (equippedSkills.Count < maxSkillSlots && !equippedSkills.Contains(skill.id))
            {
                equippedSkills.Add(skill.id);
            }
            
            Debug.Log($"📚 Skill learned: {skill.name}");
        }

        public bool CastSkill(string skillId)
        {
            if (!availableSkills.ContainsKey(skillId))
            {
                Debug.LogWarning($"Skill not found: {skillId}");
                return false;
            }

            if (skillCooldowns[skillId] > 0)
            {
                Debug.LogWarning($"Skill {skillId} is on cooldown: {skillCooldowns[skillId]:F1}s");
                return false;
            }

            Skill skill = availableSkills[skillId];
            
            if (character.Stats.CurrentMana < skill.manaCost)
            {
                Debug.LogWarning($"Not enough mana for {skill.name}");
                return false;
            }

            GameObject target = null;
            Vector3 targetPosition = transform.position;

            if (skill.targetType == TargetType.Enemy)
            {
                target = GameManager.Instance.GetCombatManager().FindNearestEnemy(transform.position);
                if (target == null)
                {
                    Debug.LogWarning($"No target found for {skill.name}");
                    return false;
                }
                targetPosition = target.transform.position;
            }

            ExecuteSkill(skill, target, targetPosition);
            return true;
        }

        private void ExecuteSkill(Skill skill, GameObject target, Vector3 position)
        {
            character.Stats.ConsumeMana(skill.manaCost);
            skillCooldowns[skill.id] = skill.cooldown;
            
            switch (skill.skillType)
            {
                case SkillType.Attack:
                    ExecuteAttackSkill(skill, target);
                    break;
                case SkillType.AoE:
                    ExecuteAoESkill(skill, position);
                    break;
                case SkillType.Movement:
                    ExecuteMovementSkill(skill);
                    break;
                case SkillType.Buff:
                    ExecuteBuffSkill(skill);
                    break;
                case SkillType.Heal:
                    ExecuteHealSkill(skill);
                    break;
                case SkillType.Control:
                    ExecuteControlSkill(skill, target);
                    break;
                case SkillType.Counter:
                    ExecuteCounterSkill(skill, target);
                    break;
                case SkillType.Ranged:
                    ExecuteRangedSkill(skill, target);
                    break;
            }

            if (character.GetComponent<PlayerController>().IsLocalPlayer)
            {
                string targetId = target != null ? target.name : "";
                GameManager.Instance.GetNetworkManager().SendSkillCast(skill.id, targetId, position);
            }

            Debug.Log($"✨ {character.characterName} cast {skill.name}");
        }

        private void ExecuteAttackSkill(Skill skill, GameObject target)
        {
            if (target != null)
            {
                var targetCharacter = target.GetComponent<Character>();
                if (targetCharacter != null)
                {
                    int damage = CalculateDamage(skill);
                    targetCharacter.Stats.TakeDamage(damage);
                    
                    if (character.GetComponent<PlayerController>().IsLocalPlayer)
                    {
                        GameManager.Instance.GetNetworkManager().SendCombatAction(target.name, damage, skill.id);
                    }
                }
            }
        }

        private void ExecuteAoESkill(Skill skill, Vector3 position)
        {
            Collider[] targets = Physics.OverlapSphere(position, skill.range);
            foreach (var collider in targets)
            {
                var targetCharacter = collider.GetComponent<Character>();
                if (targetCharacter != null && targetCharacter != character)
                {
                    int damage = CalculateDamage(skill);
                    targetCharacter.Stats.TakeDamage(damage);
                }
            }
        }

        private void ExecuteMovementSkill(Skill skill)
        {
            var controller = character.GetComponent<PlayerController>();
            if (controller != null)
            {
                Vector3 dashDirection = transform.forward;
                Vector3 newPosition = transform.position + dashDirection * skill.range;
                transform.position = newPosition;
            }
        }

        private void ExecuteBuffSkill(Skill skill)
        {
            Debug.Log($"🔆 Buff applied: {skill.name}");
        }

        private void ExecuteHealSkill(Skill skill)
        {
            character.Stats.Heal(skill.damage);
        }

        private void ExecuteControlSkill(Skill skill, GameObject target)
        {
            Debug.Log($"🎯 Control effect applied: {skill.name}");
        }

        private void ExecuteCounterSkill(Skill skill, GameObject target)
        {
            Debug.Log($"🛡️ Counter skill: {skill.name}");
        }

        private void ExecuteRangedSkill(Skill skill, GameObject target)
        {
            if (target != null)
            {
                var targetCharacter = target.GetComponent<Character>();
                if (targetCharacter != null)
                {
                    int damage = CalculateDamage(skill);
                    targetCharacter.Stats.TakeDamage(damage);
                }
            }
        }

        private int CalculateDamage(Skill skill)
        {
            int baseDamage = skill.damage;
            int statBonus = 0;
            
            switch (skill.damageType)
            {
                case DamageType.Physical:
                    statBonus = character.Stats.Strength * 2;
                    break;
                case DamageType.Magical:
                    statBonus = character.Stats.Intelligence * 2;
                    break;
                case DamageType.True:
                    break;
            }
            
            return baseDamage + statBonus;
        }

        public bool IsSkillReady(string skillId)
        {
            return skillCooldowns.ContainsKey(skillId) && skillCooldowns[skillId] <= 0;
        }

        public float GetSkillCooldown(string skillId)
        {
            return skillCooldowns.ContainsKey(skillId) ? skillCooldowns[skillId] : 0f;
        }

        public List<Skill> GetAvailableSkills()
        {
            return availableSkills.Values.ToList();
        }

        public List<Skill> GetEquippedSkills()
        {
            return equippedSkills.Select(id => availableSkills[id]).ToList();
        }

        public void EquipSkill(string skillId, int slot)
        {
            if (slot >= 0 && slot < maxSkillSlots && availableSkills.ContainsKey(skillId))
            {
                while (equippedSkills.Count <= slot)
                {
                    equippedSkills.Add("");
                }
                equippedSkills[slot] = skillId;
                Debug.Log($"🎯 Skill equipped: {availableSkills[skillId].name} to slot {slot}");
            }
        }
    }

    [System.Serializable]
    public class Skill
    {
        public string id;
        public string name;
        public string description;
        public int damage = 0;
        public float cooldown = 1f;
        public int manaCost = 0;
        public float range = 5f;
        public SkillType skillType = SkillType.Attack;
        public TargetType targetType = TargetType.Enemy;
        public DamageType damageType = DamageType.Physical;
        public string animationName;
        public string effectPrefab;
    }

    public enum SkillType
    {
        Attack,
        AoE,
        Movement,
        Buff,
        Heal,
        Control,
        Counter,
        Ranged
    }

    public enum TargetType
    {
        Self,
        Enemy,
        Ally,
        Ground,
        None
    }

    public enum DamageType
    {
        Physical,
        Magical,
        True
    }
}
