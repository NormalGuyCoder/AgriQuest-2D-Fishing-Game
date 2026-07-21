using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Evaluates player performance across all minigames and determines if they did a "good job"
/// based on multiple criteria including sustainability, skill, and decision-making.
/// </summary>
public class PlayerPerformanceEvaluator : MonoBehaviour
{
    public static PlayerPerformanceEvaluator Instance { get; private set; }

    [Header("Evaluation Criteria")]
    [Tooltip("Minimum overall score to be considered 'Good Job'")]
    [Range(0f, 100f)]
    public float goodJobThreshold = 70f;
    
    [Tooltip("Minimum sustainability score required")]
    [Range(0f, 100f)]
    public float minSustainabilityScore = 60f;
    
    [Tooltip("Maximum allowed bad decisions")]
    public int maxBadDecisions = 5;
    
    [Tooltip("Minimum required good decisions")]
    public int minGoodDecisions = 10;

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
    /// Comprehensive evaluation of player performance
    /// </summary>
    public PlayerPerformanceEvaluation EvaluatePlayer()
    {
        PlayerPerformanceEvaluation evaluation = new PlayerPerformanceEvaluation();

        // Get unified analytics
        if (UnifiedAnalyticsManager.Instance == null)
        {
            Debug.LogError("UnifiedAnalyticsManager not found!");
            return evaluation;
        }

        var report = UnifiedAnalyticsManager.Instance.GetComprehensiveReport();

        // Evaluate each criterion
        evaluation.overallScore = report.overallPerformance;
        evaluation.sustainabilityScore = report.sustainabilityReport != null 
            ? report.sustainabilityReport.sustainabilityScore 
            : 0f;
        
        evaluation.skillScore = CalculateSkillScore(report);
        evaluation.decisionScore = CalculateDecisionScore(report);
        evaluation.diversityScore = CalculateDiversityScore(report);

        // Determine if player did good job
        evaluation.didGoodJob = EvaluateGoodJob(evaluation, report);

        // Generate detailed feedback
        evaluation.feedback = GenerateFeedback(evaluation, report);
        evaluation.strengths = IdentifyStrengths(report);
        evaluation.weaknesses = IdentifyWeaknesses(report);
        evaluation.improvementAreas = IdentifyImprovementAreas(report);

        // Calculate grade
        evaluation.grade = CalculateGrade(evaluation.overallScore);

        return evaluation;
    }

    /// <summary>
    /// Calculate skill score based on performance across minigames
    /// </summary>
    private float CalculateSkillScore(UnifiedAnalyticsReport report)
    {
        float deboningSkill = report.deboningData.performanceScore;
        float saltwaterSkill = report.saltwaterData.performanceScore;
        float freshwaterSkill = report.freshwaterData.performanceScore;

        // Average skill across all minigames
        float avgSkill = (deboningSkill + saltwaterSkill + freshwaterSkill) / 3f;

        // Bonus for consistency (all minigames above 60)
        float consistencyBonus = 0f;
        if (deboningSkill >= 60f && saltwaterSkill >= 60f && freshwaterSkill >= 60f)
        {
            consistencyBonus = 10f;
        }

        return Mathf.Clamp(avgSkill + consistencyBonus, 0f, 1000f);
    }

    /// <summary>
    /// Calculate decision-making score
    /// </summary>
    private float CalculateDecisionScore(UnifiedAnalyticsReport report)
    {
        if (report.sustainabilityReport == null) return 50f;

        float goodDecisions = report.sustainabilityReport.goodDecisions.Count;
        float badDecisions = report.sustainabilityReport.badDecisions.Count;
        float totalDecisions = goodDecisions + badDecisions;

        if (totalDecisions == 0) return 50f;

        float goodRatio = goodDecisions / totalDecisions;
        float decisionScore = goodRatio * 100f;

        // Penalty for critical bad decisions
        int criticalDecisions = report.sustainabilityReport.badDecisions
            .Count(d => d.severity == DecisionSeverity.Critical);
        decisionScore -= criticalDecisions * 10f;

        return Mathf.Clamp(decisionScore, 0f, 100f);
    }

    /// <summary>
    /// Calculate diversity score
    /// </summary>
    private float CalculateDiversityScore(UnifiedAnalyticsReport report)
    {
        int totalUniqueFish = report.deboningData.uniqueFishCount + 
                             report.saltwaterData.uniqueFishCount + 
                             report.freshwaterData.uniqueFishCount;

        // Ideal diversity: 15+ unique species
        float diversityScore = Mathf.Min((totalUniqueFish / 15f) * 100f, 100f);

        // Bonus for balanced diversity across minigames
        if (report.deboningData.uniqueFishCount >= 3 &&
            report.saltwaterData.uniqueFishCount >= 3 &&
            report.freshwaterData.uniqueFishCount >= 3)
        {
            diversityScore += 10f;
        }

        return Mathf.Clamp(diversityScore, 0f, 100f);
    }

    /// <summary>
    /// Determine if player did a good job
    /// </summary>
    private bool EvaluateGoodJob(PlayerPerformanceEvaluation evaluation, UnifiedAnalyticsReport report)
    {
        // Must meet minimum thresholds
        bool meetsOverallScore = evaluation.overallScore >= goodJobThreshold;
        bool meetsSustainability = evaluation.sustainabilityScore >= minSustainabilityScore;
        
        // Check decision quality
        bool goodDecisions = report.sustainabilityReport != null &&
                            report.sustainabilityReport.goodDecisions.Count >= minGoodDecisions;
        bool acceptableBadDecisions = report.sustainabilityReport == null ||
                                     report.sustainabilityReport.badDecisions.Count <= maxBadDecisions;

        // Check for critical issues
        bool noCriticalIssues = report.sustainabilityReport == null ||
                               report.sustainabilityReport.badDecisions
                                   .Count(d => d.severity == DecisionSeverity.Critical) == 0;

        return meetsOverallScore && meetsSustainability && goodDecisions && 
               acceptableBadDecisions && noCriticalIssues;
    }

    /// <summary>
    /// Generate detailed feedback
    /// </summary>
    private string GenerateFeedback(PlayerPerformanceEvaluation evaluation, UnifiedAnalyticsReport report)
    {
        if (evaluation.didGoodJob)
        {
            if (evaluation.overallScore >= 90f)
            {
                return $"Outstanding work! Your overall score is {evaluation.overallScore:F1}% " +
                       $"with a sustainability score of {evaluation.sustainabilityScore:F1}%. " +
                       $"You consistently make excellent, sustainable decisions across all minigames – keep it up!";
            }

            if (evaluation.overallScore >= 75f)
            {
                return $"Great job! Your overall score is {evaluation.overallScore:F1}% " +
                       $"and your choices are mostly sustainable. " +
                       $"A few more careful decisions will push you into the top tier of sustainable fishers.";
            }

            return $"Nice work! You met the goals with an overall score of {evaluation.overallScore:F1}%. " +
                   $"Stay consistent with your good decisions to keep improving your impact on the ecosystem.";
        }
        else
        {
            string feedback;

            if (evaluation.overallScore < 20f)
            {
                feedback = $"You’re just beginning – your overall performance is {evaluation.overallScore:F1}%. ";
                feedback += "Experiment more with different strategies and pay close attention to which choices harm or help the ecosystem. ";
            }
            else if (evaluation.overallScore < 40f)
            {
                feedback = $"You’re making early progress with an overall score of {evaluation.overallScore:F1}%. ";
                feedback += "Try to avoid clearly harmful actions, such as repeatedly catching endangered or overfished species. ";
            }
            else if (evaluation.overallScore < 60f)
            {
                feedback = $"You’re halfway there with an overall score of {evaluation.overallScore:F1}%. ";
                feedback += "Focus on making more sustainable decisions and improving your weakest minigame. ";
            }
            else
            {
                feedback = $"You’re close to a good job with an overall score of {evaluation.overallScore:F1}%. ";
                feedback += "A little more consistency in your sustainable choices will get you over the threshold. ";
            }

            if (evaluation.sustainabilityScore < minSustainabilityScore)
            {
                feedback += "Work on raising your sustainability score by protecting endangered species and avoiding overfishing. ";
            }

            if (report.sustainabilityReport != null &&
                report.sustainabilityReport.badDecisions.Count > maxBadDecisions)
            {
                feedback += "Review your unsustainable decisions and look for safer, more ecosystem‑friendly alternatives. ";
            }

            feedback += "Keep going – every session is a chance to improve!";
            return feedback;
        }
    }

    /// <summary>
    /// Identify player strengths with extensive dynamic feedback
    /// </summary>
    private List<string> IdentifyStrengths(UnifiedAnalyticsReport report)
    {
        List<string> strengths = new List<string>();

        // === DEBONING MINIGAME STRENGTHS ===
        if (report.deboningData.performanceScore >= 95f)
        {
            strengths.Add("Master deboner! Your precision and speed are exceptional.");
        }
        else if (report.deboningData.performanceScore >= 85f)
        {
            strengths.Add("Expert-level deboning skills with excellent accuracy.");
        }
        else if (report.deboningData.performanceScore >= 75f)
        {
            strengths.Add("Strong deboning technique with consistent results.");
        }
        else if (report.deboningData.performanceScore >= 65f)
        {
            strengths.Add("Good deboning fundamentals are developing well.");
        }

        if (report.deboningData.totalCatches >= 50)
        {
            strengths.Add($"Experienced processor with {report.deboningData.totalCatches} fish deboned.");
        }
        else if (report.deboningData.totalCatches >= 25)
        {
            strengths.Add("Building solid deboning experience through practice.");
        }

        if (report.deboningData.uniqueFishCount >= 8)
        {
            strengths.Add("Versatile deboner skilled with many different fish types.");
        }
        else if (report.deboningData.uniqueFishCount >= 5)
        {
            strengths.Add("Good variety in fish processing experience.");
        }

        // === SALTWATER FISHING STRENGTHS ===
        if (report.saltwaterData.performanceScore >= 95f)
        {
            strengths.Add("Saltwater fishing legend! Unmatched ocean expertise.");
        }
        else if (report.saltwaterData.performanceScore >= 85f)
        {
            strengths.Add("Expert saltwater angler with excellent technique.");
        }
        else if (report.saltwaterData.performanceScore >= 75f)
        {
            strengths.Add("Strong saltwater fishing skills and good instincts.");
        }
        else if (report.saltwaterData.performanceScore >= 65f)
        {
            strengths.Add("Solid saltwater fishing fundamentals.");
        }

        if (report.saltwaterData.uniqueFishCount >= 10)
        {
            strengths.Add("Impressive saltwater species diversity - you know the ocean well!");
        }
        else if (report.saltwaterData.uniqueFishCount >= 7)
        {
            strengths.Add("Good variety of saltwater catches shows adaptability.");
        }
        else if (report.saltwaterData.uniqueFishCount >= 5)
        {
            strengths.Add("Decent species diversity in saltwater fishing.");
        }

        if (report.saltwaterData.totalCatches >= 100)
        {
            strengths.Add($"Prolific saltwater fisher with {report.saltwaterData.totalCatches} catches!");
        }
        else if (report.saltwaterData.totalCatches >= 50)
        {
            strengths.Add("Experienced saltwater angler with many successful catches.");
        }

        // === FRESHWATER FISHING STRENGTHS ===
        if (report.freshwaterData.performanceScore >= 95f)
        {
            strengths.Add("Freshwater fishing master! Rivers and lakes are your domain.");
        }
        else if (report.freshwaterData.performanceScore >= 85f)
        {
            strengths.Add("Expert freshwater angler with refined techniques.");
        }
        else if (report.freshwaterData.performanceScore >= 75f)
        {
            strengths.Add("Strong freshwater fishing abilities and patience.");
        }
        else if (report.freshwaterData.performanceScore >= 65f)
        {
            strengths.Add("Good freshwater fishing fundamentals developing.");
        }

        if (report.freshwaterData.uniqueFishCount >= 8)
        {
            strengths.Add("Excellent freshwater species knowledge and diversity.");
        }
        else if (report.freshwaterData.uniqueFishCount >= 5)
        {
            strengths.Add("Good variety of freshwater catches.");
        }

        if (report.freshwaterData.totalCatches >= 75)
        {
            strengths.Add($"Dedicated freshwater fisher with {report.freshwaterData.totalCatches} catches.");
        }
        else if (report.freshwaterData.totalCatches >= 40)
        {
            strengths.Add("Growing freshwater fishing experience.");
        }

        // === CROSS-MINIGAME STRENGTHS ===
        float avgScore = (report.deboningData.performanceScore + 
                         report.saltwaterData.performanceScore + 
                         report.freshwaterData.performanceScore) / 3f;
        
        if (avgScore >= 85f)
        {
            strengths.Add("Well-rounded excellence across all fishing activities!");
        }
        else if (avgScore >= 75f)
        {
            strengths.Add("Consistent performer across multiple minigames.");
        }

        bool allAbove70 = report.deboningData.performanceScore >= 70f &&
                         report.saltwaterData.performanceScore >= 70f &&
                         report.freshwaterData.performanceScore >= 70f;
        if (allAbove70)
        {
            strengths.Add("Balanced skills - no weak areas in your fishing toolkit.");
        }

        int totalUniqueFish = report.deboningData.uniqueFishCount + 
                             report.saltwaterData.uniqueFishCount + 
                             report.freshwaterData.uniqueFishCount;
        if (totalUniqueFish >= 20)
        {
            strengths.Add($"Exceptional biodiversity awareness with {totalUniqueFish} unique species encountered!");
        }
        else if (totalUniqueFish >= 15)
        {
            strengths.Add("Good species diversity across all fishing activities.");
        }

        // === SUSTAINABILITY STRENGTHS ===
        if (report.sustainabilityReport != null)
        {
            float susScore = report.sustainabilityReport.sustainabilityScore;
            int goodCount = report.sustainabilityReport.goodDecisions?.Count ?? 0;
            int badCount = report.sustainabilityReport.badDecisions?.Count ?? 0;
            int totalDecisions = goodCount + badCount;

            if (susScore >= 95f)
            {
                strengths.Add("Sustainability champion! Your fishing practices are exemplary.");
            }
            else if (susScore >= 85f)
            {
                strengths.Add("Excellent sustainability score - you fish responsibly.");
            }
            else if (susScore >= 75f)
            {
                strengths.Add("Good sustainability practices guiding your decisions.");
            }
            else if (susScore >= 65f)
            {
                strengths.Add("Developing sustainable fishing habits.");
            }

            if (goodCount >= 50)
            {
                strengths.Add($"Impressive {goodCount} sustainable decisions made!");
            }
            else if (goodCount >= 30)
            {
                strengths.Add($"Strong track record with {goodCount} good decisions.");
            }
            else if (goodCount >= 20)
            {
                strengths.Add("Building a solid foundation of sustainable choices.");
            }
            else if (goodCount >= 10)
            {
                strengths.Add("Starting to make consistent sustainable decisions.");
            }

            if (totalDecisions >= 10)
            {
                float goodRatio = (float)goodCount / totalDecisions;
                if (goodRatio >= 0.95f)
                {
                    strengths.Add("Near-perfect decision making - almost all choices are sustainable!");
                }
                else if (goodRatio >= 0.85f)
                {
                    strengths.Add("Excellent judgment - the vast majority of your decisions help the ecosystem.");
                }
                else if (goodRatio >= 0.75f)
                {
                    strengths.Add("Good decision-making ratio favoring sustainability.");
                }
            }

            if (report.sustainabilityReport.overfishedSpecies.Count == 0)
            {
                strengths.Add("Zero overfishing - great population management awareness!");
            }

            if (report.sustainabilityReport.endangeredSpeciesCaught == 0)
            {
                strengths.Add("Protected all endangered species - outstanding conservation ethics!");
            }

            if (report.sustainabilityReport.speciesDiversity >= 15)
            {
                strengths.Add("Exceptional species diversity promotes ecosystem balance.");
            }
            else if (report.sustainabilityReport.speciesDiversity >= 10)
            {
                strengths.Add("Good species diversity helps prevent overfishing any single population.");
            }

            if (badCount == 0 && goodCount >= 5)
            {
                strengths.Add("Flawless record - no unsustainable decisions recorded!");
            }

            int criticalBad = report.sustainabilityReport.badDecisions?
                .Count(d => d.severity == DecisionSeverity.Critical) ?? 0;
            if (criticalBad == 0 && totalDecisions >= 10)
            {
                strengths.Add("No critical negative impacts - you avoid the worst mistakes.");
            }
        }

        // === ECONOMY/EARNINGS STRENGTHS ===
        if (EconomyManager.Instance != null)
        {
            float debtProgress = EconomyManager.Instance.DebtProgress;
            float totalEarned = EconomyManager.Instance.TotalEarned;

            if (debtProgress >= 1f)
            {
                strengths.Add("DEBT FREE! You've achieved financial freedom through fishing!");
            }
            else if (debtProgress >= 0.75f)
            {
                strengths.Add("Strong debt progress - freedom is within reach!");
            }
            else if (debtProgress >= 0.5f)
            {
                strengths.Add("Halfway to paying off your debt - great persistence!");
            }

            if (totalEarned >= 10000)
            {
                strengths.Add($"Successful fisher earning ₱{totalEarned:0} total!");
            }
            else if (totalEarned >= 5000)
            {
                strengths.Add("Building a profitable fishing operation.");
            }
        }

        if (strengths.Count == 0)
        {
            strengths.Add("Every journey starts somewhere - keep practicing to develop your strengths!");
            strengths.Add("Your potential is untapped - continued effort will reveal your talents.");
        }

        return strengths;
    }

    /// <summary>
    /// Identify player weaknesses with extensive dynamic feedback
    /// </summary>
    private List<string> IdentifyWeaknesses(UnifiedAnalyticsReport report)
    {
        List<string> weaknesses = new List<string>();

        // === DEBONING MINIGAME WEAKNESSES ===
        if (report.deboningData.performanceScore < 20f)
        {
            weaknesses.Add("Deboning skills are at a beginner level - practice the fundamentals.");
        }
        else if (report.deboningData.performanceScore < 35f)
        {
            weaknesses.Add("Deboning technique needs significant improvement.");
        }
        else if (report.deboningData.performanceScore < 50f)
        {
            weaknesses.Add("Deboning accuracy is below average - work on precision.");
        }
        else if (report.deboningData.performanceScore < 60f)
        {
            weaknesses.Add("Deboning skills need some improvement to reach proficiency.");
        }

        if (report.deboningData.totalCatches < 5 && report.deboningData.performanceScore < 70f)
        {
            weaknesses.Add("Limited deboning experience - try processing more fish.");
        }

        if (report.deboningData.uniqueFishCount < 2)
        {
            weaknesses.Add("Very limited variety in deboning - try different fish types.");
        }
        else if (report.deboningData.uniqueFishCount < 4 && report.deboningData.totalCatches >= 10)
        {
            weaknesses.Add("Narrow deboning experience - branch out to more species.");
        }

        // === SALTWATER FISHING WEAKNESSES ===
        if (report.saltwaterData.performanceScore < 20f)
        {
            weaknesses.Add("Saltwater fishing skills are undeveloped - learn the basics.");
        }
        else if (report.saltwaterData.performanceScore < 35f)
        {
            weaknesses.Add("Saltwater technique needs major work - practice patience and timing.");
        }
        else if (report.saltwaterData.performanceScore < 50f)
        {
            weaknesses.Add("Saltwater fishing performance is below average.");
        }
        else if (report.saltwaterData.performanceScore < 60f)
        {
            weaknesses.Add("Saltwater fishing could use more refinement.");
        }

        if (report.saltwaterData.uniqueFishCount < 2)
        {
            weaknesses.Add("Very low saltwater species diversity - explore different waters.");
        }
        else if (report.saltwaterData.uniqueFishCount < 4)
        {
            weaknesses.Add("Limited saltwater variety - try targeting different species.");
        }

        if (report.saltwaterData.totalCatches < 5 && report.saltwaterData.performanceScore < 70f)
        {
            weaknesses.Add("Not enough saltwater fishing experience yet.");
        }

        // === FRESHWATER FISHING WEAKNESSES ===
        if (report.freshwaterData.performanceScore < 20f)
        {
            weaknesses.Add("Freshwater fishing is at a beginner level - keep practicing.");
        }
        else if (report.freshwaterData.performanceScore < 35f)
        {
            weaknesses.Add("Freshwater skills need significant improvement.");
        }
        else if (report.freshwaterData.performanceScore < 50f)
        {
            weaknesses.Add("Freshwater fishing performance is below average.");
        }
        else if (report.freshwaterData.performanceScore < 60f)
        {
            weaknesses.Add("Freshwater fishing could be better with more practice.");
        }

        if (report.freshwaterData.uniqueFishCount < 2)
        {
            weaknesses.Add("Very limited freshwater variety - explore lakes and rivers.");
        }
        else if (report.freshwaterData.uniqueFishCount < 4)
        {
            weaknesses.Add("Low freshwater diversity - try different baits and locations.");
        }

        if (report.freshwaterData.totalCatches < 5 && report.freshwaterData.performanceScore < 70f)
        {
            weaknesses.Add("Not enough freshwater fishing practice.");
        }

        // === CROSS-MINIGAME WEAKNESSES ===
        float avgScore = (report.deboningData.performanceScore + 
                         report.saltwaterData.performanceScore + 
                         report.freshwaterData.performanceScore) / 3f;

        if (avgScore < 30f)
        {
            weaknesses.Add("Overall fishing skills need fundamental development across all areas.");
        }
        else if (avgScore < 50f)
        {
            weaknesses.Add("Average performance is below par - focus on your weakest minigame.");
        }

        float minScore = Mathf.Min(
            report.deboningData.performanceScore,
            report.saltwaterData.performanceScore,
            report.freshwaterData.performanceScore);
        float maxScore = Mathf.Max(
            report.deboningData.performanceScore,
            report.saltwaterData.performanceScore,
            report.freshwaterData.performanceScore);

        if (maxScore - minScore > 40f)
        {
            weaknesses.Add("Highly unbalanced skills - some areas are much weaker than others.");
        }
        else if (maxScore - minScore > 25f)
        {
            weaknesses.Add("Skill imbalance detected - work on your weakest fishing activity.");
        }

        int totalUniqueFish = report.deboningData.uniqueFishCount + 
                             report.saltwaterData.uniqueFishCount + 
                             report.freshwaterData.uniqueFishCount;
        if (totalUniqueFish < 5)
        {
            weaknesses.Add("Very low overall species diversity - explore more fish types!");
        }
        else if (totalUniqueFish < 8)
        {
            weaknesses.Add("Limited species variety - try catching different fish.");
        }

        // === SUSTAINABILITY WEAKNESSES ===
        if (report.sustainabilityReport != null)
        {
            float susScore = report.sustainabilityReport.sustainabilityScore;
            int goodCount = report.sustainabilityReport.goodDecisions?.Count ?? 0;
            int badCount = report.sustainabilityReport.badDecisions?.Count ?? 0;
            int totalDecisions = goodCount + badCount;

            if (susScore < 20f)
            {
                weaknesses.Add("Sustainability score is critical - your practices harm the ecosystem.");
            }
            else if (susScore < 35f)
            {
                weaknesses.Add("Sustainability is very low - major changes needed in fishing practices.");
            }
            else if (susScore < 50f)
            {
                weaknesses.Add("Sustainability score is below average - reconsider your choices.");
            }
            else if (susScore < minSustainabilityScore)
            {
                weaknesses.Add("Sustainability score hasn't reached the target threshold yet.");
            }

            if (totalDecisions >= 5)
            {
                float badRatio = (float)badCount / totalDecisions;
                if (badRatio >= 0.8f)
                {
                    weaknesses.Add("Almost all decisions are unsustainable - this is concerning.");
                }
                else if (badRatio >= 0.6f)
                {
                    weaknesses.Add("Majority of decisions harm sustainability - reverse this trend.");
                }
                else if (badRatio >= 0.4f)
                {
                    weaknesses.Add("Too many bad decisions compared to good ones.");
                }
            }

            if (badCount > maxBadDecisions * 3)
            {
                weaknesses.Add($"Excessive unsustainable decisions ({badCount}) - urgent improvement needed.");
            }
            else if (badCount > maxBadDecisions * 2)
            {
                weaknesses.Add($"Many unsustainable decisions ({badCount}) recorded.");
            }
            else if (badCount > maxBadDecisions)
            {
                weaknesses.Add($"Too many unsustainable decisions ({badCount}) - aim to reduce these.");
            }

            int overfishedCount = report.sustainabilityReport.overfishedSpecies?.Count ?? 0;
            if (overfishedCount >= 5)
            {
                weaknesses.Add($"Severe overfishing - {overfishedCount} species populations are depleted!");
            }
            else if (overfishedCount >= 3)
            {
                weaknesses.Add($"Multiple species ({overfishedCount}) are being overfished.");
            }
            else if (overfishedCount > 0)
            {
                weaknesses.Add($"Overfishing detected for {overfishedCount} species - spread your catches.");
            }

            int endangeredCaught = report.sustainabilityReport.endangeredSpeciesCaught;
            if (endangeredCaught >= 10)
            {
                weaknesses.Add($"Caught {endangeredCaught} endangered fish - this is a serious conservation issue!");
            }
            else if (endangeredCaught >= 5)
            {
                weaknesses.Add($"Multiple endangered species caught ({endangeredCaught}) - please release them.");
            }
            else if (endangeredCaught >= 2)
            {
                weaknesses.Add("Caught several endangered fish - learn to identify protected species.");
            }
            else if (endangeredCaught > 0)
            {
                weaknesses.Add("Caught an endangered species - be more careful to protect wildlife.");
            }

            int criticalBad = report.sustainabilityReport.badDecisions?
                .Count(d => d.severity == DecisionSeverity.Critical) ?? 0;
            int highBad = report.sustainabilityReport.badDecisions?
                .Count(d => d.severity == DecisionSeverity.High) ?? 0;
            int mediumBad = report.sustainabilityReport.badDecisions?
                .Count(d => d.severity == DecisionSeverity.Medium) ?? 0;

            if (criticalBad >= 5)
            {
                weaknesses.Add($"{criticalBad} critical decisions caused severe ecosystem damage.");
            }
            else if (criticalBad >= 2)
            {
                weaknesses.Add($"Multiple critical negative impacts ({criticalBad}) on the ecosystem.");
            }
            else if (criticalBad > 0)
            {
                weaknesses.Add("A critical decision caused significant ecosystem harm.");
            }

            if (highBad >= 5)
            {
                weaknesses.Add($"{highBad} high-severity bad decisions are impacting fish populations.");
            }
            else if (highBad >= 2)
            {
                weaknesses.Add("Several high-impact negative decisions recorded.");
            }

            if (mediumBad >= 10)
            {
                weaknesses.Add("Many moderate sustainability violations adding up over time.");
            }
            else if (mediumBad >= 5)
            {
                weaknesses.Add("Multiple medium-severity sustainability issues detected.");
            }

            if (report.sustainabilityReport.speciesDiversity < 3)
            {
                weaknesses.Add("Very low species diversity - focusing on too few fish types.");
            }
            else if (report.sustainabilityReport.speciesDiversity < 6)
            {
                weaknesses.Add("Species diversity is below healthy levels.");
            }

            if (goodCount < minGoodDecisions / 2 && totalDecisions >= 10)
            {
                weaknesses.Add("Very few sustainable decisions made - try to choose better options.");
            }
            else if (goodCount < minGoodDecisions && totalDecisions >= 10)
            {
                weaknesses.Add("Not enough sustainable decisions to meet the goal.");
            }
        }

        // === ECONOMY/EARNINGS WEAKNESSES ===
        if (EconomyManager.Instance != null)
        {
            float debtProgress = EconomyManager.Instance.DebtProgress;
            float totalEarned = EconomyManager.Instance.TotalEarned;

            if (debtProgress < 0.1f && totalEarned > 500)
            {
                weaknesses.Add("Debt progress is minimal - focus on selling more catches.");
            }
            else if (debtProgress < 0.25f && totalEarned > 1000)
            {
                weaknesses.Add("Debt reduction is slow - sell fish more consistently.");
            }

            if (totalEarned < 100 && report.saltwaterData.totalCatches + 
                report.freshwaterData.totalCatches >= 20)
            {
                weaknesses.Add("Catching fish but not selling - visit the shop to earn money!");
            }
        }

        return weaknesses;
    }

    /// <summary>
    /// Identify areas for improvement with extensive actionable advice
    /// </summary>
    private List<string> IdentifyImprovementAreas(UnifiedAnalyticsReport report)
    {
        List<string> areas = new List<string>();

        // === IDENTIFY WEAKEST MINIGAME AND PROVIDE SPECIFIC ADVICE ===
        float deboningScore = report.deboningData.performanceScore;
        float saltwaterScore = report.saltwaterData.performanceScore;
        float freshwaterScore = report.freshwaterData.performanceScore;

        float minScore = Mathf.Min(deboningScore, saltwaterScore, freshwaterScore);
        float maxScore = Mathf.Max(deboningScore, saltwaterScore, freshwaterScore);

        // Deboning improvements
        if (deboningScore == minScore || deboningScore < 60f)
        {
            if (deboningScore < 30f)
            {
                areas.Add("Start with basic deboning tutorials - focus on learning proper knife angles.");
                areas.Add("Practice on easier fish first before attempting larger or bony species.");
            }
            else if (deboningScore < 50f)
            {
                areas.Add("Focus on deboning accuracy - take your time before speeding up.");
                areas.Add("Study fish anatomy to understand bone placement better.");
            }
            else if (deboningScore < 70f)
            {
                areas.Add("Refine your deboning technique - small adjustments make big differences.");
                areas.Add("Try deboning different fish types to broaden your skills.");
            }
        }

        // Saltwater improvements
        if (saltwaterScore == minScore || saltwaterScore < 60f)
        {
            if (saltwaterScore < 30f)
            {
                areas.Add("Learn saltwater fishing basics - timing and patience are key.");
                areas.Add("Start with common species before targeting rare ocean fish.");
            }
            else if (saltwaterScore < 50f)
            {
                areas.Add("Practice reading water conditions for better saltwater catches.");
                areas.Add("Experiment with different baits and techniques in the ocean.");
            }
            else if (saltwaterScore < 70f)
            {
                areas.Add("Fine-tune your saltwater approach - try new fishing spots.");
                areas.Add("Target specific species to improve your saltwater expertise.");
            }
        }

        // Freshwater improvements
        if (freshwaterScore == minScore || freshwaterScore < 60f)
        {
            if (freshwaterScore < 30f)
            {
                areas.Add("Focus on freshwater fundamentals - rivers and lakes have unique challenges.");
                areas.Add("Learn which baits work best in freshwater environments.");
            }
            else if (freshwaterScore < 50f)
            {
                areas.Add("Practice patience in freshwater - fish behave differently than in salt.");
                areas.Add("Explore different freshwater locations and conditions.");
            }
            else if (freshwaterScore < 70f)
            {
                areas.Add("Work on consistency in freshwater fishing.");
                areas.Add("Try targeting specific freshwater species to build expertise.");
            }
        }

        // === BALANCE IMPROVEMENTS ===
        if (maxScore - minScore > 30f)
        {
            areas.Add("Balance your skills - spend more time on your weakest fishing activity.");
        }

        if (deboningScore > 80f && saltwaterScore < 50f)
        {
            areas.Add("You're great at deboning! Now apply that dedication to saltwater fishing.");
        }
        if (deboningScore > 80f && freshwaterScore < 50f)
        {
            areas.Add("Excellent deboning skills! Transfer that focus to freshwater fishing.");
        }
        if (saltwaterScore > 80f && deboningScore < 50f)
        {
            areas.Add("Skilled saltwater fisher! Now master deboning your catches.");
        }
        if (saltwaterScore > 80f && freshwaterScore < 50f)
        {
            areas.Add("Ocean expert! Try applying your skills to freshwater fishing.");
        }
        if (freshwaterScore > 80f && deboningScore < 50f)
        {
            areas.Add("Great freshwater fisher! Learn to properly debone your catches too.");
        }
        if (freshwaterScore > 80f && saltwaterScore < 50f)
        {
            areas.Add("Freshwater master! Challenge yourself with ocean fishing.");
        }

        // === DIVERSITY IMPROVEMENTS ===
        int totalUniqueFish = report.deboningData.uniqueFishCount + 
                             report.saltwaterData.uniqueFishCount + 
                             report.freshwaterData.uniqueFishCount;

        if (totalUniqueFish < 5)
        {
            areas.Add("Explore to find new fish species - each one teaches you something new!");
            areas.Add("Try fishing in different locations to discover more varieties.");
        }
        else if (totalUniqueFish < 10)
        {
            areas.Add("Expand your species diversity - variety helps you learn faster.");
            areas.Add("Challenge yourself to catch fish you haven't seen before.");
        }
        else if (totalUniqueFish < 15)
        {
            areas.Add("Good variety so far! Keep exploring for even more species.");
        }

        if (report.deboningData.uniqueFishCount < 3)
        {
            areas.Add("Debone more fish types to understand different anatomies.");
        }
        if (report.saltwaterData.uniqueFishCount < 3)
        {
            areas.Add("Try catching more saltwater species - the ocean has endless variety.");
        }
        if (report.freshwaterData.uniqueFishCount < 3)
        {
            areas.Add("Explore freshwater biodiversity - rivers and lakes hold many surprises.");
        }

        // === SUSTAINABILITY IMPROVEMENTS ===
        if (report.sustainabilityReport != null)
        {
            float susScore = report.sustainabilityReport.sustainabilityScore;
            int goodCount = report.sustainabilityReport.goodDecisions?.Count ?? 0;
            int badCount = report.sustainabilityReport.badDecisions?.Count ?? 0;
            int totalDecisions = goodCount + badCount;

            // General sustainability advice
            if (susScore < 30f)
            {
                areas.Add("Urgently learn about sustainable fishing - your practices are harmful.");
                areas.Add("Release endangered and overfished species immediately when caught.");
                areas.Add("Think about long-term ecosystem health, not just short-term profits.");
            }
            else if (susScore < 50f)
            {
                areas.Add("Study which species are endangered or overfished in your area.");
                areas.Add("Make a conscious effort to protect vulnerable fish populations.");
                areas.Add("Consider the impact of each catch on the ecosystem.");
            }
            else if (susScore < 70f)
            {
                areas.Add("Continue improving sustainability - you're on the right track.");
                areas.Add("Research best practices for responsible fishing.");
            }
            else if (susScore < 85f)
            {
                areas.Add("Fine-tune your sustainability approach to reach excellence.");
            }

            // Bad decision specific advice
            if (badCount > 0)
            {
                int criticalBad = report.sustainabilityReport.badDecisions
                    .Count(d => d.severity == DecisionSeverity.Critical);
                int highBad = report.sustainabilityReport.badDecisions
                    .Count(d => d.severity == DecisionSeverity.High);

                if (criticalBad > 0)
                {
                    areas.Add("Avoid critical mistakes - learn which actions cause the most harm.");
                    areas.Add("Never keep endangered species - always release them safely.");
                }
                if (highBad > 0)
                {
                    areas.Add("Reduce high-impact decisions by being more selective with catches.");
                }
                if (badCount >= 10)
                {
                    areas.Add("Review your decision history and identify patterns in your mistakes.");
                }
                if (badCount >= 5)
                {
                    areas.Add("Before each action, ask: 'Is this helping or hurting the ecosystem?'");
                }
                areas.Add("Look for sustainable alternatives to your unsustainable choices.");
            }

            // Good decision encouragement
            if (goodCount < minGoodDecisions / 2)
            {
                areas.Add("Actively seek out opportunities to make sustainable choices.");
                areas.Add("Releasing undersized or endangered fish counts as a good decision!");
            }
            else if (goodCount < minGoodDecisions)
            {
                areas.Add("Keep making sustainable decisions - you're getting closer to the goal.");
                areas.Add("Every good choice adds up - consistency is key to improvement.");
            }

            // Overfishing specific advice
            int overfishedCount = report.sustainabilityReport.overfishedSpecies?.Count ?? 0;
            if (overfishedCount >= 3)
            {
                areas.Add("Spread your fishing across more species to prevent overfishing.");
                areas.Add("Let depleted populations recover by targeting other fish.");
            }
            else if (overfishedCount > 0)
            {
                areas.Add("Rotate which species you target to maintain population balance.");
            }

            // Endangered species advice
            int endangeredCaught = report.sustainabilityReport.endangeredSpeciesCaught;
            if (endangeredCaught >= 5)
            {
                areas.Add("Learn to identify endangered species before catching them.");
                areas.Add("If you accidentally catch endangered fish, release them gently.");
            }
            else if (endangeredCaught > 0)
            {
                areas.Add("Study which fish in your area are protected - avoid targeting them.");
            }

            // Diversity for sustainability
            if (report.sustainabilityReport.speciesDiversity < 5)
            {
                areas.Add("Targeting diverse species reduces pressure on any single population.");
            }
            else if (report.sustainabilityReport.speciesDiversity < 10)
            {
                areas.Add("More species diversity means healthier, more resilient ecosystems.");
            }
        }

        // === ECONOMY IMPROVEMENTS ===
        if (EconomyManager.Instance != null)
        {
            float debtProgress = EconomyManager.Instance.DebtProgress;
            float totalEarned = EconomyManager.Instance.TotalEarned;
            float wallet = EconomyManager.Instance.CurrentWallet;

            if (debtProgress < 0.1f)
            {
                areas.Add("Start selling your catches at the shop to pay off debt.");
                areas.Add("Every fish sold brings you closer to financial freedom!");
            }
            else if (debtProgress < 0.25f)
            {
                areas.Add("Keep selling fish consistently - debt progress will accelerate.");
            }
            else if (debtProgress < 0.5f)
            {
                areas.Add("You're making progress on debt - maintain this selling momentum.");
            }
            else if (debtProgress < 0.75f)
            {
                areas.Add("Over halfway to freedom! Keep selling to clear the remaining debt.");
            }
            else if (debtProgress < 1f)
            {
                areas.Add("So close to being debt-free! A few more sales will do it.");
            }

            if (wallet > 1000 && debtProgress < 0.9f)
            {
                areas.Add("You have savings - consider paying more toward your debt.");
            }

            int totalFishCaught = report.saltwaterData.totalCatches + 
                                  report.freshwaterData.totalCatches;
            if (totalEarned < 200 && totalFishCaught >= 30)
            {
                areas.Add("You're catching fish but not profiting - visit the sell shop!");
            }
        }

        // === GENERAL IMPROVEMENT ADVICE ===
        float avgScore = (deboningScore + saltwaterScore + freshwaterScore) / 3f;

        if (avgScore < 40f)
        {
            areas.Add("Play each minigame regularly to build fundamental skills.");
            areas.Add("Don't get discouraged - improvement comes with practice!");
        }
        else if (avgScore < 60f)
        {
            areas.Add("Consistency is key - try to improve a little each session.");
        }
        else if (avgScore < 80f)
        {
            areas.Add("You're doing well! Push for excellence in all areas.");
        }
        else
        {
            areas.Add("Excellent progress! Maintain your skills and help the ecosystem thrive.");
        }

        // Always add at least one if list is empty
        if (areas.Count == 0)
        {
            areas.Add("Keep playing and exploring - there's always room to grow!");
        }

        return areas;
    }

    /// <summary>
    /// Calculate letter grade
    /// </summary>
    private string CalculateGrade(float score)
    {
        if (score >= 90f) return "A+";
        if (score >= 85f) return "A";
        if (score >= 80f) return "B+";
        if (score >= 75f) return "B";
        if (score >= 70f) return "C+";
        if (score >= 65f) return "C";
        if (score >= 60f) return "D+";
        if (score >= 50f) return "D";
        return "F";
    }
}

/// <summary>
/// Comprehensive player performance evaluation
/// </summary>
[System.Serializable]
public class PlayerPerformanceEvaluation
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
}

