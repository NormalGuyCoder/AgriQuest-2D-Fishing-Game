using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class FishingStats : MonoBehaviour
{
    public static FishingStats Instance { get; private set; }

    [System.Serializable]
    public class FishCatchData
    {
        public FishData fish;
        public int catchCount;
        public float totalTimeSpent;
        public List<DateTime> catchTimes = new List<DateTime>();
    }

    [System.Serializable]
    public class FishingSession
    {
        public DateTime startTime;
        public DateTime endTime;
        public List<FishCatchData> catches = new List<FishCatchData>();
    }

    private Dictionary<FishData, FishCatchData> fishCatchStats = new Dictionary<FishData, FishCatchData>();
    private List<FishingSession> fishingSessions = new List<FishingSession>();
    private FishingSession currentSession;
    private DateTime sessionStartTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartFishingSession()
    {
        sessionStartTime = DateTime.Now;
        currentSession = new FishingSession { startTime = sessionStartTime };
    }

    public void EndFishingSession()
    {
        if (currentSession != null)
        {
            currentSession.endTime = DateTime.Now;
            fishingSessions.Add(currentSession);
            currentSession = null;
            SaveStats();
        }
    }

    public void RecordFishCatch(FishData fish)
    {
        if (fish == null) return;

        if (!fishCatchStats.ContainsKey(fish))
        {
            fishCatchStats[fish] = new FishCatchData { fish = fish };
        }

        var catchData = fishCatchStats[fish];
        catchData.catchCount++;
        catchData.catchTimes.Add(DateTime.Now);

        if (currentSession != null)
        {
            var sessionCatch = currentSession.catches.FirstOrDefault(c => c.fish == fish);
            if (sessionCatch == null)
            {
                sessionCatch = new FishCatchData { fish = fish };
                currentSession.catches.Add(sessionCatch);
            }
            sessionCatch.catchCount++;
            sessionCatch.catchTimes.Add(DateTime.Now);
        }

        SaveStats();
    }

    public Dictionary<FishData, FishCatchData> GetAllFishStats()
    {
        return fishCatchStats;
    }

    public List<FishingSession> GetAllFishingSessions()
    {
        return fishingSessions;
    }

    public float GetAverageSessionTime()
    {
        if (fishingSessions.Count == 0) return 0f;
        return (float)fishingSessions.Average(s => (s.endTime - s.startTime).TotalMinutes);
    }

    public FishData GetMostCaughtFish()
    {
        return fishCatchStats.OrderByDescending(x => x.Value.catchCount).FirstOrDefault().Key;
    }

    public FishData GetLeastCaughtFish()
    {
        return fishCatchStats.OrderBy(x => x.Value.catchCount).FirstOrDefault().Key;
    }

    private void SaveStats()
    {
        string statsJson = JsonUtility.ToJson(new SerializableStats
        {
            fishCatchStats = fishCatchStats.Values.ToList(),
            fishingSessions = fishingSessions
        });
        PlayerPrefs.SetString("FishingStats", statsJson);
        PlayerPrefs.Save();
    }

    private void LoadStats()
    {
        if (PlayerPrefs.HasKey("FishingStats"))
        {
            string statsJson = PlayerPrefs.GetString("FishingStats");
            var stats = JsonUtility.FromJson<SerializableStats>(statsJson);
            
            fishCatchStats.Clear();
            foreach (var catchData in stats.fishCatchStats)
            {
                fishCatchStats[catchData.fish] = catchData;
            }
            
            fishingSessions = stats.fishingSessions;
        }
    }

    /// <summary>
    /// Reset all stored fishing analytics and remove the PlayerPrefs entry.
    /// Call this as part of a full "New Game" reset.
    /// </summary>
    public void ResetAllStats()
    {
        fishCatchStats.Clear();
        fishingSessions.Clear();
        currentSession = null;

        PlayerPrefs.DeleteKey("FishingStats");
        PlayerPrefs.Save();
    }

    [System.Serializable]
    private class SerializableStats
    {
        public List<FishCatchData> fishCatchStats = new List<FishCatchData>();
        public List<FishingSession> fishingSessions = new List<FishingSession>();
    }
} 