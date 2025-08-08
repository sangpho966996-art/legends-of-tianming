using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class FriendSystem : MonoBehaviour
    {
        [Header("Friend System Settings")]
        public int maxFriends = 100;
        public float onlineStatusUpdateInterval = 30f;
        
        private List<Friend> friendsList = new List<Friend>();
        private List<FriendRequest> pendingRequests = new List<FriendRequest>();
        private List<FriendRequest> sentRequests = new List<FriendRequest>();
        private Character character;
        private float lastStatusUpdate;

        public event System.Action<Friend> OnFriendAdded;
        public event System.Action<string> OnFriendRemoved;
        public event System.Action<FriendRequest> OnFriendRequestReceived;
        public event System.Action<string> OnFriendRequestAccepted;
        public event System.Action<string> OnFriendRequestDeclined;
        public event System.Action<Friend> OnFriendStatusChanged;
        public event System.Action<List<Friend>> OnFriendsListUpdated;

        public List<Friend> FriendsList => new List<Friend>(friendsList);
        public List<FriendRequest> PendingRequests => new List<FriendRequest>(pendingRequests);
        public List<FriendRequest> SentRequests => new List<FriendRequest>(sentRequests);
        public int FriendCount => friendsList.Count;
        public int OnlineFriendCount => friendsList.FindAll(f => f.isOnline).Count;

        private void Start()
        {
            character = GetComponent<Character>();
            LoadFriendsData();
            InvokeRepeating(nameof(UpdateOnlineStatus), onlineStatusUpdateInterval, onlineStatusUpdateInterval);
        }

        private void Update()
        {
            if (Time.time - lastStatusUpdate >= onlineStatusUpdateInterval)
            {
                UpdateOnlineStatus();
                lastStatusUpdate = Time.time;
            }
        }

        private void LoadFriendsData()
        {
            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var requestData = new
                {
                    playerId = character.characterId
                };
                
                Debug.Log($"👥 Loading friends data for {character.characterName}");
            }
        }

        public void SendFriendRequest(string targetPlayerId, string targetPlayerName)
        {
            if (string.IsNullOrEmpty(targetPlayerId) || targetPlayerId == character.characterId)
            {
                Debug.LogWarning("Invalid friend request target");
                return;
            }

            if (IsFriend(targetPlayerId))
            {
                Debug.LogWarning("Player is already a friend");
                return;
            }

            if (HasSentRequest(targetPlayerId))
            {
                Debug.LogWarning("Friend request already sent to this player");
                return;
            }

            if (friendsList.Count >= maxFriends)
            {
                Debug.LogWarning("Friends list is full");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var requestData = new
                {
                    senderId = character.characterId,
                    senderName = character.characterName,
                    targetId = targetPlayerId,
                    targetName = targetPlayerName
                };
                
                Debug.Log($"👥 Sending friend request to {targetPlayerName}");
            }
        }

        public void HandleFriendRequestReceived(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("senderId") || !data.ContainsKey("senderName")) return;
            
            string senderId = data["senderId"].ToString();
            string senderName = data["senderName"].ToString();
            
            if (IsFriend(senderId) || HasPendingRequest(senderId))
            {
                Debug.LogWarning("Duplicate friend request received");
                return;
            }
            
            var friendRequest = new FriendRequest
            {
                id = System.Guid.NewGuid().ToString(),
                senderId = senderId,
                senderName = senderName,
                receiverId = character.characterId,
                receiverName = character.characterName,
                timestamp = System.DateTime.UtcNow,
                status = FriendRequestStatus.Pending
            };
            
            pendingRequests.Add(friendRequest);
            OnFriendRequestReceived?.Invoke(friendRequest);
            
            Debug.Log($"👥 Friend request received from {senderName}");
        }

        public void AcceptFriendRequest(string requestId)
        {
            var request = pendingRequests.Find(r => r.id == requestId);
            if (request == null)
            {
                Debug.LogWarning("Friend request not found");
                return;
            }

            if (friendsList.Count >= maxFriends)
            {
                Debug.LogWarning("Cannot accept: friends list is full");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var acceptData = new
                {
                    requestId = requestId,
                    accepterId = character.characterId,
                    senderId = request.senderId
                };
                
                Debug.Log($"✅ Accepting friend request from {request.senderName}");
            }
        }

        public void DeclineFriendRequest(string requestId)
        {
            var request = pendingRequests.Find(r => r.id == requestId);
            if (request == null)
            {
                Debug.LogWarning("Friend request not found");
                return;
            }

            pendingRequests.Remove(request);
            
            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var declineData = new
                {
                    requestId = requestId,
                    declinerId = character.characterId,
                    senderId = request.senderId
                };
                
                Debug.Log($"❌ Declining friend request from {request.senderName}");
            }
            
            OnFriendRequestDeclined?.Invoke(request.senderName);
        }

        public void HandleFriendRequestAccepted(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("friendData")) return;
            
            var friendData = data["friendData"] as Dictionary<string, object>;
            var friend = ParseFriendData(friendData);
            
            if (friend != null)
            {
                friendsList.Add(friend);
                OnFriendAdded?.Invoke(friend);
                OnFriendsListUpdated?.Invoke(friendsList);
                
                sentRequests.RemoveAll(r => r.receiverId == friend.playerId);
                
                Debug.Log($"✅ {friend.playerName} accepted your friend request");
            }
        }

        public void HandleFriendAdded(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("friendData")) return;
            
            var friendData = data["friendData"] as Dictionary<string, object>;
            var friend = ParseFriendData(friendData);
            
            if (friend != null && !IsFriend(friend.playerId))
            {
                friendsList.Add(friend);
                
                pendingRequests.RemoveAll(r => r.senderId == friend.playerId);
                
                OnFriendAdded?.Invoke(friend);
                OnFriendsListUpdated?.Invoke(friendsList);
                
                Debug.Log($"👥 {friend.playerName} is now your friend");
            }
        }

        public void RemoveFriend(string friendId)
        {
            var friend = friendsList.Find(f => f.playerId == friendId);
            if (friend == null)
            {
                Debug.LogWarning("Friend not found");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var removeData = new
                {
                    playerId = character.characterId,
                    friendId = friendId
                };
                
                Debug.Log($"❌ Removing {friend.playerName} from friends list");
            }
        }

        public void HandleFriendRemoved(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("friendId")) return;
            
            string friendId = data["friendId"].ToString();
            var friend = friendsList.Find(f => f.playerId == friendId);
            
            if (friend != null)
            {
                friendsList.Remove(friend);
                OnFriendRemoved?.Invoke(friend.playerName);
                OnFriendsListUpdated?.Invoke(friendsList);
                
                Debug.Log($"❌ {friend.playerName} removed from friends list");
            }
        }

        public void UpdateOnlineStatus()
        {
            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var statusData = new
                {
                    playerId = character.characterId,
                    isOnline = true,
                    lastSeen = System.DateTime.UtcNow
                };
                
                var requestData = new
                {
                    playerId = character.characterId,
                    friendIds = friendsList.ConvertAll(f => f.playerId)
                };
            }
        }

        public void HandleFriendStatusUpdate(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("friendStatuses")) return;
            
            var statusesData = data["friendStatuses"] as List<object>;
            
            foreach (var statusData in statusesData)
            {
                var statusDict = statusData as Dictionary<string, object>;
                string friendId = statusDict["playerId"].ToString();
                bool isOnline = bool.Parse(statusDict["isOnline"].ToString());
                
                var friend = friendsList.Find(f => f.playerId == friendId);
                if (friend != null && friend.isOnline != isOnline)
                {
                    friend.isOnline = isOnline;
                    friend.lastSeen = System.DateTime.Parse(statusDict["lastSeen"].ToString());
                    
                    OnFriendStatusChanged?.Invoke(friend);
                    
                    Debug.Log($"👥 {friend.playerName} is now {(isOnline ? "online" : "offline")}");
                }
            }
        }

        public void SendPrivateMessage(string friendId, string message)
        {
            var friend = friendsList.Find(f => f.playerId == friendId);
            if (friend == null)
            {
                Debug.LogWarning("Cannot send message: not a friend");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var messageData = new
                {
                    senderId = character.characterId,
                    senderName = character.characterName,
                    receiverId = friendId,
                    message = message,
                    timestamp = System.DateTime.UtcNow
                };
                
                Debug.Log($"💬 Sending message to {friend.playerName}: {message}");
            }
        }

        public void InviteToParty(string friendId)
        {
            var friend = friendsList.Find(f => f.playerId == friendId);
            if (friend == null || !friend.isOnline)
            {
                Debug.LogWarning("Cannot invite: friend not found or offline");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var inviteData = new
                {
                    inviterId = character.characterId,
                    inviterName = character.characterName,
                    friendId = friendId,
                    inviteType = "party"
                };
                
                Debug.Log($"🎉 Inviting {friend.playerName} to party");
            }
        }

        public void InviteToGuild(string friendId)
        {
            var friend = friendsList.Find(f => f.playerId == friendId);
            if (friend == null || !friend.isOnline)
            {
                Debug.LogWarning("Cannot invite: friend not found or offline");
                return;
            }

            var guildSystem = GetComponent<GuildSystem>();
            if (guildSystem == null || !guildSystem.IsInGuild)
            {
                Debug.LogWarning("Cannot invite: not in a guild");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var inviteData = new
                {
                    inviterId = character.characterId,
                    inviterName = character.characterName,
                    friendId = friendId,
                    guildId = guildSystem.CurrentGuild.id,
                    inviteType = "guild"
                };
                
                Debug.Log($"🏰 Inviting {friend.playerName} to guild");
            }
        }

        private Friend ParseFriendData(Dictionary<string, object> data)
        {
            try
            {
                return new Friend
                {
                    playerId = data["playerId"].ToString(),
                    playerName = data["playerName"].ToString(),
                    level = int.Parse(data["level"].ToString()),
                    characterClass = data["characterClass"].ToString(),
                    isOnline = bool.Parse(data["isOnline"].ToString()),
                    lastSeen = System.DateTime.Parse(data["lastSeen"].ToString()),
                    currentRegion = data.ContainsKey("currentRegion") ? data["currentRegion"].ToString() : "",
                    friendshipDate = System.DateTime.Parse(data["friendshipDate"].ToString())
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing friend data: {e.Message}");
                return null;
            }
        }

        public bool IsFriend(string playerId)
        {
            return friendsList.Exists(f => f.playerId == playerId);
        }

        public bool HasPendingRequest(string playerId)
        {
            return pendingRequests.Exists(r => r.senderId == playerId);
        }

        public bool HasSentRequest(string playerId)
        {
            return sentRequests.Exists(r => r.receiverId == playerId);
        }

        public Friend GetFriend(string playerId)
        {
            return friendsList.Find(f => f.playerId == playerId);
        }

        public List<Friend> GetOnlineFriends()
        {
            return friendsList.FindAll(f => f.isOnline);
        }

        public List<Friend> GetFriendsByClass(string characterClass)
        {
            return friendsList.FindAll(f => f.characterClass == characterClass);
        }

        public List<Friend> GetFriendsInRegion(string region)
        {
            return friendsList.FindAll(f => f.isOnline && f.currentRegion == region);
        }

        private void OnDestroy()
        {
            CancelInvokeRepeating();
        }
    }

    [System.Serializable]
    public class Friend
    {
        public string playerId;
        public string playerName;
        public int level;
        public string characterClass;
        public bool isOnline;
        public System.DateTime lastSeen;
        public string currentRegion;
        public System.DateTime friendshipDate;
    }

    [System.Serializable]
    public class FriendRequest
    {
        public string id;
        public string senderId;
        public string senderName;
        public string receiverId;
        public string receiverName;
        public System.DateTime timestamp;
        public FriendRequestStatus status;
    }

    public enum FriendRequestStatus
    {
        Pending,
        Accepted,
        Declined,
        Expired
    }
}
