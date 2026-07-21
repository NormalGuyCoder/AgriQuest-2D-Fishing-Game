using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Calculates educational effectiveness metrics for evaluating learning outcomes.
/// Used for research and assessment of the game's educational value.
/// </summary>
public class EducationalEffectivenessCalculator : MonoBehaviour
{
    public static EducationalEffectivenessCalculator Instance { get; private set; }

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
    /// Calculate comprehensive educational effectiveness report
    /// </summary>
    public EducationalEffectivenessReport CalculateEffectiveness(SustainabilityReport sustainabilityReport)
    {
        if (sustainabilityReport == null)
        {
            return new EducationalEffectivenessReport();
        }

        EducationalEffectivenessReport report = new EducationalEffectivenessReport();

        // 1. Knowledge Acquisition Score (0-100)
        report.knowledgeAcquisitionScore = CalculateKnowledgeAcquisition(sustainabilityReport);

        // 2. Behavioral Change Score (0-100)
        report.behavioralChangeScore = CalculateBehavioralChange(sustainabilityReport);

        // 3. Engagement Score (0-100)
        report.engagementScore = CalculateEngagement(sustainabilityReport);

        // 4. Overall Effectiveness Score
        report.overallEffectivenessScore = (
            report.knowledgeAcquisitionScore * 0.4f +
            report.behavioralChangeScore * 0.4f +
            report.engagementScore * 0.2f
        );

        // 5. Learning Indicators
        report.learningIndicators = AnalyzeLearningIndicators(sustainabilityReport);

        // 6. Recommendations
        report.recommendations = GenerateRecommendations(sustainabilityReport, report);

        return report;
    }

    /// <summary>
    /// Calculate knowledge acquisition based on good decisions and learning activities
    /// </summary>
    private float CalculateKnowledgeAcquisition(SustainabilityReport report)
    {
        float score = 0f;
        float maxScore = 100f;

        // Learning activities (deboning, fish identification)
        int learningActivities = report.goodDecisions.Count(d => 
            d.description.Contains("Learned") || 
            d.description.Contains("anatomy") ||
            d.description.Contains("identification")
        );
        score += Mathf.Min(learningActivities * 10f, 40f); // Max 40 points

        // Understanding of sustainability concepts
        int sustainabilityDecisions = report.goodDecisions.Count(d =>
            d.description.Contains("Sustainable") ||
            d.description.Contains("diversity") ||
            d.description.Contains("conservation")
        );
        score += Mathf.Min(sustainabilityDecisions * 5f, 30f); // Max 30 points

        // Avoidance of bad practices
        int avoidedBadPractices = report.badDecisions.Count(d => d.severity == DecisionSeverity.Critical);
        score += Mathf.Max(0f, 30f - (avoidedBadPractices * 10f)); // Deduct for critical mistakes

        return Mathf.Clamp(score, 0f, maxScore);
    }

    /// <summary>
    /// Calculate behavioral change based on actual fishing practices
    /// </summary>
    private float CalculateBehavioralChange(SustainabilityReport report)
    {
        float score = 100f;

        // Deduct for overfishing
        score -= report.overfishedSpecies.Count * 15f;

        // Deduct for catching endangered species
        score -= report.endangeredSpeciesCaught * 20f;

        // Deduct for bad decisions
        score -= report.badDecisions.Count(d => d.severity == DecisionSeverity.High || d.severity == DecisionSeverity.Critical) * 5f;

        // Bonus for good decisions
        score += Mathf.Min(report.goodDecisions.Count * 2f, 20f);

        // Bonus for diversity (shows understanding of ecosystem balance)
        if (report.speciesDiversity >= 5)
        {
            score += 10f;
        }

        return Mathf.Clamp(score, 0f, 100f);
    }

    /// <summary>
    /// Calculate engagement based on play time and activity
    /// </summary>
    private float CalculateEngagement(SustainabilityReport report)
    {
        float score = 0f;

        // Play time (more time = more engaged)
        float hoursPlayed = report.totalPlayTime / 3600f;
        score += Mathf.Min(hoursPlayed * 20f, 40f); // Max 40 points for 2+ hours

        // Number of sessions (shows repeated engagement)
        score += Mathf.Min(report.totalSessions * 5f, 30f); // Max 30 points for 6+ sessions

        // Activity level (catches per session)
        if (report.totalSessions > 0)
        {
            float avgCatchesPerSession = (float)report.totalCatches / report.totalSessions;
            score += Mathf.Min(avgCatchesPerSession * 2f, 30f); // Max 30 points
        }

        return Mathf.Clamp(score, 0f, 100f);
    }

    /// <summary>
    /// Analyze learning indicators
    /// </summary>
    private LearningIndicators AnalyzeLearningIndicators(SustainabilityReport report)
    {
        LearningIndicators indicators = new LearningIndicators();

        // Positive indicators
        indicators.understandsCatchLimits = report.overfishedSpecies.Count == 0;
        indicators.avoidsEndangeredSpecies = report.endangeredSpeciesCaught == 0;
        indicators.practicesDiversity = report.speciesDiversity >= 5;
        indicators.makesSustainableChoices = report.goodDecisions.Count > report.badDecisions.Count;
        indicators.showsLearningProgression = report.goodDecisions.Count(d => d.description.Contains("Learned")) >= 3;

        // Negative indicators
        indicators.overfishesSpecies = report.overfishedSpecies.Count > 0;
        indicators.catchesEndangered = report.endangeredSpeciesCaught > 0;
        indicators.lacksDiversity = report.speciesDiversity < 3;
        indicators.makesPoorChoices = report.badDecisions.Count(d => d.severity == DecisionSeverity.Critical || d.severity == DecisionSeverity.High) > 2;

        // Engagement indicators
        indicators.highlyEngaged = report.totalPlayTime > 3600f; // More than 1 hour
        indicators.moderatelyEngaged = report.totalPlayTime > 1800f; // More than 30 minutes
        indicators.lowEngagement = report.totalPlayTime < 600f; // Less than 10 minutes

        return indicators;
    }

    /// <summary>
    /// Generate recommendations for improvement
    /// </summary>
    private List<string> GenerateRecommendations(SustainabilityReport report, EducationalEffectivenessReport effectivenessReport)
    {
        List<string> recommendations = new List<string>();

        if (effectivenessReport.knowledgeAcquisitionScore < 60f)
        {
            recommendations.Add("Focus on completing more learning activities (deboning, fish identification) to improve knowledge acquisition.");
        }

        if (effectivenessReport.behavioralChangeScore < 60f)
        {
            recommendations.Add("Practice sustainable fishing: avoid overfishing single species and never catch endangered fish.");
        }

        if (report.overfishedSpecies.Count > 0)
        {
            recommendations.Add($"Reduce catches of overfished species: {string.Join(", ", report.overfishedSpecies)}");
        }

        if (report.endangeredSpeciesCaught > 0)
        {
            recommendations.Add("Never catch endangered species. Learn to identify and avoid them.");
        }

        if (report.speciesDiversity < 5)
        {
            recommendations.Add("Increase species diversity in your catches to better understand ecosystem balance.");
        }

        if (report.badDecisions.Count > report.goodDecisions.Count)
        {
            recommendations.Add("Make more sustainable choices. Review your decisions and learn from mistakes.");
        }

        if (effectivenessReport.engagementScore < 50f)
        {
            recommendations.Add("Spend more time exploring the game to fully understand sustainable fishing practices.");
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add("Excellent work! Continue practicing sustainable fishing and sharing knowledge with others.");
        }

        return recommendations;
    }
}

/// <summary>
/// Comprehensive educational effectiveness report
/// </summary>
[System.Serializable]
public class EducationalEffectivenessReport
{
    [Header("Scores (0-100)")]
    public float knowledgeAcquisitionScore;
    public float behavioralChangeScore;
    public float engagementScore;
    public float overallEffectivenessScore;

    [Header("Learning Indicators")]
    public LearningIndicators learningIndicators;

    [Header("Recommendations")]
    public List<string> recommendations;
}

/// <summary>
/// Learning indicators for assessment
/// </summary>
[System.Serializable]
public class LearningIndicators
{
    // Positive indicators
    public bool understandsCatchLimits;
    public bool avoidsEndangeredSpecies;
    public bool practicesDiversity;
    public bool makesSustainableChoices;
    public bool showsLearningProgression;

    // Negative indicators
    public bool overfishesSpecies;
    public bool catchesEndangered;
    public bool lacksDiversity;
    public bool makesPoorChoices;

    // Engagement indicators
    public bool highlyEngaged;
    public bool moderatelyEngaged;
    public bool lowEngagement;
}

