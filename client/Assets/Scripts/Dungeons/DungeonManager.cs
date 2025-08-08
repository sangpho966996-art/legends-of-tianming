using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class DungeonManager : MonoBehaviour
    {
        [Header("Dungeon Settings")]
        public int maxPartySize = 5;
        public float dungeonTimeout = 3600f; // 1 hour
        
        private Dictionary<string, Dungeon> availableDungeons = new Dictionary<string, Dungeon>();
        private Dictionary<string, DungeonInstance> activeDungeons = new Dictionary<string, DungeonInstance>();
        private Character character;

        public event System.Action<DungeonInstance> OnDungeonEntered;
        public event System.Action<DungeonInstance> OnDungeonCompleted;
        public event System.Action<DungeonInstance> OnDungeonFailed;

        private void Start()
        {
            character = GetComponent<Character>();
            InitializeDungeons();
        }

        private void Update()
        {
            UpdateActiveDungeons();
        }

        private void InitializeDungeons()
        {
            var dungeons = new List<Dungeon>
            {
                new Dungeon
                {
                    id = "ancient_ruins",
                    name = "Ancient Ruins",
                    description = "Mysterious ruins filled with ancient guardians and forgotten treasures.",
                    region = "Qingze Plains",
                    levelRange = new Vector2Int(30, 40),
                    difficulty = DungeonDifficulty.Normal,
                    maxPlayers = 5,
                    estimatedTime = 30, // minutes
                    entrancePosition = new Vector3(50, 0, 50),
                    bosses = new List<string> { "stone_guardian", "ancient_spirit" },
                    rewards = new DungeonRewards
                    {
                        experience = 500,
                        gold = 200,
                        items = new List<string> { "ancient_sword", "guardian_armor", "spirit_gem" }
                    }
                },
                new Dungeon
                {
                    id = "ancient_ruins_hard",
                    name = "Ancient Ruins (Hard)",
                    description = "The same ruins, but with more dangerous guardians and better rewards.",
                    region = "Qingze Plains",
                    levelRange = new Vector2Int(35, 45),
                    difficulty = DungeonDifficulty.Hard,
                    maxPlayers = 5,
                    estimatedTime = 45,
                    entrancePosition = new Vector3(50, 0, 50),
                    bosses = new List<string> { "elite_stone_guardian", "vengeful_ancient_spirit", "ruin_overlord" },
                    rewards = new DungeonRewards
                    {
                        experience = 800,
                        gold = 350,
                        items = new List<string> { "legendary_ancient_sword", "guardian_plate_armor", "major_spirit_gem" }
                    }
                },
                new Dungeon
                {
                    id = "beast_den",
                    name = "Wild Beast Den",
                    description = "A dangerous cave system inhabited by ferocious beasts.",
                    region = "Qingze Plains",
                    levelRange = new Vector2Int(32, 42),
                    difficulty = DungeonDifficulty.Normal,
                    maxPlayers = 5,
                    estimatedTime = 25,
                    entrancePosition = new Vector3(-30, 0, 40),
                    bosses = new List<string> { "alpha_wolf", "cave_bear_king" },
                    rewards = new DungeonRewards
                    {
                        experience = 450,
                        gold = 180,
                        items = new List<string> { "beast_fang_dagger", "fur_cloak", "beast_essence" }
                    }
                },
                new Dungeon
                {
                    id = "floating_temple",
                    name = "Floating Temple",
                    description = "A mystical temple suspended in the clouds above Yun City.",
                    region = "Yun City",
                    levelRange = new Vector2Int(50, 60),
                    difficulty = DungeonDifficulty.Normal,
                    maxPlayers = 5,
                    estimatedTime = 40,
                    entrancePosition = new Vector3(1000, 150, 0),
                    bosses = new List<string> { "cloud_monk", "wind_elemental", "sky_dragon" },
                    rewards = new DungeonRewards
                    {
                        experience = 1000,
                        gold = 400,
                        items = new List<string> { "cloud_walker_boots", "wind_staff", "dragon_scale" }
                    }
                },
                new Dungeon
                {
                    id = "frozen_caverns",
                    name = "Frozen Caverns",
                    description = "Ice-covered caverns in the highest peaks, home to ancient ice spirits.",
                    region = "Hanlin Peaks",
                    levelRange = new Vector2Int(70, 80),
                    difficulty = DungeonDifficulty.Normal,
                    maxPlayers = 5,
                    estimatedTime = 50,
                    entrancePosition = new Vector3(0, 600, 1000),
                    bosses = new List<string> { "ice_wraith", "frost_giant", "glacier_lord" },
                    rewards = new DungeonRewards
                    {
                        experience = 1500,
                        gold = 600,
                        items = new List<string> { "ice_blade", "frost_armor", "glacier_heart" }
                    }
                }
            };

            foreach (var dungeon in dungeons)
            {
                availableDungeons[dungeon.id] = dungeon;
            }
        }

        private void UpdateActiveDungeons()
        {
            var expiredDungeons = new List<string>();
            
            foreach (var instance in activeDungeons)
            {
                if (Time.time - instance.Value.startTime > dungeonTimeout)
                {
                    expiredDungeons.Add(instance.Key);
                }
            }
            
            foreach (var expired in expiredDungeons)
            {
                FailDungeon(expired, "Timeout");
            }
        }

        public bool EnterDungeon(string dungeonId, List<string> partyMembers = null)
        {
            if (!availableDungeons.ContainsKey(dungeonId))
            {
                Debug.LogWarning($"Dungeon not found: {dungeonId}");
                return false;
            }

            var dungeon = availableDungeons[dungeonId];
            
            if (character.level < dungeon.levelRange.x || character.level > dungeon.levelRange.y)
            {
                Debug.LogWarning($"Level requirement not met for {dungeon.name}");
                return false;
            }

            string instanceId = System.Guid.NewGuid().ToString();
            var instance = new DungeonInstance
            {
                id = instanceId,
                dungeonId = dungeonId,
                dungeon = dungeon,
                partyMembers = partyMembers ?? new List<string> { character.characterId },
                startTime = Time.time,
                status = DungeonStatus.Active,
                currentObjectives = new List<string> { "Clear all monsters", "Defeat the boss" }
            };

            activeDungeons[instanceId] = instance;
            
            TeleportToDungeon(instance);
            
            OnDungeonEntered?.Invoke(instance);
            Debug.Log($"🏛️ Entered dungeon: {dungeon.name}");
            
            return true;
        }

        private void TeleportToDungeon(DungeonInstance instance)
        {
            var playerManager = GameManager.Instance.GetPlayerManager();
            if (playerManager.LocalPlayer != null)
            {
                playerManager.LocalPlayer.transform.position = instance.dungeon.entrancePosition;
                Debug.Log($"🚪 Teleported to {instance.dungeon.name}");
            }
        }

        public void CompleteDungeon(string instanceId)
        {
            if (!activeDungeons.ContainsKey(instanceId)) return;
            
            var instance = activeDungeons[instanceId];
            instance.status = DungeonStatus.Completed;
            instance.endTime = Time.time;
            
            AwardDungeonRewards(instance);
            
            activeDungeons.Remove(instanceId);
            OnDungeonCompleted?.Invoke(instance);
            
            Debug.Log($"✅ Dungeon completed: {instance.dungeon.name}");
        }

        public void FailDungeon(string instanceId, string reason = "")
        {
            if (!activeDungeons.ContainsKey(instanceId)) return;
            
            var instance = activeDungeons[instanceId];
            instance.status = DungeonStatus.Failed;
            instance.endTime = Time.time;
            
            activeDungeons.Remove(instanceId);
            OnDungeonFailed?.Invoke(instance);
            
            Debug.Log($"❌ Dungeon failed: {instance.dungeon.name} - {reason}");
        }

        private void AwardDungeonRewards(DungeonInstance instance)
        {
            var rewards = instance.dungeon.rewards;
            
            if (rewards.experience > 0)
            {
                Debug.Log($"🎉 Gained {rewards.experience} experience from dungeon");
            }
            
            if (rewards.gold > 0)
            {
                character.Stats.AddGold(rewards.gold);
            }
            
            var inventory = character.GetComponent<InventorySystem>();
            if (inventory != null && rewards.items != null)
            {
                foreach (var itemId in rewards.items)
                {
                    if (Random.Range(0f, 1f) < 0.3f) // 30% chance
                    {
                        Debug.Log($"📦 Received dungeon reward: {itemId}");
                    }
                }
            }
        }

        public List<Dungeon> GetAvailableDungeons(string region = "")
        {
            var dungeons = new List<Dungeon>();
            
            foreach (var dungeon in availableDungeons.Values)
            {
                if (string.IsNullOrEmpty(region) || dungeon.region == region)
                {
                    if (character.level >= dungeon.levelRange.x && character.level <= dungeon.levelRange.y)
                    {
                        dungeons.Add(dungeon);
                    }
                }
            }
            
            return dungeons;
        }

        public DungeonInstance GetCurrentDungeon()
        {
            foreach (var instance in activeDungeons.Values)
            {
                if (instance.partyMembers.Contains(character.characterId))
                {
                    return instance;
                }
            }
            return null;
        }

        public void LeaveDungeon()
        {
            var currentDungeon = GetCurrentDungeon();
            if (currentDungeon != null)
            {
                currentDungeon.partyMembers.Remove(character.characterId);
                
                if (currentDungeon.partyMembers.Count == 0)
                {
                    FailDungeon(currentDungeon.id, "All players left");
                }
                
                var worldManager = GameManager.Instance.GetComponent<WorldManager>();
                if (worldManager != null)
                {
                    var playerManager = GameManager.Instance.GetPlayerManager();
                    if (playerManager.LocalPlayer != null)
                    {
                        playerManager.LocalPlayer.transform.position = worldManager.GetSpawnPosition();
                    }
                }
                
                Debug.Log($"🚪 Left dungeon: {currentDungeon.dungeon.name}");
            }
        }

        public void UpdateDungeonObjective(string instanceId, string objective, bool completed)
        {
            if (!activeDungeons.ContainsKey(instanceId)) return;
            
            var instance = activeDungeons[instanceId];
            
            if (completed && instance.currentObjectives.Contains(objective))
            {
                instance.currentObjectives.Remove(objective);
                instance.completedObjectives.Add(objective);
                
                Debug.Log($"✅ Dungeon objective completed: {objective}");
                
                if (instance.currentObjectives.Count == 0)
                {
                    CompleteDungeon(instanceId);
                }
            }
        }
    }

    [System.Serializable]
    public class Dungeon
    {
        public string id;
        public string name;
        public string description;
        public string region;
        public Vector2Int levelRange;
        public DungeonDifficulty difficulty;
        public int maxPlayers;
        public int estimatedTime; // in minutes
        public Vector3 entrancePosition;
        public List<string> bosses;
        public DungeonRewards rewards;
    }

    [System.Serializable]
    public class DungeonInstance
    {
        public string id;
        public string dungeonId;
        public Dungeon dungeon;
        public List<string> partyMembers;
        public float startTime;
        public float endTime;
        public DungeonStatus status;
        public List<string> currentObjectives = new List<string>();
        public List<string> completedObjectives = new List<string>();
    }

    [System.Serializable]
    public class DungeonRewards
    {
        public int experience;
        public int gold;
        public List<string> items;
    }

    public enum DungeonDifficulty
    {
        Normal,
        Hard,
        Nightmare
    }

    public enum DungeonStatus
    {
        Active,
        Completed,
        Failed
    }
}
