using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Persists achievement analytics data (sustainability + effectiveness reports)
/// to disk so progress survives restarts.
/// </summary>
public class AchievementsDataStore : MonoBehaviour
{
    public static AchievementsDataStore Instance { get; private set; }

    [Header("Persistence Settings")]
    [SerializeField] private string saveFileName = "achievements_analytics.json";
    [SerializeField] private bool savePrettyJson = true;

    private AchievementsAnalyticsData cachedData;

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFromDisk();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool HasSavedData => cachedData != null;

    public SustainabilityReport GetSavedSustainabilityReport()
    {
        return cachedData?.sustainability?.ToReport();
    }

    public EducationalEffectivenessReport GetSavedEffectivenessReport()
    {
        return cachedData?.effectiveness?.ToReport();
    }

    /// <summary>
    /// Get the list of unlocked achievement IDs loaded from disk.
    /// Returns an empty list if there is no saved data.
    /// </summary>
    public List<string> GetUnlockedAchievementIds()
    {
        if (cachedData?.unlockedAchievementIds == null)
        {
            return new List<string>();
        }

        // Return a copy so callers can't accidentally mutate our cached data
        return new List<string>(cachedData.unlockedAchievementIds);
    }

    /// <summary>
    /// Persist the list of unlocked achievement IDs into the JSON file.
    /// This lets achievements be reset by deleting or replacing the JSON,
    /// instead of relying on PlayerPrefs only.
    /// </summary>
    public void SetUnlockedAchievementIds(List<string> ids)
    {
        if (cachedData == null)
        {
            cachedData = new AchievementsAnalyticsData();
        }

        cachedData.lastUpdatedUtc = System.DateTime.UtcNow.ToString("o");
        cachedData.unlockedAchievementIds = ids ?? new List<string>();

        try
        {
            string json = JsonUtility.ToJson(cachedData, savePrettyJson);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"AchievementsDataStore: Saved unlocked achievements to {SaveFilePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"AchievementsDataStore: Failed to save unlocked achievements. {ex.Message}");
        }
    }

    /// <summary>
    /// Get saved unified analytics report
    /// </summary>
    public UnifiedAnalyticsReport GetSavedUnifiedAnalyticsReport()
    {
        if (cachedData?.unifiedAnalytics == null)
            return null;

        // Reconstruct UnifiedAnalyticsReport from snapshot
        var report = new UnifiedAnalyticsReport
        {
            deboningData = cachedData.unifiedAnalytics.deboningData?.ToMinigameAnalytics(),
            saltwaterData = cachedData.unifiedAnalytics.saltwaterData?.ToMinigameAnalytics(),
            freshwaterData = cachedData.unifiedAnalytics.freshwaterData?.ToMinigameAnalytics(),
            overallPerformance = cachedData.unifiedAnalytics.overallPerformance,
            didGoodJob = cachedData.unifiedAnalytics.didGoodJob,
            recommendations = cachedData.unifiedAnalytics.recommendations != null
                ? new List<string>(cachedData.unifiedAnalytics.recommendations)
                : new List<string>()
        };

        // Add sustainability report if available
        if (cachedData.sustainability != null)
        {
            report.sustainabilityReport = cachedData.sustainability.ToReport();
        }

        return report;
    }

    /// <summary>
    /// Get saved performance evaluation
    /// </summary>
    public PlayerPerformanceEvaluation GetSavedPerformanceEvaluation()
    {
        if (cachedData?.performanceEvaluation == null)
            return null;

        return new PlayerPerformanceEvaluation
        {
            overallScore = cachedData.performanceEvaluation.overallScore,
            sustainabilityScore = cachedData.performanceEvaluation.sustainabilityScore,
            skillScore = cachedData.performanceEvaluation.skillScore,
            decisionScore = cachedData.performanceEvaluation.decisionScore,
            diversityScore = cachedData.performanceEvaluation.diversityScore,
            didGoodJob = cachedData.performanceEvaluation.didGoodJob,
            grade = cachedData.performanceEvaluation.grade,
            feedback = cachedData.performanceEvaluation.feedback,
            strengths = cachedData.performanceEvaluation.strengths != null
                ? new List<string>(cachedData.performanceEvaluation.strengths)
                : new List<string>(),
            weaknesses = cachedData.performanceEvaluation.weaknesses != null
                ? new List<string>(cachedData.performanceEvaluation.weaknesses)
                : new List<string>(),
            improvementAreas = cachedData.performanceEvaluation.improvementAreas != null
                ? new List<string>(cachedData.performanceEvaluation.improvementAreas)
                : new List<string>()
        };
    }

    /// <summary>
    /// Get saved economy snapshot
    /// </summary>
    public EconomySnapshot GetSavedEconomySnapshot()
    {
        return cachedData?.economy;
    }

    /// <summary>
    /// Get the file path where data is saved (for user reference)
    /// </summary>
    public string GetSaveFilePath()
    {
        return SaveFilePath;
    }

    /// <summary>
    /// Completely delete the analytics save file and clear any cached data.
    /// Use this when starting a brand new game so that no old analytics or
    /// achievement progress leaks into the new profile.
    /// </summary>
    public void DeleteAllSavedAnalytics()
    {
        cachedData = null;

        try
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log($"AchievementsDataStore: Deleted analytics save file at {SaveFilePath}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"AchievementsDataStore: Failed to delete analytics save file. {ex.Message}");
        }
    }

    /// <summary>
    /// Force save all current data (useful for manual saves or periodic backups)
    /// </summary>
    public void ForceSaveAllData()
    {
        SustainabilityReport sustainabilityReport = null;
        if (SustainableFishingMetrics.Instance != null)
        {
            sustainabilityReport = SustainableFishingMetrics.Instance.GetReport();
        }
        else if (cachedData != null)
        {
            sustainabilityReport = cachedData.sustainability?.ToReport();
        }

        EducationalEffectivenessReport effectivenessReport = null;
        if (EducationalEffectivenessCalculator.Instance != null && sustainabilityReport != null)
        {
            effectivenessReport = EducationalEffectivenessCalculator.Instance.CalculateEffectiveness(sustainabilityReport);
        }
        else if (cachedData != null)
        {
            effectivenessReport = cachedData.effectiveness?.ToReport();
        }

        UnifiedAnalyticsReport unifiedReport = null;
        if (UnifiedAnalyticsManager.Instance != null)
        {
            unifiedReport = UnifiedAnalyticsManager.Instance.GetComprehensiveReport();
        }

        PlayerPerformanceEvaluation performanceEvaluation = null;
        if (PlayerPerformanceEvaluator.Instance != null)
        {
            performanceEvaluation = PlayerPerformanceEvaluator.Instance.EvaluatePlayer();
        }

        EconomySnapshot economySnapshot = null;
        if (EconomyManager.Instance != null)
        {
            economySnapshot = EconomySnapshot.FromEconomyManager(EconomyManager.Instance);
        }

        SaveSnapshot(sustainabilityReport, effectivenessReport, unifiedReport, performanceEvaluation, economySnapshot);
    }

    void OnApplicationQuit()
    {
        // Save all data when application quits
        ForceSaveAllData();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Save when app is paused (mobile platforms)
        if (pauseStatus)
        {
            ForceSaveAllData();
        }
    }

    /// <summary>
    /// Save the latest reports to disk for future sessions.
    /// Now saves ALL data: sustainability, effectiveness, unified analytics, performance evaluation, and economy.
    /// </summary>
    public void SaveSnapshot(
        SustainabilityReport report,
        EducationalEffectivenessReport effectiveness,
        UnifiedAnalyticsReport providedUnifiedReport = null,
        PlayerPerformanceEvaluation providedPerformanceEvaluation = null,
        EconomySnapshot providedEconomySnapshot = null)
    {
        // Use provided data if available, otherwise collect all available data
        UnifiedAnalyticsReport unifiedReport = providedUnifiedReport;
        if (unifiedReport == null && UnifiedAnalyticsManager.Instance != null)
        {
            unifiedReport = UnifiedAnalyticsManager.Instance.GetComprehensiveReport();
        }

        PlayerPerformanceEvaluation performanceEvaluation = providedPerformanceEvaluation;
        if (performanceEvaluation == null && PlayerPerformanceEvaluator.Instance != null)
        {
            performanceEvaluation = PlayerPerformanceEvaluator.Instance.EvaluatePlayer();
        }

        EconomySnapshot economySnapshot = providedEconomySnapshot;
        if (economySnapshot == null && EconomyManager.Instance != null)
        {
            economySnapshot = EconomySnapshot.FromEconomyManager(EconomyManager.Instance);
        }

        bool hasIncomingData =
            HasMeaningfulSustainability(report) ||
            HasMeaningfulEffectiveness(effectiveness) ||
            HasMeaningfulUnifiedAnalytics(unifiedReport) ||
            HasMeaningfulPerformanceEvaluation(performanceEvaluation) ||
            HasMeaningfulEconomy(economySnapshot);

        bool hasExistingData =
            (cachedData?.sustainability != null) ||
            (cachedData?.effectiveness != null) ||
            (cachedData?.unifiedAnalytics != null) ||
            (cachedData?.performanceEvaluation != null) ||
            (cachedData?.economy != null);

        if (!hasIncomingData && !hasExistingData)
        {
            Debug.Log("AchievementsDataStore: No data to save yet. Skipping write to avoid blank file.");
            return;
        }

        var sustainabilitySnapshot = HasMeaningfulSustainability(report)
            ? SustainabilitySnapshot.FromReport(report)
            : cachedData?.sustainability;

        var effectivenessSnapshot = HasMeaningfulEffectiveness(effectiveness)
            ? EffectivenessSnapshot.FromReport(effectiveness)
            : cachedData?.effectiveness;

        var unifiedSnapshot = HasMeaningfulUnifiedAnalytics(unifiedReport)
            ? UnifiedAnalyticsSnapshot.FromReport(unifiedReport)
            : cachedData?.unifiedAnalytics;

        var performanceSnapshot = HasMeaningfulPerformanceEvaluation(performanceEvaluation)
            ? PerformanceEvaluationSnapshot.FromEvaluation(performanceEvaluation)
            : cachedData?.performanceEvaluation;

        var economySnapshotFinal = HasMeaningfulEconomy(economySnapshot)
            ? economySnapshot
            : cachedData?.economy;

        var unlockedAchievements = AchievementSystem.Instance != null
            ? AchievementSystem.Instance.GetUnlockedAchievementIds()
            : (cachedData?.unlockedAchievementIds ?? new List<string>());

        cachedData = new AchievementsAnalyticsData
        {
            lastUpdatedUtc = System.DateTime.UtcNow.ToString("o"),
            sustainability = sustainabilitySnapshot,
            effectiveness = effectivenessSnapshot,
            unifiedAnalytics = unifiedSnapshot,
            performanceEvaluation = performanceSnapshot,
            economy = economySnapshotFinal,
            unlockedAchievementIds = unlockedAchievements
        };

        string json = JsonUtility.ToJson(cachedData, savePrettyJson);
        try
        {
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"AchievementsDataStore: Successfully saved comprehensive analytics data to {SaveFilePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"AchievementsDataStore: Failed to save analytics data. {ex.Message}");
        }
    }

    private bool HasMeaningfulSustainability(SustainabilityReport report)
    {
        if (report == null) return false;

        bool hasCatchData = report.totalCatches > 0 || report.speciesDiversity > 0;
        bool hasDecisionData = (report.goodDecisions != null && report.goodDecisions.Count > 0) ||
                               (report.badDecisions != null && report.badDecisions.Count > 0);
        bool hasSpeciesData = report.catchesPerSpecies != null && report.catchesPerSpecies.Count > 0;
        bool hasOverfishData = report.overfishedSpecies != null && report.overfishedSpecies.Count > 0;

        return hasCatchData || hasDecisionData || hasSpeciesData || hasOverfishData;
    }

    private bool HasMeaningfulEffectiveness(EducationalEffectivenessReport report)
    {
        if (report == null) return false;
        return report.knowledgeAcquisitionScore > 0f ||
               report.behavioralChangeScore > 0f ||
               report.engagementScore > 0f ||
               report.overallEffectivenessScore > 0f;
    }

    private bool HasMeaningfulUnifiedAnalytics(UnifiedAnalyticsReport report)
    {
        if (report == null) return false;
        return (report.deboningData != null && report.deboningData.gamesPlayed > 0) ||
               (report.saltwaterData != null && report.saltwaterData.totalCatches > 0) ||
               (report.freshwaterData != null && report.freshwaterData.gamesPlayed > 0) ||
               report.overallPerformance > 0f;
    }

    private bool HasMeaningfulPerformanceEvaluation(PlayerPerformanceEvaluation evaluation)
    {
        if (evaluation == null) return false;
        if (evaluation.overallScore > 0f ||
            evaluation.skillScore > 0f ||
            evaluation.decisionScore > 0f ||
            evaluation.diversityScore > 0f)
        {
            return true;
        }

        return (evaluation.strengths != null && evaluation.strengths.Count > 0) ||
               (evaluation.weaknesses != null && evaluation.weaknesses.Count > 0) ||
               (evaluation.improvementAreas != null && evaluation.improvementAreas.Count > 0);
    }

    private bool HasMeaningfulEconomy(EconomySnapshot snapshot)
    {
        if (snapshot == null) return false;
        return snapshot.walletBalance != 0f ||
               snapshot.outstandingDebt != snapshot.startingDebt ||
               snapshot.totalEarned > 0f ||
               snapshot.totalDebtPaid > 0f ||
               snapshot.recentTransactions != null && snapshot.recentTransactions.Count > 0;
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.Log($"AchievementsDataStore: No saved data file found at {SaveFilePath}. Starting fresh.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("AchievementsDataStore: Save file is empty.");
                return;
            }

            cachedData = JsonUtility.FromJson<AchievementsAnalyticsData>(json);
            
            if (cachedData != null)
            {
                Debug.Log($"AchievementsDataStore: Successfully loaded analytics data from {SaveFilePath}");
                Debug.Log($"  - Last updated: {cachedData.lastUpdatedUtc}");
                Debug.Log($"  - Has sustainability data: {cachedData.sustainability != null}");
                Debug.Log($"  - Has effectiveness data: {cachedData.effectiveness != null}");
                Debug.Log($"  - Has unified analytics: {cachedData.unifiedAnalytics != null}");
                Debug.Log($"  - Has performance evaluation: {cachedData.performanceEvaluation != null}");
                Debug.Log($"  - Has economy data: {cachedData.economy != null}");
                Debug.Log($"  - Unlocked achievements: {cachedData.unlockedAchievementIds?.Count ?? 0}");
            }
            else
            {
                Debug.LogWarning("AchievementsDataStore: Loaded data is null.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"AchievementsDataStore: Failed to load analytics data. {ex.Message}");
            Debug.LogException(ex);
        }
    }
}

#region Serializable Data Structures

[System.Serializable]
public class AchievementsAnalyticsData
{
    public string lastUpdatedUtc;
    public SustainabilitySnapshot sustainability;
    public EffectivenessSnapshot effectiveness;
    public UnifiedAnalyticsSnapshot unifiedAnalytics;
    public PerformanceEvaluationSnapshot performanceEvaluation;
    public EconomySnapshot economy;
    public List<string> unlockedAchievementIds = new List<string>();
}

[System.Serializable]
public class SustainabilitySnapshot
{
    public int totalCatches;
    public int totalSessions;
    public float totalPlayTime;
    public int speciesDiversity;
    public float sustainabilityScore;
    public List<SerializableDecision> goodDecisions = new List<SerializableDecision>();
    public List<SerializableDecision> badDecisions = new List<SerializableDecision>();
    public List<SerializableCatchEntry> catchesPerSpecies = new List<SerializableCatchEntry>();
    public int endangeredSpeciesCaught;
    public List<string> overfishedSpecies = new List<string>();

    public static SustainabilitySnapshot FromReport(SustainabilityReport report)
    {
        if (report == null)
            return null;

        var snapshot = new SustainabilitySnapshot
        {
            totalCatches = report.totalCatches,
            totalSessions = report.totalSessions,
            totalPlayTime = report.totalPlayTime,
            speciesDiversity = report.speciesDiversity,
            sustainabilityScore = report.sustainabilityScore,
            endangeredSpeciesCaught = report.endangeredSpeciesCaught,
            overfishedSpecies = report.overfishedSpecies != null
                ? new List<string>(report.overfishedSpecies)
                : new List<string>()
        };

        if (report.goodDecisions != null)
        {
            snapshot.goodDecisions = report.goodDecisions
                .Select(SerializableDecision.FromDecision)
                .ToList();
        }

        if (report.badDecisions != null)
        {
            snapshot.badDecisions = report.badDecisions
                .Select(SerializableDecision.FromDecision)
                .ToList();
        }

        if (report.catchesPerSpecies != null)
        {
            snapshot.catchesPerSpecies = report.catchesPerSpecies
                .Select(pair => new SerializableCatchEntry { fishId = pair.Key, count = pair.Value })
                .ToList();
        }

        return snapshot;
    }

    public SustainabilityReport ToReport()
    {
        var report = new SustainabilityReport
        {
            totalCatches = totalCatches,
            totalSessions = totalSessions,
            totalPlayTime = totalPlayTime,
            speciesDiversity = speciesDiversity,
            sustainabilityScore = sustainabilityScore,
            endangeredSpeciesCaught = endangeredSpeciesCaught,
            overfishedSpecies = overfishedSpecies != null ? new List<string>(overfishedSpecies) : new List<string>(),
            goodDecisions = goodDecisions != null
                ? goodDecisions.Select(sd => sd.ToPlayerDecision()).ToList()
                : new List<PlayerDecision>(),
            badDecisions = badDecisions != null
                ? badDecisions.Select(sd => sd.ToPlayerDecision()).ToList()
                : new List<PlayerDecision>(),
            catchesPerSpecies = catchesPerSpecies != null
                ? catchesPerSpecies.ToDictionary(entry => entry.fishId, entry => entry.count)
                : new Dictionary<string, int>()
        };

        return report;
    }
}

[System.Serializable]
public class SerializableDecision
{
    public string description;
    public string explanation;
    public float timestamp;
    public DecisionSeverity severity;

    public static SerializableDecision FromDecision(PlayerDecision decision)
    {
        if (decision == null) return null;
        return new SerializableDecision
        {
            description = decision.description,
            explanation = decision.explanation,
            timestamp = decision.timestamp,
            severity = decision.severity
        };
    }

    public PlayerDecision ToPlayerDecision()
    {
        return new PlayerDecision
        {
            description = description,
            explanation = explanation,
            timestamp = timestamp,
            severity = severity
        };
    }
}

[System.Serializable]
public class SerializableCatchEntry
{
    public string fishId;
    public int count;
}

[System.Serializable]
public class EffectivenessSnapshot
{
    public float knowledgeScore;
    public float behavioralScore;
    public float engagementScore;
    public float overallScore;
    public LearningIndicators learningIndicators = new LearningIndicators();
    public List<string> recommendations = new List<string>();

    public static EffectivenessSnapshot FromReport(EducationalEffectivenessReport report)
    {
        if (report == null)
            return null;

        return new EffectivenessSnapshot
        {
            knowledgeScore = report.knowledgeAcquisitionScore,
            behavioralScore = report.behavioralChangeScore,
            engagementScore = report.engagementScore,
            overallScore = report.overallEffectivenessScore,
            learningIndicators = report.learningIndicators ?? new LearningIndicators(),
            recommendations = report.recommendations != null
                ? new List<string>(report.recommendations)
                : new List<string>()
        };
    }

    public EducationalEffectivenessReport ToReport()
    {
        return new EducationalEffectivenessReport
        {
            knowledgeAcquisitionScore = knowledgeScore,
            behavioralChangeScore = behavioralScore,
            engagementScore = engagementScore,
            overallEffectivenessScore = overallScore,
            learningIndicators = learningIndicators ?? new LearningIndicators(),
            recommendations = recommendations != null ? new List<string>(recommendations) : new List<string>()
        };
    }
}

/// <summary>
/// Serializable snapshot of UnifiedAnalyticsReport
/// </summary>
[System.Serializable]
public class UnifiedAnalyticsSnapshot
{
    public MinigameAnalyticsSnapshot deboningData;
    public MinigameAnalyticsSnapshot saltwaterData;
    public MinigameAnalyticsSnapshot freshwaterData;
    public float overallPerformance;
    public bool didGoodJob;
    public List<string> recommendations = new List<string>();

    public static UnifiedAnalyticsSnapshot FromReport(UnifiedAnalyticsReport report)
    {
        if (report == null)
            return null;

        return new UnifiedAnalyticsSnapshot
        {
            deboningData = MinigameAnalyticsSnapshot.FromAnalytics(report.deboningData),
            saltwaterData = MinigameAnalyticsSnapshot.FromAnalytics(report.saltwaterData),
            freshwaterData = MinigameAnalyticsSnapshot.FromAnalytics(report.freshwaterData),
            overallPerformance = report.overallPerformance,
            didGoodJob = report.didGoodJob,
            recommendations = report.recommendations != null ? new List<string>(report.recommendations) : new List<string>()
        };
    }

    public UnifiedAnalyticsReport ToReport()
    {
        return new UnifiedAnalyticsReport
        {
            deboningData = deboningData?.ToMinigameAnalytics(),
            saltwaterData = saltwaterData?.ToMinigameAnalytics(),
            freshwaterData = freshwaterData?.ToMinigameAnalytics(),
               sustainabilityReport = null,
            overallPerformance = overallPerformance,
            didGoodJob = didGoodJob,
            recommendations = recommendations != null ? new System.Collections.Generic.List<string>(recommendations) : new System.Collections.Generic.List<string>()
        };
    }
}

/// <summary>
/// Serializable snapshot of MinigameAnalytics
/// </summary>
[System.Serializable]
public class MinigameAnalyticsSnapshot
{
    public string minigameName;
    public int minigameType; // enum as int
    public int totalScore;
    public int gamesPlayed;
    public int highestScore;
    public int totalCatches;
    public int uniqueFishCount;
    public int totalMistakes;
    public float performanceScore;
    public List<SerializableFishStatData> fishStatistics = new List<SerializableFishStatData>();

    public static MinigameAnalyticsSnapshot FromAnalytics(MinigameAnalytics analytics)
    {
        if (analytics == null)
            return null;

        var snapshot = new MinigameAnalyticsSnapshot
        {
            minigameName = analytics.minigameName,
            minigameType = (int)analytics.minigameType,
            totalScore = analytics.totalScore,
            gamesPlayed = analytics.gamesPlayed,
            highestScore = analytics.highestScore,
            totalCatches = analytics.totalCatches,
            uniqueFishCount = analytics.uniqueFishCount,
            totalMistakes = analytics.totalMistakes,
            performanceScore = analytics.performanceScore
        };

        if (analytics.fishStatistics != null)
        {
            snapshot.fishStatistics = analytics.fishStatistics
                .Select(kvp => SerializableFishStatData.FromData(kvp.Key, kvp.Value))
                .ToList();
        }

        return snapshot;
    }

    public MinigameAnalytics ToMinigameAnalytics()
    {
        var analytics = new MinigameAnalytics
        {
            minigameName = minigameName,
            minigameType = (MinigameType)minigameType,
            totalScore = totalScore,
            gamesPlayed = gamesPlayed,
            highestScore = highestScore,
            totalCatches = totalCatches,
            uniqueFishCount = uniqueFishCount,
            totalMistakes = totalMistakes,
            performanceScore = performanceScore,
            fishStatistics = new Dictionary<string, FishStatData>()
        };

        if (fishStatistics != null)
        {
            foreach (var stat in fishStatistics)
            {
                if (stat != null && !string.IsNullOrEmpty(stat.fishName))
                {
                    analytics.fishStatistics[stat.fishName] = new FishStatData
                    {
                        fishName = stat.fishName,
                        timesPlayed = stat.timesPlayed,
                        bestScore = stat.bestScore,
                        averageMistakes = stat.averageMistakes,
                        averageCatchTime = stat.averageCatchTime,
                        totalMistakes = stat.totalMistakes,
                        totalExperience = stat.totalExperience,
                        totalGold = stat.totalGold
                    };
                }
            }
        }

        return analytics;
    }
}

/// <summary>
/// Serializable wrapper for FishStatData
/// </summary>
[System.Serializable]
public class SerializableFishStatData
{
    public string fishName;
    public int timesPlayed;
    public int bestScore;
    public float averageMistakes;
    public float averageCatchTime;
    public int totalMistakes;
    public int totalExperience;
    public int totalGold;

    public static SerializableFishStatData FromData(string fishName, FishStatData data)
    {
        if (data == null)
            return null;

        return new SerializableFishStatData
        {
            fishName = fishName,
            timesPlayed = data.timesPlayed,
            bestScore = data.bestScore,
            averageMistakes = data.averageMistakes,
            averageCatchTime = data.averageCatchTime,
            totalMistakes = data.totalMistakes,
            totalExperience = data.totalExperience,
            totalGold = data.totalGold
        };
    }
}

/// <summary>
/// Serializable snapshot of PlayerPerformanceEvaluation
/// </summary>
[System.Serializable]
public class PerformanceEvaluationSnapshot
{
    public float overallScore;
    public float sustainabilityScore;
    public float skillScore;
    public float decisionScore;
    public float diversityScore;
    public bool didGoodJob;
    public string grade;
    public string feedback;
    public List<string> strengths = new List<string>();
    public List<string> weaknesses = new List<string>();
    public List<string> improvementAreas = new List<string>();

    public static PerformanceEvaluationSnapshot FromEvaluation(PlayerPerformanceEvaluation evaluation)
    {
        if (evaluation == null)
            return null;

        return new PerformanceEvaluationSnapshot
        {
            overallScore = evaluation.overallScore,
            sustainabilityScore = evaluation.sustainabilityScore,
            skillScore = evaluation.skillScore,
            decisionScore = evaluation.decisionScore,
            diversityScore = evaluation.diversityScore,
            didGoodJob = evaluation.didGoodJob,
            grade = evaluation.grade,
            feedback = evaluation.feedback,
            strengths = evaluation.strengths != null ? new List<string>(evaluation.strengths) : new List<string>(),
            weaknesses = evaluation.weaknesses != null ? new List<string>(evaluation.weaknesses) : new List<string>(),
            improvementAreas = evaluation.improvementAreas != null ? new List<string>(evaluation.improvementAreas) : new List<string>()
        };
    }

    public PlayerPerformanceEvaluation ToEvaluation()
    {
        return new PlayerPerformanceEvaluation
        {
            overallScore = overallScore,
            sustainabilityScore = sustainabilityScore,
            skillScore = skillScore,
            decisionScore = decisionScore,
            diversityScore = diversityScore,
            didGoodJob = didGoodJob,
            grade = grade,
            feedback = feedback,
            strengths = strengths != null ? new System.Collections.Generic.List<string>(strengths) : new System.Collections.Generic.List<string>(),
            weaknesses = weaknesses != null ? new System.Collections.Generic.List<string>(weaknesses) : new System.Collections.Generic.List<string>(),
            improvementAreas = improvementAreas != null ? new System.Collections.Generic.List<string>(improvementAreas) : new System.Collections.Generic.List<string>()
        };
    }
}

/// <summary>
/// Serializable snapshot of EconomyManager state
/// </summary>
[System.Serializable]
public class EconomySnapshot
{
    public float walletBalance;
    public float outstandingDebt;
    public float totalEarned;
    public float totalDebtPaid;
    public float debtProgress;
    public int startingDebt;
    public int startingWallet;
    public List<SerializableEconomyTransaction> recentTransactions = new List<SerializableEconomyTransaction>();

    public static EconomySnapshot FromEconomyManager(EconomyManager manager)
    {
        if (manager == null)
            return null;

        var snapshot = new EconomySnapshot
        {
            walletBalance = manager.CurrentWallet,
            outstandingDebt = manager.CurrentDebt,
            totalEarned = manager.TotalEarned,
            totalDebtPaid = manager.TotalDebtPaid,
            debtProgress = manager.DebtProgress,
            startingDebt = manager.startingDebt,
            startingWallet = manager.startingWallet
        };

        // Note: EconomyManager's recentTransactions is private, so we'll get what we can from public API
        // If you need transaction history, you may need to add a public getter to EconomyManager

        return snapshot;
    }
}

/// <summary>
/// Serializable wrapper for EconomyTransaction
/// </summary>
[System.Serializable]
public class SerializableEconomyTransaction
{
    public string timestamp;
    public string description;
    public float amount;
    public float debtApplied;
    public string fishId;
    public int source; // enum as int
    public bool endangeredSale;
    public bool processedSale;
    public float balanceAfter;
    public float debtAfter;

    public static SerializableEconomyTransaction FromTransaction(EconomyTransaction transaction)
    {
        return new SerializableEconomyTransaction
        {
            timestamp = transaction.timestamp,
            description = transaction.description,
            amount = transaction.amount,
            debtApplied = transaction.debtApplied,
            fishId = transaction.fishId,
            source = (int)transaction.source,
            endangeredSale = transaction.endangeredSale,
            processedSale = transaction.processedSale,
            balanceAfter = transaction.balanceAfter,
            debtAfter = transaction.debtAfter
        };
    }
}

#endregion

