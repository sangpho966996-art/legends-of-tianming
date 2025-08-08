using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class WorldBoss : MonoBehaviour
    {
        [Header("World Boss Settings")]
        public string bossId = "ancient_dragon";
        public string bossName = "Ancient Dragon of Qingze";
        public int level = 50;
        public int maxHealth = 10000;
        public int attackDamage = 200;
        public float spawnInterval = 7200f; // 2 hours
        public Vector3 spawnLocation = Vector3.zero;
        
        [Header("Boss Mechanics")]
        public float enrageTimer = 600f; // 10 minutes
        public float specialAttackInterval = 30f;
        public int maxParticipants = 50;
        
        private int currentHealth;
        private bool isActive = false;
        private bool isEnraged = false;
        private float spawnTime;
        private float lastSpecialAttack;
        private List<GameObject> participants = new List<GameObject>();
        private Dictionary<string, int> damageDealt = new Dictionary<string, int>();

        public bool IsActive => isActive;
        public bool IsEnraged => isEnraged;
        public float HealthPercentage => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        public int ParticipantCount => participants.Count;

        private void Start()
        {
            currentHealth = maxHealth;
            ScheduleNextSpawn();
        }

        private void Update()
        {
            if (isActive)
            {
                UpdateBossFight();
            }
            else
            {
                CheckSpawnTime();
            }
        }

        private void CheckSpawnTime()
        {
            if (Time.time >= spawnTime)
            {
                SpawnBoss();
            }
        }

        private void SpawnBoss()
        {
            isActive = true;
            currentHealth = maxHealth;
            isEnraged = false;
            spawnTime = Time.time;
            lastSpecialAttack = Time.time;
            
            transform.position = spawnLocation;
            gameObject.SetActive(true);
            
            participants.Clear();
            damageDealt.Clear();
            
            AnnounceBossSpawn();
            
            Debug.Log($"🐉 World Boss spawned: {bossName} (Level {level})");
        }

        private void UpdateBossFight()
        {
            if (!isEnraged && Time.time - spawnTime >= enrageTimer)
            {
                EnrageBoss();
            }
            
            if (Time.time - lastSpecialAttack >= specialAttackInterval)
            {
                PerformSpecialAttack();
                lastSpecialAttack = Time.time;
            }
            
            participants.RemoveAll(p => p == null);
            
            if (participants.Count == 0 && Time.time - spawnTime > 300f) // 5 minutes grace period
            {
                DespawnBoss();
            }
        }

        private void EnrageBoss()
        {
            isEnraged = true;
            attackDamage = Mathf.RoundToInt(attackDamage * 1.5f);
            specialAttackInterval *= 0.7f; // More frequent special attacks
            
            AnnounceBossEnrage();
            Debug.Log($"😡 {bossName} has become enraged!");
        }

        private void PerformSpecialAttack()
        {
            if (participants.Count == 0) return;
            
            string attackType = GetRandomSpecialAttack();
            
            switch (attackType)
            {
                case "meteor_strike":
                    PerformMeteorStrike();
                    break;
                case "dragon_breath":
                    PerformDragonBreath();
                    break;
                case "earthquake":
                    PerformEarthquake();
                    break;
                case "heal":
                    PerformBossHeal();
                    break;
            }
        }

        private string GetRandomSpecialAttack()
        {
            string[] attacks = { "meteor_strike", "dragon_breath", "earthquake", "heal" };
            return attacks[Random.Range(0, attacks.Length)];
        }

        private void PerformMeteorStrike()
        {
            if (participants.Count > 0)
            {
                GameObject target = participants[Random.Range(0, participants.Count)];
                Vector3 targetPos = target.transform.position;
                
                Collider[] hitTargets = Physics.OverlapSphere(targetPos, 8f);
                foreach (var hit in hitTargets)
                {
                    var character = hit.GetComponent<Character>();
                    if (character != null && character.IsAlive())
                    {
                        int damage = isEnraged ? 400 : 300;
                        character.Stats.TakeDamage(damage);
                        Debug.Log($"☄️ Meteor Strike hit {character.characterName} for {damage} damage");
                    }
                }
                
                AnnounceBossAttack("Meteor Strike", targetPos);
            }
        }

        private void PerformDragonBreath()
        {
            Vector3 forward = transform.forward;
            Vector3 bossPos = transform.position;
            
            Collider[] hitTargets = Physics.OverlapSphere(bossPos + forward * 10f, 12f);
            foreach (var hit in hitTargets)
            {
                var character = hit.GetComponent<Character>();
                if (character != null && character.IsAlive())
                {
                    Vector3 dirToTarget = (hit.transform.position - bossPos).normalized;
                    float angle = Vector3.Angle(forward, dirToTarget);
                    
                    if (angle <= 45f) // 90-degree cone
                    {
                        int damage = isEnraged ? 350 : 250;
                        character.Stats.TakeDamage(damage);
                        Debug.Log($"🔥 Dragon Breath hit {character.characterName} for {damage} damage");
                    }
                }
            }
            
            AnnounceBossAttack("Dragon Breath", bossPos + forward * 10f);
        }

        private void PerformEarthquake()
        {
            foreach (var participant in participants)
            {
                if (participant != null)
                {
                    var character = participant.GetComponent<Character>();
                    if (character != null && character.IsAlive())
                    {
                        int damage = isEnraged ? 200 : 150;
                        character.Stats.TakeDamage(damage);
                        Debug.log($"🌍 Earthquake hit {character.characterName} for {damage} damage");
                    }
                }
            }
            
            AnnounceBossAttack("Earthquake", transform.position);
        }

        private void PerformBossHeal()
        {
            if (HealthPercentage < 0.3f) // Only heal when below 30% health
            {
                int healAmount = maxHealth / 10; // Heal 10% of max health
                currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
                
                Debug.Log($"💚 {bossName} healed for {healAmount} health ({currentHealth}/{maxHealth})");
                AnnounceBossAttack("Regeneration", transform.position);
            }
        }

        public void TakeDamage(int damage, string attackerId)
        {
            if (!isActive) return;
            
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            if (!damageDealt.ContainsKey(attackerId))
                damageDealt[attackerId] = 0;
            damageDealt[attackerId] += damage;
            
            Debug.Log($"🐉 {bossName} took {damage} damage ({currentHealth}/{maxHealth})");
            
            if (currentHealth <= 0)
            {
                DefeatBoss();
            }
        }

        public void AddParticipant(GameObject player)
        {
            if (!participants.Contains(player) && participants.Count < maxParticipants)
            {
                participants.Add(player);
                Debug.Log($"⚔️ {player.name} joined the world boss fight");
            }
        }

        public void RemoveParticipant(GameObject player)
        {
            if (participants.Contains(player))
            {
                participants.Remove(player);
                Debug.Log($"🚪 {player.name} left the world boss fight");
            }
        }

        private void DefeatBoss()
        {
            isActive = false;
            
            DistributeRewards();
            
            AnnounceBossDefeat();
            
            ScheduleNextSpawn();
            
            gameObject.SetActive(false);
            
            Debug.Log($"💀 World Boss defeated: {bossName}");
        }

        private void DespawnBoss()
        {
            isActive = false;
            ScheduleNextSpawn();
            gameObject.SetActive(false);
            
            Debug.Log($"⏰ World Boss despawned: {bossName} (no participants)");
        }

        private void DistributeRewards()
        {
            var sortedDamage = new List<KeyValuePair<string, int>>(damageDealt);
            sortedDamage.Sort((x, y) => y.Value.CompareTo(x.Value));
            
            for (int i = 0; i < sortedDamage.Count && i < 10; i++) // Top 10 participants
            {
                string playerId = sortedDamage[i].Key;
                int damage = sortedDamage[i].Value;
                
                float damagePercent = (float)damage / maxHealth;
                int goldReward = Mathf.RoundToInt(1000 * damagePercent);
                int expReward = Mathf.RoundToInt(500 * damagePercent);
                
                Debug.Log($"🏆 {playerId} earned {goldReward} gold and {expReward} experience");
            }
        }

        private void ScheduleNextSpawn()
        {
            spawnTime = Time.time + spawnInterval;
            Debug.Log($"⏰ Next world boss spawn in {spawnInterval / 3600f:F1} hours");
        }

        private void AnnounceBossSpawn()
        {
            Debug.Log($"📢 WORLD BOSS: {bossName} has appeared in {GetCurrentRegionName()}!");
        }

        private void AnnounceBossEnrage()
        {
            Debug.Log($"📢 WORLD BOSS: {bossName} has become enraged!");
        }

        private void AnnounceBossAttack(string attackName, Vector3 position)
        {
            Debug.Log($"⚠️ BOSS ATTACK: {bossName} uses {attackName}!");
        }

        private void AnnounceBossDefeat()
        {
            Debug.Log($"📢 WORLD BOSS: {bossName} has been defeated by brave warriors!");
        }

        private string GetCurrentRegionName()
        {
            var regionManager = FindObjectOfType<RegionManager>();
            if (regionManager != null)
            {
                var region = regionManager.GetCurrentRegion();
                return region != null ? region.name : "Unknown Region";
            }
            return "Unknown Region";
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnLocation, 5f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 12f); // Dragon breath range
            
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(transform.position, 8f); // Meteor strike range
        }
    }
}
