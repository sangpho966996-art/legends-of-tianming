using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class RegionManager : MonoBehaviour
    {
        [Header("Region Settings")]
        public string currentRegionId = "qingze_plains";
        public Transform[] spawnPoints;
        public GameObject[] monsterPrefabs;
        
        [Header("Monster Spawning")]
        public int maxMonstersPerRegion = 50;
        public float monsterSpawnRadius = 100f;
        public float respawnTime = 30f;
        
        private Dictionary<string, RegionConfig> regions = new Dictionary<string, RegionConfig>();
        private List<GameObject> activeMonsters = new List<GameObject>();
        private Dictionary<Vector3, float> monsterSpawnTimers = new Dictionary<Vector3, float>();
        private WorldManager worldManager;

        private void Start()
        {
            worldManager = GetComponent<WorldManager>();
            InitializeRegions();
            StartMonsterSpawning();
        }

        private void Update()
        {
            UpdateMonsterSpawning();
        }

        private void InitializeRegions()
        {
            regions["qingze_plains"] = new RegionConfig
            {
                id = "qingze_plains",
                name = "Qingze Plains",
                levelRange = new Vector2Int(30, 50),
                description = "Vast grasslands with rolling hills and ancient ruins. Home to wild beasts and wandering spirits.",
                ambientMusic = "qingze_ambient",
                weatherTypes = new WeatherType[] { WeatherType.Clear, WeatherType.Rain, WeatherType.Fog },
                monsterTypes = new MonsterSpawnData[]
                {
                    new MonsterSpawnData { monsterId = "wild_rabbit", level = 30, spawnWeight = 40, maxCount = 15 },
                    new MonsterSpawnData { monsterId = "grass_wolf", level = 35, spawnWeight = 30, maxCount = 10 },
                    new MonsterSpawnData { monsterId = "stone_golem", level = 40, spawnWeight = 20, maxCount = 8 },
                    new MonsterSpawnData { monsterId = "wind_spirit", level = 45, spawnWeight = 10, maxCount = 5 }
                },
                npcs = new NPCSpawnData[]
                {
                    new NPCSpawnData { npcId = "equipment_master", position = new Vector3(10, 0, 10), rotation = Quaternion.identity },
                    new NPCSpawnData { npcId = "quest_giver_chen", position = new Vector3(-15, 0, 20), rotation = Quaternion.identity },
                    new NPCSpawnData { npcId = "merchant_li", position = new Vector3(25, 0, -10), rotation = Quaternion.identity },
                    new NPCSpawnData { npcId = "trainer_master", position = new Vector3(0, 0, 30), rotation = Quaternion.identity }
                },
                dungeonEntrances = new DungeonEntrance[]
                {
                    new DungeonEntrance { dungeonId = "ancient_ruins", position = new Vector3(50, 0, 50), levelRequirement = 35 },
                    new DungeonEntrance { dungeonId = "spirit_cave", position = new Vector3(-40, 0, 60), levelRequirement = 40 }
                }
            };

            regions["yun_city"] = new RegionConfig
            {
                id = "yun_city",
                name = "Yun City",
                levelRange = new Vector2Int(50, 70),
                description = "A bustling martial arts city built on floating islands connected by bridges.",
                ambientMusic = "yun_city_ambient",
                weatherTypes = new WeatherType[] { WeatherType.Clear, WeatherType.Fog, WeatherType.Wind },
                monsterTypes = new MonsterSpawnData[]
                {
                    new MonsterSpawnData { monsterId = "sky_hawk", level = 50, spawnWeight = 35, maxCount = 12 },
                    new MonsterSpawnData { monsterId = "cloud_serpent", level = 55, spawnWeight = 30, maxCount = 10 },
                    new MonsterSpawnData { monsterId = "wind_guardian", level = 60, spawnWeight = 25, maxCount = 8 },
                    new MonsterSpawnData { monsterId = "storm_elemental", level = 65, spawnWeight = 10, maxCount = 5 }
                },
                npcs = new NPCSpawnData[]
                {
                    new NPCSpawnData { npcId = "city_guard_captain", position = new Vector3(1000, 100, 0), rotation = Quaternion.identity },
                    new NPCSpawnData { npcId = "flying_mount_trainer", position = new Vector3(1020, 100, 10), rotation = Quaternion.identity },
                    new NPCSpawnData { npcId = "advanced_merchant", position = new Vector3(980, 100, -15), rotation = Quaternion.identity }
                },
                dungeonEntrances = new DungeonEntrance[]
                {
                    new DungeonEntrance { dungeonId = "floating_temple", position = new Vector3(1050, 120, 30), levelRequirement = 55 },
                    new DungeonEntrance { dungeonId = "sky_fortress", position = new Vector3(950, 110, 40), levelRequirement = 60 }
                }
            };

            regions["hanlin_peaks"] = new RegionConfig
            {
                id = "hanlin_peaks",
                name = "Hanlin Peaks",
                levelRange = new Vector2Int(70, 90),
                description = "Treacherous mountain peaks shrouded in mist, home to powerful cultivators and ancient secrets.",
                ambientMusic = "hanlin_ambient",
                weatherTypes = new WeatherType[] { WeatherType.Snow, WeatherType.Fog, WeatherType.Clear },
                monsterTypes = new MonsterSpawnData[]
                {
                    new MonsterSpawnData { monsterId = "frost_tiger", level = 70, spawnWeight = 30, maxCount = 10 },
                    new MonsterSpawnData { monsterId = "ice_dragon", level = 75, spawnWeight = 25, maxCount = 8 },
                    new MonsterSpawnData { monsterId = "mountain_giant", level = 80, spawnWeight = 25, maxCount = 6 },
                    new MonsterSpawnData { monsterId = "ancient_cultivator", level = 85, spawnWeight = 20, maxCount = 4 }
                },
                npcs = new NPCSpawnData[]
                {
                    new NPCSpawnData { npcId = "peak_master", position = new Vector3(0, 500, 1000), rotation = Quaternion.identity },
                    new NPCSpawnData { npcId = "hermit_sage", position = new Vector3(20, 520, 980), rotation = Quaternion.identity },
                    new NPCSpawnData { npcId = "artifact_merchant", position = new Vector3(-30, 510, 1020), rotation = Quaternion.identity }
                },
                dungeonEntrances = new DungeonEntrance[]
                {
                    new DungeonEntrance { dungeonId = "frozen_palace", position = new Vector3(40, 530, 960), levelRequirement = 75 },
                    new DungeonEntrance { dungeonId = "dragon_lair", position = new Vector3(-50, 540, 1040), levelRequirement = 80 }
                }
            };
        }

        private void StartMonsterSpawning()
        {
            if (!regions.ContainsKey(currentRegionId)) return;
            
            var region = regions[currentRegionId];
            
            foreach (var monsterType in region.monsterTypes)
            {
                for (int i = 0; i < monsterType.maxCount; i++)
                {
                    SpawnMonster(monsterType);
                }
            }
            
            Debug.Log($"🗺️ Started monster spawning for {region.name}");
        }

        private void UpdateMonsterSpawning()
        {
            activeMonsters.RemoveAll(monster => monster == null);
            
            var expiredSpawns = new List<Vector3>();
            foreach (var spawn in monsterSpawnTimers)
            {
                if (Time.time >= spawn.Value)
                {
                    expiredSpawns.Add(spawn.Key);
                }
            }
            
            foreach (var spawnPos in expiredSpawns)
            {
                monsterSpawnTimers.Remove(spawnPos);
                RespawnMonsterAtLocation(spawnPos);
            }
        }

        private void SpawnMonster(MonsterSpawnData monsterData)
        {
            if (activeMonsters.Count >= maxMonstersPerRegion) return;
            
            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject monsterPrefab = GetMonsterPrefab(monsterData.monsterId);
            
            if (monsterPrefab != null)
            {
                GameObject monster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
                
                var monsterAI = monster.GetComponent<MonsterAI>();
                if (monsterAI != null)
                {
                    monsterAI.level = monsterData.level;
                    monsterAI.health = monsterData.level * 50;
                    monsterAI.maxHealth = monsterAI.health;
                    monsterAI.attackDamage = monsterData.level * 3;
                }
                
                activeMonsters.Add(monster);
                
                var monsterStats = monster.GetComponent<CharacterStats>();
                if (monsterStats != null)
                {
                    monsterStats.OnHealthChanged += (current, max) =>
                    {
                        if (current <= 0)
                        {
                            ScheduleRespawn(spawnPosition);
                        }
                    };
                }
                
                Debug.Log($"👹 Spawned {monsterData.monsterId} (Level {monsterData.level}) at {spawnPosition}");
            }
        }

        private void ScheduleRespawn(Vector3 position)
        {
            monsterSpawnTimers[position] = Time.time + respawnTime;
        }

        private void RespawnMonsterAtLocation(Vector3 position)
        {
            if (!regions.ContainsKey(currentRegionId)) return;
            
            var region = regions[currentRegionId];
            var randomMonster = GetRandomMonsterType(region.monsterTypes);
            
            if (randomMonster != null)
            {
                SpawnMonster(randomMonster);
            }
        }

        private MonsterSpawnData GetRandomMonsterType(MonsterSpawnData[] monsterTypes)
        {
            int totalWeight = 0;
            foreach (var monster in monsterTypes)
            {
                totalWeight += monster.spawnWeight;
            }
            
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;
            
            foreach (var monster in monsterTypes)
            {
                currentWeight += monster.spawnWeight;
                if (randomValue < currentWeight)
                {
                    return monster;
                }
            }
            
            return monsterTypes[0];
        }

        private Vector3 GetRandomSpawnPosition()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                Vector3 randomOffset = Random.insideUnitSphere * monsterSpawnRadius;
                randomOffset.y = 0;
                return randomSpawnPoint.position + randomOffset;
            }
            
            Vector3 randomPos = Random.insideUnitSphere * monsterSpawnRadius;
            randomPos.y = 0;
            return randomPos;
        }

        private GameObject GetMonsterPrefab(string monsterId)
        {
            if (monsterPrefabs != null && monsterPrefabs.Length > 0)
            {
                return monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];
            }
            return null;
        }

        public void ChangeRegion(string newRegionId)
        {
            if (!regions.ContainsKey(newRegionId)) return;
            
            ClearCurrentRegion();
            
            currentRegionId = newRegionId;
            var newRegion = regions[newRegionId];
            
            if (worldManager != null)
            {
                worldManager.ChangeRegion(newRegion.name);
            }
            
            SpawnNPCs(newRegion);
            
            StartMonsterSpawning();
            
            Debug.Log($"🗺️ Changed to region: {newRegion.name}");
        }

        private void ClearCurrentRegion()
        {
            foreach (var monster in activeMonsters)
            {
                if (monster != null)
                    Destroy(monster);
            }
            activeMonsters.Clear();
            monsterSpawnTimers.Clear();
            
            var npcManager = FindObjectOfType<NPCManager>();
            if (npcManager != null)
            {
                npcManager.DespawnAllNPCs();
            }
        }

        private void SpawnNPCs(RegionConfig region)
        {
            var npcManager = FindObjectOfType<NPCManager>();
            if (npcManager != null)
            {
                foreach (var npcData in region.npcs)
                {
                    npcManager.SpawnNPC(npcData.npcId, npcData.position, npcData.rotation);
                }
            }
        }

        public RegionConfig GetCurrentRegion()
        {
            return regions.ContainsKey(currentRegionId) ? regions[currentRegionId] : null;
        }

        public List<RegionConfig> GetAllRegions()
        {
            return new List<RegionConfig>(regions.Values);
        }

        public bool IsValidRegion(string regionId)
        {
            return regions.ContainsKey(regionId);
        }
    }

    [System.Serializable]
    public class RegionConfig
    {
        public string id;
        public string name;
        public Vector2Int levelRange;
        public string description;
        public string ambientMusic;
        public WeatherType[] weatherTypes;
        public MonsterSpawnData[] monsterTypes;
        public NPCSpawnData[] npcs;
        public DungeonEntrance[] dungeonEntrances;
    }

    [System.Serializable]
    public class MonsterSpawnData
    {
        public string monsterId;
        public int level;
        public int spawnWeight;
        public int maxCount;
    }

    [System.Serializable]
    public class NPCSpawnData
    {
        public string npcId;
        public Vector3 position;
        public Quaternion rotation;
    }

    [System.Serializable]
    public class DungeonEntrance
    {
        public string dungeonId;
        public Vector3 position;
        public int levelRequirement;
    }
}
