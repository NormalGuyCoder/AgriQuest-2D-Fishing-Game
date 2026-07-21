using UnityEngine;

/// <summary>
/// Central helper for wiping all player-facing progress and analytics.
/// Hook this up to a "New Game" or "Reset Data" button so that:
/// - JSON save files are cleared
/// - PlayerPrefs-based data is wiped
/// - Runtime singletons reset their in-memory state
/// After running this once, the game should behave as if it's a fresh install
/// (aside from any default starting values like initial debt or starting coins).
/// </summary>
public class PlayerDataResetManager : MonoBehaviour
{
    public static PlayerDataResetManager Instance { get; private set; }

    private void Awake()
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
    /// Reset ALL known player data across minigames, achievements, analytics,
    /// economy, inventories, and encyclopedia.
    /// Call this from a UI button to start a completely new game.
    /// </summary>
    public void ResetAllPlayerData()
    {
        // 1) Achievements & analytics JSON
        if (AchievementsDataStore.Instance != null)
        {
            AchievementsDataStore.Instance.DeleteAllSavedAnalytics();
        }

        if (PerformancePanelDataStore.Instance != null)
        {
            PerformancePanelDataStore.Instance.DeleteSavedPerformancePanel();
        }

        // 2) Achievement unlocks
        if (AchievementSystem.Instance != null)
        {
            AchievementSystem.Instance.ResetAchievements();
        }

        // 3) Sustainability metrics / decisions
        if (SustainableFishingMetrics.Instance != null)
        {
            SustainableFishingMetrics.Instance.ResetForNewGame();
        }

        // 4) Economy (wallet, debt, transactions)
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.ResetEconomy();
        }

        // 5) Deboning scores and history
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetAllScores();
        }

        // 6) Freshwater fishing scores and history
        if (FreshwaterFishingScoreManager.Instance != null)
        {
            FreshwaterFishingScoreManager.Instance.ResetAllScores();
        }

        // 7) Shared catch inventory across minigames
        if (FishInventoryManager.Instance != null)
        {
            FishInventoryManager.Instance.ResetAllData();
        }

        // 8) Detailed inventory for the saltwater fishing minigame
        if (DetailedFishInventory.Instance != null)
        {
            DetailedFishInventory.Instance.ClearInventory();
        }

        // 9) Per-fish analytics for the saltwater fishing minigame
        if (FishingStats.Instance != null)
        {
            FishingStats.Instance.ResetAllStats();
        }

        // 10) Encyclopedia discovery progress
        if (FishEncyclopedia.Instance != null)
        {
            FishEncyclopedia.Instance.ResetDiscoveredFish();
        }

        Debug.Log("PlayerDataResetManager: All player data has been reset for a new game.");
    }
}

