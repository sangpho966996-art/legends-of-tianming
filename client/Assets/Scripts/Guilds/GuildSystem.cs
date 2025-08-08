using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class GuildSystem : MonoBehaviour
    {
        [Header("Guild Settings")]
        public int maxGuildMembers = 100;
        public int maxGuildLevel = 10;
        public int guildCreationCost = 10000;
        
        private Guild currentGuild;
        private Character character;
        private Dictionary<string, GuildInvitation> pendingInvitations = new Dictionary<string, GuildInvitation>();

        public event System.Action<Guild> OnGuildJoined;
        public event System.Action OnGuildLeft;
        public event System.Action<Guild> OnGuildUpdated;
        public event System.Action<string> OnGuildInvitationReceived;

        public Guild CurrentGuild => currentGuild;
        public bool IsInGuild => currentGuild != null;
        public bool IsGuildLeader => IsInGuild && currentGuild.leaderId == character.characterId;

        private void Start()
        {
            character = GetComponent<Character>();
        }

        public void CreateGuild(string guildName, string guildTag)
        {
            if (IsInGuild)
            {
                Debug.LogWarning("Already in a guild");
                return;
            }

            if (!character.Stats.SpendGold(guildCreationCost))
            {
                Debug.LogWarning($"Not enough gold to create guild (need {guildCreationCost})");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var createData = new
                {
                    guildName = guildName,
                    guildTag = guildTag,
                    leaderId = character.characterId,
                    leaderName = character.characterName
                };
                
                Debug.Log($"🏰 Creating guild: {guildName} [{guildTag}]");
            }
        }

        public void HandleGuildCreated(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("guild")) return;
            
            var guildData = data["guild"] as Dictionary<string, object>;
            currentGuild = ParseGuildData(guildData);
            
            OnGuildJoined?.Invoke(currentGuild);
            Debug.Log($"🏰 Guild created successfully: {currentGuild.name}");
        }

        public void InvitePlayer(string playerId, string playerName)
        {
            if (!IsInGuild)
            {
                Debug.LogWarning("Not in a guild");
                return;
            }

            if (!CanInviteMembers())
            {
                Debug.LogWarning("No permission to invite members");
                return;
            }

            if (currentGuild.members.Count >= maxGuildMembers)
            {
                Debug.LogWarning("Guild is full");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var inviteData = new
                {
                    guildId = currentGuild.id,
                    guildName = currentGuild.name,
                    targetPlayerId = playerId,
                    targetPlayerName = playerName,
                    inviterId = character.characterId,
                    inviterName = character.characterName
                };
                
                Debug.Log($"📨 Inviting {playerName} to guild {currentGuild.name}");
            }
        }

        public void HandleGuildInvitation(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("guildId") || !data.ContainsKey("guildName") || !data.ContainsKey("inviterName")) return;
            
            string guildId = data["guildId"].ToString();
            string guildName = data["guildName"].ToString();
            string inviterName = data["inviterName"].ToString();
            
            var invitation = new GuildInvitation
            {
                guildId = guildId,
                guildName = guildName,
                inviterName = inviterName,
                timestamp = Time.time
            };
            
            pendingInvitations[guildId] = invitation;
            OnGuildInvitationReceived?.Invoke(guildName);
            
            Debug.Log($"📨 Guild invitation received from {guildName} (invited by {inviterName})");
        }

        public void AcceptGuildInvitation(string guildId)
        {
            if (!pendingInvitations.ContainsKey(guildId))
            {
                Debug.LogWarning("No pending invitation from this guild");
                return;
            }

            if (IsInGuild)
            {
                Debug.LogWarning("Already in a guild");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var acceptData = new
                {
                    guildId = guildId,
                    playerId = character.characterId,
                    playerName = character.characterName
                };
                
                Debug.Log($"✅ Accepting guild invitation: {pendingInvitations[guildId].guildName}");
            }
            
            pendingInvitations.Remove(guildId);
        }

        public void DeclineGuildInvitation(string guildId)
        {
            if (pendingInvitations.ContainsKey(guildId))
            {
                Debug.Log($"❌ Declined guild invitation: {pendingInvitations[guildId].guildName}");
                pendingInvitations.Remove(guildId);
            }
        }

        public void HandleGuildJoined(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("guild")) return;
            
            var guildData = data["guild"] as Dictionary<string, object>;
            currentGuild = ParseGuildData(guildData);
            
            OnGuildJoined?.Invoke(currentGuild);
            Debug.Log($"🏰 Joined guild: {currentGuild.name}");
        }

        public void LeaveGuild()
        {
            if (!IsInGuild)
            {
                Debug.LogWarning("Not in a guild");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var leaveData = new
                {
                    guildId = currentGuild.id,
                    playerId = character.characterId
                };
                
                Debug.Log($"🚪 Leaving guild: {currentGuild.name}");
            }
        }

        public void HandleGuildLeft()
        {
            if (currentGuild != null)
            {
                Debug.Log($"🚪 Left guild: {currentGuild.name}");
                currentGuild = null;
                OnGuildLeft?.Invoke();
            }
        }

        public void KickMember(string memberId)
        {
            if (!IsGuildLeader && !CanKickMembers())
            {
                Debug.LogWarning("No permission to kick members");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var kickData = new
                {
                    guildId = currentGuild.id,
                    targetMemberId = memberId,
                    kickerId = character.characterId
                };
                
                Debug.Log($"👢 Kicking member from guild");
            }
        }

        public void PromoteMember(string memberId, GuildRank newRank)
        {
            if (!IsGuildLeader)
            {
                Debug.LogWarning("Only guild leader can promote members");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var promoteData = new
                {
                    guildId = currentGuild.id,
                    targetMemberId = memberId,
                    newRank = newRank.ToString(),
                    promoterId = character.characterId
                };
                
                Debug.Log($"⬆️ Promoting member to {newRank}");
            }
        }

        public void DonateToGuild(int goldAmount)
        {
            if (!IsInGuild)
            {
                Debug.LogWarning("Not in a guild");
                return;
            }

            if (!character.Stats.SpendGold(goldAmount))
            {
                Debug.LogWarning("Not enough gold to donate");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var donateData = new
                {
                    guildId = currentGuild.id,
                    donorId = character.characterId,
                    donorName = character.characterName,
                    amount = goldAmount
                };
                
                Debug.Log($"💰 Donated {goldAmount} gold to guild");
            }
        }

        public void UpgradeGuildBuilding(string buildingType)
        {
            if (!IsGuildLeader && !CanManageBuildings())
            {
                Debug.LogWarning("No permission to upgrade buildings");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var upgradeData = new
                {
                    guildId = currentGuild.id,
                    buildingType = buildingType,
                    upgraderId = character.characterId
                };
                
                Debug.Log($"🏗️ Upgrading guild building: {buildingType}");
            }
        }

        public void StartGuildWar(string targetGuildId)
        {
            if (!IsGuildLeader)
            {
                Debug.LogWarning("Only guild leader can declare war");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var warData = new
                {
                    attackerGuildId = currentGuild.id,
                    defenderGuildId = targetGuildId,
                    declarerId = character.characterId
                };
                
                Debug.Log($"⚔️ Declaring war on guild");
            }
        }

        private Guild ParseGuildData(Dictionary<string, object> data)
        {
            var guild = new Guild
            {
                id = data["id"].ToString(),
                name = data["name"].ToString(),
                tag = data["tag"].ToString(),
                leaderId = data["leaderId"].ToString(),
                level = int.Parse(data["level"].ToString()),
                experience = int.Parse(data["experience"].ToString()),
                treasury = int.Parse(data["treasury"].ToString()),
                members = new List<GuildMember>()
            };

            if (data.ContainsKey("members"))
            {
                var membersData = data["members"] as List<object>;
                foreach (var memberData in membersData)
                {
                    var memberDict = memberData as Dictionary<string, object>;
                    var member = new GuildMember
                    {
                        id = memberDict["id"].ToString(),
                        name = memberDict["name"].ToString(),
                        rank = (GuildRank)System.Enum.Parse(typeof(GuildRank), memberDict["rank"].ToString()),
                        joinDate = System.DateTime.Parse(memberDict["joinDate"].ToString()),
                        lastOnline = System.DateTime.Parse(memberDict["lastOnline"].ToString()),
                        contribution = int.Parse(memberDict["contribution"].ToString())
                    };
                    guild.members.Add(member);
                }
            }

            return guild;
        }

        private bool CanInviteMembers()
        {
            if (!IsInGuild) return false;
            
            var myMember = GetMyGuildMember();
            return myMember != null && (myMember.rank == GuildRank.Leader || myMember.rank == GuildRank.Officer);
        }

        private bool CanKickMembers()
        {
            if (!IsInGuild) return false;
            
            var myMember = GetMyGuildMember();
            return myMember != null && (myMember.rank == GuildRank.Leader || myMember.rank == GuildRank.Officer);
        }

        private bool CanManageBuildings()
        {
            if (!IsInGuild) return false;
            
            var myMember = GetMyGuildMember();
            return myMember != null && (myMember.rank == GuildRank.Leader || myMember.rank == GuildRank.Officer);
        }

        private GuildMember GetMyGuildMember()
        {
            if (!IsInGuild) return null;
            
            return currentGuild.members.Find(m => m.id == character.characterId);
        }

        public List<GuildInvitation> GetPendingInvitations()
        {
            return new List<GuildInvitation>(pendingInvitations.Values);
        }

        public void HandleGuildUpdated(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("guild")) return;
            
            var guildData = data["guild"] as Dictionary<string, object>;
            currentGuild = ParseGuildData(guildData);
            
            OnGuildUpdated?.Invoke(currentGuild);
        }
    }

    [System.Serializable]
    public class Guild
    {
        public string id;
        public string name;
        public string tag;
        public string leaderId;
        public int level;
        public int experience;
        public int treasury;
        public List<GuildMember> members;
        public Dictionary<string, int> buildings;
        public System.DateTime createdDate;
    }

    [System.Serializable]
    public class GuildMember
    {
        public string id;
        public string name;
        public GuildRank rank;
        public System.DateTime joinDate;
        public System.DateTime lastOnline;
        public int contribution;
    }

    [System.Serializable]
    public class GuildInvitation
    {
        public string guildId;
        public string guildName;
        public string inviterName;
        public float timestamp;
    }

    public enum GuildRank
    {
        Member,
        Elite,
        Officer,
        Leader
    }
}
