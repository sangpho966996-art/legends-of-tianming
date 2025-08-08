using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class NPCManager : MonoBehaviour
    {
        [Header("NPC Settings")]
        public float interactionRange = 3f;
        public LayerMask playerLayerMask = -1;
        
        private Dictionary<string, NPC> npcs = new Dictionary<string, NPC>();
        private Dictionary<string, GameObject> npcGameObjects = new Dictionary<string, GameObject>();

        private void Start()
        {
            InitializeNPCs();
        }

        private void InitializeNPCs()
        {
            var qingzeNPCs = new List<NPC>
            {
                new NPC
                {
                    id = "master_chen",
                    name = "Master Chen",
                    title = "Sect Elder",
                    npcType = NPCType.QuestGiver,
                    region = "Qingze Plains",
                    position = new Vector3(10, 0, 10),
                    dialogue = new List<string>
                    {
                        "Welcome, young cultivator, to the world of Tianming.",
                        "The path of martial arts is long and treacherous.",
                        "Prove your worth by completing the trials ahead."
                    },
                    questIds = new List<string> { "welcome_to_tianming", "first_equipment" },
                    shopItems = new List<string>()
                },
                new NPC
                {
                    id = "equipment_master",
                    name = "Blacksmith Wang",
                    title = "Equipment Master",
                    npcType = NPCType.Merchant,
                    region = "Qingze Plains",
                    position = new Vector3(15, 0, 5),
                    dialogue = new List<string>
                    {
                        "Need better equipment? You've come to the right place!",
                        "I forge the finest weapons and armor in all of Qingze Plains.",
                        "Quality costs gold, but your life is worth more."
                    },
                    questIds = new List<string> { "first_equipment" },
                    shopItems = new List<string> { "starter_sword", "starter_staff", "leather_armor", "health_potion_small" }
                },
                new NPC
                {
                    id = "herb_collector",
                    name = "Elder Liu",
                    title = "Herb Collector",
                    npcType = NPCType.QuestGiver,
                    region = "Qingze Plains",
                    position = new Vector3(-5, 0, 20),
                    dialogue = new List<string>
                    {
                        "The herbs in these plains have mystical properties.",
                        "I need someone brave enough to gather them for me.",
                        "Beware of the wild beasts that guard the precious plants."
                    },
                    questIds = new List<string> { "herb_gathering", "beast_threat" },
                    shopItems = new List<string> { "health_potion_small", "mana_potion_small", "herb_seeds" }
                },
                new NPC
                {
                    id = "training_master",
                    name = "Instructor Zhang",
                    title = "Combat Instructor",
                    npcType = NPCType.Trainer,
                    region = "Qingze Plains",
                    position = new Vector3(0, 0, -10),
                    dialogue = new List<string>
                    {
                        "Your stance is weak! Your form needs work!",
                        "True martial arts comes from discipline and practice.",
                        "Show me your skills, and I'll teach you new techniques."
                    },
                    questIds = new List<string> { "skill_training", "combat_mastery" },
                    shopItems = new List<string>()
                },
                new NPC
                {
                    id = "merchant_li",
                    name = "Trader Li",
                    title = "Traveling Merchant",
                    npcType = NPCType.Merchant,
                    region = "Qingze Plains",
                    position = new Vector3(20, 0, 15),
                    dialogue = new List<string>
                    {
                        "Fresh goods from distant lands!",
                        "I travel the three regions, bringing rare items.",
                        "Gold talks, everything else walks!"
                    },
                    questIds = new List<string>(),
                    shopItems = new List<string> { "travel_rations", "repair_kit", "basic_gems", "mount_feed" }
                }
            };

            foreach (var npc in qingzeNPCs)
            {
                npcs[npc.id] = npc;
                SpawnNPC(npc);
            }
        }

        private void SpawnNPC(NPC npc)
        {
            GameObject npcObj = new GameObject($"NPC_{npc.name}");
            npcObj.transform.position = npc.position;
            
            var npcComponent = npcObj.AddComponent<NPCBehavior>();
            npcComponent.Initialize(npc);
            
            var collider = npcObj.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.isTrigger = true;
            
            npcGameObjects[npc.id] = npcObj;
            
            Debug.Log($"👤 Spawned NPC: {npc.name} at {npc.position}");
        }

        public NPC GetNPC(string npcId)
        {
            return npcs.ContainsKey(npcId) ? npcs[npcId] : null;
        }

        public GameObject GetNPCGameObject(string npcId)
        {
            return npcGameObjects.ContainsKey(npcId) ? npcGameObjects[npcId] : null;
        }

        public List<NPC> GetNPCsInRegion(string region)
        {
            var regionNPCs = new List<NPC>();
            foreach (var npc in npcs.Values)
            {
                if (npc.region == region)
                    regionNPCs.Add(npc);
            }
            return regionNPCs;
        }

        public NPC FindNearestNPC(Vector3 position, float maxDistance = 10f)
        {
            NPC nearestNPC = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var npc in npcs.Values)
            {
                float distance = Vector3.Distance(position, npc.position);
                if (distance <= maxDistance && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestNPC = npc;
                }
            }
            
            return nearestNPC;
        }

        public void InteractWithNPC(string npcId, string playerId)
        {
            if (!npcs.ContainsKey(npcId)) return;
            
            var npc = npcs[npcId];
            var player = GameManager.Instance.GetPlayerManager().GetPlayer(playerId);
            
            if (player == null) return;
            
            float distance = Vector3.Distance(player.transform.position, npc.position);
            if (distance > interactionRange)
            {
                Debug.LogWarning($"Too far from NPC {npc.name}");
                return;
            }
            
            switch (npc.npcType)
            {
                case NPCType.QuestGiver:
                    HandleQuestGiverInteraction(npc, player);
                    break;
                case NPCType.Merchant:
                    HandleMerchantInteraction(npc, player);
                    break;
                case NPCType.Trainer:
                    HandleTrainerInteraction(npc, player);
                    break;
                case NPCType.Guard:
                    HandleGuardInteraction(npc, player);
                    break;
            }
            
            Debug.Log($"💬 {playerId} interacted with {npc.name}");
        }

        private void HandleQuestGiverInteraction(NPC npc, GameObject player)
        {
            var character = player.GetComponent<Character>();
            var questSystem = character.GetComponent<QuestSystem>();
            
            if (questSystem != null)
            {
                foreach (var questId in npc.questIds)
                {
                    var availableQuests = questSystem.GetAvailableQuests();
                    var quest = availableQuests.Find(q => q.id == questId);
                    
                    if (quest != null)
                    {
                        Debug.Log($"📜 Quest available: {quest.title}");
                    }
                }
                
                questSystem.OnNPCInteracted(npc.id);
            }
            
            ShowDialogue(npc, player);
        }

        private void HandleMerchantInteraction(NPC npc, GameObject player)
        {
            Debug.Log($"🛒 Opening shop for {npc.name}");
            ShowDialogue(npc, player);
        }

        private void HandleTrainerInteraction(NPC npc, GameObject player)
        {
            var character = player.GetComponent<Character>();
            var skillTree = character.GetComponent<SkillTree>();
            
            if (skillTree != null)
            {
                Debug.Log($"🎯 Training available with {npc.name}");
            }
            
            ShowDialogue(npc, player);
        }

        private void HandleGuardInteraction(NPC npc, GameObject player)
        {
            Debug.Log($"🛡️ Guard {npc.name} provides information");
            ShowDialogue(npc, player);
        }

        private void ShowDialogue(NPC npc, GameObject player)
        {
            if (npc.dialogue.Count > 0)
            {
                string randomDialogue = npc.dialogue[Random.Range(0, npc.dialogue.Count)];
                Debug.Log($"💬 {npc.name}: {randomDialogue}");
            }
        }

        public void UpdateNPCPosition(string npcId, Vector3 newPosition)
        {
            if (npcs.ContainsKey(npcId))
            {
                npcs[npcId].position = newPosition;
                
                if (npcGameObjects.ContainsKey(npcId))
                {
                    npcGameObjects[npcId].transform.position = newPosition;
                }
            }
        }

        public void AddNPC(NPC npc)
        {
            npcs[npc.id] = npc;
            SpawnNPC(npc);
        }

        public void RemoveNPC(string npcId)
        {
            if (npcGameObjects.ContainsKey(npcId))
            {
                Destroy(npcGameObjects[npcId]);
                npcGameObjects.Remove(npcId);
            }
            
            if (npcs.ContainsKey(npcId))
            {
                npcs.Remove(npcId);
            }
        }
    }

    [System.Serializable]
    public class NPC
    {
        public string id;
        public string name;
        public string title;
        public NPCType npcType;
        public string region;
        public Vector3 position;
        public List<string> dialogue;
        public List<string> questIds;
        public List<string> shopItems;
        public int level = 1;
        public bool isActive = true;
    }

    public enum NPCType
    {
        QuestGiver,
        Merchant,
        Trainer,
        Guard,
        Civilian
    }
}
