using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class ChatSystem : MonoBehaviour
    {
        [Header("Chat Settings")]
        public int maxChatHistory = 100;
        public int maxMessageLength = 200;
        public float chatCooldown = 1f;
        public bool profanityFilter = true;
        
        private Dictionary<ChatChannel, List<ChatMessage>> chatHistory = new Dictionary<ChatChannel, List<ChatMessage>>();
        private ChatChannel currentChannel = ChatChannel.World;
        private float lastMessageTime = 0f;
        private Character character;
        private List<string> blockedPlayers = new List<string>();

        public event System.Action<ChatMessage> OnMessageReceived;
        public event System.Action<ChatChannel> OnChannelChanged;
        public event System.Action<string> OnPlayerBlocked;
        public event System.Action<string> OnPlayerUnblocked;

        public ChatChannel CurrentChannel => currentChannel;
        public bool CanSendMessage => Time.time - lastMessageTime >= chatCooldown;

        private void Start()
        {
            character = GetComponent<Character>();
            InitializeChatChannels();
        }

        private void InitializeChatChannels()
        {
            foreach (ChatChannel channel in System.Enum.GetValues(typeof(ChatChannel)))
            {
                chatHistory[channel] = new List<ChatMessage>();
            }
        }

        public void SendMessage(string message, ChatChannel channel = ChatChannel.World)
        {
            if (!CanSendMessage)
            {
                Debug.LogWarning("Chat cooldown active");
                return;
            }

            if (string.IsNullOrEmpty(message) || message.Length > maxMessageLength)
            {
                Debug.LogWarning("Invalid message length");
                return;
            }

            if (profanityFilter)
            {
                message = FilterProfanity(message);
            }

            var chatMessage = new ChatMessage
            {
                id = System.Guid.NewGuid().ToString(),
                senderId = character.characterId,
                senderName = character.characterName,
                content = message,
                channel = channel,
                timestamp = System.DateTime.UtcNow,
                senderLevel = character.Stats.Level,
                senderClass = character.characterClass.ToString()
            };

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var messageData = new
                {
                    senderId = chatMessage.senderId,
                    senderName = chatMessage.senderName,
                    content = chatMessage.content,
                    channel = chatMessage.channel.ToString(),
                    timestamp = chatMessage.timestamp,
                    senderLevel = chatMessage.senderLevel,
                    senderClass = chatMessage.senderClass
                };
                
                Debug.Log($"💬 [{channel}] {character.characterName}: {message}");
            }

            AddMessageToHistory(chatMessage);
            lastMessageTime = Time.time;
        }

        public void HandleMessageReceived(Dictionary<string, object> data)
        {
            try
            {
                var message = new ChatMessage
                {
                    id = data["id"].ToString(),
                    senderId = data["senderId"].ToString(),
                    senderName = data["senderName"].ToString(),
                    content = data["content"].ToString(),
                    channel = (ChatChannel)System.Enum.Parse(typeof(ChatChannel), data["channel"].ToString()),
                    timestamp = System.DateTime.Parse(data["timestamp"].ToString()),
                    senderLevel = int.Parse(data["senderLevel"].ToString()),
                    senderClass = data["senderClass"].ToString()
                };

                if (blockedPlayers.Contains(message.senderId))
                {
                    return;
                }

                AddMessageToHistory(message);
                OnMessageReceived?.Invoke(message);
                
                Debug.Log($"💬 [{message.channel}] {message.senderName}: {message.content}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing chat message: {e.Message}");
            }
        }

        private void AddMessageToHistory(ChatMessage message)
        {
            if (!chatHistory.ContainsKey(message.channel))
            {
                chatHistory[message.channel] = new List<ChatMessage>();
            }

            var channelHistory = chatHistory[message.channel];
            channelHistory.Add(message);

            if (channelHistory.Count > maxChatHistory)
            {
                channelHistory.RemoveAt(0);
            }
        }

        public void SwitchChannel(ChatChannel channel)
        {
            if (currentChannel != channel)
            {
                currentChannel = channel;
                OnChannelChanged?.Invoke(channel);
                Debug.Log($"💬 Switched to {channel} channel");
            }
        }

        public void SendPrivateMessage(string targetPlayerId, string message)
        {
            if (string.IsNullOrEmpty(message) || message.Length > maxMessageLength)
            {
                Debug.LogWarning("Invalid private message length");
                return;
            }

            if (blockedPlayers.Contains(targetPlayerId))
            {
                Debug.LogWarning("Cannot send message to blocked player");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var messageData = new
                {
                    senderId = character.characterId,
                    senderName = character.characterName,
                    targetId = targetPlayerId,
                    content = FilterProfanity(message),
                    timestamp = System.DateTime.UtcNow
                };
                
                Debug.Log($"💬 [Private] To {targetPlayerId}: {message}");
            }
        }

        public void SendGuildMessage(string message)
        {
            var guildSystem = GetComponent<GuildSystem>();
            if (guildSystem == null || !guildSystem.IsInGuild)
            {
                Debug.LogWarning("Not in a guild");
                return;
            }

            SendMessage(message, ChatChannel.Guild);
        }

        public void SendPartyMessage(string message)
        {
            SendMessage(message, ChatChannel.Party);
        }

        public void BlockPlayer(string playerId, string playerName)
        {
            if (!blockedPlayers.Contains(playerId))
            {
                blockedPlayers.Add(playerId);
                OnPlayerBlocked?.Invoke(playerName);
                
                foreach (var channelHistory in chatHistory.Values)
                {
                    channelHistory.RemoveAll(m => m.senderId == playerId);
                }
                
                Debug.Log($"🚫 Blocked player: {playerName}");
            }
        }

        public void UnblockPlayer(string playerId, string playerName)
        {
            if (blockedPlayers.Remove(playerId))
            {
                OnPlayerUnblocked?.Invoke(playerName);
                Debug.Log($"✅ Unblocked player: {playerName}");
            }
        }

        public bool IsPlayerBlocked(string playerId)
        {
            return blockedPlayers.Contains(playerId);
        }

        public List<ChatMessage> GetChannelHistory(ChatChannel channel)
        {
            if (chatHistory.ContainsKey(channel))
            {
                return new List<ChatMessage>(chatHistory[channel]);
            }
            return new List<ChatMessage>();
        }

        public List<ChatMessage> GetRecentMessages(ChatChannel channel, int count = 20)
        {
            var history = GetChannelHistory(channel);
            int startIndex = Mathf.Max(0, history.Count - count);
            return history.GetRange(startIndex, history.Count - startIndex);
        }

        private string FilterProfanity(string message)
        {
            string[] profanityList = { "badword1", "badword2", "badword3" };
            
            foreach (string word in profanityList)
            {
                message = message.Replace(word, new string('*', word.Length));
            }
            
            return message;
        }

        public void ClearChannelHistory(ChatChannel channel)
        {
            if (chatHistory.ContainsKey(channel))
            {
                chatHistory[channel].Clear();
                Debug.Log($"💬 Cleared {channel} channel history");
            }
        }

        public void SetChatCooldown(float cooldown)
        {
            chatCooldown = Mathf.Max(0.5f, cooldown);
        }

        public void ToggleProfanityFilter(bool enabled)
        {
            profanityFilter = enabled;
            Debug.Log($"💬 Profanity filter {(enabled ? "enabled" : "disabled")}");
        }

        public ChatMessage GetLastMessage(ChatChannel channel)
        {
            var history = GetChannelHistory(channel);
            return history.Count > 0 ? history[history.Count - 1] : null;
        }

        public int GetUnreadMessageCount(ChatChannel channel)
        {
            return 0;
        }

        public List<string> GetBlockedPlayers()
        {
            return new List<string>(blockedPlayers);
        }

        public void HandleSystemMessage(string message, ChatChannel channel = ChatChannel.System)
        {
            var systemMessage = new ChatMessage
            {
                id = System.Guid.NewGuid().ToString(),
                senderId = "SYSTEM",
                senderName = "System",
                content = message,
                channel = channel,
                timestamp = System.DateTime.UtcNow,
                senderLevel = 0,
                senderClass = "System"
            };

            AddMessageToHistory(systemMessage);
            OnMessageReceived?.Invoke(systemMessage);
        }
    }

    [System.Serializable]
    public class ChatMessage
    {
        public string id;
        public string senderId;
        public string senderName;
        public string content;
        public ChatChannel channel;
        public System.DateTime timestamp;
        public int senderLevel;
        public string senderClass;
    }

    public enum ChatChannel
    {
        World,
        Region,
        Guild,
        Party,
        Private,
        Trade,
        System,
        Combat
    }
}
