using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class BattlegroundManager : MonoBehaviour
    {
        [Header("Battleground Settings")]
        public string battlegroundId = "azure_valley";
        public string battlegroundName = "Azure Valley Battleground";
        public int maxPlayersPerTeam = 10;
        public float matchDuration = 900f; // 15 minutes
        public int scoreToWin = 1000;
        
        [Header("Spawn Points")]
        public Transform[] team1SpawnPoints;
        public Transform[] team2SpawnPoints;
        public Transform[] capturePoints;
        
        private BattlegroundState currentState = BattlegroundState.Waiting;
        private float matchStartTime;
        private int team1Score = 0;
        private int team2Score = 0;
        private List<GameObject> team1Players = new List<GameObject>();
        private List<GameObject> team2Players = new List<GameObject>();
        private Dictionary<int, CapturePoint> capturePointData = new Dictionary<int, CapturePoint>();
        private float lastScoreUpdate = 0f;

        public event System.Action<BattlegroundState> OnStateChanged;
        public event System.Action<int, int> OnScoreUpdated;
        public event System.Action<int> OnTeamWon;
        public event System.Action<GameObject, int> OnPlayerJoined;
        public event System.Action<GameObject> OnPlayerLeft;

        public BattlegroundState State => currentState;
        public float TimeRemaining => matchDuration - (Time.time - matchStartTime);
        public int Team1Score => team1Score;
        public int Team2Score => team2Score;
        public int Team1PlayerCount => team1Players.Count;
        public int Team2PlayerCount => team2Players.Count;

        private void Start()
        {
            InitializeCapturePoints();
            SetState(BattlegroundState.Waiting);
        }

        private void Update()
        {
            switch (currentState)
            {
                case BattlegroundState.Active:
                    UpdateActiveBattleground();
                    break;
                case BattlegroundState.Ending:
                    UpdateEndingBattleground();
                    break;
            }
        }

        private void InitializeCapturePoints()
        {
            for (int i = 0; i < capturePoints.Length; i++)
            {
                var capturePoint = new CapturePoint
                {
                    id = i,
                    position = capturePoints[i].position,
                    controllingTeam = 0,
                    captureProgress = 0f,
                    playersInRange = new List<GameObject>()
                };
                capturePointData[i] = capturePoint;
                
                var trigger = capturePoints[i].gameObject.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 8f;
                
                var captureZone = capturePoints[i].gameObject.AddComponent<CaptureZone>();
                captureZone.Initialize(i, this);
            }
        }

        private void UpdateActiveBattleground()
        {
            if (TimeRemaining <= 0f)
            {
                EndMatch(team1Score > team2Score ? 1 : team2Score > team1Score ? 2 : 0);
                return;
            }
            
            if (team1Score >= scoreToWin)
            {
                EndMatch(1);
                return;
            }
            else if (team2Score >= scoreToWin)
            {
                EndMatch(2);
                return;
            }
            
            UpdateCapturePoints();
            
            if (Time.time - lastScoreUpdate >= 1f)
            {
                AwardControlPoints();
                lastScoreUpdate = Time.time;
            }
        }

        private void UpdateEndingBattleground()
        {
        }

        private void UpdateCapturePoints()
        {
            foreach (var kvp in capturePointData)
            {
                var capturePoint = kvp.Value;
                UpdateCapturePoint(capturePoint);
            }
        }

        private void UpdateCapturePoint(CapturePoint capturePoint)
        {
            int team1Count = 0;
            int team2Count = 0;
            
            capturePoint.playersInRange.RemoveAll(p => p == null);
            
            foreach (var player in capturePoint.playersInRange)
            {
                int playerTeam = GetPlayerTeam(player);
                if (playerTeam == 1) team1Count++;
                else if (playerTeam == 2) team2Count++;
            }
            
            if (team1Count > team2Count && team1Count > 0)
            {
                if (capturePoint.controllingTeam == 1)
                {
                }
                else
                {
                    capturePoint.captureProgress += Time.deltaTime * team1Count;
                    if (capturePoint.captureProgress >= 10f) // 10 seconds to capture
                    {
                        CapturePointForTeam(capturePoint, 1);
                    }
                }
            }
            else if (team2Count > team1Count && team2Count > 0)
            {
                if (capturePoint.controllingTeam == 2)
                {
                }
                else
                {
                    capturePoint.captureProgress += Time.deltaTime * team2Count;
                    if (capturePoint.captureProgress >= 10f)
                    {
                        CapturePointForTeam(capturePoint, 2);
                    }
                }
            }
            else
            {
                capturePoint.captureProgress = Mathf.Max(0f, capturePoint.captureProgress - Time.deltaTime * 2f);
            }
        }

        private void CapturePointForTeam(CapturePoint capturePoint, int team)
        {
            int previousTeam = capturePoint.controllingTeam;
            capturePoint.controllingTeam = team;
            capturePoint.captureProgress = 0f;
            
            if (team == 1)
                team1Score += 50;
            else if (team == 2)
                team2Score += 50;
            
            OnScoreUpdated?.Invoke(team1Score, team2Score);
            
            Debug.Log($"🏁 Capture Point {capturePoint.id} captured by Team {team}");
            
            AnnounceCapturePointTaken(capturePoint.id, team, previousTeam);
        }

        private void AwardControlPoints()
        {
            int team1ControlledPoints = 0;
            int team2ControlledPoints = 0;
            
            foreach (var capturePoint in capturePointData.Values)
            {
                if (capturePoint.controllingTeam == 1)
                    team1ControlledPoints++;
                else if (capturePoint.controllingTeam == 2)
                    team2ControlledPoints++;
            }
            
            team1Score += team1ControlledPoints * 2;
            team2Score += team2ControlledPoints * 2;
            
            OnScoreUpdated?.Invoke(team1Score, team2Score);
        }

        public void JoinBattleground(GameObject player)
        {
            if (currentState != BattlegroundState.Waiting && currentState != BattlegroundState.Active)
            {
                Debug.LogWarning("Cannot join battleground in current state");
                return;
            }
            
            int team = team1Players.Count <= team2Players.Count ? 1 : 2;
            
            if (team == 1 && team1Players.Count < maxPlayersPerTeam)
            {
                team1Players.Add(player);
                SpawnPlayer(player, team);
            }
            else if (team == 2 && team2Players.Count < maxPlayersPerTeam)
            {
                team2Players.Add(player);
                SpawnPlayer(player, team);
            }
            else
            {
                Debug.LogWarning("Battleground is full");
                return;
            }
            
            OnPlayerJoined?.Invoke(player, team);
            
            if (currentState == BattlegroundState.Waiting && 
                team1Players.Count >= 5 && team2Players.Count >= 5)
            {
                StartMatch();
            }
            
            Debug.Log($"🏟️ {player.name} joined Team {team} ({GetTeamPlayerCount(team)}/{maxPlayersPerTeam})");
        }

        public void LeaveBattleground(GameObject player)
        {
            bool wasInTeam1 = team1Players.Remove(player);
            bool wasInTeam2 = team2Players.Remove(player);
            
            if (wasInTeam1 || wasInTeam2)
            {
                OnPlayerLeft?.Invoke(player);
                
                if (currentState == BattlegroundState.Active)
                {
                    if (team1Players.Count < 3 || team2Players.Count < 3)
                    {
                        EndMatch(team1Players.Count > team2Players.Count ? 1 : 2);
                    }
                }
                
                Debug.Log($"🚪 {player.name} left the battleground");
            }
        }

        private void SpawnPlayer(GameObject player, int team)
        {
            Transform[] spawnPoints = team == 1 ? team1SpawnPoints : team2SpawnPoints;
            if (spawnPoints.Length > 0)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                player.transform.position = spawnPoint.position;
                player.transform.rotation = spawnPoint.rotation;
                
                var character = player.GetComponent<Character>();
                if (character != null)
                {
                    character.Stats.RestoreHealth();
                    character.Stats.RestoreMana();
                }
            }
        }

        private void StartMatch()
        {
            SetState(BattlegroundState.Active);
            matchStartTime = Time.time;
            team1Score = 0;
            team2Score = 0;
            lastScoreUpdate = Time.time;
            
            foreach (var capturePoint in capturePointData.Values)
            {
                capturePoint.controllingTeam = 0;
                capturePoint.captureProgress = 0f;
                capturePoint.playersInRange.Clear();
            }
            
            OnScoreUpdated?.Invoke(team1Score, team2Score);
            
            Debug.Log($"🏟️ Battleground match started! {team1Players.Count} vs {team2Players.Count}");
            AnnounceBattlegroundStart();
        }

        private void EndMatch(int winningTeam)
        {
            SetState(BattlegroundState.Ending);
            
            OnTeamWon?.Invoke(winningTeam);
            
            DistributeRewards(winningTeam);
            
            Debug.Log($"🏆 Battleground ended! Team {winningTeam} wins ({team1Score} - {team2Score})");
            AnnounceBattlegroundEnd(winningTeam);
            
            Invoke(nameof(ResetBattleground), 30f);
        }

        private void DistributeRewards(int winningTeam)
        {
            var winningPlayers = winningTeam == 1 ? team1Players : team2Players;
            var losingPlayers = winningTeam == 1 ? team2Players : team1Players;
            
            foreach (var player in winningPlayers)
            {
                if (player != null)
                {
                    var character = player.GetComponent<Character>();
                    if (character != null)
                    {
                        character.Stats.AddExperience(200);
                        character.Stats.AddGold(100);
                        Debug.Log($"🏆 {player.name} earned winner rewards: 200 XP, 100 gold");
                    }
                }
            }
            
            foreach (var player in losingPlayers)
            {
                if (player != null)
                {
                    var character = player.GetComponent<Character>();
                    if (character != null)
                    {
                        character.Stats.AddExperience(100);
                        character.Stats.AddGold(50);
                        Debug.Log($"🎖️ {player.name} earned participation rewards: 100 XP, 50 gold");
                    }
                }
            }
        }

        private void ResetBattleground()
        {
            foreach (var player in team1Players.ToArray())
            {
                LeaveBattleground(player);
            }
            foreach (var player in team2Players.ToArray())
            {
                LeaveBattleground(player);
            }
            
            SetState(BattlegroundState.Waiting);
            Debug.Log($"🔄 Battleground reset and ready for new match");
        }

        private void SetState(BattlegroundState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        private int GetPlayerTeam(GameObject player)
        {
            if (team1Players.Contains(player)) return 1;
            if (team2Players.Contains(player)) return 2;
            return 0;
        }

        private int GetTeamPlayerCount(int team)
        {
            return team == 1 ? team1Players.Count : team2Players.Count;
        }

        public void OnPlayerEnteredCaptureZone(int capturePointId, GameObject player)
        {
            if (capturePointData.ContainsKey(capturePointId))
            {
                var capturePoint = capturePointData[capturePointId];
                if (!capturePoint.playersInRange.Contains(player))
                {
                    capturePoint.playersInRange.Add(player);
                }
            }
        }

        public void OnPlayerExitedCaptureZone(int capturePointId, GameObject player)
        {
            if (capturePointData.ContainsKey(capturePointId))
            {
                var capturePoint = capturePointData[capturePointId];
                capturePoint.playersInRange.Remove(player);
            }
        }

        private void AnnounceBattlegroundStart()
        {
            Debug.Log($"📢 BATTLEGROUND: {battlegroundName} match has begun!");
        }

        private void AnnounceBattlegroundEnd(int winningTeam)
        {
            Debug.Log($"📢 BATTLEGROUND: Team {winningTeam} has won {battlegroundName}!");
        }

        private void AnnounceCapturePointTaken(int pointId, int newTeam, int previousTeam)
        {
            Debug.Log($"📢 BATTLEGROUND: Team {newTeam} has captured Point {pointId}!");
        }

        private void OnDrawGizmosSelected()
        {
            if (team1SpawnPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (var spawn in team1SpawnPoints)
                {
                    if (spawn != null)
                        Gizmos.DrawWireSphere(spawn.position, 2f);
                }
            }
            
            if (team2SpawnPoints != null)
            {
                Gizmos.color = Color.red;
                foreach (var spawn in team2SpawnPoints)
                {
                    if (spawn != null)
                        Gizmos.DrawWireSphere(spawn.position, 2f);
                }
            }
            
            if (capturePoints != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var point in capturePoints)
                {
                    if (point != null)
                        Gizmos.DrawWireSphere(point.position, 8f);
                }
            }
        }
    }

    [System.Serializable]
    public class CapturePoint
    {
        public int id;
        public Vector3 position;
        public int controllingTeam; // 0 = neutral, 1 = team1, 2 = team2
        public float captureProgress;
        public List<GameObject> playersInRange;
    }

    public class CaptureZone : MonoBehaviour
    {
        private int capturePointId;
        private BattlegroundManager battlegroundManager;

        public void Initialize(int id, BattlegroundManager manager)
        {
            capturePointId = id;
            battlegroundManager = manager;
        }

        private void OnTriggerEnter(Collider other)
        {
            var character = other.GetComponent<Character>();
            if (character != null)
            {
                battlegroundManager.OnPlayerEnteredCaptureZone(capturePointId, other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var character = other.GetComponent<Character>();
            if (character != null)
            {
                battlegroundManager.OnPlayerExitedCaptureZone(capturePointId, other.gameObject);
            }
        }
    }

    public enum BattlegroundState
    {
        Waiting,
        Starting,
        Active,
        Ending,
        Closed
    }
}
