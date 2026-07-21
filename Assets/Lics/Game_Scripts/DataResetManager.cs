using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Reflection;

public class DataResetManager : MonoBehaviour
{
    public static DataResetManager Instance { get; private set; }

    [Header("Debug Settings")]
    public bool debugMode = true;

    [Header("Scene Settings")]
    public string startingScene = "Auditorium";

    [Header("Starting Values")]
    public int startingCoins = 100;
    public float startingWallet = 100.0f;
    public int startingDebt = 5000;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("DataResetManager initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetAllGameData()
    {
        Debug.Log("=== STARTING NEW GAME - RESETTING ALL DATA (MEMORY + JSON) ===");

        // CRITICAL: Mark QuestManager for new game BEFORE destroying
        try
        {
            QuestManager.MarkForNewGame();
            Debug.Log("✓ QuestManager marked for new game");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Could not mark QuestManager for new game: {e.Message}");
        }

        // 1. Reset JSON files FIRST
        ResetAllJSONData();

        // 2. Destroy existing managers
        DestroyExistingSingletons();

        // 3. Reset in-memory data
        ResetAllInMemoryData();

        // 4. Clear PlayerPrefs (keeping audio settings)
        ResetPlayerPrefs();

        // 5. Force garbage collection
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();

        Debug.Log("=== COMPLETE RESET FINISHED ===");
        Debug.Log($"✓ Memory cleared");
        Debug.Log($"✓ JSON files reset");
        Debug.Log($"✓ Starting coins: {startingCoins}");
        Debug.Log($"✓ Starting debt: {startingDebt}");

        // 6. Load starting scene
        LoadStartingScene();
    }

    private void DestroyExistingSingletons()
    {
        Debug.Log("Destroying existing singleton managers...");

        // 1. Reset specific systems that have internal state
        if (SustainableFishingMetrics.Instance != null)
        {
            SustainableFishingMetrics.Instance.ResetForNewGame();
        }

        if (AchievementSystem.Instance != null)
        {
            AchievementSystem.Instance.ResetAchievements();
        }

        if (AchievementsDataStore.Instance != null)
        {
            AchievementsDataStore.Instance.DeleteAllSavedAnalytics();
        }

        // 2. Destroy objects that use DontDestroyOnLoad
        string[] typesToDestroy = {
            "EconomyManager",
            "DetailedFishInventory",
            "QuestManager",
            "SustainableFishingMetrics",
            "AchievementSystem",
            "AchievementsDataStore",
            "FishInventoryManager",
            "FishEncyclopedia",
            "FishingStats",
            "FishCatchBridge",
            "PerformancePanelDataStore",
            "ScoreManager" // Deboning score manager
        };

        foreach (string typeName in typesToDestroy)
        {
            Type t = Type.GetType(typeName);
            if (t != null)
            {
                var objects = FindObjectsOfType(t, true);
                foreach (var obj in objects)
                {
                    if (obj is MonoBehaviour mb)
                    {
                        Debug.Log($"Destroying {typeName} instance on {mb.gameObject.name}...");
                        Destroy(mb.gameObject);
                    }
                }
            }
        }

        // Destroy QuestListUI components to force recreation
        var questUIs = FindObjectsOfType<QuestListUI>(true);
        foreach (var questUI in questUIs)
        {
            Destroy(questUI.gameObject);
            Debug.Log($"Destroyed QuestListUI: {questUI.name}");
        }

        // Clear static instance reference
        ClearStaticInstances();

        Debug.Log("Singleton destruction complete");
    }

    private void ClearStaticInstances()
    {
        string[] typesToClear = {
            "QuestManager",
            "EconomyManager",
            "DetailedFishInventory",
            "SustainableFishingMetrics",
            "AchievementSystem",
            "AchievementsDataStore",
            "FishInventoryManager"
        };

        foreach (string typeName in typesToClear)
        {
            try
            {
                Type t = Type.GetType(typeName);
                if (t != null)
                {
                    var instanceField = t.GetField("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    if (instanceField != null)
                    {
                        instanceField.SetValue(null, null);
                        Debug.Log($"Cleared {typeName}.Instance static field");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not clear static instance for {typeName}: {e.Message}");
            }
        }
    }

    private void ResetAllJSONData()
    {
        Debug.Log("Resetting all JSON save files...");

        string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        string persistentDataPath = Application.persistentDataPath;

        Debug.Log($"Persistent data path: {persistentDataPath}");

        // 1. Reset detailed_fish_inventory.json
        string fishInventoryPath = Path.Combine(persistentDataPath, "detailed_fish_inventory.json");
        string fishInventoryJson = @"{
    ""records"": [],
    ""coins"": " + startingCoins + @",
    ""currentInventoryCount"": 0,
    ""currentInventoryWeight"": 0.0,
    ""totalFishCaught"": 0,
    ""totalFishReleased"": 0,
    ""totalFishSold"": 0,
    ""totalWeightCaught"": 0.0,
    ""speciesInventoryCounts"": [],
    ""speciesInventoryWeights"": []
}";
        SafeWriteFile(fishInventoryPath, fishInventoryJson);

        // 2. Reset economy_state.json
        string economyPath = Path.Combine(persistentDataPath, "economy_state.json");
        string economyJson = @"{
    ""wallet"": " + startingWallet.ToString("0.0") + @",
    ""debt"": " + startingDebt + @".0,
    ""totalEarned"": 0.0,
    ""totalDebtPaid"": 0.0,
    ""transactions"": []
}";
        SafeWriteFile(economyPath, economyJson);

        // 3. CRITICAL: Reset quest_save.json with ORIENTATION_DAY_0 as ACTIVE
        string questSavePath = Path.Combine(persistentDataPath, "quest_save.json");
        string questSaveJson = @"{
    ""lastSaveTime"": """ + timestamp + @""",
    ""playerCoins"": " + startingCoins + @",
    ""talkedToNPCs"": [],
    ""completedTutorials"": [],
    ""questProgress"": [
        {
            ""questId"": ""ORIENTATION_DAY_0"",
            ""state"": 1,
            ""triggers"": [0, 0]
        }
    ]
}";
        SafeWriteFile(questSavePath, questSaveJson);

        // 4. Reset savegame.json
        string saveGamePath = Path.Combine(persistentDataPath, "savegame.json");
        string saveGameJson = @"{
    ""sceneName"": """ + startingScene + @""",
    ""playerPosition"": {
        ""x"": 0.0,
        ""y"": 0.0,
        ""z"": 0.0
    },
    ""lastSaveTime"": """ + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"""
}";
        SafeWriteFile(saveGamePath, saveGameJson);

        // 5. Reset market_trends.json
        string marketTrendsPath = Path.Combine(persistentDataPath, "market_trends.json");
        string marketTrendsJson = @"{
    ""lastUpdate"": """ + timestamp + @""",
    ""trends"": []
}";
        SafeWriteFile(marketTrendsPath, marketTrendsJson);

        // 6. Reset reputation.json
        string reputationPath = Path.Combine(persistentDataPath, "reputation.json");
        string reputationJson = @"{
    ""Greenvale"": 0,
    ""Saltyshore"": 0,
    ""Fresh Finds"": 0
}";
        SafeWriteFile(reputationPath, reputationJson);

        // 7. NEW: Delete all other progress-related files to be thorough
        string[] otherFilesToDelete = {
            "achievements_analytics.json",
            "performance_panel_data.json",
            "freshwater_fishing_scores.json",
            "deboning_scores.json",
            "sustainable_fishing_metrics.json",
            "analytics_summary.json"
        };

        foreach (string file in otherFilesToDelete)
        {
            string path = Path.Combine(persistentDataPath, file);
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    if (debugMode) Debug.Log($"✓ Deleted extra save file: {file}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not delete {file}: {e.Message}");
                }
            }
        }
    }

    private void ResetAllInMemoryData()
    {
        Debug.Log("Resetting all in-memory game data...");

        // Reset DetailedFishInventory
        if (DetailedFishInventory.Instance != null)
        {
            DetailedFishInventory.Instance.ClearInventory();
            DetailedFishInventory.Instance.SetCoins(startingCoins);
            if (debugMode) Debug.Log("✓ Reset DetailedFishInventory in memory");
        }

        // NEW: Reset EconomyManager
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.ResetEconomy();
            if (debugMode) Debug.Log("✓ Reset EconomyManager in memory");
        }

        // Reset FishInventoryManager
        if (FishInventoryManager.Instance != null)
        {
            FishInventoryManager.Instance.ClearInventory(false);
            if (debugMode) Debug.Log("✓ Reset FishInventoryManager in memory");
        }
        else
        {
            Debug.LogWarning("FishInventoryManager.Instance is null");
        }

        Debug.Log("In-memory reset complete");
    }

    private void SafeWriteFile(string filePath, string content)
    {
        try
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, content);
            if (debugMode)
            {
                Debug.Log($"✓ File saved: {Path.GetFileName(filePath)}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write file {Path.GetFileName(filePath)}: {e.Message}");
        }
    }

    private void ResetPlayerPrefs()
    {
        if (debugMode) Debug.Log("Resetting PlayerPrefs...");

        // Save current audio settings
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.298f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.0f);

        // Clear all PlayerPrefs
        PlayerPrefs.DeleteAll();

        // Restore audio settings
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);

        // Set game state flags
        PlayerPrefs.SetInt("FirstLaunch", 1);
        PlayerPrefs.SetInt("TutorialCompleted", 0);
        PlayerPrefs.SetString("PlayerName", "Fisher");
        PlayerPrefs.SetInt("GameStarted", 0);
        PlayerPrefs.SetInt("NewGameRequested", 1); // NEW: Flag for new game

        PlayerPrefs.Save();

        if (debugMode) Debug.Log("✓ PlayerPrefs reset");
    }

    private void LoadStartingScene()
    {
        Debug.Log($"Loading starting scene: {startingScene}");

        if (LevelManager.Instance != null)
        {
            Debug.Log("LevelManager found, loading scene with CrossFade");
            LevelManager.Instance.LoadScene(startingScene, "CrossFade");
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayMusic("RiversideBGM");
            }

            // Wait for scene to load then verify
            StartCoroutine(VerifyNewGameAfterLoad());
        }
        else
        {
            Debug.LogError("LevelManager.Instance is null! Cannot load starting scene.");

            // Try direct scene loading as fallback
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(startingScene);
                Debug.Log($"Loaded {startingScene} directly via SceneManager");

                // Start verification anyway
                StartCoroutine(VerifyNewGameAfterLoad());
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load scene {startingScene}: {e.Message}");
            }
        }
    }

    private IEnumerator VerifyNewGameAfterLoad()
    {
        Debug.Log("Starting verification after scene load...");

        // Wait for scene to load
        yield return new WaitForSeconds(2.0f);

        // Wait for QuestManager to initialize
        yield return WaitForQuestManager();

        // Force QuestManager to reload fresh data
        ForceQuestManagerReset();

        // NEW: Wait for QuestListUI to be ready
        yield return WaitForQuestUIReady();

        // Force refresh all QuestListUI components
        ForceRefreshAllQuestUI();

        // Verify the reset
        VerifyQuestReset();
    }

    private IEnumerator WaitForQuestManager()
    {
        int maxAttempts = 30;
        int attempts = 0;

        Debug.Log("Waiting for QuestManager to initialize...");

        while (QuestManager.Instance == null && attempts < maxAttempts)
        {
            attempts++;
            if (attempts % 10 == 0)
            {
                Debug.Log($"Still waiting for QuestManager... ({attempts}/{maxAttempts})");
            }
            yield return new WaitForSeconds(0.1f);
        }

        if (QuestManager.Instance != null)
        {
            Debug.Log("✓ QuestManager found after new game start");
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.LogError("QuestManager not found after new game start!");
        }
    }

    // NEW: Wait for QuestListUI to be ready
    private IEnumerator WaitForQuestUIReady()
    {
        Debug.Log("Waiting for QuestListUI to be ready...");

        int maxAttempts = 50; // 5 seconds total
        int attempts = 0;
        int uiReadyCount = 0;

        while (attempts < maxAttempts)
        {
            attempts++;

            // Find ALL QuestListUI components
            var questUIs = FindObjectsOfType<QuestListUI>(true);

            if (questUIs.Length == 0)
            {
                // No QuestListUI found yet
                if (attempts % 10 == 0)
                {
                    Debug.Log($"No QuestListUI found yet... ({attempts}/{maxAttempts})");
                }
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            // Check if all QuestListUI components are ready
            bool allReady = true;
            foreach (var questUI in questUIs)
            {
                if (!IsQuestListUIReady(questUI))
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady)
            {
                uiReadyCount++;
                if (uiReadyCount >= 3) // Require 3 consecutive checks to be stable
                {
                    Debug.Log($"✓ All {questUIs.Length} QuestListUI components are ready (attempt {attempts})");
                    yield return new WaitForSeconds(0.5f); // Extra safety wait
                    yield break;
                }
            }
            else
            {
                uiReadyCount = 0; // Reset counter if not all ready

                if (attempts % 10 == 0)
                {
                    Debug.Log($"Waiting for QuestListUI to initialize... ({attempts}/{maxAttempts})");
                    Debug.Log($"Found {questUIs.Length} QuestListUI components");

                    // Log which ones are not ready
                    foreach (var questUI in questUIs)
                    {
                        Debug.Log($"  - {questUI.name}: Ready={IsQuestListUIReady(questUI)}");
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        Debug.LogWarning($"QuestListUI not ready after {maxAttempts} attempts. Forcing refresh anyway.");
    }

    // NEW: Helper method to check if QuestListUI is ready
    private bool IsQuestListUIReady(QuestListUI questUI)
    {
        if (questUI == null) return false;

        try
        {
            // Use reflection to check if QuestListUI is initialized
            Type questUIType = questUI.GetType();

            // Check isInitialized field
            var isInitializedField = questUIType.GetField("isInitialized",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (isInitializedField != null)
            {
                bool isInitialized = (bool)isInitializedField.GetValue(questUI);
                if (!isInitialized) return false;
            }

            // Check contentParent
            var contentParentField = questUIType.GetField("contentParent",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (contentParentField != null)
            {
                var contentParent = contentParentField.GetValue(questUI) as Transform;
                if (contentParent == null) return false;
            }

            // Check questPanel
            var questPanelField = questUIType.GetField("questPanel",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (questPanelField != null)
            {
                var questPanel = questPanelField.GetValue(questUI) as GameObject;
                if (questPanel == null) return false;
            }

            // Check QuestManager reference
            if (QuestManager.Instance == null) return false;

            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error checking QuestListUI readiness: {e.Message}");
            return false;
        }
    }

    private void ForceQuestManagerReset()
    {
        try
        {
            var questManager = QuestManager.Instance;
            if (questManager != null)
            {
                // Use reflection to call the reset method
                Type questManagerType = questManager.GetType();
                var resetMethod = questManagerType.GetMethod("ForceCompleteNewGameReset",
                    BindingFlags.Public | BindingFlags.Instance);

                if (resetMethod != null)
                {
                    resetMethod.Invoke(questManager, null);
                    Debug.Log("✓ Forced QuestManager new game reset");
                }
                else
                {
                    Debug.LogError("ForceCompleteNewGameReset method not found in QuestManager!");

                    // Try alternative method
                    var forceResetMethod = questManagerType.GetMethod("ForceCompleteReset",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (forceResetMethod != null)
                    {
                        forceResetMethod.Invoke(questManager, null);
                        Debug.Log("✓ Used ForceCompleteReset as fallback");
                    }
                    else
                    {
                        // Last resort: manually set first quest to active
                        questManager.ActivateQuestManually("ORIENTATION_DAY_0");
                        Debug.Log("✓ Manually activated ORIENTATION_DAY_0 as fallback");
                    }
                }
            }
            else
            {
                Debug.LogWarning("QuestManager.Instance is null, cannot force reset");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to force QuestManager reset: {e.Message}");
        }
    }

    private void ForceRefreshAllQuestUI()
    {
        Debug.Log("Forcing refresh of all QuestListUI components...");

        // Find ALL QuestListUI components (including inactive ones)
        var questUIs = FindObjectsOfType<QuestListUI>(true);
        Debug.Log($"Found {questUIs.Length} QuestListUI components");

        if (questUIs.Length == 0)
        {
            Debug.LogWarning("No QuestListUI components found in scene!");
            return;
        }

        foreach (var questUI in questUIs)
        {
            if (questUI != null)
            {
                // Call the ForceCompleteUIRefresh method using reflection
                Type questUIType = questUI.GetType();
                var refreshMethod = questUIType.GetMethod("ForceCompleteUIRefresh",
                    BindingFlags.Public | BindingFlags.Instance);

                if (refreshMethod != null)
                {
                    refreshMethod.Invoke(questUI, null);
                    Debug.Log($"✓ ForceCompleteUIRefresh called on QuestListUI: {questUI.name}");
                }
                else
                {
                    Debug.LogWarning($"ForceCompleteUIRefresh method not found on {questUI.name}");

                    // Try calling ManualRefresh as fallback
                    var manualRefreshMethod = questUIType.GetMethod("ManualRefresh",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (manualRefreshMethod != null)
                    {
                        manualRefreshMethod.Invoke(questUI, null);
                        Debug.Log($"✓ ManualRefresh called on QuestListUI: {questUI.name}");
                    }
                }
            }
        }

        // Also refresh any QuestIndicator components
        var questIndicators = FindObjectsOfType<QuestIndicator>(true);
        foreach (var indicator in questIndicators)
        {
            if (indicator != null)
            {
                // QuestIndicator should automatically update via event subscription
                // But we can force it to check its state
                var updateMethod = indicator.GetType().GetMethod("UpdateBubbleState",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (updateMethod != null)
                {
                    updateMethod.Invoke(indicator, null);
                    Debug.Log($"✓ Refreshed QuestIndicator on {indicator.name}");
                }
            }
        }
    }

    private void VerifyQuestReset()
    {
        if (QuestManager.Instance != null)
        {
            var firstQuest = QuestManager.Instance.GetQuest("ORIENTATION_DAY_0");
            if (firstQuest != null)
            {
                Debug.Log($"=== NEW GAME VERIFICATION ===");
                Debug.Log($"First quest (ORIENTATION_DAY_0) state: {firstQuest.state}");
                Debug.Log($"Expected: ACTIVE (1), Got: {(int)firstQuest.state}");

                if (firstQuest.state == QuestState.ACTIVE)
                {
                    Debug.Log("✓ NEW GAME SUCCESS: Starting from Orientation Day!");

                    // Double-check by looking at active quests
                    var activeQuests = QuestManager.Instance.GetActiveQuests();
                    Debug.Log($"Active quests count: {activeQuests.Count}");

                    foreach (var quest in activeQuests)
                    {
                        Debug.Log($"  - {quest.questName} ({quest.questId})");
                    }

                    // Additional check: Verify UI is showing the quest
                    StartCoroutine(VerifyUIAfterDelay());
                }
                else
                {
                    Debug.LogError("✗ NEW GAME FAILED: Not starting from beginning!");

                    // Try one more time with direct activation
                    QuestManager.Instance.ActivateQuestManually("ORIENTATION_DAY_0");
                    Debug.Log("Manually activated ORIENTATION_DAY_0 as final attempt");
                }
            }
            else
            {
                Debug.LogError("First quest (ORIENTATION_DAY_0) not found!");
            }
        }
        else
        {
            Debug.LogError("QuestManager.Instance is null for verification!");
        }
    }

    // NEW: Verify UI is showing the quest after a delay
    private IEnumerator VerifyUIAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);

        Debug.Log("=== UI VERIFICATION ===");

        // Check QuestListUI components
        var questUIs = FindObjectsOfType<QuestListUI>(true);
        if (questUIs.Length == 0)
        {
            Debug.LogWarning("No QuestListUI found for verification");
        }
        else
        {
            foreach (var questUI in questUIs)
            {
                Debug.Log($"QuestListUI: {questUI.name}");

                // Check if quest panel is active
                Type questUIType = questUI.GetType();
                var questPanelField = questUIType.GetField("questPanel",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (questPanelField != null)
                {
                    var questPanel = questPanelField.GetValue(questUI) as GameObject;
                    if (questPanel != null)
                    {
                        Debug.Log($"  Quest panel active: {questPanel.activeSelf}");
                    }
                }

                // Check content parent children count
                var contentParentField = questUIType.GetField("contentParent",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (contentParentField != null)
                {
                    var contentParent = contentParentField.GetValue(questUI) as Transform;
                    if (contentParent != null)
                    {
                        int childCount = contentParent.childCount;
                        Debug.Log($"  Content parent children: {childCount}");

                        // Count actual quest slots (excluding prefab and message)
                        int questSlotCount = 0;
                        for (int i = 0; i < childCount; i++)
                        {
                            var child = contentParent.GetChild(i);
                            if (child.name.Contains("QuestSlot_"))
                            {
                                questSlotCount++;
                            }
                        }
                        Debug.Log($"  Quest slots displayed: {questSlotCount}");
                    }
                }
            }
        }
    }

    public bool HasExistingSaveData()
    {
        string[] saveFiles = {
            "quest_save.json",
            "detailed_fish_inventory.json",
            "savegame.json"
        };

        foreach (string file in saveFiles)
        {
            string path = Path.Combine(Application.persistentDataPath, file);
            if (File.Exists(path))
            {
                try
                {
                    string content = File.ReadAllText(path);
                    if (!string.IsNullOrEmpty(content) && content.Length > 50)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not read save file {file}: {e.Message}");
                }
            }
        }

        return false;
    }
}