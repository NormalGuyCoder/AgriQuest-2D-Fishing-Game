using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// Manages player scores for the freshwater fishing minigame.
/// Persists data using JSON files, similar to deboning minigame's ScoreManager.
/// </summary>
public class FreshwaterFishingScoreManager : MonoBehaviour
{
    public static FreshwaterFishingScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [Tooltip("Should scores persist across game sessions?")]
    public bool persistAcrossSessions = true;

    private const string TOTAL_SCORE_KEY = "FreshwaterFishing_TotalScore";
    private const string GAMES_PLAYED_KEY = "FreshwaterFishing_GamesPlayed";
    private const string HIGHEST_SCORE_KEY = "FreshwaterFishing_HighestScore";
    private const string TOTAL_FISH_CAUGHT_KEY = "FreshwaterFishing_TotalFishCaught";
    private const string SAVE_FILE_NAME = "freshwater_fishing_score_data.json";

    private int sessionScore = 0;
    private int totalScore = 0;
    private int gamesPlayed = 0;
    private int highestSingleGameScore = 0;
    private int totalFishCaught = 0;
    private List<FishingSessionData> sessionHistory = new List<FishingSessionData>();
    
    // Fish statistics: Dictionary<FishName, FishStatistics>
    private Dictionary<string, FishStatistics> fishStats = new Dictionary<string, FishStatistics>();

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    [System.Serializable]
    public class FishingSessionData
    {
        public string fishName;
        public string fishId;
        public int score;
        public int experiencePoints;
        public int goldValue;
        public float catchTime; // Time taken to catch in seconds
        public System.DateTime timestamp;

        public FishingSessionData(string fish, string id, int gameScore, int exp, int gold, float timeTaken, System.DateTime? customTimestamp = null)
        {
            fishName = fish;
            fishId = id;
            score = gameScore;
            experiencePoints = exp;
            goldValue = gold;
            catchTime = timeTaken;
            timestamp = customTimestamp ?? System.DateTime.Now;
        }
    }

    [System.Serializable]
    public class FishStatistics
    {
        public string fishName;
        public string fishId;
        public int timesCaught;
        public int bestScore;
        public float averageCatchTime;
        public int totalExperience;
        public int totalGold;

        public FishStatistics(string name, string id)
        {
            fishName = name;
            fishId = id;
            timesCaught = 0;
            bestScore = 0;
            averageCatchTime = 0f;
            totalExperience = 0;
            totalGold = 0;
        }
    }

    [System.Serializable]
    private class SerializableFishingSessionData
    {
        public string fishName;
        public string fishId;
        public int score;
        public int experiencePoints;
        public int goldValue;
        public float catchTime;
        public string timestamp;

        public SerializableFishingSessionData() { }

        public SerializableFishingSessionData(FishingSessionData data)
        {
            fishName = data.fishName;
            fishId = data.fishId;
            score = data.score;
            experiencePoints = data.experiencePoints;
            goldValue = data.goldValue;
            catchTime = data.catchTime;
            timestamp = data.timestamp.ToString("o");
        }

        public FishingSessionData ToFishingSessionData()
        {
            System.DateTime parsedTimestamp;
            if (!System.DateTime.TryParseExact(timestamp, "o", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out parsedTimestamp))
            {
                parsedTimestamp = System.DateTime.Now;
            }
            return new FishingSessionData(fishName, fishId, score, experiencePoints, goldValue, catchTime, parsedTimestamp);
        }
    }

    [System.Serializable]
    private class ScoreSaveData
    {
        public int totalScore;
        public int sessionScore;
        public int gamesPlayed;
        public int highestSingleGameScore;
        public int totalFishCaught;
        public List<SerializableFishingSessionData> sessionHistory;
        public List<FishStatistics> fishStats;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadScoreData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Record a caught fish and calculate score
    /// </summary>
    public void RecordCatch(string fishName, string fishId, int experiencePoints, int goldValue, float catchTime)
    {
        // Calculate score based on catch time and rewards
        // Faster catches = higher score, with bonus for experience and gold
        int baseScore = 100;
        int timeBonus = Mathf.Max(0, (int)((30f - catchTime) * 10)); // Bonus for fast catches (max 30 seconds)
        int rewardBonus = experiencePoints * 2 + goldValue;
        int totalGameScore = baseScore + timeBonus + rewardBonus;

        sessionScore += totalGameScore;
        totalScore += totalGameScore;
        gamesPlayed++;
        totalFishCaught++;

        // Track highest single game score
        if (totalGameScore > highestSingleGameScore)
        {
            highestSingleGameScore = totalGameScore;
        }

        // Update fish-specific statistics
        string statsKey = string.IsNullOrEmpty(fishId) ? fishName : fishId;
        if (!fishStats.ContainsKey(statsKey))
        {
            fishStats[statsKey] = new FishStatistics(fishName, fishId);
        }

        FishStatistics stats = fishStats[statsKey];
        stats.timesCaught++;
        stats.totalExperience += experiencePoints;
        stats.totalGold += goldValue;
        
        // Update best score for this fish
        if (totalGameScore > stats.bestScore)
        {
            stats.bestScore = totalGameScore;
        }

        // Calculate average catch time
        stats.averageCatchTime = (stats.averageCatchTime * (stats.timesCaught - 1) + catchTime) / stats.timesCaught;

        // Record session data
        FishingSessionData sessionData = new FishingSessionData(fishName, fishId, totalGameScore, experiencePoints, goldValue, catchTime);
        sessionHistory.Add(sessionData);

        // Persist data
        if (persistAcrossSessions)
        {
            SaveScoreData();
        }

        Debug.Log($"Freshwater Fishing: Caught {fishName} - Score: {totalGameScore} (Time: {catchTime:F1}s, Exp: {experiencePoints}, Gold: {goldValue}). Total: {totalScore}");
    }

    public int GetTotalScore()
    {
        return totalScore;
    }

    public int GetSessionScore()
    {
        return sessionScore;
    }

    public int GetGamesPlayed()
    {
        return gamesPlayed;
    }

    public int GetHighestSingleGameScore()
    {
        return highestSingleGameScore;
    }

    public int GetTotalFishCaught()
    {
        return totalFishCaught;
    }

    public List<FishingSessionData> GetSessionHistory(int count = 10)
    {
        if (sessionHistory.Count <= count)
            return new List<FishingSessionData>(sessionHistory);

        List<FishingSessionData> recent = new List<FishingSessionData>();
        int startIndex = sessionHistory.Count - count;
        for (int i = startIndex; i < sessionHistory.Count; i++)
        {
            recent.Add(sessionHistory[i]);
        }
        return recent;
    }

    public FishStatistics GetFishStatistics(string fishNameOrId)
    {
        if (fishStats.ContainsKey(fishNameOrId))
        {
            return fishStats[fishNameOrId];
        }
        return null;
    }

    public Dictionary<string, FishStatistics> GetAllFishStatistics()
    {
        return new Dictionary<string, FishStatistics>(fishStats);
    }

    public List<FishStatistics> GetFishStatisticsSortedByCatchCount()
    {
        return fishStats.Values.OrderByDescending(f => f.timesCaught).ToList();
    }

    public List<FishStatistics> GetFishStatisticsSortedByBestScore()
    {
        return fishStats.Values.OrderByDescending(f => f.bestScore).ToList();
    }

    public FishStatistics GetMostCaughtFish()
    {
        if (fishStats.Count == 0) return null;
        return fishStats.Values.OrderByDescending(f => f.timesCaught).First();
    }

    public int GetUniqueFishCount()
    {
        return fishStats.Count;
    }

    public void ResetAllScores()
    {
        sessionScore = 0;
        totalScore = 0;
        gamesPlayed = 0;
        highestSingleGameScore = 0;
        totalFishCaught = 0;
        sessionHistory.Clear();
        fishStats.Clear();

        if (persistAcrossSessions)
        {
            PlayerPrefs.DeleteKey(TOTAL_SCORE_KEY);
            PlayerPrefs.DeleteKey(GAMES_PLAYED_KEY);
            PlayerPrefs.DeleteKey(HIGHEST_SCORE_KEY);
            PlayerPrefs.DeleteKey(TOTAL_FISH_CAUGHT_KEY);
            PlayerPrefs.Save();
        }

        DeleteSaveFile();
        Debug.Log("Freshwater Fishing: All scores reset!");
    }

    public void ResetSessionScore()
    {
        sessionScore = 0;
        Debug.Log("Freshwater Fishing: Session score reset!");
    }

    private void LoadScoreData()
    {
        if (!persistAcrossSessions)
            return;

        totalScore = PlayerPrefs.GetInt(TOTAL_SCORE_KEY, 0);
        gamesPlayed = PlayerPrefs.GetInt(GAMES_PLAYED_KEY, 0);
        highestSingleGameScore = PlayerPrefs.GetInt(HIGHEST_SCORE_KEY, 0);
        totalFishCaught = PlayerPrefs.GetInt(TOTAL_FISH_CAUGHT_KEY, 0);

        LoadFromDisk();
        Debug.Log($"Freshwater Fishing: Loaded score data - Total={totalScore}, Games={gamesPlayed}, Highest={highestSingleGameScore}, Fish Caught={totalFishCaught}");
    }

    private void SaveScoreData()
    {
        if (!persistAcrossSessions)
            return;

        PlayerPrefs.SetInt(TOTAL_SCORE_KEY, totalScore);
        PlayerPrefs.SetInt(GAMES_PLAYED_KEY, gamesPlayed);
        PlayerPrefs.SetInt(HIGHEST_SCORE_KEY, highestSingleGameScore);
        PlayerPrefs.SetInt(TOTAL_FISH_CAUGHT_KEY, totalFishCaught);
        PlayerPrefs.Save();

        SaveToDisk();
    }

    private void SaveToDisk()
    {
        if (!persistAcrossSessions)
            return;

        try
        {
            ScoreSaveData saveData = new ScoreSaveData
            {
                totalScore = totalScore,
                sessionScore = sessionScore,
                gamesPlayed = gamesPlayed,
                highestSingleGameScore = highestSingleGameScore,
                totalFishCaught = totalFishCaught,
                sessionHistory = sessionHistory.Select(s => new SerializableFishingSessionData(s)).ToList(),
                fishStats = new List<FishStatistics>(fishStats.Values)
            };

            string directory = Path.GetDirectoryName(SaveFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SaveFilePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FreshwaterFishingScoreManager: Failed to save score data. {e.Message}");
        }
    }

    private void LoadFromDisk()
    {
        if (!persistAcrossSessions)
            return;

        if (!File.Exists(SaveFilePath))
            return;

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            if (string.IsNullOrEmpty(json))
                return;

            ScoreSaveData saveData = JsonUtility.FromJson<ScoreSaveData>(json);
            if (saveData == null)
                return;

            totalScore = saveData.totalScore;
            sessionScore = saveData.sessionScore;
            gamesPlayed = saveData.gamesPlayed;
            highestSingleGameScore = saveData.highestSingleGameScore;
            totalFishCaught = saveData.totalFishCaught;

            sessionHistory = saveData.sessionHistory != null
                ? saveData.sessionHistory.Select(s => s.ToFishingSessionData()).ToList()
                : new List<FishingSessionData>();

            fishStats.Clear();
            if (saveData.fishStats != null)
            {
                foreach (var stats in saveData.fishStats)
                {
                    if (stats != null && !string.IsNullOrEmpty(stats.fishName))
                    {
                        string key = string.IsNullOrEmpty(stats.fishId) ? stats.fishName : stats.fishId;
                        fishStats[key] = stats;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FreshwaterFishingScoreManager: Failed to load score data. {e.Message}");
        }
    }

    private void DeleteSaveFile()
    {
        if (!File.Exists(SaveFilePath))
            return;

        try
        {
            File.Delete(SaveFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FreshwaterFishingScoreManager: Failed to delete save file. {e.Message}");
        }
    }

    void OnApplicationQuit()
    {
        if (persistAcrossSessions)
        {
            SaveScoreData();
        }
    }
}

