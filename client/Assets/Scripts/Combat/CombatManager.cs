using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class CombatManager : MonoBehaviour
    {
        [Header("Combat Settings")]
        public float autoTargetRange = 15f;
        public LayerMask enemyLayerMask = -1;
        public bool enableAutoTarget = true;
        
        private Dictionary<string, CombatInstance> activeCombats = new Dictionary<string, CombatInstance>();
        private List<GameObject> potentialTargets = new List<GameObject>();

        public void HandleCombatHit(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("attackerId") || !data.ContainsKey("targetId")) return;
            
            string attackerId = data["attackerId"].ToString();
            string targetId = data["targetId"].ToString();
            float damage = float.Parse(data["damage"].ToString());
            
            GameObject attacker = GameManager.Instance.GetPlayerManager().GetPlayer(attackerId);
            GameObject target = GameManager.Instance.GetPlayerManager().GetPlayer(targetId);
            
            if (attacker != null && target != null)
            {
                ApplyDamage(target, damage);
                ShowDamageEffect(target, damage);
                
                Debug.Log($"⚔️ {attackerId} hit {targetId} for {damage} damage");
            }
        }

        public void HandleSkillCast(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("casterId") || !data.ContainsKey("skillId")) return;
            
            string casterId = data["casterId"].ToString();
            string skillId = data["skillId"].ToString();
            
            GameObject caster = GameManager.Instance.GetPlayerManager().GetPlayer(casterId);
            
            if (caster != null)
            {
                var character = caster.GetComponent<Character>();
                if (character != null)
                {
                    character.Skills.CastSkill(skillId);
                }
                
                Debug.Log($"✨ {casterId} cast skill {skillId}");
            }
        }

        public GameObject FindNearestEnemy(Vector3 position)
        {
            if (!enableAutoTarget) return null;
            
            Collider[] colliders = Physics.OverlapSphere(position, autoTargetRange, enemyLayerMask);
            GameObject nearestEnemy = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var collider in colliders)
            {
                if (collider.gameObject != GameManager.Instance.GetPlayerManager().LocalPlayer)
                {
                    float distance = Vector3.Distance(position, collider.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = collider.gameObject;
                    }
                }
            }
            
            return nearestEnemy;
        }

        public bool IsInRange(GameObject attacker, GameObject target, float range)
        {
            if (attacker == null || target == null) return false;
            
            float distance = Vector3.Distance(attacker.transform.position, target.transform.position);
            return distance <= range;
        }

        public void StartCombat(GameObject player1, GameObject player2)
        {
            string combatId = $"{player1.name}_{player2.name}_{Time.time}";
            
            CombatInstance combat = new CombatInstance
            {
                id = combatId,
                player1 = player1,
                player2 = player2,
                startTime = Time.time,
                isActive = true
            };
            
            activeCombats[combatId] = combat;
            
            var char1 = player1.GetComponent<Character>();
            var char2 = player2.GetComponent<Character>();
            
            if (char1 != null) char1.Combat.EnterCombat(player2);
            if (char2 != null) char2.Combat.EnterCombat(player1);
            
            Debug.Log($"⚔️ Combat started: {player1.name} vs {player2.name}");
        }

        public void EndCombat(string combatId)
        {
            if (activeCombats.ContainsKey(combatId))
            {
                var combat = activeCombats[combatId];
                combat.isActive = false;
                
                var char1 = combat.player1.GetComponent<Character>();
                var char2 = combat.player2.GetComponent<Character>();
                
                if (char1 != null) char1.Combat.ExitCombat();
                if (char2 != null) char2.Combat.ExitCombat();
                
                activeCombats.Remove(combatId);
                
                Debug.Log($"🏁 Combat ended: {combatId}");
            }
        }

        private void ApplyDamage(GameObject target, float damage)
        {
            var character = target.GetComponent<Character>();
            if (character != null)
            {
                character.Stats.TakeDamage((int)damage);
                
                if (!character.IsAlive())
                {
                    HandleCharacterDeath(character);
                }
            }
        }

        private void ShowDamageEffect(GameObject target, float damage)
        {
            Debug.Log($"💥 Damage effect: {damage} on {target.name}");
        }

        private void HandleCharacterDeath(Character character)
        {
            Debug.Log($"💀 {character.characterName} has been defeated!");
        }

        public List<CombatInstance> GetActiveCombats()
        {
            return new List<CombatInstance>(activeCombats.Values);
        }
    }

    [System.Serializable]
    public class CombatInstance
    {
        public string id;
        public GameObject player1;
        public GameObject player2;
        public float startTime;
        public bool isActive;
        public Dictionary<string, object> combatData = new Dictionary<string, object>();
    }
}
