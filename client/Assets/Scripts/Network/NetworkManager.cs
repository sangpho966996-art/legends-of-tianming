using UnityEngine;
using System;
using System.Collections.Generic;
using SocketIOClient;
using Newtonsoft.Json;

namespace LegendsOfTianming.Core
{
    public class NetworkManager : MonoBehaviour
    {
        [Header("Connection Settings")]
        public float reconnectDelay = 5f;
        public int maxReconnectAttempts = 5;
        
        private SocketIOUnity socket;
        private bool isConnected = false;
        private bool isConnecting = false;
        private int reconnectAttempts = 0;
        private string currentServerUrl;

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnConnectionError;

        public bool IsConnected => isConnected;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void ConnectToServer(string serverUrl)
        {
            if (isConnecting || isConnected) return;
            
            currentServerUrl = serverUrl;
            isConnecting = true;
            
            try
            {
                var uri = new Uri(serverUrl);
                socket = new SocketIOUnity(uri);
                
                SetupSocketEvents();
                socket.Connect();
                
                Debug.Log($"🌐 Connecting to server: {serverUrl}");
            }
            catch (Exception e)
            {
                Debug.LogError($"💥 Failed to connect to server: {e.Message}");
                OnConnectionError?.Invoke(e.Message);
                isConnecting = false;
            }
        }

        private void SetupSocketEvents()
        {
            socket.OnConnected += (sender, e) =>
            {
                isConnected = true;
                isConnecting = false;
                reconnectAttempts = 0;
                Debug.Log("✅ Connected to game server");
                OnConnected?.Invoke();
            };

            socket.OnDisconnected += (sender, e) =>
            {
                isConnected = false;
                isConnecting = false;
                Debug.Log("❌ Disconnected from game server");
                OnDisconnected?.Invoke();
                
                if (reconnectAttempts < maxReconnectAttempts)
                {
                    Invoke(nameof(AttemptReconnect), reconnectDelay);
                }
            };

            socket.OnError += (sender, e) =>
            {
                Debug.LogError($"💥 Socket error: {e}");
                OnConnectionError?.Invoke(e);
            };

            SetupGameEvents();
        }

        private void SetupGameEvents()
        {
            socket.On("player:joined", (response) =>
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
                Debug.Log("🎮 Player joined game world");
            });

            socket.On("player:moved", (response) =>
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
                GameManager.Instance.GetPlayerManager().HandlePlayerMoved(data);
            });

            socket.On("combat:hit", (response) =>
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
                GameManager.Instance.GetCombatManager().HandleCombatHit(data);
            });

            socket.On("skill:cast", (response) =>
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
                GameManager.Instance.GetCombatManager().HandleSkillCast(data);
            });

            socket.On("chat:message", (response) =>
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
                GameManager.Instance.GetUIManager().HandleChatMessage(data);
            });

            socket.On("pvp:challenge_received", (response) =>
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
                GameManager.Instance.GetUIManager().HandlePvPChallenge(data);
            });
        }

        public void SendPlayerJoin(string playerId, object characterData)
        {
            if (!isConnected) return;
            
            var data = new
            {
                playerId = playerId,
                characterData = characterData
            };
            
            socket.Emit("player:join", data);
        }

        public void SendPlayerMove(Vector3 position, Quaternion rotation)
        {
            if (!isConnected) return;
            
            var data = new
            {
                position = new { x = position.x, y = position.y, z = position.z },
                rotation = new { x = rotation.x, y = rotation.y, z = rotation.z, w = rotation.w }
            };
            
            socket.Emit("player:move", data);
        }

        public void SendCombatAction(string targetId, float damage, string skillId)
        {
            if (!isConnected) return;
            
            var data = new
            {
                targetId = targetId,
                damage = damage,
                skillId = skillId
            };
            
            socket.Emit("combat:attack", data);
        }

        public void SendSkillCast(string skillId, string targetId, Vector3 position)
        {
            if (!isConnected) return;
            
            var data = new
            {
                skillId = skillId,
                targetId = targetId,
                position = new { x = position.x, y = position.y, z = position.z }
            };
            
            socket.Emit("skill:cast", data);
        }

        public void SendChatMessage(string message, string channel = "global")
        {
            if (!isConnected) return;
            
            var data = new
            {
                message = message,
                channel = channel
            };
            
            socket.Emit("chat:message", data);
        }

        public void SendPvPChallenge(string targetPlayerId, string challengerName)
        {
            if (!isConnected) return;
            
            var data = new
            {
                targetPlayerId = targetPlayerId,
                challengerName = challengerName
            };
            
            socket.Emit("pvp:challenge", data);
        }

        private void AttemptReconnect()
        {
            if (isConnected || isConnecting) return;
            
            reconnectAttempts++;
            Debug.Log($"🔄 Attempting to reconnect ({reconnectAttempts}/{maxReconnectAttempts})");
            ConnectToServer(currentServerUrl);
        }

        public void Disconnect()
        {
            if (socket != null)
            {
                socket.Disconnect();
                socket = null;
            }
            isConnected = false;
            isConnecting = false;
        }

        private void OnDestroy()
        {
            Disconnect();
        }
    }
}
