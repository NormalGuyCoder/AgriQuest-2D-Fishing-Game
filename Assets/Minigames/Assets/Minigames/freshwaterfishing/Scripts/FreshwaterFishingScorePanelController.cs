using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Controls the score panel UI for freshwater fishing minigame.
/// Displays player's total score, session score, and statistics.
/// Similar to deboning minigame's ScorePanelController.
/// </summary>
public class FreshwaterFishingScorePanelController : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject scorePanel;
    public Button openScorePanelButton;
    public Button closeScorePanelButton;

    [Header("Score Display")]
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI sessionScoreText;
    public TextMeshProUGUI gamesPlayedText;
    public TextMeshProUGUI highestScoreText;
    public TextMeshProUGUI averageScoreText;
    public TextMeshProUGUI totalFishCaughtText;

    [Header("Fish Statistics")]
    [Tooltip("Display number of unique fish caught")]
    public TextMeshProUGUI uniqueFishCountText;
    [Tooltip("Display most caught fish")]
    public TextMeshProUGUI mostCaughtFishText;
    [Tooltip("Container for displaying fish statistics list")]
    public Transform fishStatsContainer;
    [Tooltip("Prefab for displaying individual fish statistics")]
    public GameObject fishStatItemPrefab;
    [Tooltip("Title text for fish statistics section")]
    public TextMeshProUGUI fishStatsTitleText;

    [Header("Recent Catches")]
    public Transform recentCatchesContainer;
    public GameObject recentCatchItemPrefab;
    public TextMeshProUGUI recentCatchesTitleText;

    [Header("Additional Info")]
    public TextMeshProUGUI lastUpdatedText;

    private FreshwaterFishingScoreManager scoreManager;

    void Start()
    {
        // Find ScoreManager
        scoreManager = FreshwaterFishingScoreManager.Instance;
        if (scoreManager == null)
        {
            // Create ScoreManager if it doesn't exist
            GameObject scoreManagerObj = new GameObject("FreshwaterFishingScoreManager");
            scoreManager = scoreManagerObj.AddComponent<FreshwaterFishingScoreManager>();
        }

        // Set up buttons
        if (openScorePanelButton != null)
        {
            openScorePanelButton.onClick.RemoveAllListeners();
            openScorePanelButton.onClick.AddListener(OnOpenScorePanel);
            Debug.Log("FreshwaterFishingScorePanelController: Open Score Panel button connected.");
        }
        else
        {
            Debug.LogWarning("FreshwaterFishingScorePanelController: Open Score Panel Button is not assigned!");
        }

        if (closeScorePanelButton != null)
        {
            closeScorePanelButton.onClick.RemoveAllListeners();
            closeScorePanelButton.onClick.AddListener(OnCloseScorePanel);
        }
        else
        {
            Debug.LogWarning("FreshwaterFishingScorePanelController: Close Score Panel Button is not assigned!");
        }

        // Check if score panel is assigned
        if (scorePanel == null)
        {
            Debug.LogError("FreshwaterFishingScorePanelController: Score Panel GameObject is not assigned!");
        }
        else
        {
            // Hide panel initially
            if (scorePanel.activeSelf)
            {
                scorePanel.SetActive(false);
            }
        }
    }

    void OnEnable()
    {
        // Refresh when panel is enabled
        if (scorePanel != null && scorePanel.activeSelf)
        {
            RefreshScoreDisplay();
        }
    }

    public void OnOpenScorePanel()
    {
        if (scorePanel != null)
        {
            scorePanel.SetActive(true);
            RefreshScoreDisplay();
        }
        else
        {
            Debug.LogError("FreshwaterFishingScorePanelController: Score Panel is not assigned!");
        }
    }

    public void OnCloseScorePanel()
    {
        if (scorePanel != null)
        {
            scorePanel.SetActive(false);
        }
        else
        {
            Debug.LogError("FreshwaterFishingScorePanelController: Score Panel is not assigned!");
        }
    }
    
    public void RefreshScoreDisplay()
    {
        if (scoreManager == null)
        {
            Debug.LogWarning("FreshwaterFishingScorePanelController: ScoreManager is null!");
            return;
        }

        // Update total score
        if (totalScoreText != null)
        {
            totalScoreText.text = $"Total Score: {scoreManager.GetTotalScore():N0}";
        }

        // Update session score
        if (sessionScoreText != null)
        {
            sessionScoreText.text = $"Session Score: {scoreManager.GetSessionScore():N0}";
        }

        // Update games played
        if (gamesPlayedText != null)
        {
            gamesPlayedText.text = $"Games Played: {scoreManager.GetGamesPlayed()}";
        }

        // Update highest single game score
        if (highestScoreText != null)
        {
            highestScoreText.text = $"Best Catch: {scoreManager.GetHighestSingleGameScore():N0}";
        }

        // Calculate and display average score
        if (averageScoreText != null)
        {
            int gamesPlayed = scoreManager.GetGamesPlayed();
            if (gamesPlayed > 0)
            {
                float average = scoreManager.GetTotalScore() / (float)gamesPlayed;
                averageScoreText.text = $"Average Score: {average:F0}";
            }
            else
            {
                averageScoreText.text = "Average Score: 0";
            }
        }

        // Update total fish caught
        if (totalFishCaughtText != null)
        {
            totalFishCaughtText.text = $"Total Fish Caught: {scoreManager.GetTotalFishCaught()}";
        }

        // Update fish statistics
        if (uniqueFishCountText != null)
        {
            int uniqueCount = scoreManager.GetUniqueFishCount();
            uniqueFishCountText.text = $"Unique Fish Caught: {uniqueCount}";
        }

        if (mostCaughtFishText != null)
        {
            FreshwaterFishingScoreManager.FishStatistics mostCaught = scoreManager.GetMostCaughtFish();
            if (mostCaught != null)
            {
                mostCaughtFishText.text = $"Most Caught: {mostCaught.fishName} ({mostCaught.timesCaught}x)";
            }
            else
            {
                mostCaughtFishText.text = "Most Caught: None";
            }
        }

        // Update fish statistics list
        RefreshFishStatistics();

        // Update last updated time
        if (lastUpdatedText != null)
        {
            lastUpdatedText.text = $"Last Updated: {System.DateTime.Now.ToString("HH:mm:ss")}";
        }

        // Update recent catches list
        RefreshRecentCatches();
    }

    private void RefreshFishStatistics()
    {
        if (fishStatsContainer == null || fishStatItemPrefab == null || scoreManager == null)
            return;

        // Clear existing items
        foreach (Transform child in fishStatsContainer)
        {
            Destroy(child.gameObject);
        }

        // Get all fish statistics sorted by catch count
        List<FreshwaterFishingScoreManager.FishStatistics> fishStats = scoreManager.GetFishStatisticsSortedByCatchCount();

        if (fishStats == null || fishStats.Count == 0)
        {
            if (fishStatsTitleText != null)
            {
                fishStatsTitleText.text = "Fish Statistics: No fish caught yet";
            }
            return;
        }

        if (fishStatsTitleText != null)
        {
            fishStatsTitleText.text = $"Fish Statistics ({fishStats.Count} unique fish)";
        }

        // Create UI items for each fish (limit to top 10)
        int displayCount = Mathf.Min(fishStats.Count, 10);
        for (int i = 0; i < displayCount; i++)
        {
            FreshwaterFishingScoreManager.FishStatistics stats = fishStats[i];
            GameObject itemObj = Instantiate(fishStatItemPrefab, fishStatsContainer);
            
            // Try to find text components in the prefab
            TextMeshProUGUI[] texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
            
            // Format: "Fish Name - Caught: X, Best: Y, Avg Time: Zs"
            if (texts.Length > 0)
            {
                texts[0].text = $"{stats.fishName}";
            }
            
            if (texts.Length > 1)
            {
                texts[1].text = $"Caught: {stats.timesCaught}x | Best: {stats.bestScore:N0} | Avg Time: {stats.averageCatchTime:F1}s";
            }
        }
    }

    private void RefreshRecentCatches()
    {
        if (recentCatchesContainer == null || recentCatchItemPrefab == null || scoreManager == null)
            return;

        // Clear existing items
        foreach (Transform child in recentCatchesContainer)
        {
            Destroy(child.gameObject);
        }

        // Get recent catches
        List<FreshwaterFishingScoreManager.FishingSessionData> recentCatches = scoreManager.GetSessionHistory(10);

        if (recentCatches == null || recentCatches.Count == 0)
        {
            if (recentCatchesTitleText != null)
            {
                recentCatchesTitleText.text = "Recent Catches: No catches yet";
            }
            return;
        }

        if (recentCatchesTitleText != null)
        {
            recentCatchesTitleText.text = $"Recent Catches ({recentCatches.Count})";
        }

        // Display recent catches (most recent first)
        for (int i = recentCatches.Count - 1; i >= 0; i--)
        {
            FreshwaterFishingScoreManager.FishingSessionData catchData = recentCatches[i];
            GameObject itemObj = Instantiate(recentCatchItemPrefab, recentCatchesContainer);
            
            TextMeshProUGUI[] texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
            
            if (texts.Length > 0)
            {
                texts[0].text = $"{catchData.fishName}";
            }
            
            if (texts.Length > 1)
            {
                texts[1].text = $"Score: {catchData.score:N0} | Time: {catchData.catchTime:F1}s | Exp: {catchData.experiencePoints} | Gold: {catchData.goldValue}";
            }
        }
    }
}

