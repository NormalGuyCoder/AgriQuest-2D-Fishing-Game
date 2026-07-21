using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    public static event Action<QuestData> OnQuestStarted;
    public static event Action<QuestData> OnQuestProgressUpdated;
    public static event Action<QuestData> OnQuestCompleted;
    public static event Action<QuestData> OnQuestTurnedIn;

    // NEW: Event for new game reset
    public static event Action OnNewGameReset;

    [Header("Configuration")]
    public TextAsset questDefinitionsJson;
    public bool autoSave = true;
    public bool debugMode = true;

    [Header("Save Settings")]
    public float autoSaveInterval = 60f;
    private float lastSaveTime = 0f;

    private QuestSaveData saveData;
    private QuestLoader questLoader;
    private QuestTriggerHandler triggerHandler;
    private QuestEventDispatcher eventDispatcher;
    private QuestDebugger questDebugger;

    // Track initialization state
    private bool isInitialized = false;
    private bool isNewGameReset = false;

    void Awake()
    {
        // Check if we're forcing a new game reset
        bool forceNewGame = PlayerPrefs.GetInt("ForceNewGameReset", 0) == 1;
        bool newGameRequested = PlayerPrefs.GetInt("NewGameRequested", 0) == 1;

        if (Instance != null && Instance != this)
        {
            if (forceNewGame || newGameRequested)
            {
                if (debugMode) Debug.Log("QuestManager: New game requested, destroying old instance");
                Destroy(Instance.gameObject);
                Instance = null;

                // Clear the flags
                if (forceNewGame)
                {
                    PlayerPrefs.SetInt("ForceNewGameReset", 0);
                    PlayerPrefs.Save();
                }
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Check if this is a new game (no save file exists)
            string saveFilePath = Path.Combine(Application.persistentDataPath, "quest_save.json");
            bool saveFileExists = File.Exists(saveFilePath);

            if ((forceNewGame || newGameRequested) && !saveFileExists)
            {
                isNewGameReset = true;
                if (debugMode) Debug.Log("QuestManager: New game detected, will force fresh initialization");
            }

            InitializeComponents();
        }
    }

    void Start()
    {
        triggerHandler?.InitializeInventoryListeners();

        if (debugMode)
        {
            var allQuests = GetAllQuests();
            Debug.Log($"Total quests after Start: {allQuests.Count}");

            // If this is a new game reset, force the first quest to be active
            if (isNewGameReset)
            {
                var firstQuest = GetQuest("ORIENTATION_DAY_0");
                if (firstQuest != null && firstQuest.state != QuestState.ACTIVE)
                {
                    if (debugMode) Debug.LogWarning("Force activating first quest for new game");
                    ForceActivateFirstQuest();
                }
            }
        }
    }

    void Update()
    {
        if (autoSave && Time.time - lastSaveTime > autoSaveInterval)
        {
            SaveData();
            lastSaveTime = Time.time;
        }
    }

    void OnDestroy()
    {
        SaveData();
        triggerHandler?.CleanupInventoryListeners();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveData();
    }

    void OnApplicationQuit()
    {
        SaveData();
    }

    private void InitializeComponents()
    {
        if (isInitialized && !isNewGameReset) return;

        questLoader = new QuestLoader(this);
        triggerHandler = new QuestTriggerHandler(this);
        eventDispatcher = new QuestEventDispatcher(this);
        questDebugger = new QuestDebugger(this);

        // FORCE fresh load for new game
        if (isNewGameReset)
        {
            // Delete existing save file to ensure fresh start
            string saveFilePath = Path.Combine(Application.persistentDataPath, "quest_save.json");
            if (File.Exists(saveFilePath))
            {
                try
                {
                    File.Delete(saveFilePath);
                    if (debugMode) Debug.Log($"QuestManager: Deleted old save file for new game: {saveFilePath}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to delete save file: {e.Message}");
                }
            }

            // Create completely fresh save data
            saveData = questLoader.CreateNewSaveData();
            isNewGameReset = false;

            // Clear the new game flag
            PlayerPrefs.SetInt("NewGameRequested", 0);
            PlayerPrefs.Save();
        }
        else
        {
            saveData = questLoader.LoadQuestData();
        }

        isInitialized = true;

        if (debugMode)
        {
            Debug.Log($"QuestManager initialized (New Game: {isNewGameReset})");
            Debug.Log($"Active quests on load: {saveData.activeQuestIds?.Count ?? 0}");

            // Log all quest states for debugging
            foreach (var quest in GetAllQuests())
            {
                Debug.Log($"Quest {quest.questId}: {quest.state}");
            }
        }

        // Trigger new game reset event
        if (isNewGameReset)
        {
            OnNewGameReset?.Invoke();
        }
    }

    private void ForceActivateFirstQuest()
    {
        var firstQuest = GetQuest("ORIENTATION_DAY_0");
        if (firstQuest != null && firstQuest.state != QuestState.ACTIVE)
        {
            if (debugMode) Debug.Log($"Force activating first quest: {firstQuest.questId}");

            // Activate the quest
            ActivateQuestManually("ORIENTATION_DAY_0");

            // Save immediately
            SaveData();
        }
    }

    // ============= PUBLIC METHODS FROM OLD VERSION =============

    // DataResetManager needs this
    public static void MarkForNewGame()
    {
        PlayerPrefs.SetInt("ForceNewGameReset", 1);
        PlayerPrefs.Save();
        Debug.Log("QuestManager marked for new game on next load");
    }

    // DialogInteractor and DialogSystem need these
    public void RecordTalkedToNPC(string npcName)
    {
        triggerHandler?.RecordTalkedToNPC(npcName);
        AutoSaveIfEnabled();
    }

    public void RecordItemObtained(string itemId, int amount)
    {
        triggerHandler?.RecordItemObtained(itemId, amount);
        AutoSaveIfEnabled();
    }

    public void RecordFishCaught(int amount = 1)
    {
        triggerHandler?.RecordFishCaught(amount);
        AutoSaveIfEnabled();
    }

    public void RecordFishSold(int amount = 1)
    {
        triggerHandler?.RecordFishSold(amount);
        AutoSaveIfEnabled();
    }

    public void RecordFishProcessed(int amount = 1)
    {
        triggerHandler?.RecordFishProcessed(amount);
        AutoSaveIfEnabled();
    }

    public void UpdateCoins(int coins)
    {
        triggerHandler?.UpdateCoins(coins);
        
        // Force update of active quests to check for completion
        var activeQuests = GetActiveQuests();
        foreach (var quest in activeQuests)
        {
            CheckQuestCompletion(quest);
        }
        
        AutoSaveIfEnabled();
    }

    public void UpdateSkillLevel(string skill, int level)
    {
        triggerHandler?.UpdateSkillLevel(skill, level);
        AutoSaveIfEnabled();
    }

    public void DeductGold(int amount)
    {
        triggerHandler?.DeductGold(amount);
        AutoSaveIfEnabled();
    }

    private void AutoSaveIfEnabled()
    {
        if (autoSave) SaveData();
    }

    // DialogSystem needs these
    public int GetFishCountForQuest(string targetId) => triggerHandler?.GetFishCountForQuest(targetId) ?? 0;

    public int GetSkillLevel(string skill) => triggerHandler?.GetSkillLevel(skill, saveData) ?? 0;

    public int GetReputation(string faction)
    {
        if (saveData != null && saveData.reputation.ContainsKey(faction))
            return saveData.reputation[faction];
        return 0;
    }

    public float GetPopulationHealth(string populationKey)
    {
        if (saveData != null && saveData.populationHealth.ContainsKey(populationKey))
            return saveData.populationHealth[populationKey];
        return 100f;
    }

    public bool HasTalkedToNPC(string npcName) => triggerHandler?.HasTalkedToNPC(npcName) ?? false;

    public bool IsTutorialCompleted(string tutorialKey)
    {
        if (saveData != null)
            return saveData.completedTutorials.Contains(tutorialKey);
        return false;
    }

    public void CompleteQuestManually(string questId) => eventDispatcher?.CompleteQuestManually(questId);

    // QuestTriggerHandler needs this
    public void InvokeQuestProgressUpdated(QuestData quest)
    {
        UpdateQuestProgress(quest);
        OnQuestProgressUpdated?.Invoke(quest);
        if (debugMode) Debug.Log($"Quest progress updated: {quest.questName}");
    }

    // ============= EXISTING PUBLIC METHODS =============

    // COMPLETE Reset method for new game
    public void ForceCompleteNewGameReset()
    {
        if (debugMode) Debug.Log("=== QuestManager: ForceCompleteNewGameReset ===");

        // 1. Delete save file
        string saveFilePath = Path.Combine(Application.persistentDataPath, "quest_save.json");
        try
        {
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                if (debugMode) Debug.Log($"Deleted save file: {saveFilePath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
        }

        // 2. Clear current save data
        saveData = null;

        // 3. Reinitialize components with fresh data
        isInitialized = false;
        isNewGameReset = true;

        // Destroy existing components
        questLoader = null;
        triggerHandler = null;
        eventDispatcher = null;
        questDebugger = null;

        // Reinitialize
        InitializeComponents();

        // 4. Force save
        SaveData();

        if (debugMode)
        {
            Debug.Log("QuestManager new game reset complete");
            var firstQuest = GetQuest("ORIENTATION_DAY_0");
            Debug.Log($"First quest state: {firstQuest?.state.ToString() ?? "NULL"}");
        }
    }

    // Mark for new game on next load
    public void MarkForNewGameInstance()
    {
        PlayerPrefs.SetInt("NewGameRequested", 1);
        PlayerPrefs.Save();
        if (debugMode) Debug.Log("QuestManager marked for new game on next load");
    }

    // Public methods for quest management
    public QuestSaveData GetSaveData()
    {
        return saveData;
    }

    public QuestData GetQuest(string questId)
    {
        return questLoader?.GetQuest(questId, saveData);
    }

    public List<QuestData> GetAllQuests()
    {
        return questLoader?.GetAllQuests(saveData) ?? new List<QuestData>();
    }

    public List<QuestData> GetActiveQuests()
    {
        return questLoader?.GetActiveQuests(saveData) ?? new List<QuestData>();
    }

    public List<QuestData> GetCompletedQuests()
    {
        return questLoader?.GetCompletedQuests(saveData) ?? new List<QuestData>();
    }

    public void UpdateQuestProgress(QuestData quest)
    {
        questLoader?.UpdateQuestProgress(quest, saveData);
        OnQuestProgressUpdated?.Invoke(quest);
        if (autoSave) SaveData();
    }

    public void CheckQuestCompletion(QuestData quest)
    {
        eventDispatcher?.CheckQuestCompletion(quest);
    }

    public void ActivateQuestManually(string questId)
    {
        eventDispatcher?.ActivateQuestManually(questId);
    }

    public void InvokeQuestStarted(QuestData quest)
    {
        OnQuestStarted?.Invoke(quest);
    }

    public void InvokeQuestCompleted(QuestData quest)
    {
        OnQuestCompleted?.Invoke(quest);
    }

    public void InvokeQuestTurnedIn(QuestData quest)
    {
        OnQuestTurnedIn?.Invoke(quest);
    }

    public void SaveData()
    {
        if (debugMode) Debug.Log("QuestManager: Saving data...");

        if (saveData != null && questLoader != null)
        {
            questLoader.SaveData(saveData);
        }
    }

    public bool HasAvailableQuestForNPC(string npcName) => questLoader?.HasAvailableQuestForNPC(npcName, saveData) ?? false;

    public bool IsNPCInvolvedInActiveQuest(string npcName) => questLoader?.IsNPCInvolvedInActiveQuest(npcName, saveData) ?? false;

    public int GetPlayerCoins() => saveData?.playerCoins ?? 0;

    public void ResetQuestManually(string questId) => eventDispatcher?.ResetQuestManually(questId);

    public void ResetAllQuests() => questLoader?.ResetAllQuests();

    // Component access methods
    public T GetComponent<T>() where T : class
    {
        if (typeof(T) == typeof(QuestTriggerHandler)) return triggerHandler as T;
        if (typeof(T) == typeof(QuestEventDispatcher)) return eventDispatcher as T;
        if (typeof(T) == typeof(QuestDebugger)) return questDebugger as T;
        if (typeof(T) == typeof(QuestLoader)) return questLoader as T;
        return null;
    }

    public string GetSaveFilePath()
    {
        return questLoader?.GetSaveFilePath() ?? "QuestLoader not initialized";
    }

    public void PrintDebugInfo()
    {
        Debug.Log($"Active Quests: {GetActiveQuests().Count}");
        Debug.Log($"Completed Quests: {GetCompletedQuests().Count}");
        Debug.Log($"Player Coins: {GetPlayerCoins()}");

        if (saveData != null)
        {
            Debug.Log($"Save Data Info:");
            Debug.Log($"- Last Save Time: {saveData.lastSaveTime}");
            Debug.Log($"- Talked to NPCs: {saveData.talkedToNPCs?.Count ?? 0}");
            Debug.Log($"- Quest Progress Entries: {saveData.questProgress?.Count ?? 0}");
        }
    }
}