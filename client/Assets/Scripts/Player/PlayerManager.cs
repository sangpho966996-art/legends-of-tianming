using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Player Settings")]
        public GameObject playerPrefab;
        public Transform spawnPoint;
        
        private Dictionary<string, GameObject> remotePlayers = new Dictionary<string, GameObject>();
        private GameObject localPlayer;
        private string localPlayerId;

        public GameObject LocalPlayer => localPlayer;
        public string LocalPlayerId => localPlayerId;

        private void Start()
        {
            if (spawnPoint == null)
            {
                spawnPoint = new GameObject("SpawnPoint").transform;
                spawnPoint.position = Vector3.zero;
            }
        }

        public void SpawnLocalPlayer(string playerId, CharacterData characterData)
        {
            localPlayerId = playerId;
            
            if (localPlayer != null)
            {
                Destroy(localPlayer);
            }

            localPlayer = CreatePlayerObject(characterData);
            localPlayer.transform.position = spawnPoint.position;
            
            var playerController = localPlayer.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetAsLocalPlayer(true);
            }

            Debug.Log($"👤 Local player spawned: {characterData.name} ({characterData.characterClass})");
        }

        public void SpawnRemotePlayer(string playerId, CharacterData characterData)
        {
            if (remotePlayers.ContainsKey(playerId))
            {
                Destroy(remotePlayers[playerId]);
            }

            GameObject remotePlayer = CreatePlayerObject(characterData);
            remotePlayer.transform.position = spawnPoint.position;
            
            var playerController = remotePlayer.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetAsLocalPlayer(false);
            }

            remotePlayers[playerId] = remotePlayer;
            Debug.Log($"👥 Remote player spawned: {characterData.name}");
        }

        private GameObject CreatePlayerObject(CharacterData characterData)
        {
            GameObject player;
            
            if (playerPrefab != null)
            {
                player = Instantiate(playerPrefab);
            }
            else
            {
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Player";
                
                player.AddComponent<PlayerController>();
                player.AddComponent<CharacterStats>();
                player.AddComponent<SkillSystem>();
                player.AddComponent<EquipmentSystem>();
            }

            var characterComponent = player.GetComponent<Character>();
            if (characterComponent == null)
            {
                characterComponent = player.AddComponent<Character>();
            }
            
            characterComponent.Initialize(characterData);
            
            return player;
        }

        public void HandlePlayerMoved(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("playerId")) return;
            
            string playerId = data["playerId"].ToString();
            
            if (playerId == localPlayerId) return;
            
            if (remotePlayers.ContainsKey(playerId))
            {
                var player = remotePlayers[playerId];
                var controller = player.GetComponent<PlayerController>();
                
                if (controller != null && data.ContainsKey("position"))
                {
                    var posData = data["position"] as Dictionary<string, object>;
                    Vector3 newPosition = new Vector3(
                        float.Parse(posData["x"].ToString()),
                        float.Parse(posData["y"].ToString()),
                        float.Parse(posData["z"].ToString())
                    );
                    
                    controller.SetTargetPosition(newPosition);
                }
            }
        }

        public void RemovePlayer(string playerId)
        {
            if (remotePlayers.ContainsKey(playerId))
            {
                Destroy(remotePlayers[playerId]);
                remotePlayers.Remove(playerId);
                Debug.Log($"👥 Remote player removed: {playerId}");
            }
        }

        public GameObject GetPlayer(string playerId)
        {
            if (playerId == localPlayerId)
                return localPlayer;
            
            return remotePlayers.ContainsKey(playerId) ? remotePlayers[playerId] : null;
        }

        public List<GameObject> GetAllPlayers()
        {
            List<GameObject> allPlayers = new List<GameObject>();
            
            if (localPlayer != null)
                allPlayers.Add(localPlayer);
            
            allPlayers.AddRange(remotePlayers.Values);
            
            return allPlayers;
        }
    }

    [System.Serializable]
    public class CharacterData
    {
        public string id;
        public string name;
        public string characterClass;
        public int level;
        public long experience;
        public CharacterStatsData stats;
        public Vector3 position;
        public string region;
    }

    [System.Serializable]
    public class CharacterStatsData
    {
        public int strength = 10;
        public int agility = 10;
        public int intelligence = 10;
        public int vitality = 10;
        public int health = 100;
        public int maxHealth = 100;
        public int mana = 100;
        public int maxMana = 100;
        public int gold = 1000;
        public int silver = 0;
    }
}
