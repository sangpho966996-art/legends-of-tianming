using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class CitySiegeManager : MonoBehaviour
    {
        [Header("City Siege Settings")]
        public string cityName = "Tianming Fortress";
        public int maxPlayersPerSide = 40;
        public float siegeDuration = 3600f; // 1 hour
        public int gatesHealth = 5000;
        public int crystalHealth = 10000;
        
        [Header("Siege Points")]
        public Transform[] attackerSpawnPoints;
        public Transform[] defenderSpawnPoints;
        public Transform[] gatePositions;
        public Transform crystalPosition;
        public Transform[] catapultPositions;
        
        private CitySiegeState currentState = CitySiegeState.Scheduled;
        private float siegeStartTime;
        private Guild attackingGuild;
        private Guild defendingGuild;
        private List<GameObject> attackers = new List<GameObject>();
        private List<GameObject> defenders = new List<GameObject>();
        private Dictionary<int, SiegeGate> gates = new Dictionary<int, SiegeGate>();
        private SiegeCrystal crystal;
        private List<SiegeCatapult> catapults = new List<SiegeCatapult>();
        private int attackerKills = 0;
        private int defenderKills = 0;

        public event System.Action<CitySiegeState> OnStateChanged;
        public event System.Action<Guild> OnSiegeWon;
        public event System.Action<int, int> OnKillCountUpdated;
        public event System.Action<int> OnGateDestroyed;
        public event System.Action OnCrystalDestroyed;

        public CitySiegeState State => currentState;
        public float TimeRemaining => siegeDuration - (Time.time - siegeStartTime);
        public Guild AttackingGuild => attackingGuild;
        public Guild DefendingGuild => defendingGuild;
        public int AttackerCount => attackers.Count;
        public int DefenderCount => defenders.Count;
        public int AttackerKills => attackerKills;
        public int DefenderKills => defenderKills;

        private void Start()
        {
            InitializeSiegeStructures();
            SetState(CitySiegeState.Scheduled);
        }

        private void Update()
        {
            switch (currentState)
            {
                case CitySiegeState.Active:
                    UpdateActiveSiege();
                    break;
                case CitySiegeState.Ending:
                    UpdateEndingSiege();
                    break;
            }
        }

        private void InitializeSiegeStructures()
        {
            for (int i = 0; i < gatePositions.Length; i++)
            {
                var gate = new SiegeGate
                {
                    id = i,
                    position = gatePositions[i].position,
                    maxHealth = gatesHealth,
                    currentHealth = gatesHealth,
                    isDestroyed = false
                };
                gates[i] = gate;
                
                var gateObject = gatePositions[i].gameObject;
                var gateComponent = gateObject.AddComponent<SiegeGateComponent>();
                gateComponent.Initialize(i, this);
            }
            
            crystal = new SiegeCrystal
            {
                position = crystalPosition.position,
                maxHealth = crystalHealth,
                currentHealth = crystalHealth,
                isDestroyed = false
            };
            
            var crystalComponent = crystalPosition.gameObject.AddComponent<SiegeCrystalComponent>();
            crystalComponent.Initialize(this);
            
            for (int i = 0; i < catapultPositions.Length; i++)
            {
                var catapult = new SiegeCatapult
                {
                    id = i,
                    position = catapultPositions[i].position,
                    isOperational = true,
                    cooldown = 0f,
                    operatorPlayer = null
                };
                catapults.Add(catapult);
                
                var catapultComponent = catapultPositions[i].gameObject.AddComponent<SiegeCatapultComponent>();
                catapultComponent.Initialize(i, this);
            }
        }

        private void UpdateActiveSiege()
        {
            if (TimeRemaining <= 0f)
            {
                EndSiege(defendingGuild, "Time limit reached");
                return;
            }
            
            if (crystal.isDestroyed)
            {
                EndSiege(attackingGuild, "Crystal destroyed");
                return;
            }
            
            if (attackers.Count == 0)
            {
                EndSiege(defendingGuild, "All attackers eliminated");
                return;
            }
            
            UpdateCatapults();
        }

        private void UpdateEndingSiege()
        {
        }

        private void UpdateCatapults()
        {
            foreach (var catapult in catapults)
            {
                if (catapult.cooldown > 0f)
                {
                    catapult.cooldown -= Time.deltaTime;
                }
            }
        }

        public void ScheduleSiege(Guild attacking, Guild defending, float startTime)
        {
            if (currentState != CitySiegeState.Scheduled)
            {
                Debug.LogWarning("Cannot schedule siege in current state");
                return;
            }
            
            attackingGuild = attacking;
            defendingGuild = defending;
            siegeStartTime = startTime;
            
            Debug.Log($"🏰 City siege scheduled: {attacking.name} vs {defending.name} at {cityName}");
            AnnounceSiegeScheduled();
        }

        public void StartSiege()
        {
            if (currentState != CitySiegeState.Scheduled)
            {
                Debug.LogWarning("Cannot start siege in current state");
                return;
            }
            
            SetState(CitySiegeState.Active);
            siegeStartTime = Time.time;
            attackerKills = 0;
            defenderKills = 0;
            
            ResetSiegeStructures();
            
            Debug.Log($"🏰 City siege started: {attackingGuild.name} attacks {cityName}!");
            AnnounceSiegeStart();
        }

        private void ResetSiegeStructures()
        {
            foreach (var gate in gates.Values)
            {
                gate.currentHealth = gate.maxHealth;
                gate.isDestroyed = false;
            }
            
            crystal.currentHealth = crystal.maxHealth;
            crystal.isDestroyed = false;
            
            foreach (var catapult in catapults)
            {
                catapult.isOperational = true;
                catapult.cooldown = 0f;
                catapult.operatorPlayer = null;
            }
        }

        public void JoinSiege(GameObject player, bool isAttacker)
        {
            if (currentState != CitySiegeState.Active)
            {
                Debug.LogWarning("Cannot join siege in current state");
                return;
            }
            
            var character = player.GetComponent<Character>();
            if (character == null) return;
            
            var guildSystem = character.GetComponent<GuildSystem>();
            if (guildSystem == null || !guildSystem.IsInGuild) return;
            
            bool canJoin = false;
            if (isAttacker && guildSystem.CurrentGuild.id == attackingGuild.id && attackers.Count < maxPlayersPerSide)
            {
                attackers.Add(player);
                SpawnPlayer(player, true);
                canJoin = true;
            }
            else if (!isAttacker && guildSystem.CurrentGuild.id == defendingGuild.id && defenders.Count < maxPlayersPerSide)
            {
                defenders.Add(player);
                SpawnPlayer(player, false);
                canJoin = true;
            }
            
            if (canJoin)
            {
                Debug.Log($"🏰 {player.name} joined siege as {(isAttacker ? "attacker" : "defender")}");
            }
            else
            {
                Debug.LogWarning($"Cannot join siege: wrong guild or side full");
            }
        }

        public void LeaveSiege(GameObject player)
        {
            bool wasAttacker = attackers.Remove(player);
            bool wasDefender = defenders.Remove(player);
            
            if (wasAttacker || wasDefender)
            {
                Debug.Log($"🚪 {player.name} left the siege");
                
                foreach (var catapult in catapults)
                {
                    if (catapult.operatorPlayer == player)
                    {
                        catapult.operatorPlayer = null;
                        break;
                    }
                }
            }
        }

        private void SpawnPlayer(GameObject player, bool isAttacker)
        {
            Transform[] spawnPoints = isAttacker ? attackerSpawnPoints : defenderSpawnPoints;
            if (spawnPoints.Length > 0)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                player.transform.position = spawnPoint.position;
                player.transform.rotation = spawnPoint.rotation;
                
                var character = player.GetComponent<Character>();
                if (character != null)
                {
                    character.Stats.RestoreHealth();
                    character.Stats.RestoreMana();
                }
            }
        }

        public void DamageGate(int gateId, int damage)
        {
            if (!gates.ContainsKey(gateId) || gates[gateId].isDestroyed) return;
            
            var gate = gates[gateId];
            gate.currentHealth = Mathf.Max(0, gate.currentHealth - damage);
            
            if (gate.currentHealth <= 0)
            {
                gate.isDestroyed = true;
                OnGateDestroyed?.Invoke(gateId);
                Debug.Log($"🚪 Gate {gateId} has been destroyed!");
                AnnounceGateDestroyed(gateId);
            }
            
            Debug.Log($"🚪 Gate {gateId} took {damage} damage ({gate.currentHealth}/{gate.maxHealth})");
        }

        public void DamageCrystal(int damage)
        {
            if (crystal.isDestroyed) return;
            
            crystal.currentHealth = Mathf.Max(0, crystal.currentHealth - damage);
            
            if (crystal.currentHealth <= 0)
            {
                crystal.isDestroyed = true;
                OnCrystalDestroyed?.Invoke();
                Debug.Log($"💎 Crystal has been destroyed!");
                AnnounceCrystalDestroyed();
            }
            
            Debug.Log($"💎 Crystal took {damage} damage ({crystal.currentHealth}/{crystal.maxHealth})");
        }

        public void RecordKill(GameObject killer, GameObject victim)
        {
            bool killerIsAttacker = attackers.Contains(killer);
            bool victimIsAttacker = attackers.Contains(victim);
            
            if (killerIsAttacker && !victimIsAttacker)
            {
                attackerKills++;
            }
            else if (!killerIsAttacker && victimIsAttacker)
            {
                defenderKills++;
            }
            
            OnKillCountUpdated?.Invoke(attackerKills, defenderKills);
            Debug.Log($"💀 {killer.name} killed {victim.name} (A:{attackerKills} D:{defenderKills})");
        }

        public bool CanUseCatapult(GameObject player, int catapultId)
        {
            if (catapultId < 0 || catapultId >= catapults.Count) return false;
            
            var catapult = catapults[catapultId];
            return catapult.isOperational && catapult.operatorPlayer == null && attackers.Contains(player);
        }

        public void UseCatapult(GameObject player, int catapultId, Vector3 targetPosition)
        {
            if (!CanUseCatapult(player, catapultId)) return;
            
            var catapult = catapults[catapultId];
            if (catapult.cooldown > 0f) return;
            
            catapult.operatorPlayer = player;
            catapult.cooldown = 30f; // 30 second cooldown
            
            FireCatapult(catapult, targetPosition);
            
            Debug.Log($"🏹 {player.name} fired catapult {catapultId} at {targetPosition}");
        }

        private void FireCatapult(SiegeCatapult catapult, Vector3 targetPosition)
        {
            Collider[] hitTargets = Physics.OverlapSphere(targetPosition, 10f);
            foreach (var hit in hitTargets)
            {
                var character = hit.GetComponent<Character>();
                if (character != null && defenders.Contains(hit.gameObject))
                {
                    character.Stats.TakeDamage(300);
                    Debug.Log($"💥 Catapult hit {character.characterName} for 300 damage");
                }
                
                var gateComponent = hit.GetComponent<SiegeGateComponent>();
                if (gateComponent != null)
                {
                    DamageGate(gateComponent.GateId, 500);
                }
                
                var crystalComponent = hit.GetComponent<SiegeCrystalComponent>();
                if (crystalComponent != null)
                {
                    DamageCrystal(200);
                }
            }
        }

        private void EndSiege(Guild winningGuild, string reason)
        {
            SetState(CitySiegeState.Ending);
            
            OnSiegeWon?.Invoke(winningGuild);
            
            DistributeSiegeRewards(winningGuild);
            
            Debug.Log($"🏆 City siege ended: {winningGuild.name} wins! Reason: {reason}");
            AnnounceSiegeEnd(winningGuild, reason);
            
            Invoke(nameof(ResetSiege), 60f);
        }

        private void DistributeSiegeRewards(Guild winningGuild)
        {
            var winners = winningGuild.id == attackingGuild.id ? attackers : defenders;
            var losers = winningGuild.id == attackingGuild.id ? defenders : attackers;
            
            foreach (var player in winners)
            {
                if (player != null)
                {
                    var character = player.GetComponent<Character>();
                    if (character != null)
                    {
                        character.Stats.AddExperience(500);
                        character.Stats.AddGold(300);
                        Debug.Log($"🏆 {player.name} earned siege victory rewards: 500 XP, 300 gold");
                    }
                }
            }
            
            foreach (var player in losers)
            {
                if (player != null)
                {
                    var character = player.GetComponent<Character>();
                    if (character != null)
                    {
                        character.Stats.AddExperience(250);
                        character.Stats.AddGold(150);
                        Debug.Log($"🎖️ {player.name} earned siege participation rewards: 250 XP, 150 gold");
                    }
                }
            }
        }

        private void ResetSiege()
        {
            foreach (var player in attackers.ToArray())
            {
                LeaveSiege(player);
            }
            foreach (var player in defenders.ToArray())
            {
                LeaveSiege(player);
            }
            
            attackingGuild = null;
            defendingGuild = null;
            SetState(CitySiegeState.Scheduled);
            
            Debug.Log($"🔄 City siege reset and ready for scheduling");
        }

        private void SetState(CitySiegeState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        private void AnnounceSiegeScheduled()
        {
            Debug.Log($"📢 CITY SIEGE: {attackingGuild.name} will attack {cityName} defended by {defendingGuild.name}!");
        }

        private void AnnounceSiegeStart()
        {
            Debug.Log($"📢 CITY SIEGE: The battle for {cityName} has begun!");
        }

        private void AnnounceSiegeEnd(Guild winner, string reason)
        {
            Debug.Log($"📢 CITY SIEGE: {winner.name} has conquered {cityName}! {reason}");
        }

        private void AnnounceGateDestroyed(int gateId)
        {
            Debug.Log($"📢 CITY SIEGE: Gate {gateId} has been breached!");
        }

        private void AnnounceCrystalDestroyed()
        {
            Debug.Log($"📢 CITY SIEGE: The crystal has been shattered! Attackers victory!");
        }

        public SiegeGate GetGate(int gateId)
        {
            return gates.ContainsKey(gateId) ? gates[gateId] : null;
        }

        public SiegeCrystal GetCrystal()
        {
            return crystal;
        }

        public List<SiegeCatapult> GetCatapults()
        {
            return new List<SiegeCatapult>(catapults);
        }
    }

    [System.Serializable]
    public class SiegeGate
    {
        public int id;
        public Vector3 position;
        public int maxHealth;
        public int currentHealth;
        public bool isDestroyed;
    }

    [System.Serializable]
    public class SiegeCrystal
    {
        public Vector3 position;
        public int maxHealth;
        public int currentHealth;
        public bool isDestroyed;
    }

    [System.Serializable]
    public class SiegeCatapult
    {
        public int id;
        public Vector3 position;
        public bool isOperational;
        public float cooldown;
        public GameObject operatorPlayer;
    }

    public class SiegeGateComponent : MonoBehaviour
    {
        private int gateId;
        private CitySiegeManager siegeManager;

        public int GateId => gateId;

        public void Initialize(int id, CitySiegeManager manager)
        {
            gateId = id;
            siegeManager = manager;
        }

        public void TakeDamage(int damage)
        {
            siegeManager.DamageGate(gateId, damage);
        }
    }

    public class SiegeCrystalComponent : MonoBehaviour
    {
        private CitySiegeManager siegeManager;

        public void Initialize(CitySiegeManager manager)
        {
            siegeManager = manager;
        }

        public void TakeDamage(int damage)
        {
            siegeManager.DamageCrystal(damage);
        }
    }

    public class SiegeCatapultComponent : MonoBehaviour
    {
        private int catapultId;
        private CitySiegeManager siegeManager;

        public void Initialize(int id, CitySiegeManager manager)
        {
            catapultId = id;
            siegeManager = manager;
        }

        public void Fire(GameObject player, Vector3 targetPosition)
        {
            siegeManager.UseCatapult(player, catapultId, targetPosition);
        }
    }

    public enum CitySiegeState
    {
        Scheduled,
        Starting,
        Active,
        Ending,
        Completed
    }
}
