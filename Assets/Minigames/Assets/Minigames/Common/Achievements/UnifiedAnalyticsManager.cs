using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Unified analytics manager that aggregates data from all three minigames:
/// 1. Deboning Minigame (ScoreManager)
/// 2. Saltwater Fishing (FishingMinigame)
/// 3. Freshwater Fishing (FreshwaterFishingScoreManager)
/// 
/// Provides comprehensive evaluation of player performance across all minigames.
/// </summary>
public class UnifiedAnalyticsManager : MonoBehaviour
{
    public static UnifiedAnalyticsManager Instance { get; private set; }

    [Header("Configuration")]
    [Tooltip("Minimum score threshold for 'Good Job' evaluation")]
    public float goodJobScoreThreshold = 70f;
    
    [Tooltip("Weight for deboning performance (0-1)")]
    [Range(0f, 1f)]
    public float deboningWeight = 0.33f;
    
    [Tooltip("Weight for saltwater fishing performance (0-1)")]
    [Range(0f, 1f)]
    public float saltwaterWeight = 0.33f;
    
    [Tooltip("Weight for freshwater fishing performance (0-1)")]
    [Range(0f, 1f)]
    public float freshwaterWeight = 0.34f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Get comprehensive analytics report across all minigames
    /// </summary>
    public UnifiedAnalyticsReport GetComprehensiveReport()
    {
        UnifiedAnalyticsReport report = new UnifiedAnalyticsReport();

        // Get data from each minigame
        report.deboningData = GetDeboningData();
        report.saltwaterData = GetSaltwaterData();
        report.freshwaterData = GetFreshwaterData();
        
        // Get sustainability metrics (shared across all)
        if (SustainableFishingMetrics.Instance != null)
        {
            report.sustainabilityReport = SustainableFishingMetrics.Instance.GetReport();
        }

        // Calculate overall performance
        report.overallPerformance = CalculateOverallPerformance(report);
        report.didGoodJob = report.overallPerformance >= goodJobScoreThreshold;

        // Generate recommendations
        report.recommendations = GenerateRecommendations(report);

        return report;
    }

    /// <summary>
    /// Get deboning minigame data
    /// </summary>
    private MinigameAnalytics GetDeboningData()
    {
        MinigameAnalytics data = new MinigameAnalytics
        {
            minigameName = "Deboning",
            minigameType = MinigameType.Deboning
        };

        if (ScoreManager.Instance != null)
        {
            data.totalScore = ScoreManager.Instance.GetTotalScore();
            data.gamesPlayed = ScoreManager.Instance.GetGamesPlayed();
            data.highestScore = ScoreManager.Instance.GetHighestSingleGameScore();
            data.totalMistakes = ScoreManager.Instance.GetTotalMistakes();
            data.uniqueFishCount = ScoreManager.Instance.GetUniqueFishCount();
            
            // Get fish statistics
            var fishStats = ScoreManager.Instance.GetAllFishStatistics();
            data.fishStatistics = new Dictionary<string, FishStatData>();
            foreach (var stat in fishStats.Values)
            {
                data.fishStatistics[stat.fishName] = new FishStatData
                {
                    fishName = stat.fishName,
                    timesPlayed = stat.timesDeboned,
                    bestScore = stat.bestScore,
                    averageMistakes = stat.averageMistakes,
                    totalMistakes = stat.totalMistakes
                };
            }

            // Calculate performance score (0-100)
            float avgScore = data.gamesPlayed > 0 ? (float)data.totalScore / data.gamesPlayed : 0f;
            float mistakePenalty = Mathf.Clamp01((float)data.totalMistakes / (data.gamesPlayed * 10f)) * 30f;
            data.performanceScore = Mathf.Clamp(100f - mistakePenalty + (avgScore / 100f), 0f, 100f);
        }

        return data;
    }

    /// <summary>
    /// Get saltwater fishing minigame data
    /// </summary>
    private MinigameAnalytics GetSaltwaterData()
    {
        MinigameAnalytics data = new MinigameAnalytics
        {
            minigameName = "Saltwater Fishing",
            minigameType = MinigameType.SaltwaterFishing
        };

        // Get data from FishInventoryManager (tracks all catches)
        if (FishInventoryManager.Instance != null)
        {
            var allTotals = FishInventoryManager.Instance.GetAllTotals();
            var saltwaterTotals = allTotals.Where(t => t.saltwaterCaught > 0).ToList();
            
            data.totalCatches = saltwaterTotals.Sum(t => t.saltwaterCaught);
            data.uniqueFishCount = saltwaterTotals.Count();
            
            // Group by fish
            data.fishStatistics = new Dictionary<string, FishStatData>();
            
            foreach (var totals in saltwaterTotals)
            {
                string fishName = totals.catalogEntry != null ? totals.catalogEntry.displayName : totals.fishId;
                data.fishStatistics[fishName] = new FishStatData
                {
                    fishName = fishName,
                    timesPlayed = totals.saltwaterCaught,
                    bestScore = 0, // Saltwater doesn't track scores the same way
                    averageMistakes = 0f,
                    totalMistakes = 0
                };
            }

            // Calculate performance score based on diversity and sustainability
            float diversityScore = Mathf.Min(data.uniqueFishCount * 10f, 50f);
            float sustainabilityBonus = 0f;
            
            if (SustainableFishingMetrics.Instance != null)
            {
                var report = SustainableFishingMetrics.Instance.GetReport();
                sustainabilityBonus = report.sustainabilityScore * 0.5f;
            }
            
            data.performanceScore = Mathf.Clamp(diversityScore + sustainabilityBonus, 0f, 100f);
        }

        return data;
    }

    /// <summary>
    /// Get freshwater fishing minigame data
    /// </summary>
    private MinigameAnalytics GetFreshwaterData()
    {
        MinigameAnalytics data = new MinigameAnalytics
        {
            minigameName = "Freshwater Fishing",
            minigameType = MinigameType.FreshwaterFishing
        };

        if (FreshwaterFishingScoreManager.Instance != null)
        {
            data.totalScore = FreshwaterFishingScoreManager.Instance.GetTotalScore();
            data.gamesPlayed = FreshwaterFishingScoreManager.Instance.GetGamesPlayed();
            data.highestScore = FreshwaterFishingScoreManager.Instance.GetHighestSingleGameScore();
            data.totalCatches = FreshwaterFishingScoreManager.Instance.GetTotalFishCaught();
            data.uniqueFishCount = FreshwaterFishingScoreManager.Instance.GetUniqueFishCount();
            
            // Get fish statistics
            var fishStats = FreshwaterFishingScoreManager.Instance.GetAllFishStatistics();
            data.fishStatistics = new Dictionary<string, FishStatData>();
            foreach (var stat in fishStats.Values)
            {
                data.fishStatistics[stat.fishName] = new FishStatData
                {
                    fishName = stat.fishName,
                    timesPlayed = stat.timesCaught,
                    bestScore = stat.bestScore,
                    averageCatchTime = stat.averageCatchTime,
                    totalExperience = stat.totalExperience,
                    totalGold = stat.totalGold
                };
            }

            // Calculate performance score
            float avgScore = data.gamesPlayed > 0 ? (float)data.totalScore / data.gamesPlayed : 0f;
            float diversityBonus = Mathf.Min(data.uniqueFishCount * 5f, 30f);
            data.performanceScore = Mathf.Clamp((avgScore / 50f) + diversityBonus, 0f, 100f);
        }

        return data;
    }

    /// <summary>
    /// Calculate overall performance across all minigames
    /// </summary>
    private float CalculateOverallPerformance(UnifiedAnalyticsReport report)
    {
        float deboningScore = report.deboningData.performanceScore * deboningWeight;
        float saltwaterScore = report.saltwaterData.performanceScore * saltwaterWeight;
        float freshwaterScore = report.freshwaterData.performanceScore * freshwaterWeight;

        float weightedScore = deboningScore + saltwaterScore + freshwaterScore;

        // Apply sustainability modifier
        if (report.sustainabilityReport != null)
        {
            float sustainabilityModifier = report.sustainabilityReport.sustainabilityScore / 100f;
            weightedScore = weightedScore * 0.7f + (weightedScore * sustainabilityModifier * 0.3f);
        }

        return Mathf.Clamp(weightedScore, 0f, 100f);
    }

    /// <summary>
    /// Generate recommendations based on performance
    /// </summary>
    private List<string> GenerateRecommendations(UnifiedAnalyticsReport report)
    {
        List<string> recommendations = new List<string>();

        // Overall performance
        if (report.overallPerformance < 50f)
        {
            recommendations.Add("Your overall performance needs improvement. Focus on sustainable fishing practices.");
        }
        else if (report.overallPerformance >= 90f)
        {
            recommendations.Add("Excellent work! You're demonstrating great sustainable fishing practices!");
        }

        // Minigame-specific recommendations
        if (report.deboningData.performanceScore < 60f)
        {
            recommendations.Add("Practice more in the deboning minigame to improve your skills and reduce mistakes.");
        }

        if (report.saltwaterData.uniqueFishCount < 3)
        {
            recommendations.Add("Try catching more diverse fish species in saltwater fishing to improve biodiversity.");
        }

        if (report.freshwaterData.performanceScore < 60f)
        {
            recommendations.Add("Work on improving your catch times and diversity in freshwater fishing.");
        }

        // Sustainability recommendations
        if (report.sustainabilityReport != null)
        {
            if (report.sustainabilityReport.overfishedSpecies.Count > 0)
            {
                recommendations.Add($"⚠️ You've overfished {report.sustainabilityReport.overfishedSpecies.Count} species. Try to diversify your catches.");
            }

            if (report.sustainabilityReport.endangeredSpeciesCaught > 0)
            {
                recommendations.Add("⚠️ You've caught endangered species. Please avoid catching protected fish.");
            }

            if (report.sustainabilityReport.speciesDiversity < 5)
            {
                recommendations.Add("Try to catch more different species to improve ecosystem diversity.");
            }
        }

        return recommendations;
    }

    /// <summary>
    /// Get performance breakdown by minigame
    /// </summary>
    public Dictionary<string, float> GetPerformanceBreakdown()
    {
        var report = GetComprehensiveReport();
        return new Dictionary<string, float>
        {
            { "Deboning", report.deboningData.performanceScore },
            { "Saltwater Fishing", report.saltwaterData.performanceScore },
            { "Freshwater Fishing", report.freshwaterData.performanceScore },
            { "Overall", report.overallPerformance }
        };
    }
}

/// <summary>
/// Comprehensive analytics report across all minigames
/// </summary>
[System.Serializable]
public class UnifiedAnalyticsReport
{
    public MinigameAnalytics deboningData;
    public MinigameAnalytics saltwaterData;
    public MinigameAnalytics freshwaterData;
    public SustainabilityReport sustainabilityReport;
    public float overallPerformance;
    public bool didGoodJob;
    public List<string> recommendations = new List<string>();
}

/// <summary>
/// Analytics data for a single minigame
/// </summary>
[System.Serializable]
public class MinigameAnalytics
{
    public string minigameName;
    public MinigameType minigameType;
    public int totalScore;
    public int gamesPlayed;
    public int highestScore;
    public int totalCatches;
    public int uniqueFishCount;
    public int totalMistakes;
    public float performanceScore; // 0-100
    public Dictionary<string, FishStatData> fishStatistics = new Dictionary<string, FishStatData>();
}

/// <summary>
/// Fish statistics data
/// </summary>
[System.Serializable]
public class FishStatData
{
    public string fishName;
    public int timesPlayed;
    public int bestScore;
    public float averageMistakes;
    public float averageCatchTime;
    public int totalMistakes;
    public int totalExperience;
    public int totalGold;
}

/// <summary>
/// Minigame types
/// </summary>
public enum MinigameType
{
    Deboning,
    SaltwaterFishing,
    FreshwaterFishing
}

