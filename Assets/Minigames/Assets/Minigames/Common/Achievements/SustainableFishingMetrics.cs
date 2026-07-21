using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tracks sustainable fishing metrics across all minigames.
/// Monitors player behavior to evaluate sustainable and ethical fishing practices.
/// </summary>
public class SustainableFishingMetrics : MonoBehaviour
{
    public static SustainableFishingMetrics Instance { get; private set; }

    [Header("Configuration")]
    [Tooltip("Maximum catches per fish species before it becomes endangered")]
    public int maxCatchesPerSpecies = 10;
    
    [Tooltip("Minimum diversity required (different species caught)")]
    public int minSpeciesDiversity = 5;
    
    [Tooltip("Maximum total catches per session before warning")]
    public int maxCatchesPerSession = 20;

    // Tracking Data
    private Dictionary<string, int> catchesPerSpecies = new Dictionary<string, int>();
    private Dictionary<string, List<SustainabilityCatchRecord>> catchHistory = new Dictionary<string, List<SustainabilityCatchRecord>>();
    private List<PlayerDecision> goodDecisions = new List<PlayerDecision>();
    private List<PlayerDecision> badDecisions = new List<PlayerDecision>();
    private int totalCatches = 0;
    private int totalSessions = 0;
    private float totalPlayTime = 0f;

    // Session tracking
    private int currentSessionCatches = 0;
    private HashSet<string> currentSessionSpecies = new HashSet<string>();
    private float sessionStartTime = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMetrics();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartNewSession();
    }

    /// <summary>
    /// Start tracking a new play session
    /// </summary>
    public void StartNewSession()
    {
        currentSessionCatches = 0;
        currentSessionSpecies.Clear();
        sessionStartTime = Time.time;
        totalSessions++;
    }

    /// <summary>
    /// Record a fish catch and evaluate sustainability
    /// </summary>
    public void RecordCatch(string fishId, string fishName, FishCatchSource source, bool isEndangered = false)
    {
        if (string.IsNullOrEmpty(fishId)) return;

        totalCatches++;
        currentSessionCatches++;

        // Track species
        if (!catchesPerSpecies.ContainsKey(fishId))
        {
            catchesPerSpecies[fishId] = 0;
            catchHistory[fishId] = new List<SustainabilityCatchRecord>();
        }

        catchesPerSpecies[fishId]++;
        currentSessionSpecies.Add(fishId);

        // Create catch record
        SustainabilityCatchRecord record = new SustainabilityCatchRecord
        {
            fishId = fishId,
            fishName = fishName,
            source = source,
            timestamp = Time.time,
            isEndangered = isEndangered
        };

        catchHistory[fishId].Add(record);

        // Evaluate sustainability
        EvaluateCatch(record);
        
        // Update achievements in real-time
        if (AchievementSystem.Instance != null)
        {
            SustainabilityReport report = GetReport();
            AchievementSystem.Instance.UpdateAchievements(report);
        }
    }

    /// <summary>
    /// Evaluate if a catch is sustainable
    /// </summary>
    private void EvaluateCatch(SustainabilityCatchRecord record)
    {
        int speciesCount = catchesPerSpecies[record.fishId];
        bool isOverfishing = speciesCount > maxCatchesPerSpecies;
        bool isEndangeredSpecies = record.isEndangered;
        bool isSessionOverfishing = currentSessionCatches > maxCatchesPerSession;

        // Bad decisions
        if (isOverfishing)
        {
            AddBadDecision(
                $"Overfishing: Caught {speciesCount} {record.fishName} (max: {maxCatchesPerSpecies})",
                "Overfishing a single species can lead to population decline and ecosystem imbalance.",
                DecisionSeverity.High
            );
        }

        if (isEndangeredSpecies)
        {
            AddBadDecision(
                $"Caught endangered species: {record.fishName}",
                "Catching endangered species threatens biodiversity and violates conservation principles.",
                DecisionSeverity.Critical
            );
        }

        if (isSessionOverfishing)
        {
            AddBadDecision(
                $"Excessive fishing: {currentSessionCatches} catches in one session (max: {maxCatchesPerSession})",
                "Fishing too intensively in a short time can deplete local fish populations.",
                DecisionSeverity.Medium
            );
        }

        // Good decisions
        if (speciesCount <= maxCatchesPerSpecies / 2)
        {
            AddGoodDecision(
                $"Sustainable catch: {record.fishName} (count: {speciesCount})",
                "Maintaining catch limits helps preserve fish populations for future generations.",
                DecisionSeverity.Low
            );
        }

        if (currentSessionSpecies.Count >= minSpeciesDiversity)
        {
            AddGoodDecision(
                $"Good diversity: Caught {currentSessionSpecies.Count} different species",
                "Diversifying catches prevents overexploitation of any single species.",
                DecisionSeverity.Medium
            );
        }
    }

    /// <summary>
    /// Record a deboning completion (educational activity)
    /// </summary>
    public void RecordDeboningCompletion(string fishId, string fishName, int bonesRemoved, int mistakes, float timeTaken)
    {
        // Good decision: Learning about fish anatomy
        AddGoodDecision(
            $"Learned about {fishName} anatomy: Removed {bonesRemoved} bones",
            "Understanding fish anatomy helps with proper preparation and reduces waste.",
            DecisionSeverity.Medium
        );

        // Bad decision: Too many mistakes (waste)
        if (mistakes > 5)
        {
            AddBadDecision(
                $"High mistakes while deboning {fishName}: {mistakes} errors",
                "Too many mistakes can lead to food waste and improper fish preparation.",
                DecisionSeverity.Low
            );
        }
    }

    /// <summary>
    /// Record a decision made by the player
    /// </summary>
    public void RecordDecision(string description, string explanation, bool isGood, DecisionSeverity severity = DecisionSeverity.Medium)
    {
        if (isGood)
        {
            AddGoodDecision(description, explanation, severity);
        }
        else
        {
            AddBadDecision(description, explanation, severity);
        }
    }

    private void AddGoodDecision(string description, string explanation, DecisionSeverity severity)
    {
        goodDecisions.Add(new PlayerDecision
        {
            description = description,
            explanation = explanation,
            timestamp = Time.time,
            severity = severity
        });
    }

    private void AddBadDecision(string description, string explanation, DecisionSeverity severity)
    {
        badDecisions.Add(new PlayerDecision
        {
            description = description,
            explanation = explanation,
            timestamp = Time.time,
            severity = severity
        });
    }

    /// <summary>
    /// Get sustainability score (0-100)
    /// </summary>
    public float GetSustainabilityScore()
    {
        float score = 30f;  // Start at 30

        // Deduct for overfishing (reduce penalty since base is lower)
        foreach (var pair in catchesPerSpecies)
        {
            if (pair.Value > maxCatchesPerSpecies)
            {
                int excess = pair.Value - maxCatchesPerSpecies;
                score -= excess * 1f; // Reduced from 2f to 1f
            }
        }

        // Deduct for bad decisions (reduced penalties)
        score -= badDecisions.Count(d => d.severity == DecisionSeverity.Critical) * 5f;  // Was 10f
        score -= badDecisions.Count(d => d.severity == DecisionSeverity.High) * 3f;       // Was 5f
        score -= badDecisions.Count(d => d.severity == DecisionSeverity.Medium) * 1f;     // Was 2f
        score -= badDecisions.Count(d => d.severity == DecisionSeverity.Low) * 0.5f;      // Was 1f

        // Bonus for good decisions (increased bonuses)
        score += goodDecisions.Count(d => d.severity == DecisionSeverity.Critical) * 5f;  // New
        score += goodDecisions.Count(d => d.severity == DecisionSeverity.High) * 3f;       // New
        score += goodDecisions.Count(d => d.severity == DecisionSeverity.Medium) * 2f;     // Was 1f
        score += goodDecisions.Count(d => d.severity == DecisionSeverity.Low) * 1f;        // New

        // Bonus for diversity (increased)
        if (currentSessionSpecies.Count >= minSpeciesDiversity)
        {
            score += 10f;  // Increased from 5f
        }

        // Additional bonus for overall diversity
        if (catchesPerSpecies.Count >= 3)
        {
            score += 5f;  // New bonus for long-term diversity
        }

        return Mathf.Clamp(score, 0f, 100f);
    }

    /// <summary>
    /// Get all metrics for display
    /// </summary>
    public SustainabilityReport GetReport()
    {
        return new SustainabilityReport
        {
            totalCatches = totalCatches,
            totalSessions = totalSessions,
            totalPlayTime = totalPlayTime + (Time.time - sessionStartTime),
            speciesDiversity = catchesPerSpecies.Count,
            sustainabilityScore = GetSustainabilityScore(),
            goodDecisions = new List<PlayerDecision>(goodDecisions),
            badDecisions = new List<PlayerDecision>(badDecisions),
            catchesPerSpecies = new Dictionary<string, int>(catchesPerSpecies),
            endangeredSpeciesCaught = catchHistory.Values.SelectMany(r => r).Count(r => r.isEndangered),
            overfishedSpecies = catchesPerSpecies.Where(p => p.Value > maxCatchesPerSpecies).Select(p => p.Key).ToList()
        };
    }

    /// <summary>
    /// Save metrics to PlayerPrefs
    /// </summary>
    public void SaveMetrics()
    {
        PlayerPrefs.SetInt("TotalCatches", totalCatches);
        PlayerPrefs.SetInt("TotalSessions", totalSessions);
        PlayerPrefs.SetFloat("TotalPlayTime", totalPlayTime);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load metrics from PlayerPrefs
    /// </summary>
    private void LoadMetrics()
    {
        totalCatches = PlayerPrefs.GetInt("TotalCatches", 0);
        totalSessions = PlayerPrefs.GetInt("TotalSessions", 0);
        totalPlayTime = PlayerPrefs.GetFloat("TotalPlayTime", 0f);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            totalPlayTime += Time.time - sessionStartTime;
            SaveMetrics();
        }
    }

    void OnApplicationQuit()
    {
        totalPlayTime += Time.time - sessionStartTime;
        SaveMetrics();
    }

    /// <summary>
    /// Reset all metrics for new game
    /// </summary>
    public void ResetForNewGame()
    {
        catchesPerSpecies.Clear();
        catchHistory.Clear();
        goodDecisions.Clear();
        badDecisions.Clear();
        totalCatches = 0;
        totalSessions = 0;
        totalPlayTime = 0f;
        currentSessionCatches = 0;
        currentSessionSpecies.Clear();
        sessionStartTime = Time.time;

        // Clear PlayerPrefs
        PlayerPrefs.DeleteKey("TotalCatches");
        PlayerPrefs.DeleteKey("TotalSessions");
        PlayerPrefs.DeleteKey("TotalPlayTime");
        PlayerPrefs.Save();

        Debug.Log("SustainableFishingMetrics reset for new game");
    }
}

/// <summary>
/// Record of a fish catch for sustainability tracking
/// </summary>
[System.Serializable]
public class SustainabilityCatchRecord
{
    public string fishId;
    public string fishName;
    public FishCatchSource source;
    public float timestamp;
    public bool isEndangered;
}

/// <summary>
/// Record of a player decision
/// </summary>
[System.Serializable]
public class PlayerDecision
{
    public string description;
    public string explanation;
    public float timestamp;
    public DecisionSeverity severity;
}

/// <summary>
/// Severity of a decision
/// </summary>
public enum DecisionSeverity
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Comprehensive sustainability report
/// </summary>
[System.Serializable]
public class SustainabilityReport
{
    public int totalCatches;
    public int totalSessions;
    public float totalPlayTime;
    public int speciesDiversity;
    public float sustainabilityScore;
    public List<PlayerDecision> goodDecisions;
    public List<PlayerDecision> badDecisions;
    public Dictionary<string, int> catchesPerSpecies;
    public int endangeredSpeciesCaught;
    public List<string> overfishedSpecies;
}

