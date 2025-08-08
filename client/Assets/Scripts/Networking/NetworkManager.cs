using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class NetworkManager : MonoBehaviour
    {
        [Header("Network Settings")]
        public string serverUrl = "ws://localhost:3000";
        public bool autoConnect = true;
        public float reconnectDelay = 5f;
        
        private bool isConnected = false;
        private Dictionary<string, System.Action<Dictionary<string, object>>> messageHandlers;

        private void Start()
        {
            InitializeNetworkManager();
            
            if (autoConnect)
            {
                ConnectToServer();
            }
        }

        private void InitializeNetworkManager()
        {
            messageHandlers = new Dictionary<string, System.Action<Dictionary<string, object>>>();
            
            RegisterMessageHandler("player_move", OnPlayerMove);
            RegisterMessageHandler("skill_cast", OnSkillCast);
            RegisterMessageHandler("combat_hit", OnCombatHit);
            RegisterMessageHandler("chat_message", OnChatMessage);
            RegisterMessageHandler("duel_challenge", OnDuelChallenge);
            RegisterMessageHandler("match_found", OnMatchFound);
        }

        public void ConnectToServer()
        {
            Debug.Log($"🌐 Connecting to server: {serverUrl}");
            isConnected = true;
        }

        public void DisconnectFromServer()
        {
            Debug.Log("🌐 Disconnected from server");
            isConnected = false;
        }

        public void RegisterMessageHandler(string messageType, System.Action<Dictionary<string, object>> handler)
        {
            messageHandlers[messageType] = handler;
        }

        public void SendPlayerMove(Vector3 position, Quaternion rotation)
        {
            if (!isConnected) return;
            
            var data = new Dictionary<string, object>
            {
                ["type"] = "player_move",
                ["position"] = new { x = position.x, y = position.y, z = position.z },
                ["rotation"] = new { x = rotation.x, y = rotation.y, z = rotation.z, w = rotation.w }
            };
            
            SendMessage(data);
        }

        public void SendSkillCast(string skillId, Vector3 position, Vector3 direction)
        {
            if (!isConnected) return;
            
            var data = new Dictionary<string, object>
            {
                ["type"] = "skill_cast",
                ["skillId"] = skillId,
                ["position"] = new { x = position.x, y = position.y, z = position.z },
                ["direction"] = new { x = direction.x, y = direction.y, z = direction.z }
            };
            
            SendMessage(data);
        }

        public void SendChatMessage(string message, string channel)
        {
            if (!isConnected) return;
            
            var data = new Dictionary<string, object>
            {
                ["type"] = "chat_message",
                ["message"] = message,
                ["channel"] = channel
            };
            
            SendMessage(data);
        }

        public void SendWhisper(string targetPlayer, string message)
        {
            if (!isConnected) return;
            
            var data = new Dictionary<string, object>
            {
                ["type"] = "whisper",
                ["target"] = targetPlayer,
                ["message"] = message
            };
            
            SendMessage(data);
        }

        public void SendDuelChallenge(string targetPlayer)
        {
            if (!isConnected) return;
            
            var data = new Dictionary<string, object>
            {
                ["type"] = "duel_challenge",
                ["target"] = targetPlayer
            };
            
            SendMessage(data);
        }

        public void SendDuelResponse(bool accept)
        {
            if (!isConnected) return;
            
            var data = new Dictionary<string, object>
            {
                ["type"] = "duel_response",
                ["accept"] = accept
            };
            
            SendMessage(data);
        }

        public void SendPvPQueueRequest(string matchType)
        {
            if (!isConnected) return;
            
            var data = new Dictionary<string, object>
            {
                ["type"] = "pvp_queue",
                ["matchType"] = matchType
            };
            
            SendMessage(data);
        }

        public void SendLeaveMatch()
        {
            if (!isConnected) return;
            
            var data = new Dictionary<string, object>
            {
                ["type"] = "leave_match"
            };
            
            SendMessage(data);
        }

        private void SendMessage(Dictionary<string, object> data)
        {
            Debug.Log($"📤 Sending message: {data["type"]}");
        }

        private void OnPlayerMove(Dictionary<string, object> data)
        {
            var combatManager = GameManager.Instance?.GetCombatManager();
            if (combatManager != null)
            {
                combatManager.HandleCombatHit(data);
            }
        }

        private void OnSkillCast(Dictionary<string, object> data)
        {
            var combatManager = GameManager.Instance?.GetCombatManager();
            if (combatManager != null)
            {
                combatManager.HandleSkillCast(data);
            }
        }

        private void OnCombatHit(Dictionary<string, object> data)
        {
            var combatManager = GameManager.Instance?.GetCombatManager();
            if (combatManager != null)
            {
                combatManager.HandleCombatHit(data);
            }
        }

        private void OnChatMessage(Dictionary<string, object> data)
        {
            var chatUI = FindObjectOfType<ChatUI>();
            if (chatUI != null && data.ContainsKey("playerName") && data.ContainsKey("message") && data.ContainsKey("channel"))
            {
                chatUI.OnChatMessageReceived(
                    data["playerName"].ToString(),
                    data["message"].ToString(),
                    data["channel"].ToString()
                );
            }
        }

        private void OnDuelChallenge(Dictionary<string, object> data)
        {
            var pvpPanel = FindObjectOfType<PvPPanelUI>();
            if (pvpPanel != null && data.ContainsKey("fromPlayer"))
            {
                pvpPanel.OnDuelChallengeReceived(data["fromPlayer"].ToString());
            }
        }

        private void OnMatchFound(Dictionary<string, object> data)
        {
            var pvpPanel = FindObjectOfType<PvPPanelUI>();
            if (pvpPanel != null && data.ContainsKey("matchType"))
            {
                pvpPanel.OnMatchFound(data["matchType"].ToString());
            }
        }

        public bool IsConnected => isConnected;
    }
}
