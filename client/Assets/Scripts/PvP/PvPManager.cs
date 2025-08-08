using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class PvPManager : MonoBehaviour
    {
        [Header("PvP Settings")]
        public bool pvpEnabled = true;
        public float duelRequestTimeout = 30f;
        public int battlegroundMaxPlayers = 20; // 10v10
        public int citySiegeMaxPlayers = 80; // 40v40
        
        private Dictionary<string, PvPDuel> activeDuels = new Dictionary<string, PvPDuel>();
        private Dictionary<string, float> duelRequests = new Dictionary<string, float>();
        private PvPMode currentMode = PvPMode.None;
        private string currentBattleground = "";
        private Character character;

        public event System.Action<string> OnDuelRequested;
        public event System.Action<PvPDuel> OnDuelStarted;
        public event System.Action<PvPDuel> OnDuelEnded;
        public event System.Action<string> OnBattlegroundJoined;
        public event System.Action OnCitySiegeStarted;

        public PvPMode CurrentMode => currentMode;
        public bool IsInPvP => currentMode != PvPMode.None;

        private void Start()
        {
            character = GetComponent<Character>();
        }

        private void Update()
        {
            UpdateDuelRequests();
            UpdateActiveDuels();
        }

        private void UpdateDuelRequests()
        {
            var expiredRequests = new List<string>();
            
            foreach (var request in duelRequests)
            {
                if (Time.time - request.Value > duelRequestTimeout)
                {
                    expiredRequests.Add(request.Key);
                }
            }
            
            foreach (var expired in expiredRequests)
            {
                duelRequests.Remove(expired);
                Debug.Log($"⏰ Duel request from {expired} expired");
            }
        }

        private void UpdateActiveDuels()
        {
            var completedDuels = new List<string>();
            
            foreach (var duel in activeDuels)
            {
                if (duel.Value.IsCompleted())
                {
                    completedDuels.Add(duel.Key);
                }
            }
            
            foreach (var completed in completedDuels)
            {
                EndDuel(completed);
            }
        }

        public void RequestDuel(string targetPlayerId)
        {
            if (!pvpEnabled)
            {
                Debug.LogWarning("PvP is disabled");
                return;
            }

            if (IsInPvP)
            {
                Debug.LogWarning("Already in PvP mode");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var requestData = new
                {
                    targetId = targetPlayerId,
                    challengerId = character.characterId,
                    challengerName = character.characterName
                };
                
                Debug.Log($"⚔️ Requesting duel with {targetPlayerId}");
            }
        }

        public void HandleDuelRequest(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("challengerId") || !data.ContainsKey("challengerName")) return;
            
            string challengerId = data["challengerId"].ToString();
            string challengerName = data["challengerName"].ToString();
            
            duelRequests[challengerId] = Time.time;
            OnDuelRequested?.Invoke(challengerName);
            
            Debug.Log($"⚔️ Duel request received from {challengerName}");
        }

        public void AcceptDuel(string challengerId)
        {
            if (!duelRequests.ContainsKey(challengerId))
            {
                Debug.LogWarning("No duel request from this player");
                return;
            }

            duelRequests.Remove(challengerId);
            
            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var acceptData = new
                {
                    challengerId = challengerId,
                    accepterId = character.characterId
                };
                
                Debug.Log($"✅ Accepted duel from {challengerId}");
            }
        }

        public void DeclineDuel(string challengerId)
        {
            if (duelRequests.ContainsKey(challengerId))
            {
                duelRequests.Remove(challengerId);
                Debug.Log($"❌ Declined duel from {challengerId}");
            }
        }

        public void StartDuel(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("duelId") || !data.ContainsKey("player1Id") || !data.ContainsKey("player2Id")) return;
            
            string duelId = data["duelId"].ToString();
            string player1Id = data["player1Id"].ToString();
            string player2Id = data["player2Id"].ToString();
            
            var duel = new PvPDuel
            {
                id = duelId,
                player1Id = player1Id,
                player2Id = player2Id,
                startTime = Time.time,
                status = DuelStatus.Active
            };
            
            activeDuels[duelId] = duel;
            currentMode = PvPMode.Duel;
            
            OnDuelStarted?.Invoke(duel);
            Debug.Log($"⚔️ Duel started: {player1Id} vs {player2Id}");
        }

        public void EndDuel(string duelId)
        {
            if (!activeDuels.ContainsKey(duelId)) return;
            
            var duel = activeDuels[duelId];
            duel.status = DuelStatus.Completed;
            duel.endTime = Time.time;
            
            activeDuels.Remove(duelId);
            currentMode = PvPMode.None;
            
            OnDuelEnded?.Invoke(duel);
            Debug.Log($"🏁 Duel ended: {duelId}");
        }

        public void JoinBattleground(string battlegroundType)
        {
            if (!pvpEnabled)
            {
                Debug.LogWarning("PvP is disabled");
                return;
            }

            if (IsInPvP)
            {
                Debug.LogWarning("Already in PvP mode");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var joinData = new
                {
                    playerId = character.characterId,
                    battlegroundType = battlegroundType
                };
                
                Debug.Log($"🏟️ Joining battleground: {battlegroundType}");
            }
        }

        public void HandleBattlegroundJoined(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("battlegroundId") || !data.ContainsKey("battlegroundType")) return;
            
            string battlegroundId = data["battlegroundId"].ToString();
            string battlegroundType = data["battlegroundType"].ToString();
            
            currentBattleground = battlegroundId;
            currentMode = PvPMode.Battleground;
            
            OnBattlegroundJoined?.Invoke(battlegroundType);
            Debug.log($"🏟️ Joined battleground: {battlegroundType}");
        }

        public void LeaveBattleground()
        {
            if (currentMode != PvPMode.Battleground) return;
            
            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var leaveData = new
                {
                    playerId = character.characterId,
                    battlegroundId = currentBattleground
                };
                
                Debug.Log($"🚪 Leaving battleground: {currentBattleground}");
            }
            
            currentBattleground = "";
            currentMode = PvPMode.None;
        }

        public void JoinCitySiege()
        {
            if (!pvpEnabled)
            {
                Debug.LogWarning("PvP is disabled");
                return;
            }

            if (IsInPvP)
            {
                Debug.LogWarning("Already in PvP mode");
                return;
            }

            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var joinData = new
                {
                    playerId = character.characterId,
                    guildId = character.guildId
                };
                
                Debug.Log($"🏰 Joining city siege");
            }
        }

        public void HandleCitySiegeStarted(Dictionary<string, object> data)
        {
            currentMode = PvPMode.CitySiege;
            OnCitySiegeStarted?.Invoke();
            Debug.Log($"🏰 City siege started!");
        }

        public void HandlePvPKill(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("killerId") || !data.ContainsKey("victimId")) return;
            
            string killerId = data["killerId"].ToString();
            string victimId = data["victimId"].ToString();
            
            if (killerId == character.characterId)
            {
                Debug.Log($"💀 You defeated {victimId}!");
                
                AwardPvPRewards();
            }
            else if (victimId == character.characterId)
            {
                Debug.Log($"💀 You were defeated by {killerId}!");
                
                HandlePvPDeath();
            }
        }

        private void AwardPvPRewards()
        {
            Debug.Log($"🏆 PvP rewards awarded!");
        }

        private void HandlePvPDeath()
        {
            Debug.Log($"💀 PvP death handled");
        }

        public void SetPvPEnabled(bool enabled)
        {
            pvpEnabled = enabled;
            Debug.Log($"⚔️ PvP {(enabled ? "enabled" : "disabled")}");
        }

        public bool CanEngageInPvP()
        {
            return pvpEnabled && !IsInPvP && character.IsAlive();
        }

        public List<PvPDuel> GetActiveDuels()
        {
            return new List<PvPDuel>(activeDuels.Values);
        }

        public PvPDuel GetCurrentDuel()
        {
            if (currentMode == PvPMode.Duel)
            {
                foreach (var duel in activeDuels.Values)
                {
                    if (duel.player1Id == character.characterId || duel.player2Id == character.characterId)
                        return duel;
                }
            }
            return null;
        }
    }

    [System.Serializable]
    public class PvPDuel
    {
        public string id;
        public string player1Id;
        public string player2Id;
        public float startTime;
        public float endTime;
        public DuelStatus status;
        public string winnerId;
        
        public bool IsCompleted()
        {
            return status == DuelStatus.Completed || status == DuelStatus.Cancelled;
        }
        
        public float GetDuration()
        {
            if (status == DuelStatus.Active)
                return Time.time - startTime;
            else
                return endTime - startTime;
        }
    }

    public enum PvPMode
    {
        None,
        Duel,
        Battleground,
        CitySiege
    }

    public enum DuelStatus
    {
        Pending,
        Active,
        Completed,
        Cancelled
    }
}
