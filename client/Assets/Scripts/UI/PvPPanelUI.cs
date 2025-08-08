using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class PvPPanelUI : MonoBehaviour
    {
        [Header("PvP Modes")]
        public Button duelButton;
        public Button battlegroundButton;
        public Button siegeButton;
        
        [Header("Rankings")]
        public Transform rankingContent;
        public GameObject rankingEntryPrefab;
        public Text playerRankText;
        public Text playerRatingText;
        
        [Header("Duel Panel")]
        public GameObject duelPanel;
        public InputField targetPlayerInput;
        public Button challengeButton;
        public Button acceptDuelButton;
        public Button declineDuelButton;
        public Text duelStatusText;
        
        [Header("Match Info")]
        public GameObject matchInfoPanel;
        public Text matchTypeText;
        public Text matchStatusText;
        public Text matchTimeText;
        public Button leaveMatchButton;
        
        private Character playerCharacter;
        private List<GameObject> rankingEntries = new List<GameObject>();

        private void Start()
        {
            InitializePvPPanel();
            FindPlayerCharacter();
        }

        private void InitializePvPPanel()
        {
            if (duelButton != null)
                duelButton.onClick.AddListener(OnDuelModeSelected);
                
            if (battlegroundButton != null)
                battlegroundButton.onClick.AddListener(OnBattlegroundModeSelected);
                
            if (siegeButton != null)
                siegeButton.onClick.AddListener(OnSiegeModeSelected);
                
            if (challengeButton != null)
                challengeButton.onClick.AddListener(OnChallengeDuel);
                
            if (acceptDuelButton != null)
                acceptDuelButton.onClick.AddListener(OnAcceptDuel);
                
            if (declineDuelButton != null)
                declineDuelButton.onClick.AddListener(OnDeclineDuel);
                
            if (leaveMatchButton != null)
                leaveMatchButton.onClick.AddListener(OnLeaveMatch);
                
            if (duelPanel != null)
                duelPanel.SetActive(false);
                
            if (matchInfoPanel != null)
                matchInfoPanel.SetActive(false);
                
            LoadRankings();
        }

        private void FindPlayerCharacter()
        {
            var playerManager = GameManager.Instance?.GetPlayerManager();
            if (playerManager != null && playerManager.LocalPlayer != null)
            {
                playerCharacter = playerManager.LocalPlayer.GetComponent<Character>();
                UpdatePlayerRanking();
            }
        }

        private void OnDuelModeSelected()
        {
            if (duelPanel != null)
                duelPanel.SetActive(true);
                
            if (duelStatusText != null)
                duelStatusText.text = "Select a player to challenge";
        }

        private void OnBattlegroundModeSelected()
        {
            Debug.Log("⚔️ Joining battleground queue...");
            
            var networkManager = GameManager.Instance?.GetNetworkManager();
            if (networkManager != null)
            {
                networkManager.SendPvPQueueRequest("battleground");
            }
            
            ShowMatchInfo("Battleground", "Searching for match...");
        }

        private void OnSiegeModeSelected()
        {
            Debug.Log("🏰 Joining siege queue...");
            
            var networkManager = GameManager.Instance?.GetNetworkManager();
            if (networkManager != null)
            {
                networkManager.SendPvPQueueRequest("siege");
            }
            
            ShowMatchInfo("City Siege", "Searching for match...");
        }

        private void OnChallengeDuel()
        {
            if (targetPlayerInput == null || string.IsNullOrEmpty(targetPlayerInput.text.Trim()))
            {
                if (duelStatusText != null)
                    duelStatusText.text = "Please enter a player name";
                return;
            }
            
            string targetPlayer = targetPlayerInput.text.Trim();
            
            var networkManager = GameManager.Instance?.GetNetworkManager();
            if (networkManager != null)
            {
                networkManager.SendDuelChallenge(targetPlayer);
            }
            
            if (duelStatusText != null)
                duelStatusText.text = $"Duel challenge sent to {targetPlayer}";
                
            Debug.Log($"⚔️ Duel challenge sent to {targetPlayer}");
        }

        private void OnAcceptDuel()
        {
            var networkManager = GameManager.Instance?.GetNetworkManager();
            if (networkManager != null)
            {
                networkManager.SendDuelResponse(true);
            }
            
            if (duelPanel != null)
                duelPanel.SetActive(false);
                
            ShowMatchInfo("Duel", "Preparing for duel...");
        }

        private void OnDeclineDuel()
        {
            var networkManager = GameManager.Instance?.GetNetworkManager();
            if (networkManager != null)
            {
                networkManager.SendDuelResponse(false);
            }
            
            if (duelStatusText != null)
                duelStatusText.text = "Duel declined";
        }

        private void OnLeaveMatch()
        {
            var networkManager = GameManager.Instance?.GetNetworkManager();
            if (networkManager != null)
            {
                networkManager.SendLeaveMatch();
            }
            
            if (matchInfoPanel != null)
                matchInfoPanel.SetActive(false);
                
            Debug.Log("🚪 Left PvP match");
        }

        private void ShowMatchInfo(string matchType, string status)
        {
            if (matchInfoPanel != null)
                matchInfoPanel.SetActive(true);
                
            if (matchTypeText != null)
                matchTypeText.text = matchType;
                
            if (matchStatusText != null)
                matchStatusText.text = status;
        }

        private void LoadRankings()
        {
            ClearRankings();
            
            var sampleRankings = new List<RankingEntry>
            {
                new RankingEntry { rank = 1, playerName = "DragonSlayer", rating = 2450, wins = 127, losses = 23 },
                new RankingEntry { rank = 2, playerName = "SwordMaster", rating = 2380, wins = 98, losses = 31 },
                new RankingEntry { rank = 3, playerName = "IronFist", rating = 2290, wins = 156, losses = 67 },
                new RankingEntry { rank = 4, playerName = "WindWalker", rating = 2210, wins = 89, losses = 45 },
                new RankingEntry { rank = 5, playerName = "StormBringer", rating = 2150, wins = 134, losses = 78 }
            };
            
            foreach (var entry in sampleRankings)
            {
                CreateRankingEntry(entry);
            }
        }

        private void CreateRankingEntry(RankingEntry entry)
        {
            if (rankingContent == null || rankingEntryPrefab == null) return;
            
            GameObject entryObj = Instantiate(rankingEntryPrefab, rankingContent);
            
            Text[] texts = entryObj.GetComponentsInChildren<Text>();
            if (texts.Length >= 4)
            {
                texts[0].text = entry.rank.ToString();
                texts[1].text = entry.playerName;
                texts[2].text = entry.rating.ToString();
                texts[3].text = $"{entry.wins}W / {entry.losses}L";
            }
            
            rankingEntries.Add(entryObj);
        }

        private void ClearRankings()
        {
            foreach (var entry in rankingEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            rankingEntries.Clear();
        }

        private void UpdatePlayerRanking()
        {
            if (playerCharacter == null) return;
            
            if (playerRankText != null)
                playerRankText.text = "Rank: Unranked";
                
            if (playerRatingText != null)
                playerRatingText.text = "Rating: 1200";
        }

        public void OnDuelChallengeReceived(string fromPlayer)
        {
            if (duelPanel != null)
                duelPanel.SetActive(true);
                
            if (duelStatusText != null)
                duelStatusText.text = $"Duel challenge from {fromPlayer}";
                
            if (acceptDuelButton != null)
                acceptDuelButton.gameObject.SetActive(true);
                
            if (declineDuelButton != null)
                declineDuelButton.gameObject.SetActive(true);
        }

        public void OnMatchFound(string matchType)
        {
            ShowMatchInfo(matchType, "Match found! Preparing...");
        }

        public void OnMatchStarted(string matchType)
        {
            ShowMatchInfo(matchType, "Match in progress");
        }

        public void OnMatchEnded(string result)
        {
            if (matchInfoPanel != null)
                matchInfoPanel.SetActive(false);
                
            Debug.Log($"🏆 Match ended: {result}");
        }
    }

    [System.Serializable]
    public class RankingEntry
    {
        public int rank;
        public string playerName;
        public int rating;
        public int wins;
        public int losses;
    }
}
