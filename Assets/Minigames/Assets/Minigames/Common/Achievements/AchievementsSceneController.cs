using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// Main controller for the Achievements and Analytics scene.
/// Displays player performance, sustainability metrics, and educational effectiveness.
/// </summary>
public class AchievementsSceneController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainPanel;
    public GameObject sustainabilityPanel;
    public GameObject achievementsPanel;
    public GameObject effectivenessPanel;
    public GameObject decisionsPanel;
    public GameObject performancePanel;
    public GameObject DebtStatusPanel; // New: Unified performance panel

    [Header("Sustainability UI")]
    public Text sustainabilityScoreText;
    public Text totalCatchesText;
    public Text speciesDiversityText;
    public Text totalSessionsText;
    public Text playTimeText;
    public Image sustainabilityScoreFill;
    public Slider sustainabilityScoreSlider; // Alternative slider support

    [Header("Good Decisions UI")]
    public Transform goodDecisionsContainer;
    public GameObject decisionItemPrefab;

    [Header("Bad Decisions UI")]
    public Transform badDecisionsContainer;

    [Header("Achievements UI")]
    public Transform achievementsContainer;
    public GameObject achievementItemPrefab;

    [Header("Effectiveness UI")]
    public Text knowledgeScoreText;
    public Text behavioralScoreText;
    public Text engagementScoreText;
    public Text overallScoreText;
    public Text recommendationsText;

    [Header("Performance Panel UI (New - Unified Analytics)")]
    public TextMeshProUGUI overallPerformanceScoreText;
    public TextMeshProUGUI overallGradeText;
    public TextMeshProUGUI didGoodJobText;
    public Image overallScoreFill;
    public Slider overallScoreSlider; // Alternative to Image fillAmount
    public TextMeshProUGUI deboningScoreText;
    public TextMeshProUGUI saltwaterScoreText;
    public TextMeshProUGUI freshwaterScoreText;
    public Image deboningScoreFill;
    public Slider deboningScoreSlider; // Alternative to Image fillAmount
    public Image saltwaterScoreFill;
    public Slider saltwaterScoreSlider; // Alternative to Image fillAmount
    public Image freshwaterScoreFill;
    public Slider freshwaterScoreSlider; // Alternative to Image fillAmount
    public TextMeshProUGUI skillScoreText;
    public TextMeshProUGUI decisionScoreText;
    public TextMeshProUGUI diversityScoreText;
    public TextMeshProUGUI feedbackText;
    public Transform strengthsContainer;
    public Transform weaknessesContainer;
    public Transform improvementAreasContainer;
    public GameObject listItemPrefab;

    [Header("Navigation")]
    public Button sustainabilityButton;
    public Button achievementsButton;
    public Button effectivenessButton;
    public Button decisionsButton;
    public Button performanceButton; // New: Performance button
    public Button DebtStatusButton;
    public Button backButton;

    private SustainabilityReport currentReport;
    private EducationalEffectivenessReport effectivenessReport;
    private UnifiedAnalyticsReport unifiedReport;
    private PlayerPerformanceEvaluation performanceEvaluation;

    void Start()
    {
        // Ensure managers exist before loading data
        EnsureManagersExist();
        
        SetupButtons();
        LoadAndDisplayData();
    }

    void OnEnable()
    {
        // Refresh data when scene becomes active (e.g., when returning from another scene)
        // This ensures we always show the latest data
        if (Application.isPlaying)
        {
            // Small delay to ensure all managers are initialized
            StartCoroutine(RefreshDataDelayed());
        }
    }

    private System.Collections.IEnumerator RefreshDataDelayed()
    {
        // Wait one frame to ensure all managers are ready
        yield return null;
        
        // Refresh data to get the latest from current session
        RefreshData();
    }

    /// <summary>
    /// Refresh all data to ensure it's up-to-date
    /// </summary>
    public void RefreshData()
    {
        Debug.Log("Refreshing achievements data...");
        LoadAndDisplayData();
    }

    /// <summary>
    /// Ensure all required manager instances exist in the scene
    /// </summary>
    private void EnsureManagersExist()
    {
        // Create SustainableFishingMetrics if it doesn't exist
        if (SustainableFishingMetrics.Instance == null)
        {
            GameObject metricsObj = new GameObject("SustainableFishingMetrics");
            metricsObj.AddComponent<SustainableFishingMetrics>();
            Debug.Log("Created SustainableFishingMetrics instance");
        }

        // Create AchievementSystem if it doesn't exist
        if (AchievementSystem.Instance == null)
        {
            GameObject achievementObj = new GameObject("AchievementSystem");
            achievementObj.AddComponent<AchievementSystem>();
            Debug.Log("Created AchievementSystem instance");
        }

        // Create EducationalEffectivenessCalculator if it doesn't exist
        if (EducationalEffectivenessCalculator.Instance == null)
        {
            GameObject effectivenessObj = new GameObject("EducationalEffectivenessCalculator");
            effectivenessObj.AddComponent<EducationalEffectivenessCalculator>();
            Debug.Log("Created EducationalEffectivenessCalculator instance");
        }

        if (AchievementsDataStore.Instance == null)
        {
            GameObject dataStoreObj = new GameObject("AchievementsDataStore");
            dataStoreObj.AddComponent<AchievementsDataStore>();
            Debug.Log("Created AchievementsDataStore instance");
        }

        // Create UnifiedAnalyticsManager if it doesn't exist
        if (UnifiedAnalyticsManager.Instance == null)
        {
            GameObject unifiedObj = new GameObject("UnifiedAnalyticsManager");
            unifiedObj.AddComponent<UnifiedAnalyticsManager>();
            Debug.Log("Created UnifiedAnalyticsManager instance");
        }

        // Create PlayerPerformanceEvaluator if it doesn't exist
        if (PlayerPerformanceEvaluator.Instance == null)
        {
            GameObject evaluatorObj = new GameObject("PlayerPerformanceEvaluator");
            evaluatorObj.AddComponent<PlayerPerformanceEvaluator>();
            Debug.Log("Created PlayerPerformanceEvaluator instance");
        }

        if (PerformancePanelDataStore.Instance == null)
        {
            GameObject performanceStore = new GameObject("PerformancePanelDataStore");
            performanceStore.AddComponent<PerformancePanelDataStore>();
            Debug.Log("Created PerformancePanelDataStore instance");
        }

        if (EconomyManager.Instance == null)
        {
            GameObject economyObj = new GameObject("EconomyManager");
            economyObj.AddComponent<EconomyManager>();
            Debug.Log("Created EconomyManager instance");
        }
    }

    private void SetupButtons()
    {
        if (sustainabilityButton != null)
            sustainabilityButton.onClick.AddListener(() => ShowPanel(sustainabilityPanel));

        if (achievementsButton != null)
            achievementsButton.onClick.AddListener(() => ShowPanel(achievementsPanel));

        if (effectivenessButton != null)
            effectivenessButton.onClick.AddListener(() => ShowPanel(effectivenessPanel));

        if (decisionsButton != null)
            decisionsButton.onClick.AddListener(() => ShowPanel(decisionsPanel));

        if (performanceButton != null)
            performanceButton.onClick.AddListener(() => ShowPanel(performancePanel));

        if (DebtStatusButton != null)
            DebtStatusButton.onClick.AddListener(() => ShowPanel(DebtStatusPanel));

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void LoadAndDisplayData()
    {
        // PRIORITY: Current session data FIRST (most recent), then fall back to saved data
        // This ensures the latest data is always shown immediately
        
        bool hasSavedData = AchievementsDataStore.Instance != null && AchievementsDataStore.Instance.HasSavedData;
        
        // Get current session data FIRST (most recent)
        SustainabilityReport currentSessionReport = null;
        if (SustainableFishingMetrics.Instance != null)
        {
            currentSessionReport = SustainableFishingMetrics.Instance.GetReport();
        }
        
        // Get saved data as fallback
        SustainabilityReport savedReport = null;
        if (hasSavedData)
        {
            savedReport = AchievementsDataStore.Instance.GetSavedSustainabilityReport();
        }

        // PRIORITY: Use current session data if available (most recent), otherwise use saved data
        if (currentSessionReport != null)
        {
            currentReport = currentSessionReport;
            Debug.Log("Using current session sustainability data (most recent).");
        }
        else if (savedReport != null)
        {
            currentReport = savedReport;
            Debug.Log("Loaded sustainability data from saved file (no current session data).");
        }
        else
        {
            // Create report with starting score of 30
            if (SustainableFishingMetrics.Instance != null)
            {
                // Get a fresh report which will have the starting score of 30
                currentReport = SustainableFishingMetrics.Instance.GetReport();
                Debug.Log("Created fresh report with starting sustainability score of 30.");
            }
            else
            {
                // Fallback: create report with starting score
                currentReport = new SustainabilityReport
                {
                    totalCatches = 0,
                    totalSessions = 1, // Start with 1 session
                    totalPlayTime = 0f,
                    speciesDiversity = 0,
                    sustainabilityScore = 30f, // Starting score is 30
                    goodDecisions = new List<PlayerDecision>(),
                    badDecisions = new List<PlayerDecision>(),
                    catchesPerSpecies = new Dictionary<string, int>(),
                    endangeredSpeciesCaught = 0,
                    overfishedSpecies = new List<string>()
                };
                Debug.Log("Created new game report with starting sustainability score of 30.");
            }
        }

        // Calculate effectiveness - use current report (which is already the most recent)
        EducationalEffectivenessReport savedEffectiveness = null;
        if (hasSavedData && currentReport == savedReport)
        {
            // Only use saved effectiveness if we're using saved sustainability data
            savedEffectiveness = AchievementsDataStore.Instance.GetSavedEffectivenessReport();
        }
        
        if (savedEffectiveness != null && currentReport == savedReport)
        {
            effectivenessReport = savedEffectiveness;
            Debug.Log("Loaded effectiveness data from saved file.");
        }
        else if (EducationalEffectivenessCalculator.Instance != null && currentReport != null)
        {
            // Always recalculate from current report to ensure it's up-to-date
            effectivenessReport = EducationalEffectivenessCalculator.Instance.CalculateEffectiveness(currentReport);
            Debug.Log("Calculated effectiveness from current session data.");
        }
        else
        {
            // Create empty effectiveness report
            effectivenessReport = new EducationalEffectivenessReport
            {
                knowledgeAcquisitionScore = 0f,
                behavioralChangeScore = 0f,
                engagementScore = 0f,
                overallEffectivenessScore = 0f,
                learningIndicators = new LearningIndicators(),
                recommendations = new List<string> { "No data available. Play the minigames to see your progress!" }
            };
        }

        // Update achievements (only if system exists)
        if (AchievementSystem.Instance != null && currentReport != null)
        {
            AchievementSystem.Instance.UpdateAchievements(currentReport);
        }

        // Load unified analytics data - prioritize current session
        LoadUnifiedAnalytics();

        // Display all data (methods handle null checks)
        DisplaySustainabilityData();
        DisplayDecisions();
        DisplayAchievements();
        DisplayEffectiveness();
        DisplayPerformanceData(); // New: Display unified performance

        // Determine if we actually have meaningful data before saving
        bool hasSustainabilityData = HasValidData(currentReport);
        bool hasEffectivenessData = HasValidEffectivenessData(effectivenessReport);
        bool hasUnifiedData = HasValidUnifiedData(unifiedReport);
        bool hasPerformanceData = HasValidPerformanceEvaluation(performanceEvaluation);
        bool hasAnyData = hasSustainabilityData || hasEffectivenessData || hasUnifiedData || hasPerformanceData;

        // Prepare economy snapshot if needed
        EconomySnapshot economySnapshot = null;
        if (EconomyManager.Instance != null)
        {
            economySnapshot = EconomySnapshot.FromEconomyManager(EconomyManager.Instance);
        }

        // Save current state (only when we have real data to persist)
        if (AchievementsDataStore.Instance != null && hasAnyData)
        {
            AchievementsDataStore.Instance.SaveSnapshot(
                currentReport,
                effectivenessReport,
                unifiedReport,
                performanceEvaluation,
                economySnapshot);
        }
        else if (!hasAnyData)
        {
            Debug.Log("AchievementsSceneController: Skipping save because there is no meaningful data yet (prevents wiping previous progress).");
        }
    }

    /// <summary>
    /// Check if a sustainability report has valid/meaningful data
    /// </summary>
    private bool HasValidData(SustainabilityReport report)
    {
        if (report == null) return false;

        bool hasCatchData = report.totalCatches > 0 || report.speciesDiversity > 0;
        bool hasDecisionData = (report.goodDecisions != null && report.goodDecisions.Count > 0) ||
                               (report.badDecisions != null && report.badDecisions.Count > 0);
        bool hasSpeciesData = report.catchesPerSpecies != null && report.catchesPerSpecies.Count > 0;
        bool hasOverfishData = report.overfishedSpecies != null && report.overfishedSpecies.Count > 0;

        return hasCatchData || hasDecisionData || hasSpeciesData || hasOverfishData;
    }

    /// <summary>
    /// Load unified analytics data from all minigames
    /// Prioritizes current session data (most recent) over saved data
    /// </summary>
    private void LoadUnifiedAnalytics()
    {
        bool hasSavedData = AchievementsDataStore.Instance != null && AchievementsDataStore.Instance.HasSavedData;
        
        // Get current session unified analytics FIRST (most recent)
        UnifiedAnalyticsReport currentSessionReport = null;
        if (UnifiedAnalyticsManager.Instance != null)
        {
            currentSessionReport = UnifiedAnalyticsManager.Instance.GetComprehensiveReport();
        }
        
        // Get saved unified analytics as fallback
        UnifiedAnalyticsReport savedUnifiedReport = null;
        UnifiedAnalyticsReport performanceStoreUnified = null;
        if (hasSavedData)
        {
            savedUnifiedReport = AchievementsDataStore.Instance.GetSavedUnifiedAnalyticsReport();
        }
        if (PerformancePanelDataStore.Instance != null && PerformancePanelDataStore.Instance.HasSavedData)
        {
            performanceStoreUnified = PerformancePanelDataStore.Instance.GetSavedUnifiedReport();
        }
        
        // PRIORITY: Use current session data if available and valid, otherwise use saved data
        if (currentSessionReport != null && HasValidUnifiedData(currentSessionReport))
        {
            unifiedReport = currentSessionReport;
            Debug.Log("Using current session unified analytics (most recent).");
        }
        else if (savedUnifiedReport != null)
        {
            unifiedReport = savedUnifiedReport;
            Debug.Log("Loaded unified analytics from saved file (no current session data).");
        }
        else if (performanceStoreUnified != null)
        {
            unifiedReport = performanceStoreUnified;
            Debug.Log("Loaded unified analytics from performance panel store.");
        }
        else
        {
            unifiedReport = null;
        }

        // Get current session performance evaluation FIRST
        PlayerPerformanceEvaluation currentSessionEvaluation = null;
        if (PlayerPerformanceEvaluator.Instance != null)
        {
            // Always evaluate from current state to get most recent data
            currentSessionEvaluation = PlayerPerformanceEvaluator.Instance.EvaluatePlayer();
        }
        
        // Get saved performance evaluation as fallback
        PlayerPerformanceEvaluation savedEvaluation = null;
        PlayerPerformanceEvaluation performanceStoreEvaluation = null;
        if (hasSavedData)
        {
            savedEvaluation = AchievementsDataStore.Instance.GetSavedPerformanceEvaluation();
        }
        if (PerformancePanelDataStore.Instance != null && PerformancePanelDataStore.Instance.HasSavedData)
        {
            performanceStoreEvaluation = PerformancePanelDataStore.Instance.GetSavedEvaluation();
        }
        
        bool currentEvalValid = HasValidPerformanceEvaluation(currentSessionEvaluation);
        bool savedEvalValid = HasValidPerformanceEvaluation(savedEvaluation);
        bool storeEvalValid = HasValidPerformanceEvaluation(performanceStoreEvaluation);
        
        if (currentEvalValid)
        {
            performanceEvaluation = currentSessionEvaluation;
            Debug.Log("Using current session performance evaluation (most recent).");
        }
        else if (savedEvalValid)
        {
            performanceEvaluation = savedEvaluation;
            Debug.Log("Loaded performance evaluation from saved file.");
        }
        else if (storeEvalValid)
        {
            performanceEvaluation = performanceStoreEvaluation;
            Debug.Log("Loaded performance evaluation from performance panel store.");
        }
        else
        {
            performanceEvaluation = null;
        }
    }

    /// <summary>
    /// Check if unified analytics report has valid/meaningful data
    /// </summary>
    private bool HasValidUnifiedData(UnifiedAnalyticsReport report)
    {
        if (report == null) return false;
        // Consider it valid if any minigame has data
        return (report.deboningData != null && report.deboningData.gamesPlayed > 0) ||
               (report.saltwaterData != null && report.saltwaterData.totalCatches > 0) ||
               (report.freshwaterData != null && report.freshwaterData.gamesPlayed > 0) ||
               report.overallPerformance > 0;
    }

    /// <summary>
    /// Checks if performance evaluation has meaningful values
    /// </summary>
    private bool HasValidPerformanceEvaluation(PlayerPerformanceEvaluation evaluation)
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

    /// <summary>
    /// Checks if effectiveness report has meaningful values
    /// </summary>
    private bool HasValidEffectivenessData(EducationalEffectivenessReport report)
    {
        if (report == null) return false;
        return report.knowledgeAcquisitionScore > 0f ||
               report.behavioralChangeScore > 0f ||
               report.engagementScore > 0f ||
               report.overallEffectivenessScore > 0f;
    }

    private void DisplaySustainabilityData()
    {
        if (currentReport == null)
        {
            // Show default values if no report
            if (sustainabilityScoreText != null)
                sustainabilityScoreText.text = "Sustainability Score: 0/100";
            if (totalCatchesText != null)
                totalCatchesText.text = "Total Catches: 0";
            if (speciesDiversityText != null)
                speciesDiversityText.text = "Species Diversity: 0";
            if (totalSessionsText != null)
                totalSessionsText.text = "Total Sessions: 0";
            if (playTimeText != null)
                playTimeText.text = "Play Time: 0.0 hours";

            if (sustainabilityScoreSlider != null)
            {
                sustainabilityScoreSlider.value = sustainabilityScoreSlider.minValue;
                sustainabilityScoreSlider.interactable = false;
            }

            if (sustainabilityScoreFill != null)
                sustainabilityScoreFill.fillAmount = 0f;
            return;
        }

        if (sustainabilityScoreText != null)
        {
            sustainabilityScoreText.text = $"Sustainability Score: {currentReport.sustainabilityScore:F1}/100";
        }

        if (sustainabilityScoreSlider != null)
        {
            float targetMax = sustainabilityScoreSlider.maxValue <= sustainabilityScoreSlider.minValue
                ? 100f
                : sustainabilityScoreSlider.maxValue;
            sustainabilityScoreSlider.value = Mathf.Clamp(currentReport.sustainabilityScore,
                sustainabilityScoreSlider.minValue,
                targetMax);
            sustainabilityScoreSlider.interactable = false;
        }

        if (sustainabilityScoreFill != null)
        {
            sustainabilityScoreFill.fillAmount = currentReport.sustainabilityScore / 100f;
        }

        if (totalCatchesText != null)
        {
            totalCatchesText.text = $"Total Catches: {currentReport.totalCatches}";
        }

        if (speciesDiversityText != null)
        {
            speciesDiversityText.text = $"Species Diversity: {currentReport.speciesDiversity}";
        }

        if (totalSessionsText != null)
        {
            totalSessionsText.text = $"Total Sessions: {currentReport.totalSessions}";
        }

        if (playTimeText != null)
        {
            float hours = currentReport.totalPlayTime / 3600f;
            playTimeText.text = $"Play Time: {hours:F1} hours";
        }
    }

    private void DisplayDecisions()
    {
        if (currentReport == null || currentReport.goodDecisions == null || currentReport.badDecisions == null)
        {
            // Clear containers if no data
            ClearContainer(goodDecisionsContainer);
            ClearContainer(badDecisionsContainer);
            return;
        }

        // Clear existing items
        ClearContainer(goodDecisionsContainer);
        ClearContainer(badDecisionsContainer);

        // Display good decisions
        if (goodDecisionsContainer != null && decisionItemPrefab != null)
        {
            foreach (var decision in currentReport.goodDecisions.OrderByDescending(d => d.timestamp))
            {
                CreateDecisionItem(decision, goodDecisionsContainer, true);
            }
        }

        // Display bad decisions
        if (badDecisionsContainer != null && decisionItemPrefab != null)
        {
            foreach (var decision in currentReport.badDecisions.OrderByDescending(d => d.timestamp))
            {
                CreateDecisionItem(decision, badDecisionsContainer, false);
            }
        }
    }

    private void CreateDecisionItem(PlayerDecision decision, Transform parent, bool isGood)
    {
        GameObject item = Instantiate(decisionItemPrefab, parent);
        
        // Set color based on good/bad
        Image bg = item.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = isGood ? new Color(0.2f, 0.8f, 0.2f, 0.3f) : new Color(0.8f, 0.2f, 0.2f, 0.3f);
        }

        // Set text
        Text[] texts = item.GetComponentsInChildren<Text>();
        if (texts.Length > 0)
        {
            texts[0].text = decision.description;
        }
        if (texts.Length > 1)
        {
            texts[1].text = decision.explanation;
        }

        // Set severity indicator
        Text severityText = item.GetComponentInChildren<Text>();
        if (severityText != null && item.GetComponentsInChildren<Text>().Length > 2)
        {
            item.GetComponentsInChildren<Text>()[2].text = $"Severity: {decision.severity}";
        }
    }

    private void DisplayAchievements()
    {
        if (achievementsContainer == null || achievementItemPrefab == null)
        {
            Debug.LogWarning("Achievements UI references missing!");
            return;
        }

        ClearContainer(achievementsContainer);

        if (AchievementSystem.Instance == null)
        {
            Debug.LogWarning("AchievementSystem not found. Cannot display achievements.");
            return;
        }

        ClearContainer(achievementsContainer);

        // Display unlocked achievements
        var unlocked = AchievementSystem.Instance.GetUnlockedAchievements();
        foreach (var achievement in unlocked)
        {
            CreateAchievementItem(achievement, true);
        }

        // Display locked achievements (grayed out)
        var locked = AchievementSystem.Instance.GetLockedAchievements();
        foreach (var achievement in locked)
        {
            CreateAchievementItem(achievement, false);
        }
    }

    private void CreateAchievementItem(AchievementDefinition achievement, bool isUnlocked)
    {
        GameObject item = Instantiate(achievementItemPrefab, achievementsContainer);
        
        // Set unlocked state - FIXED: Keep full opacity, use subtle color difference for locked
        Image bg = item.GetComponent<Image>();
        if (bg != null)
        {
            // Keep full opacity (alpha = 1.0) but use slightly darker color for locked achievements
            bg.color = isUnlocked ? Color.white : new Color(0.85f, 0.85f, 0.85f, 1.0f);
        }

        // Set achievement data - FIXED: Use TextMeshProUGUI instead of Text
        TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0)
        {
            texts[0].text = achievement.title;
        }
        if (texts.Length > 1)
        {
            texts[1].text = achievement.description;
        }

        // Get all images once for both progress bar and icon
        Image[] images = item.GetComponentsInChildren<Image>();
        
        // Set progress - Support both Slider and Image fillAmount
        float progress = isUnlocked ? 1.0f : AchievementSystem.Instance.GetProgress(achievement.achievementId);
        
        // Method 1: Try Slider component first (recommended)
        Slider progressSlider = item.GetComponentInChildren<Slider>();
        if (progressSlider != null)
        {
            progressSlider.value = progress * 100f; // Slider expects 0-100 range
            progressSlider.interactable = false; // Make it non-interactive (display only)
        }
        // Method 2: Try Image fillAmount (fallback)
        else
        {
            // Find the fill image (usually the 3rd or 4th image - background, icon, then fill)
            foreach (Image img in images)
            {
                if (img.type == Image.Type.Filled && img != bg)
                {
                    img.fillAmount = progress;
                    break;
                }
            }
        }
        
        // Set progress text if available
        if (!isUnlocked && texts.Length > 2)
        {
            texts[2].text = $"Progress: {progress * 100f:F0}%";
        }
        // If unlocked, hide or update progress text
        else if (isUnlocked && texts.Length > 2)
        {
            texts[2].text = "Completed!";
        }

        // Set icon - FIXED: Skip the first Image (background) and get the icon
        if (images.Length > 1 && achievement.icon != null)
        {
            images[1].sprite = achievement.icon; // images[0] is the background, images[1] is the icon
        }
    }

    private void DisplayEffectiveness()
    {
        if (effectivenessReport == null)
        {
            // Show default values
            if (knowledgeScoreText != null)
                knowledgeScoreText.text = "Knowledge Acquisition: 0/100";
            if (behavioralScoreText != null)
                behavioralScoreText.text = "Behavioral Change: 0/100";
            if (engagementScoreText != null)
                engagementScoreText.text = "Engagement: 0/100";
            if (overallScoreText != null)
                overallScoreText.text = "Overall Effectiveness: 0/100";
            if (recommendationsText != null)
                recommendationsText.text = "No data available. Play the minigames to see your progress!";
            return;
        }

        if (knowledgeScoreText != null)
        {
            knowledgeScoreText.text = $"Knowledge Acquisition: {effectivenessReport.knowledgeAcquisitionScore:F1}/100";
        }

        if (behavioralScoreText != null)
        {
            behavioralScoreText.text = $"Behavioral Change: {effectivenessReport.behavioralChangeScore:F1}/100";
        }

        if (engagementScoreText != null)
        {
            engagementScoreText.text = $"Engagement: {effectivenessReport.engagementScore:F1}/100";
        }

        if (overallScoreText != null)
        {
            overallScoreText.text = $"Overall Effectiveness: {effectivenessReport.overallEffectivenessScore:F1}/100";
        }

        if (recommendationsText != null && effectivenessReport.recommendations != null)
        {
            recommendationsText.text = string.Join("\n• ", effectivenessReport.recommendations);
        }
    }

    /// <summary>
    /// Display unified performance data across all minigames
    /// </summary>
    private void DisplayPerformanceData()
    {
        if (performanceEvaluation == null)
        {
            // Show default values if no evaluation
            if (overallPerformanceScoreText != null)
                overallPerformanceScoreText.text = "Overall Performance: 0%";
            if (overallGradeText != null)
                overallGradeText.text = "Grade: N/A";
            if (didGoodJobText != null)
                didGoodJobText.text = "No data available. Play the minigames to see your performance!";
            // Reset sliders/fills to 0
            if (overallScoreSlider != null)
                overallScoreSlider.value = 0f;
            else if (overallScoreFill != null)
                overallScoreFill.fillAmount = 0f;
            if (deboningScoreSlider != null)
                deboningScoreSlider.value = 0f;
            else if (deboningScoreFill != null)
                deboningScoreFill.fillAmount = 0f;
            if (saltwaterScoreSlider != null)
                saltwaterScoreSlider.value = 0f;
            else if (saltwaterScoreFill != null)
                saltwaterScoreFill.fillAmount = 0f;
            if (freshwaterScoreSlider != null)
                freshwaterScoreSlider.value = 0f;
            else if (freshwaterScoreFill != null)
                freshwaterScoreFill.fillAmount = 0f;
            return;
        }

        Color overallColor = GetScoreColor(performanceEvaluation.overallScore);

        // Overall performance
        if (overallPerformanceScoreText != null)
        {
            overallPerformanceScoreText.text = $"Overall Performance: {performanceEvaluation.overallScore:F1}%";
            overallPerformanceScoreText.color = overallColor;
        }

        if (overallGradeText != null)
        {
            overallGradeText.text = $"Grade: {performanceEvaluation.grade}";
            overallGradeText.color = overallColor;
        }

        if (didGoodJobText != null)
        {
            if (performanceEvaluation.overallScore < 20f)
            {
                didGoodJobText.text = "You’re just getting started – keep practicing and try different strategies.";
            }
            else if (performanceEvaluation.overallScore < 40f)
            {
                didGoodJobText.text = "You’re making some progress, but you need to do more to protect the ecosystem.";
            }
            else if (performanceEvaluation.overallScore < 60f)
            {
                didGoodJobText.text = "You’re halfway there – focus on more sustainable choices to improve.";
            }
            else if (performanceEvaluation.overallScore < 80f)
            {
                didGoodJobText.text = "Good job – keep it up and aim for even more sustainable decisions!";
            }
            else
            {
                didGoodJobText.text = "Excellent work! You’re showing strong sustainable fishing practices!";
            }

            didGoodJobText.color = overallColor;
        }

        // Overall score - Support both Slider and Image fillAmount
        if (overallScoreSlider != null)
        {
            overallScoreSlider.value = performanceEvaluation.overallScore;
            overallScoreSlider.interactable = false;
        }
        else if (overallScoreFill != null)
        {
            overallScoreFill.fillAmount = performanceEvaluation.overallScore / 100f;
            overallScoreFill.color = overallColor;
        }

        // Minigame breakdown - Support both Slider and Image fillAmount
        if (unifiedReport != null)
        {
            // Deboning Score
            if (deboningScoreText != null)
            {
                deboningScoreText.text = $"Deboning: {unifiedReport.deboningData.performanceScore:F1}%";
            }
            if (deboningScoreSlider != null)
            {
                deboningScoreSlider.value = unifiedReport.deboningData.performanceScore;
                deboningScoreSlider.interactable = false;
            }
            else if (deboningScoreFill != null)
            {
                deboningScoreFill.fillAmount = unifiedReport.deboningData.performanceScore / 100f;
                deboningScoreFill.color = GetScoreColor(unifiedReport.deboningData.performanceScore);
            }

            // Saltwater Score
            if (saltwaterScoreText != null)
            {
                saltwaterScoreText.text = $"Saltwater Fishing: {unifiedReport.saltwaterData.performanceScore:F1}%";
            }
            if (saltwaterScoreSlider != null)
            {
                saltwaterScoreSlider.value = unifiedReport.saltwaterData.performanceScore;
                saltwaterScoreSlider.interactable = false;
            }
            else if (saltwaterScoreFill != null)
            {
                saltwaterScoreFill.fillAmount = unifiedReport.saltwaterData.performanceScore / 100f;
                saltwaterScoreFill.color = GetScoreColor(unifiedReport.saltwaterData.performanceScore);
            }

            // Freshwater Score
            if (freshwaterScoreText != null)
            {
                freshwaterScoreText.text = $"Freshwater Fishing: {unifiedReport.freshwaterData.performanceScore:F1}%";
            }
            if (freshwaterScoreSlider != null)
            {
                freshwaterScoreSlider.value = unifiedReport.freshwaterData.performanceScore;
                freshwaterScoreSlider.interactable = false;
            }
            else if (freshwaterScoreFill != null)
            {
                freshwaterScoreFill.fillAmount = unifiedReport.freshwaterData.performanceScore / 100f;
                freshwaterScoreFill.color = GetScoreColor(unifiedReport.freshwaterData.performanceScore);
            }
        }

        // Detailed scores
        if (skillScoreText != null)
        {
            skillScoreText.text = $"Skill Score: {performanceEvaluation.skillScore:F1}%";
        }

        if (decisionScoreText != null)
        {
            decisionScoreText.text = $"Decision Score: {performanceEvaluation.decisionScore:F1}%";
        }

        if (diversityScoreText != null)
        {
            diversityScoreText.text = $"Diversity Score: {performanceEvaluation.diversityScore:F1}%";
        }

        if (feedbackText != null)
        {
            feedbackText.text = performanceEvaluation.feedback;
        }

        // Strengths, Weaknesses, Improvement Areas
        UpdateListContainer(strengthsContainer, performanceEvaluation.strengths, "✓ ");
        UpdateListContainer(weaknessesContainer, performanceEvaluation.weaknesses, "⚠ ");
        UpdateListContainer(improvementAreasContainer, performanceEvaluation.improvementAreas, "→ ");

        // Persist performance panel data separately
        if (PerformancePanelDataStore.Instance != null &&
            (HasValidUnifiedData(unifiedReport) || HasValidPerformanceEvaluation(performanceEvaluation)))
        {
            PerformancePanelDataStore.Instance.SaveSnapshot(unifiedReport, performanceEvaluation);
        }
    }

    private void UpdateListContainer(Transform container, List<string> items, string prefix = "")
    {
        if (container == null) return;

        ClearContainer(container);

        if (listItemPrefab != null)
        {
            foreach (string item in items)
            {
                GameObject itemObj = Instantiate(listItemPrefab, container);
                TextMeshProUGUI text = itemObj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = prefix + item;
                }
            }
        }
        else
        {
            // Fallback: create simple text items
            foreach (string item in items)
            {
                GameObject textObj = new GameObject("ListItem");
                textObj.transform.SetParent(container);
                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = prefix + item;
                text.fontSize = 16;
            }
        }
    }

    private Color GetScoreColor(float score)
    {
        float clamped = Mathf.Clamp(score, 0f, 100f);

        if (clamped <= 10f) return new Color(0.8f, 0f, 0f);          // 0–10% deep red
        if (clamped <= 20f) return new Color(0.85f, 0.25f, 0f);      // 11–20% red‑orange
        if (clamped <= 30f) return new Color(0.9f, 0.45f, 0f);       // 21–30% orange
        if (clamped <= 40f) return new Color(0.9f, 0.65f, 0.05f);    // 31–40% orange‑yellow
        if (clamped <= 50f) return new Color(0.9f, 0.85f, 0.1f);     // 41–50% yellow
        if (clamped <= 60f) return new Color(0.75f, 0.9f, 0.1f);     // 51–60% yellow‑green
        if (clamped <= 70f) return new Color(0.55f, 0.9f, 0.15f);    // 61–70% light green
        if (clamped <= 80f) return new Color(0.35f, 0.85f, 0.2f);    // 71–80% medium green
        if (clamped <= 90f) return new Color(0.2f, 0.8f, 0.25f);     // 81–90% rich green
        return new Color(0.05f, 0.7f, 0.2f);                         // 91–100% deep green
    }

    private void ShowPanel(GameObject panel)
    {
        // Hide all panels
        if (sustainabilityPanel != null) sustainabilityPanel.SetActive(false);
        if (achievementsPanel != null) achievementsPanel.SetActive(false);
        if (effectivenessPanel != null) effectivenessPanel.SetActive(false);
        if (decisionsPanel != null) decisionsPanel.SetActive(false);
        if (performancePanel != null) performancePanel.SetActive(false);
        if (DebtStatusPanel != null) DebtStatusPanel.SetActive(false);
        // Show selected panel
        if (panel != null)
        {
            panel.SetActive(true);
            
            // Refresh data when switching panels to ensure latest data is shown
            // This is especially important for the performance panel which shows unified analytics
            if (panel == achievementsPanel)
            {
                RefreshAchievements();
            }
            else if (panel == performancePanel)
            {
                // Refresh unified analytics when performance panel is shown
                LoadUnifiedAnalytics();
                DisplayPerformanceData();
            }
            else if (panel == sustainabilityPanel)
            {
                // Refresh sustainability data
                LoadAndDisplayData();
            }
        }
    }
    
    /// <summary>
    /// Refresh achievements display with latest data
    /// </summary>
    private void RefreshAchievements()
    {
        // Update achievements with latest report
        if (AchievementSystem.Instance != null && currentReport != null)
        {
            AchievementSystem.Instance.UpdateAchievements(currentReport);
        }
        
        // Redisplay achievements
        DisplayAchievements();
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        
        // Collect children first to avoid modification during iteration
        List<Transform> childrenToDestroy = new List<Transform>();
        foreach (Transform child in container)
        {
            if (child != null)
            {
                childrenToDestroy.Add(child);
            }
        }
        
        // Destroy collected children
        foreach (Transform child in childrenToDestroy)
        {
            if (child != null && child.gameObject != null)
            {
                // Only destroy at runtime, not in editor
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    // In editor, use DestroyImmediate
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    private void OnBackButtonClicked()
    {
        // Load main menu or previous scene
        // Change "MainMenu" to your actual main menu scene name
        string mainMenuScene = "DeboningMainScene"; // Update this to your main menu scene name
        
        if (UnityEngine.SceneManagement.SceneManager.GetSceneByName(mainMenuScene).IsValid())
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }
        else
        {
            // If main menu doesn't exist, try to go back to previous scene
            Debug.LogWarning($"Scene '{mainMenuScene}' not found. Please update the scene name in AchievementsSceneController.");
            // Alternative: Load scene by build index 0 (usually main menu)
            // UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
}

