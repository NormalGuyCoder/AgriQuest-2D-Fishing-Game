using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;

/// <summary>
/// Manages player scores across sessions and games.
/// Persists data using PlayerPrefs.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [Tooltip("Should scores persist across game sessions?")]
    public bool persistAcrossSessions = true;

    private const string TOTAL_SCORE_KEY = "PlayerTotalScore";
    private const string GAMES_PLAYED_KEY = "GamesPlayedCount";
    private const string HIGHEST_SCORE_KEY = "HighestSingleGameScore";
    private const string TOTAL_MISTAKES_KEY = "TotalMistakes";
    private const string FISH_STATS_PREFIX = "FishStats_";
    private const string SAVE_FILE_NAME = "score_data.json";

    private int sessionScore = 0; // Score accumulated in current session
    private int totalScore = 0; // Total score across all sessions (persisted)
    private int gamesPlayed = 0; // Total games played (persisted)
    private int highestSingleGameScore = 0; // Highest score in a single game (persisted)
    private int totalMistakes = 0; // Total mistakes across all games (persisted)
    private List<GameSessionData> sessionHistory = new List<GameSessionData>();
    
    // Fish statistics: Dictionary<FishName, FishStatistics>
    private Dictionary<string, FishStatistics> fishStats = new Dictionary<string, FishStatistics>();

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    [System.Serializable]
    public class GameSessionData
    {
        public string fishName;
        public int score;
        public int timeBonus;
        public int mistakes;
        public int cleanliness;
        public float timeTaken; // Time taken in seconds
        public int bonesRemoved;
        public System.DateTime timestamp;

        public GameSessionData(string fish, int gameScore, int bonus, int mistakesCount, int cleanlinessScore, float timeTakenSeconds, int bonesRemovedCount, System.DateTime? customTimestamp = null)
        {
            fishName = fish;
            score = gameScore;
            timeBonus = bonus;
            mistakes = mistakesCount;
            cleanliness = cleanlinessScore;
            timeTaken = timeTakenSeconds;
            bonesRemoved = bonesRemovedCount;
            timestamp = customTimestamp ?? System.DateTime.Now;
        }
    }

    [System.Serializable]
    public class FishStatistics
    {
        public string fishName;
        public int timesDeboned; // How many times this fish was deboned
        public int totalMistakes; // Total mistakes made on this fish
        public int bestScore; // Best score achieved on this fish
        public float averageMistakes; // Average mistakes per deboning
        public float averageScore; // Average score per deboning
        public int totalBonesRemoved; // Total bones removed across all attempts

        public FishStatistics(string name)
        {
            fishName = name;
            timesDeboned = 0;
            totalMistakes = 0;
            bestScore = 0;
            averageMistakes = 0f;
            averageScore = 0f;
            totalBonesRemoved = 0;
        }
    }

    [System.Serializable]
    private class SerializableGameSessionData
    {
        public string fishName;
        public int score;
        public int timeBonus;
        public int mistakes;
        public int cleanliness;
        public float timeTaken;
        public int bonesRemoved;
        public string timestamp;

        public SerializableGameSessionData() { }

        public SerializableGameSessionData(GameSessionData data)
        {
            fishName = data.fishName;
            score = data.score;
            timeBonus = data.timeBonus;
            mistakes = data.mistakes;
            cleanliness = data.cleanliness;
            timeTaken = data.timeTaken;
            bonesRemoved = data.bonesRemoved;
            timestamp = data.timestamp.ToString("o");
        }

        public GameSessionData ToGameSessionData()
        {
            System.DateTime parsedTimestamp;
            if (!System.DateTime.TryParseExact(timestamp, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsedTimestamp))
            {
                parsedTimestamp = System.DateTime.Now;
            }
            return new GameSessionData(fishName, score, timeBonus, mistakes, cleanliness, timeTaken, bonesRemoved, parsedTimestamp);
        }
    }

    [System.Serializable]
    private class ScoreSaveData
    {
        public int totalScore;
        public int sessionScore;
        public int gamesPlayed;
        public int highestSingleGameScore;
        public int totalMistakes;
        public List<SerializableGameSessionData> sessionHistory;
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

    void Start()
    {
        // Reset session score when starting (in case of scene reload)
        // Session score accumulates during gameplay
    }

    /// <summary>
    /// Add score from a completed game to the total
    /// </summary>
    public void AddGameScore(string fishName, int gameScore, int timeBonus, int mistakes = 0, int cleanliness = 100, float timeTaken = 0f, int bonesRemoved = 0)
    {
        int totalGameScore = gameScore; // GameManager already adds time bonus, so this is the final score
        
        sessionScore += totalGameScore;
        totalScore += totalGameScore;
        gamesPlayed++;
        totalMistakes += mistakes;

        // Track highest single game score
        if (totalGameScore > highestSingleGameScore)
        {
            highestSingleGameScore = totalGameScore;
        }

        // Update fish-specific statistics
        if (!fishStats.ContainsKey(fishName))
        {
            fishStats[fishName] = new FishStatistics(fishName);
        }

        FishStatistics stats = fishStats[fishName];
        stats.timesDeboned++;
        stats.totalMistakes += mistakes;
        stats.totalBonesRemoved += bonesRemoved;
        
        // Update best score for this fish
        if (totalGameScore > stats.bestScore)
        {
            stats.bestScore = totalGameScore;
        }

        // Calculate averages
        stats.averageMistakes = (float)stats.totalMistakes / stats.timesDeboned;
        stats.averageScore = (stats.averageScore * (stats.timesDeboned - 1) + totalGameScore) / stats.timesDeboned;

        // Record session data
        GameSessionData sessionData = new GameSessionData(fishName, totalGameScore, timeBonus, mistakes, cleanliness, timeTaken, bonesRemoved);
        sessionHistory.Add(sessionData);

        // Persist data
        if (persistAcrossSessions)
        {
            SaveScoreData();
        }

        Debug.Log($"Score added: {totalGameScore} points from {fishName} (Mistakes: {mistakes}, Bones Removed: {bonesRemoved}). Total: {totalScore}");
    }

    /// <summary>
    /// Get total score across all sessions
    /// </summary>
    public int GetTotalScore()
    {
        return totalScore;
    }

    /// <summary>
    /// Get score accumulated in current session only
    /// </summary>
    public int GetSessionScore()
    {
        return sessionScore;
    }

    /// <summary>
    /// Get total number of games played
    /// </summary>
    public int GetGamesPlayed()
    {
        return gamesPlayed;
    }

    /// <summary>
    /// Get highest score in a single game
    /// </summary>
    public int GetHighestSingleGameScore()
    {
        return highestSingleGameScore;
    }

    /// <summary>
    /// Get recent session history
    /// </summary>
    public List<GameSessionData> GetSessionHistory(int count = 10)
    {
        if (sessionHistory.Count <= count)
            return new List<GameSessionData>(sessionHistory);

        // Return last N entries
        List<GameSessionData> recent = new List<GameSessionData>();
        int startIndex = sessionHistory.Count - count;
        for (int i = startIndex; i < sessionHistory.Count; i++)
        {
            recent.Add(sessionHistory[i]);
        }
        return recent;
    }

    /// <summary>
    /// Get total mistakes across all games
    /// </summary>
    public int GetTotalMistakes()
    {
        return totalMistakes;
    }

    /// <summary>
    /// Get average mistakes per game
    /// </summary>
    public float GetAverageMistakes()
    {
        if (gamesPlayed == 0) return 0f;
        return (float)totalMistakes / gamesPlayed;
    }

    /// <summary>
    /// Get statistics for a specific fish
    /// </summary>
    public FishStatistics GetFishStatistics(string fishName)
    {
        if (fishStats.ContainsKey(fishName))
        {
            return fishStats[fishName];
        }
        return null;
    }

    /// <summary>
    /// Get all fish statistics
    /// </summary>
    public Dictionary<string, FishStatistics> GetAllFishStatistics()
    {
        return new Dictionary<string, FishStatistics>(fishStats);
    }

    /// <summary>
    /// Get list of all fish that have been deboned, sorted by times deboned (most played first)
    /// </summary>
    public List<FishStatistics> GetFishStatisticsSortedByPlayCount()
    {
        return fishStats.Values.OrderByDescending(f => f.timesDeboned).ToList();
    }

    /// <summary>
    /// Get list of all fish that have been deboned, sorted by best score (highest first)
    /// </summary>
    public List<FishStatistics> GetFishStatisticsSortedByBestScore()
    {
        return fishStats.Values.OrderByDescending(f => f.bestScore).ToList();
    }

    /// <summary>
    /// Get the most played fish
    /// </summary>
    public FishStatistics GetMostPlayedFish()
    {
        if (fishStats.Count == 0) return null;
        return fishStats.Values.OrderByDescending(f => f.timesDeboned).First();
    }

    /// <summary>
    /// Get total number of unique fish deboned
    /// </summary>
    public int GetUniqueFishCount()
    {
        return fishStats.Count;
    }

    /// <summary>
    /// Reset all scores (for testing or new game)
    /// </summary>
    public void ResetAllScores()
    {
        sessionScore = 0;
        totalScore = 0;
        gamesPlayed = 0;
        highestSingleGameScore = 0;
        totalMistakes = 0;
        sessionHistory.Clear();
        fishStats.Clear();

        if (persistAcrossSessions)
        {
            PlayerPrefs.DeleteKey(TOTAL_SCORE_KEY);
            PlayerPrefs.DeleteKey(GAMES_PLAYED_KEY);
            PlayerPrefs.DeleteKey(HIGHEST_SCORE_KEY);
            PlayerPrefs.DeleteKey(TOTAL_MISTAKES_KEY);
            
            // Delete all fish stats keys
            string[] keys = PlayerPrefs.GetString("FishStatsKeys", "").Split(',');
            foreach (string key in keys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    PlayerPrefs.DeleteKey(FISH_STATS_PREFIX + key);
                }
            }
            PlayerPrefs.DeleteKey("FishStatsKeys");
            PlayerPrefs.Save();
        }

        DeleteSaveFile();

        Debug.Log("All scores reset!");
    }

    /// <summary>
    /// Reset only session score (keeps total persistent scores)
    /// </summary>
    public void ResetSessionScore()
    {
        sessionScore = 0;
        Debug.Log("Session score reset!");
    }

    private void LoadScoreData()
    {
        if (!persistAcrossSessions)
            return;

        totalScore = PlayerPrefs.GetInt(TOTAL_SCORE_KEY, 0);
        gamesPlayed = PlayerPrefs.GetInt(GAMES_PLAYED_KEY, 0);
        highestSingleGameScore = PlayerPrefs.GetInt(HIGHEST_SCORE_KEY, 0);
        totalMistakes = PlayerPrefs.GetInt(TOTAL_MISTAKES_KEY, 0);

        // Load fish statistics (simplified - for full persistence, use JSON)
        LoadFromDisk();

        Debug.Log($"Loaded score data: Total={totalScore}, Games={gamesPlayed}, Highest={highestSingleGameScore}, Mistakes={totalMistakes}");
    }

    private void SaveScoreData()
    {
        if (!persistAcrossSessions)
            return;

        PlayerPrefs.SetInt(TOTAL_SCORE_KEY, totalScore);
        PlayerPrefs.SetInt(GAMES_PLAYED_KEY, gamesPlayed);
        PlayerPrefs.SetInt(HIGHEST_SCORE_KEY, highestSingleGameScore);
        PlayerPrefs.SetInt(TOTAL_MISTAKES_KEY, totalMistakes);
        PlayerPrefs.Save();

        SaveToDisk();

        Debug.Log($"Saved score data: Total={totalScore}, Games={gamesPlayed}, Highest={highestSingleGameScore}, Mistakes={totalMistakes}");
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
                totalMistakes = totalMistakes,
                sessionHistory = sessionHistory.Select(s => new SerializableGameSessionData(s)).ToList(),
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
            Debug.LogError($"ScoreManager: Failed to save analytics data. {e.Message}");
        }
    }

    private void RebuildFishStatisticsFromHistory()
    {
        fishStats.Clear();

        if (sessionHistory == null || sessionHistory.Count == 0)
            return;

        foreach (var session in sessionHistory)
        {
            if (session == null || string.IsNullOrEmpty(session.fishName))
                continue;

            if (!fishStats.ContainsKey(session.fishName))
            {
                fishStats[session.fishName] = new FishStatistics(session.fishName);
            }

            FishStatistics stats = fishStats[session.fishName];
            stats.timesDeboned++;
            stats.totalMistakes += session.mistakes;
            stats.totalBonesRemoved += session.bonesRemoved;

            if (session.score > stats.bestScore)
            {
                stats.bestScore = session.score;
            }

            // Update averages incrementally
            stats.averageMistakes = (float)stats.totalMistakes / stats.timesDeboned;
            stats.averageScore = (stats.averageScore * (stats.timesDeboned - 1) + session.score) / stats.timesDeboned;
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
            totalMistakes = saveData.totalMistakes;

            sessionHistory = saveData.sessionHistory != null
                ? saveData.sessionHistory.Select(s => s.ToGameSessionData()).ToList()
                : new List<GameSessionData>();

            fishStats.Clear();
            if (saveData.fishStats != null)
            {
                foreach (var stats in saveData.fishStats)
                {
                    if (stats != null && !string.IsNullOrEmpty(stats.fishName))
                    {
                        fishStats[stats.fishName] = stats;
                    }
                }
            }

            // Ensure fish statistics are available even if fishStats list is missing
            if (fishStats.Count == 0)
            {
                RebuildFishStatisticsFromHistory();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ScoreManager: Failed to load analytics data. {e.Message}");
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
            Debug.LogError($"ScoreManager: Failed to delete analytics data file. {e.Message}");
        }
    }

    void OnApplicationQuit()
    {
        // Ensure data is saved on quit
        if (persistAcrossSessions)
        {
            SaveScoreData();
        }
    }
}


