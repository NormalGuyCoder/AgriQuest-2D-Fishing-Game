using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Comprehensive analytics dashboard that displays performance across all three minigames
/// and provides evaluation of whether the player did a "good job"
/// </summary>
public class UnifiedAnalyticsDashboard : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject overviewPanel;
    public GameObject minigameBreakdownPanel;
    public GameObject performanceEvaluationPanel;

    [Header("Overview UI")]
    public TextMeshProUGUI overallScoreText;
    public TextMeshProUGUI overallGradeText;
    public TextMeshProUGUI didGoodJobText;
    public Image overallScoreFill;
    public TextMeshProUGUI summaryText;

    [Header("Minigame Breakdown UI")]
    public TextMeshProUGUI deboningScoreText;
    public TextMeshProUGUI saltwaterScoreText;
    public TextMeshProUGUI freshwaterScoreText;
    public Image deboningScoreFill;
    public Image saltwaterScoreFill;
    public Image freshwaterScoreFill;

    [Header("Performance Evaluation UI")]
    public TextMeshProUGUI skillScoreText;
    public TextMeshProUGUI decisionScoreText;
    public TextMeshProUGUI diversityScoreText;
    public TextMeshProUGUI sustainabilityScoreText;
    public TextMeshProUGUI feedbackText;
    
    [Header("Strengths/Weaknesses UI")]
    public Transform strengthsContainer;
    public Transform weaknessesContainer;
    public Transform improvementAreasContainer;
    public GameObject listItemPrefab;

    [Header("Recommendations UI")]
    public Transform recommendationsContainer;
    public TextMeshProUGUI recommendationsTitleText;

    [Header("Navigation")]
    public Button overviewButton;
    public Button breakdownButton;
    public Button evaluationButton;
    public Button refreshButton;

    private UnifiedAnalyticsReport currentReport;
    private PlayerPerformanceEvaluation currentEvaluation;

    void Start()
    {
        EnsureManagersExist();
        SetupButtons();
        RefreshDashboard();
    }

    private void EnsureManagersExist()
    {
        if (UnifiedAnalyticsManager.Instance == null)
        {
            GameObject obj = new GameObject("UnifiedAnalyticsManager");
            obj.AddComponent<UnifiedAnalyticsManager>();
        }

        if (PlayerPerformanceEvaluator.Instance == null)
        {
            GameObject obj = new GameObject("PlayerPerformanceEvaluator");
            obj.AddComponent<PlayerPerformanceEvaluator>();
        }
    }

    private void SetupButtons()
    {
        if (overviewButton != null)
            overviewButton.onClick.AddListener(() => ShowPanel(overviewPanel));
        
        if (breakdownButton != null)
            breakdownButton.onClick.AddListener(() => ShowPanel(minigameBreakdownPanel));
        
        if (evaluationButton != null)
            evaluationButton.onClick.AddListener(() => ShowPanel(performanceEvaluationPanel));
        
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshDashboard);
    }

    public void RefreshDashboard()
    {
        // Get comprehensive report
        if (UnifiedAnalyticsManager.Instance != null)
        {
            currentReport = UnifiedAnalyticsManager.Instance.GetComprehensiveReport();
        }

        // Get performance evaluation
        if (PlayerPerformanceEvaluator.Instance != null)
        {
            currentEvaluation = PlayerPerformanceEvaluator.Instance.EvaluatePlayer();
        }

        // Update all displays
        UpdateOverview();
        UpdateMinigameBreakdown();
        UpdatePerformanceEvaluation();
    }

    private void UpdateOverview()
    {
        if (currentEvaluation == null) return;

        // Overall score
        if (overallScoreText != null)
        {
            overallScoreText.text = $"Overall Performance: {currentEvaluation.overallScore:F1}%";
        }

        if (overallGradeText != null)
        {
            overallGradeText.text = $"Grade: {currentEvaluation.grade}";
        }

        if (didGoodJobText != null)
        {
            didGoodJobText.text = currentEvaluation.didGoodJob 
                ? "✓ Good Job! You're demonstrating sustainable fishing practices!"
                : "⚠ Needs Improvement - Keep practicing sustainable fishing!";
            didGoodJobText.color = currentEvaluation.didGoodJob ? Color.green : Color.yellow;
        }

        if (overallScoreFill != null)
        {
            overallScoreFill.fillAmount = currentEvaluation.overallScore / 100f;
            overallScoreFill.color = GetScoreColor(currentEvaluation.overallScore);
        }

        if (summaryText != null)
        {
            summaryText.text = currentEvaluation.feedback;
        }
    }

    private void UpdateMinigameBreakdown()
    {
        if (currentReport == null) return;

        // Deboning
        if (deboningScoreText != null)
        {
            deboningScoreText.text = $"Deboning: {currentReport.deboningData.performanceScore:F1}%";
        }
        if (deboningScoreFill != null)
        {
            deboningScoreFill.fillAmount = currentReport.deboningData.performanceScore / 100f;
            deboningScoreFill.color = GetScoreColor(currentReport.deboningData.performanceScore);
        }

        // Saltwater
        if (saltwaterScoreText != null)
        {
            saltwaterScoreText.text = $"Saltwater Fishing: {currentReport.saltwaterData.performanceScore:F1}%";
        }
        if (saltwaterScoreFill != null)
        {
            saltwaterScoreFill.fillAmount = currentReport.saltwaterData.performanceScore / 100f;
            saltwaterScoreFill.color = GetScoreColor(currentReport.saltwaterData.performanceScore);
        }

        // Freshwater
        if (freshwaterScoreText != null)
        {
            freshwaterScoreText.text = $"Freshwater Fishing: {currentReport.freshwaterData.performanceScore:F1}%";
        }
        if (freshwaterScoreFill != null)
        {
            freshwaterScoreFill.fillAmount = currentReport.freshwaterData.performanceScore / 100f;
            freshwaterScoreFill.color = GetScoreColor(currentReport.freshwaterData.performanceScore);
        }
    }

    private void UpdatePerformanceEvaluation()
    {
        if (currentEvaluation == null) return;

        // Scores
        if (skillScoreText != null)
        {
            skillScoreText.text = $"Skill Score: {currentEvaluation.skillScore:F1}%";
        }

        if (decisionScoreText != null)
        {
            decisionScoreText.text = $"Decision Score: {currentEvaluation.decisionScore:F1}%";
        }

        if (diversityScoreText != null)
        {
            diversityScoreText.text = $"Diversity Score: {currentEvaluation.diversityScore:F1}%";
        }

        if (sustainabilityScoreText != null)
        {
            sustainabilityScoreText.text = $"Sustainability Score: {currentEvaluation.sustainabilityScore:F1}%";
        }

        if (feedbackText != null)
        {
            feedbackText.text = currentEvaluation.feedback;
        }

        // Strengths
        UpdateListContainer(strengthsContainer, currentEvaluation.strengths, "✓ ");

        // Weaknesses
        UpdateListContainer(weaknessesContainer, currentEvaluation.weaknesses, "⚠ ");

        // Improvement Areas
        UpdateListContainer(improvementAreasContainer, currentEvaluation.improvementAreas, "→ ");

        // Recommendations
        if (currentReport != null)
        {
            UpdateListContainer(recommendationsContainer, currentReport.recommendations, "• ");
            
            if (recommendationsTitleText != null)
            {
                recommendationsTitleText.text = $"Recommendations ({currentReport.recommendations.Count})";
            }
        }
    }

    private void UpdateListContainer(Transform container, List<string> items, string prefix = "")
    {
        if (container == null) return;

        // Clear existing items
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // Add new items
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
        if (overviewPanel != null) overviewPanel.SetActive(false);
        if (minigameBreakdownPanel != null) minigameBreakdownPanel.SetActive(false);
        if (performanceEvaluationPanel != null) performanceEvaluationPanel.SetActive(false);

        // Show selected panel
        if (panel != null) panel.SetActive(true);
    }
}

