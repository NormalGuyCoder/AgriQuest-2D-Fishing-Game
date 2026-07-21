using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Controls the score panel UI in the main menu.
/// Displays player's total score, session score, and statistics.
/// </summary>
public class ScorePanelController : MonoBehaviour
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

    [Header("Mistakes Statistics")]
    [Tooltip("Optional: Display total mistakes")]
    public TextMeshProUGUI totalMistakesText;
    [Tooltip("Optional: Display average mistakes per game")]
    public TextMeshProUGUI averageMistakesText;

    [Header("Fish Statistics")]
    [Tooltip("Optional: Display number of unique fish deboned")]
    public TextMeshProUGUI uniqueFishCountText;
    [Tooltip("Optional: Display most played fish")]
    public TextMeshProUGUI mostPlayedFishText;
    [Tooltip("Optional: Container for displaying fish statistics list")]
    public Transform fishStatsContainer;
    [Tooltip("Optional: Prefab for displaying individual fish statistics")]
    public GameObject fishStatItemPrefab;
    [Tooltip("Optional: Title text for fish statistics section")]
    public TextMeshProUGUI fishStatsTitleText;

    [Header("Recent Games")]
    public Transform recentGamesContainer;
    public GameObject recentGameItemPrefab;
    public TextMeshProUGUI recentGamesTitleText;

    [Header("Additional Info")]
    public TextMeshProUGUI lastUpdatedText;

    private ScoreManager scoreManager;

    void Start()
    {
        // Find ScoreManager
        scoreManager = ScoreManager.Instance;
        if (scoreManager == null)
        {
            // Create ScoreManager if it doesn't exist
            GameObject scoreManagerObj = new GameObject("ScoreManager");
            scoreManager = scoreManagerObj.AddComponent<ScoreManager>();
        }

        // Set up buttons
        if (openScorePanelButton != null)
        {
            openScorePanelButton.onClick.RemoveAllListeners();
            openScorePanelButton.onClick.AddListener(OnOpenScorePanel);
            Debug.Log("ScorePanelController: Open Score Panel button connected.");
        }
        else
        {
            Debug.LogWarning("ScorePanelController: Open Score Panel Button is not assigned! Please assign it in the Inspector.");
        }

        if (closeScorePanelButton != null)
        {
            closeScorePanelButton.onClick.RemoveAllListeners();
            closeScorePanelButton.onClick.AddListener(OnCloseScorePanel);
        }
        else
        {
            Debug.LogWarning("ScorePanelController: Close Score Panel Button is not assigned! Please assign it in the Inspector.");
        }

        // Check if score panel is assigned
        if (scorePanel == null)
        {
            Debug.LogError("ScorePanelController: Score Panel GameObject is not assigned! Please assign the score panel GameObject in the Inspector.");
        }
        else
        {
            // Hide panel initially (if not already hidden via MainMenuController)
            // The panel should be unchecked in the Inspector to match other panels
            if (scorePanel.activeSelf)
            {
                scorePanel.SetActive(false);
            }
        }
    }

    void OnEnable()
    {
        // Refresh when panel is enabled (e.g., returning to main menu)
        if (scorePanel != null && scorePanel.activeSelf)
        {
            RefreshScoreDisplay();
        }
    }

    public void OnOpenScorePanel()
    {
        // Let MainMenuController handle panel visibility (it manages all panels)
        MainMenuController mainMenu = FindObjectOfType<MainMenuController>();
        if (mainMenu != null)
        {
            mainMenu.OnOpenScorePanel();
        }
        else
        {
            // Fallback: If MainMenuController doesn't exist, handle it ourselves
            if (scorePanel != null)
            {
                scorePanel.SetActive(true);
                RefreshScoreDisplay();
            }
            else
            {
                Debug.LogError("ScorePanelController: Score Panel is not assigned! Please assign it in the Inspector.");
            }
        }
    }

    public void OnCloseScorePanel()
    {
        // Return to main menu
        MainMenuController mainMenu = FindObjectOfType<MainMenuController>();
        if (mainMenu != null)
        {
            mainMenu.ShowMainMenu();
        }
        else
        {
            // Fallback: If MainMenuController doesn't exist, hide panel directly
            if (scorePanel != null)
            {
                scorePanel.SetActive(false);
            }
            else
            {
                Debug.LogError("ScorePanelController: Score Panel is not assigned! Cannot close panel.");
            }
        }
    }
    
    /// <summary>
    /// Public method to refresh score display (called from MainMenuController)
    /// </summary>
    public void RefreshScoreDisplay()
    {
        if (scoreManager == null)
        {
            Debug.LogWarning("ScorePanelController: ScoreManager is null!");
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
            highestScoreText.text = $"Best Game: {scoreManager.GetHighestSingleGameScore():N0}";
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

        // Update mistakes statistics
        if (totalMistakesText != null)
        {
            totalMistakesText.text = $"Total Mistakes: {scoreManager.GetTotalMistakes()}";
        }

        if (averageMistakesText != null)
        {
            float avgMistakes = scoreManager.GetAverageMistakes();
            averageMistakesText.text = $"Avg Mistakes/Game: {avgMistakes:F1}";
        }

        // Update fish statistics
        if (uniqueFishCountText != null)
        {
            int uniqueCount = scoreManager.GetUniqueFishCount();
            uniqueFishCountText.text = $"Unique Fish Deboned: {uniqueCount}";
        }

        if (mostPlayedFishText != null)
        {
            ScoreManager.FishStatistics mostPlayed = scoreManager.GetMostPlayedFish();
            if (mostPlayed != null)
            {
                mostPlayedFishText.text = $"Most Played: {mostPlayed.fishName} ({mostPlayed.timesDeboned}x)";
            }
            else
            {
                mostPlayedFishText.text = "Most Played: None";
            }
        }

        // Update fish statistics list
        RefreshFishStatistics();

        // Update last updated time
        if (lastUpdatedText != null)
        {
            lastUpdatedText.text = $"Last Updated: {System.DateTime.Now.ToString("HH:mm:ss")}";
        }

        // Update recent games list
        RefreshRecentGames();
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

        // Get all fish statistics sorted by play count
        List<ScoreManager.FishStatistics> fishStats = scoreManager.GetFishStatisticsSortedByPlayCount();

        if (fishStats == null || fishStats.Count == 0)
        {
            if (fishStatsTitleText != null)
            {
                fishStatsTitleText.text = "Fish Statistics: No fish deboned yet";
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
            ScoreManager.FishStatistics stats = fishStats[i];
            GameObject itemObj = Instantiate(fishStatItemPrefab, fishStatsContainer);
            
            // Try to find text components in the prefab
            TextMeshProUGUI[] texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
            
            // Format: "Fish Name - Played: X, Best: Y, Avg Mistakes: Z"
            if (texts.Length > 0)
            {
                texts[0].text = $"{stats.fishName}";
            }
            
            if (texts.Length > 1)
            {
                texts[1].text = $"Played: {stats.timesDeboned}x | Best: {stats.bestScore:N0} | Avg Mistakes: {stats.averageMistakes:F1}";
            }
        }
    }

    private void RefreshRecentGames()
    {
        if (recentGamesContainer == null || recentGameItemPrefab == null || scoreManager == null)
            return;

        // Clear existing items
        foreach (Transform child in recentGamesContainer)
        {
            Destroy(child.gameObject);
        }

        // Get recent games (last 5)
        List<ScoreManager.GameSessionData> recentGames = scoreManager.GetSessionHistory(5);

        if (recentGames == null || recentGames.Count == 0)
        {
            if (recentGamesTitleText != null)
            {
                recentGamesTitleText.text = "Recent Games: No games played yet";
            }
            return;
        }

        if (recentGamesTitleText != null)
        {
            recentGamesTitleText.text = $"Recent Games ({recentGames.Count})";
        }

        // Create UI items for each recent game (in reverse order - newest first)
        for (int i = recentGames.Count - 1; i >= 0; i--)
        {
            ScoreManager.GameSessionData gameData = recentGames[i];
            GameObject itemObj = Instantiate(recentGameItemPrefab, recentGamesContainer);
            
            // Try to find text components in the prefab
            TextMeshProUGUI[] texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
            
            // Format: "Fish Name - Score (Time Bonus) - Mistakes: X"
            if (texts.Length > 0)
            {
                string timeBonusText = gameData.timeBonus > 0 ? $" (+{gameData.timeBonus})" : "";
                string mistakesText = gameData.mistakes > 0 ? $" | Mistakes: {gameData.mistakes}" : "";
                string timeTakenText = gameData.timeTaken > 0f ? $" | Time: {gameData.timeTaken:F1}s" : "";
                string bonesRemovedText = gameData.bonesRemoved > 0 ? $" | Bones: {gameData.bonesRemoved}" : "";
                texts[0].text = $"{gameData.fishName}: {gameData.score:N0}{timeBonusText}{mistakesText}{timeTakenText}{bonesRemovedText}";
            }
            
            // Optional: Add timestamp to second text if available
            if (texts.Length > 1)
            {
                texts[1].text = gameData.timestamp.ToString("MM/dd HH:mm");
            }
        }
    }

    /// <summary>
    /// Called by external scripts to refresh the display (e.g., after completing a game)
    /// </summary>
    public void UpdateDisplay()
    {
        if (scorePanel != null && scorePanel.activeSelf)
        {
            RefreshScoreDisplay();
        }
    }
}


