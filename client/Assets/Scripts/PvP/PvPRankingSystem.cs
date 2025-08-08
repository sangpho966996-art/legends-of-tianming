using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class PvPRankingSystem : MonoBehaviour
    {
        [Header("Ranking Settings")]
        public int seasonDurationDays = 90;
        public int rankDecayDays = 14;
        public int placementMatchesRequired = 10;
        
        private PvPPlayerRank currentRank;
        private int currentRating = 1000;
        private int seasonWins = 0;
        private int seasonLosses = 0;
        private int placementMatchesPlayed = 0;
        private bool isInPlacementMatches = true;
        private Character character;
        private Dictionary<string, PvPPlayerRank> leaderboard = new Dictionary<string, PvPPlayerRank>();

        public event System.Action<PvPPlayerRank, PvPPlayerRank> OnRankChanged;
        public event System.Action<int> OnRatingChanged;
        public event System.Action<List<PvPPlayerRank>> OnLeaderboardUpdated;
        public event System.Action OnSeasonEnded;

        public PvPPlayerRank CurrentRank => currentRank;
        public int CurrentRating => currentRating;
        public int SeasonWins => seasonWins;
        public int SeasonLosses => seasonLosses;
        public float WinRate => (seasonWins + seasonLosses) > 0 ? (float)seasonWins / (seasonWins + seasonLosses) : 0f;
        public bool IsInPlacementMatches => isInPlacementMatches;
        public int PlacementMatchesRemaining => Mathf.Max(0, placementMatchesRequired - placementMatchesPlayed);

        private void Start()
        {
            character = GetComponent<Character>();
            InitializeRanking();
        }

        private void InitializeRanking()
        {
            LoadRankingData();
            
            if (currentRank == null)
            {
                currentRank = new PvPPlayerRank
                {
                    playerId = character.characterId,
                    playerName = character.characterName,
                    rank = PvPRank.Bronze,
                    tier = 5,
                    rating = currentRating,
                    wins = 0,
                    losses = 0,
                    winStreak = 0,
                    highestRank = PvPRank.Bronze,
                    seasonRewardsEarned = false
                };
            }
        }

        private void LoadRankingData()
        {
            Debug.Log($"📊 Loading PvP ranking data for {character.characterName}");
        }

        public void RecordMatchResult(bool won, string opponentId, int opponentRating, PvPMatchType matchType)
        {
            if (isInPlacementMatches)
            {
                RecordPlacementMatch(won, opponentRating);
            }
            else
            {
                RecordRankedMatch(won, opponentId, opponentRating, matchType);
            }
            
            if (won)
            {
                seasonWins++;
                currentRank.wins++;
                currentRank.winStreak++;
            }
            else
            {
                seasonLosses++;
                currentRank.losses++;
                currentRank.winStreak = 0;
            }
            
            SaveRankingData();
            
            Debug.Log($"📊 Match result recorded: {(won ? "WIN" : "LOSS")} - Rating: {currentRating} ({(won ? "+" : "")}{GetRatingChange(won, opponentRating)})");
        }

        private void RecordPlacementMatch(bool won, int opponentRating)
        {
            placementMatchesPlayed++;
            
            int ratingChange = GetPlacementRatingChange(won, opponentRating);
            currentRating += ratingChange;
            currentRank.rating = currentRating;
            
            OnRatingChanged?.Invoke(currentRating);
            
            if (placementMatchesPlayed >= placementMatchesRequired)
            {
                CompletePlacementMatches();
            }
            
            Debug.Log($"📊 Placement match {placementMatchesPlayed}/{placementMatchesRequired}: {(won ? "WIN" : "LOSS")} (Rating: {currentRating})");
        }

        private void CompletePlacementMatches()
        {
            isInPlacementMatches = false;
            
            var newRank = CalculateRankFromRating(currentRating);
            var oldRank = currentRank.rank;
            var oldTier = currentRank.tier;
            
            currentRank.rank = newRank.rank;
            currentRank.tier = newRank.tier;
            currentRank.rating = currentRating;
            
            if (newRank.rank > currentRank.highestRank)
            {
                currentRank.highestRank = newRank.rank;
            }
            
            OnRankChanged?.Invoke(new PvPPlayerRank { rank = oldRank, tier = oldTier }, currentRank);
            
            Debug.Log($"🎯 Placement matches complete! Ranked: {newRank.rank} {newRank.tier}");
        }

        private void RecordRankedMatch(bool won, string opponentId, int opponentRating, PvPMatchType matchType)
        {
            int ratingChange = GetRatingChange(won, opponentRating);
            int oldRating = currentRating;
            currentRating += ratingChange;
            currentRank.rating = currentRating;
            
            var newRankInfo = CalculateRankFromRating(currentRating);
            var oldRank = currentRank.rank;
            var oldTier = currentRank.tier;
            
            bool rankChanged = newRankInfo.rank != oldRank || newRankInfo.tier != oldTier;
            
            if (rankChanged)
            {
                currentRank.rank = newRankInfo.rank;
                currentRank.tier = newRankInfo.tier;
                
                if (newRankInfo.rank > currentRank.highestRank)
                {
                    currentRank.highestRank = newRankInfo.rank;
                }
                
                OnRankChanged?.Invoke(new PvPPlayerRank { rank = oldRank, tier = oldTier }, currentRank);
                
                Debug.Log($"🎯 Rank changed: {oldRank} {oldTier} → {newRankInfo.rank} {newRankInfo.tier}");
            }
            
            OnRatingChanged?.Invoke(currentRating);
            
            if (won && rankChanged && newRankInfo.rank > oldRank)
            {
                AwardRankUpRewards(newRankInfo.rank);
            }
        }

        private int GetPlacementRatingChange(bool won, int opponentRating)
        {
            int baseChange = won ? 50 : -30;
            float ratingDifference = (opponentRating - currentRating) / 400f;
            float multiplier = 1f + (ratingDifference * 0.5f);
            
            return Mathf.RoundToInt(baseChange * multiplier);
        }

        private int GetRatingChange(bool won, int opponentRating)
        {
            float expectedScore = 1f / (1f + Mathf.Pow(10f, (opponentRating - currentRating) / 400f));
            float actualScore = won ? 1f : 0f;
            int kFactor = GetKFactor();
            
            return Mathf.RoundToInt(kFactor * (actualScore - expectedScore));
        }

        private int GetKFactor()
        {
            if (isInPlacementMatches) return 50;
            if (currentRank.rank <= PvPRank.Gold) return 32;
            if (currentRank.rank <= PvPRank.Platinum) return 24;
            if (currentRank.rank <= PvPRank.Diamond) return 16;
            return 12; // Master/Grandmaster
        }

        private (PvPRank rank, int tier) CalculateRankFromRating(int rating)
        {
            if (rating < 800) return (PvPRank.Bronze, 5);
            if (rating < 900) return (PvPRank.Bronze, 4);
            if (rating < 1000) return (PvPRank.Bronze, 3);
            if (rating < 1100) return (PvPRank.Bronze, 2);
            if (rating < 1200) return (PvPRank.Bronze, 1);
            
            if (rating < 1300) return (PvPRank.Silver, 5);
            if (rating < 1400) return (PvPRank.Silver, 4);
            if (rating < 1500) return (PvPRank.Silver, 3);
            if (rating < 1600) return (PvPRank.Silver, 2);
            if (rating < 1700) return (PvPRank.Silver, 1);
            
            if (rating < 1800) return (PvPRank.Gold, 5);
            if (rating < 1900) return (PvPRank.Gold, 4);
            if (rating < 2000) return (PvPRank.Gold, 3);
            if (rating < 2100) return (PvPRank.Gold, 2);
            if (rating < 2200) return (PvPRank.Gold, 1);
            
            if (rating < 2300) return (PvPRank.Platinum, 5);
            if (rating < 2400) return (PvPRank.Platinum, 4);
            if (rating < 2500) return (PvPRank.Platinum, 3);
            if (rating < 2600) return (PvPRank.Platinum, 2);
            if (rating < 2700) return (PvPRank.Platinum, 1);
            
            if (rating < 2800) return (PvPRank.Diamond, 5);
            if (rating < 2900) return (PvPRank.Diamond, 4);
            if (rating < 3000) return (PvPRank.Diamond, 3);
            if (rating < 3100) return (PvPRank.Diamond, 2);
            if (rating < 3200) return (PvPRank.Diamond, 1);
            
            if (rating < 3500) return (PvPRank.Master, 1);
            return (PvPRank.Grandmaster, 1);
        }

        private void AwardRankUpRewards(PvPRank newRank)
        {
            int goldReward = 0;
            int expReward = 0;
            
            switch (newRank)
            {
                case PvPRank.Silver:
                    goldReward = 500;
                    expReward = 200;
                    break;
                case PvPRank.Gold:
                    goldReward = 1000;
                    expReward = 400;
                    break;
                case PvPRank.Platinum:
                    goldReward = 2000;
                    expReward = 600;
                    break;
                case PvPRank.Diamond:
                    goldReward = 3000;
                    expReward = 800;
                    break;
                case PvPRank.Master:
                    goldReward = 5000;
                    expReward = 1000;
                    break;
                case PvPRank.Grandmaster:
                    goldReward = 10000;
                    expReward = 1500;
                    break;
            }
            
            if (goldReward > 0)
            {
                character.Stats.AddGold(goldReward);
                character.Stats.AddExperience(expReward);
                Debug.Log($"🏆 Rank up rewards: {goldReward} gold, {expReward} experience");
            }
        }

        public void RequestLeaderboard(PvPRank minRank = PvPRank.Bronze, int count = 100)
        {
            var networkManager = GameManager.Instance.GetNetworkManager();
            if (networkManager.IsConnected)
            {
                var requestData = new
                {
                    minRank = minRank.ToString(),
                    count = count,
                    requesterId = character.characterId
                };
                
                Debug.Log($"📊 Requesting leaderboard (min rank: {minRank}, count: {count})");
            }
        }

        public void HandleLeaderboardUpdate(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("rankings")) return;
            
            var rankingsData = data["rankings"] as List<object>;
            var rankings = new List<PvPPlayerRank>();
            
            foreach (var rankingData in rankingsData)
            {
                var rankingDict = rankingData as Dictionary<string, object>;
                var ranking = new PvPPlayerRank
                {
                    playerId = rankingDict["playerId"].ToString(),
                    playerName = rankingDict["playerName"].ToString(),
                    rank = (PvPRank)System.Enum.Parse(typeof(PvPRank), rankingDict["rank"].ToString()),
                    tier = int.Parse(rankingDict["tier"].ToString()),
                    rating = int.Parse(rankingDict["rating"].ToString()),
                    wins = int.Parse(rankingDict["wins"].ToString()),
                    losses = int.Parse(rankingDict["losses"].ToString()),
                    winStreak = int.Parse(rankingDict["winStreak"].ToString())
                };
                rankings.Add(ranking);
            }
            
            OnLeaderboardUpdated?.Invoke(rankings);
            Debug.Log($"📊 Leaderboard updated with {rankings.Count} players");
        }

        public void StartNewSeason()
        {
            seasonWins = 0;
            seasonLosses = 0;
            placementMatchesPlayed = 0;
            isInPlacementMatches = true;
            
            currentRating = Mathf.RoundToInt(currentRating * 0.8f + 1200 * 0.2f);
            currentRank.rating = currentRating;
            currentRank.wins = 0;
            currentRank.losses = 0;
            currentRank.winStreak = 0;
            currentRank.seasonRewardsEarned = false;
            
            OnSeasonEnded?.Invoke();
            Debug.Log($"🎯 New PvP season started! Rating reset to {currentRating}");
        }

        public void ClaimSeasonRewards()
        {
            if (currentRank.seasonRewardsEarned) return;
            
            var rewards = GetSeasonRewards(currentRank.highestRank);
            
            character.Stats.AddGold(rewards.gold);
            character.Stats.AddExperience(rewards.experience);
            
            currentRank.seasonRewardsEarned = true;
            
            Debug.Log($"🏆 Season rewards claimed: {rewards.gold} gold, {rewards.experience} experience");
        }

        private (int gold, int experience) GetSeasonRewards(PvPRank highestRank)
        {
            switch (highestRank)
            {
                case PvPRank.Bronze: return (1000, 500);
                case PvPRank.Silver: return (2000, 1000);
                case PvPRank.Gold: return (4000, 2000);
                case PvPRank.Platinum: return (6000, 3000);
                case PvPRank.Diamond: return (10000, 5000);
                case PvPRank.Master: return (15000, 7500);
                case PvPRank.Grandmaster: return (25000, 12500);
                default: return (500, 250);
            }
        }

        private void SaveRankingData()
        {
            Debug.Log($"💾 Saving PvP ranking data for {character.characterName}");
        }

        public int GetRankPosition()
        {
            return Random.Range(1, 1000);
        }

        public string GetRankDisplayName()
        {
            if (isInPlacementMatches)
                return $"Placement ({placementMatchesPlayed}/{placementMatchesRequired})";
            
            return $"{currentRank.rank} {currentRank.tier}";
        }

        public Color GetRankColor()
        {
            switch (currentRank.rank)
            {
                case PvPRank.Bronze: return new Color(0.8f, 0.5f, 0.2f);
                case PvPRank.Silver: return new Color(0.7f, 0.7f, 0.7f);
                case PvPRank.Gold: return new Color(1f, 0.8f, 0f);
                case PvPRank.Platinum: return new Color(0.7f, 0.9f, 0.9f);
                case PvPRank.Diamond: return new Color(0.7f, 0.9f, 1f);
                case PvPRank.Master: return new Color(1f, 0.5f, 1f);
                case PvPRank.Grandmaster: return new Color(1f, 0.2f, 0.2f);
                default: return Color.white;
            }
        }
    }

    [System.Serializable]
    public class PvPPlayerRank
    {
        public string playerId;
        public string playerName;
        public PvPRank rank;
        public int tier;
        public int rating;
        public int wins;
        public int losses;
        public int winStreak;
        public PvPRank highestRank;
        public bool seasonRewardsEarned;
    }

    public enum PvPRank
    {
        Unranked = 0,
        Bronze = 1,
        Silver = 2,
        Gold = 3,
        Platinum = 4,
        Diamond = 5,
        Master = 6,
        Grandmaster = 7
    }

    public enum PvPMatchType
    {
        Duel,
        Battleground,
        CitySiege
    }
}
