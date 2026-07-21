using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class QuestLoader
{
    private readonly QuestManager manager;
    private string saveFilePath;
    private Dictionary<string, QuestData> questDefinitions = new Dictionary<string, QuestData>();

    public QuestLoader(QuestManager manager)
    {
        this.manager = manager;
        saveFilePath = Path.Combine(Application.persistentDataPath, "quest_save.json");
        LoadQuestDefinitions();
    }

    private void LoadQuestDefinitions()
    {
        if (manager.questDefinitionsJson == null)
        {
            Debug.LogError("QuestLoader: Quest definitions JSON not assigned!");
            return;
        }

        try
        {
            var wrapper = JsonUtility.FromJson<QuestListWrapper>(manager.questDefinitionsJson.text);
            questDefinitions.Clear();

            foreach (var quest in wrapper.quests)
            {
                var questClone = CloneQuestData(quest);
                questDefinitions[questClone.questId] = questClone;

                foreach (var trigger in questClone.completionTriggers)
                {
                    trigger.currentAmount = 0;
                }
            }
            
            if (manager.debugMode)
                Debug.Log($"Loaded {questDefinitions.Count} quest definitions");
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestLoader: Failed to load quest definitions: {e.Message}");
        }
    }

    private QuestData CloneQuestData(QuestData original)
    {
        var clone = new QuestData
        {
            questId = original.questId,
            questName = original.questName,
            description = original.description,
            rewardCoins = original.rewardCoins,
            giverNPC = original.giverNPC,
            locationHint = original.locationHint,
            prerequisiteLogic = original.prerequisiteLogic,
            state = original.state
        };

        foreach (var trigger in original.completionTriggers)
        {
            clone.completionTriggers.Add(new QuestTrigger
            {
                triggerType = trigger.triggerType,
                targetId = trigger.targetId,
                requiredAmount = trigger.requiredAmount,
                currentAmount = 0,
                sequential = trigger.sequential
            });
        }

        clone.requiredQuestIds = new List<string>(original.requiredQuestIds);
        clone.nextQuestIds = new List<string>(original.nextQuestIds);

        foreach (var item in original.itemRewards)
        {
            clone.itemRewards.Add(new QuestRewardItem(item.itemId, item.itemName, item.quantity));
        }

        return clone;
    }

    public QuestSaveData LoadQuestData()
    {
        QuestSaveData saveData;

        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                saveData = JsonUtility.FromJson<QuestSaveData>(json);

                if (saveData == null)
                {
                    if (manager.debugMode) Debug.LogWarning("Loaded save data is null, creating new");
                    saveData = CreateNewSaveData();
                }
                else
                {
                    // Initialize null collections
                    if (saveData.questProgress == null) saveData.questProgress = new List<QuestProgressEntry>();
                    if (saveData.talkedToNPCs == null) saveData.talkedToNPCs = new List<string>();
                    if (saveData.inventory == null) saveData.inventory = new Dictionary<string, int>();
                    if (saveData.skills == null) saveData.skills = new Dictionary<string, int>();
                    if (saveData.completedTutorials == null) saveData.completedTutorials = new List<string>();
                    if (saveData.populationHealth == null) saveData.populationHealth = new Dictionary<string, float>();
                    if (saveData.reputation == null) saveData.reputation = new Dictionary<string, int>();
                    if (saveData.gameFlags == null) saveData.gameFlags = new Dictionary<string, string>();

                    saveData.UpdateDerivedLists();

                    if (manager.debugMode)
                        Debug.Log($"QuestLoader: Successfully loaded save from {saveFilePath} (Quests: {saveData.questProgress.Count})");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"QuestLoader: Failed to load save: {e.Message}");
                saveData = CreateNewSaveData();
            }
        }
        else
        {
            saveData = CreateNewSaveData();
            if (manager.debugMode)
                Debug.Log($"QuestLoader: No save file found at {saveFilePath}, created new save data");
        }

        InitializeAllQuestsInSave(saveData);
        return saveData;
    }

    public QuestSaveData CreateNewSaveData()
    {
        var saveData = new QuestSaveData
        {
            playerCoins = 100,
            lastSaveTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
            questProgress = new List<QuestProgressEntry>(),
            talkedToNPCs = new List<string>(),
            inventory = new Dictionary<string, int>(),
            skills = new Dictionary<string, int>(),
            completedTutorials = new List<string>(),
            populationHealth = new Dictionary<string, float>(),
            reputation = new Dictionary<string, int>(),
            gameFlags = new Dictionary<string, string>()
        };

        saveData.skills["Fishing"] = 1;
        saveData.skills["Aquaculture"] = 1;
        saveData.skills["Trading"] = 1;

        saveData.populationHealth["Default"] = 100f;

        saveData.reputation["Greenvale"] = 0;
        saveData.reputation["Saltyshore"] = 0;
        saveData.reputation["Fresh Finds"] = 0;

        saveData.gameFlags["new_game"] = "true";

        return saveData;
    }

    private void InitializeAllQuestsInSave(QuestSaveData saveData)
    {
        if (manager.debugMode)
            Debug.Log($"QuestLoader: Initializing all quests in save (Definitions: {questDefinitions.Count})");

        // Only add quests that don't already exist in save data
        foreach (var questId in questDefinitions.Keys)
        {
            var existingEntry = saveData.questProgress.Find(p => p.questId == questId);
            if (existingEntry == null)
            {
                var questDef = questDefinitions[questId];
                var entry = new QuestProgressEntry
                {
                    questId = questId,
                    state = questDef.state
                };

                foreach (var trigger in questDef.completionTriggers)
                {
                    entry.triggers.Add(0);
                }

                saveData.questProgress.Add(entry);
                
                if (questDef.state == QuestState.ACTIVE && !saveData.activeQuestIds.Contains(questId))
                {
                    saveData.activeQuestIds.Add(questId);
                }
            }
        }

        // Ensure the first quest is always active for new games
        var firstQuestEntry = saveData.questProgress.Find(p => p.questId == "ORIENTATION_DAY_0");
        if (firstQuestEntry != null && saveData.activeQuestIds.Count == 0)
        {
            firstQuestEntry.state = QuestState.ACTIVE;
            if (!saveData.activeQuestIds.Contains("ORIENTATION_DAY_0"))
            {
                saveData.activeQuestIds.Add("ORIENTATION_DAY_0");
            }

            if (manager.debugMode)
                Debug.Log($"QuestLoader: Activated first quest: ORIENTATION_DAY_0");
        }

        saveData.UpdateDerivedLists();

        if (manager.debugMode)
        {
            Debug.Log($"QuestLoader: Initialized {saveData.questProgress.Count} quests");
            Debug.Log($"Active quests: {saveData.activeQuestIds.Count}");
            Debug.Log($"Completed quests: {saveData.completedQuestIds.Count}");
        }
    }

    public void SaveData(QuestSaveData saveData)
    {
        try
        {
            saveData.lastSaveTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

            // Ensure all lists are updated before saving
            saveData.UpdateDerivedLists();

            if (manager.debugMode)
            {
                DebugPrintSaveDataContents(saveData);
            }

            string json = JsonUtility.ToJson(saveData, true);

            Directory.CreateDirectory(Path.GetDirectoryName(saveFilePath));
            File.WriteAllText(saveFilePath, json);

            if (manager.debugMode)
            {
                Debug.Log($"QuestLoader: Saved JSON to {saveFilePath}");
                Debug.Log($"Save data contains {saveData.questProgress?.Count ?? 0} quest entries");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestLoader: Failed to save: {e.Message}");
        }
    }

    public void ResetAllQuests()
    {
        if (manager.debugMode)
            Debug.Log("QuestLoader: ResetAllQuests called");

        var saveData = CreateNewSaveData();

        foreach (var questId in questDefinitions.Keys)
        {
            var questDef = questDefinitions[questId];
            var entry = new QuestProgressEntry
            {
                questId = questId,
                state = questDef.state
            };

            foreach (var trigger in questDef.completionTriggers)
            {
                entry.triggers.Add(0);
            }

            saveData.questProgress.Add(entry);
            
            if (questDef.state == QuestState.ACTIVE && !saveData.activeQuestIds.Contains(questId))
            {
                saveData.activeQuestIds.Add(questId);
            }
        }

        saveData.UpdateDerivedLists();

        SaveData(saveData);

        if (manager.debugMode)
            Debug.Log($"QuestLoader: Reset complete - saved fresh data with {saveData.questProgress.Count} quests");
    }

    public QuestData GetQuestData(string questId, QuestSaveData saveData)
    {
        if (!questDefinitions.ContainsKey(questId)) return null;

        var quest = CloneQuestData(questDefinitions[questId]);
        var progress = saveData.questProgress.Find(p => p.questId == questId);

        if (progress != null)
        {
            quest.state = progress.state;

            for (int i = 0; i < Mathf.Min(progress.triggers.Count, quest.completionTriggers.Count); i++)
            {
                quest.completionTriggers[i].currentAmount =
                    Mathf.Min(progress.triggers[i], quest.completionTriggers[i].requiredAmount);
            }
        }
        else
        {
            quest.state = questDefinitions[questId].state;
            
            var entry = new QuestProgressEntry
            {
                questId = questId,
                state = quest.state
            };

            foreach (var trigger in quest.completionTriggers)
            {
                entry.triggers.Add(0);
            }

            saveData.questProgress.Add(entry);
            
            if (quest.state == QuestState.ACTIVE && !saveData.activeQuestIds.Contains(questId))
            {
                saveData.activeQuestIds.Add(questId);
                saveData.UpdateDerivedLists();
            }
        }

        return quest;
    }

    public void UpdateQuestProgress(QuestData quest, QuestSaveData saveData)
    {
        if (manager.debugMode)
        {
            Debug.Log($"QuestLoader.UpdateQuestProgress for {quest.questId}, State: {quest.state}");
            Debug.Log($"Triggers: {quest.completionTriggers.Count}");
            for (int i = 0; i < quest.completionTriggers.Count; i++)
            {
                var trigger = quest.completionTriggers[i];
                Debug.Log($"  Trigger {i}: {trigger.currentAmount}/{trigger.requiredAmount} ({trigger.targetId})");
            }
        }

        var progress = saveData.questProgress.Find(p => p.questId == quest.questId);

        if (progress == null)
        {
            progress = new QuestProgressEntry { questId = quest.questId };
            saveData.questProgress.Add(progress);
        }

        progress.state = quest.state;
        progress.triggers.Clear();

        foreach (var trigger in quest.completionTriggers)
        {
            progress.triggers.Add(trigger.currentAmount);
        }

        saveData.UpdateDerivedLists();

        if (manager.debugMode)
        {
            Debug.Log($"After update:");
            Debug.Log($"  questProgress count = {saveData.questProgress.Count}");
            Debug.Log($"  Active quests: {saveData.activeQuestIds.Count}");
            Debug.Log($"  Completed quests: {saveData.completedQuestIds.Count}");
            
            // Log specific states
            foreach (var q in saveData.activeQuestIds)
            {
                Debug.Log($"  Active: {q}");
            }
            foreach (var q in saveData.completedQuestIds)
            {
                Debug.Log($"  Completed: {q}");
            }
        }
    }

    public List<QuestData> GetActiveQuests(QuestSaveData saveData)
    {
        var list = new List<QuestData>();
        foreach (var questId in saveData.activeQuestIds)
        {
            var quest = GetQuestData(questId, saveData);
            if (quest != null) list.Add(quest);
        }
        return list;
    }

    public List<QuestData> GetAvailableQuests(QuestSaveData saveData) => new List<QuestData>();

    public List<QuestData> GetCompletedQuests(QuestSaveData saveData)
    {
        var list = new List<QuestData>();
        foreach (var questId in saveData.completedQuestIds)
        {
            var quest = GetQuestData(questId, saveData);
            if (quest != null) list.Add(quest);
        }
        return list;
    }

    public QuestData GetQuest(string questId, QuestSaveData saveData)
    {
        return GetQuestData(questId, saveData);
    }

    public List<QuestData> GetAllQuests(QuestSaveData saveData)
    {
        var list = new List<QuestData>();
        foreach (var questId in questDefinitions.Keys)
        {
            var quest = GetQuestData(questId, saveData);
            if (quest != null) list.Add(quest);
        }
        return list;
    }

    public bool HasAvailableQuestForNPC(string npcName, QuestSaveData saveData)
    {
        foreach (var questDef in questDefinitions.Values)
        {
            if (questDef.giverNPC == npcName)
            {
                var quest = GetQuestData(questDef.questId, saveData);
                if (quest != null && quest.state == QuestState.LOCKED)
                {
                    if (quest.requiredQuestIds.Count == 0) return true;

                    bool prerequisitesMet = false;

                    switch (quest.prerequisiteLogic)
                    {
                        case PrerequisiteLogic.ANY:
                            foreach (var reqId in quest.requiredQuestIds)
                            {
                                var reqQuest = GetQuest(reqId, saveData);
                                if (reqQuest != null && reqQuest.state == QuestState.COMPLETED)
                                {
                                    prerequisitesMet = true;
                                    break;
                                }
                            }
                            break;

                        case PrerequisiteLogic.ALL:
                        default:
                            prerequisitesMet = true;
                            foreach (var reqId in quest.requiredQuestIds)
                            {
                                var reqQuest = GetQuest(reqId, saveData);
                                if (reqQuest == null || reqQuest.state != QuestState.COMPLETED)
                                {
                                    prerequisitesMet = false;
                                    break;
                                }
                            }
                            break;
                    }

                    if (prerequisitesMet) return true;
                }
            }
        }
        return false;
    }

    public bool IsNPCInvolvedInActiveQuest(string npcName, QuestSaveData saveData)
    {
        var activeQuests = GetActiveQuests(saveData);

        foreach (var quest in activeQuests)
        {
            if (quest.giverNPC == npcName) return true;

            foreach (var trigger in quest.completionTriggers)
            {
                if (trigger.triggerType == QuestTriggerType.TALK_TO_NPC &&
                    trigger.targetId == npcName)
                {
                    if (trigger.currentAmount < trigger.requiredAmount)
                    {
                        if (trigger.sequential)
                        {
                            bool previousComplete = true;
                            foreach (var otherTrigger in quest.completionTriggers)
                            {
                                if (otherTrigger == trigger) break;
                                if (otherTrigger.currentAmount < otherTrigger.requiredAmount)
                                {
                                    previousComplete = false;
                                    break;
                                }
                            }
                            return previousComplete;
                        }
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public List<QuestData> GetAllQuestDefinitions()
    {
        var list = new List<QuestData>();
        foreach (var quest in questDefinitions.Values)
        {
            list.Add(CloneQuestData(quest));
        }
        return list;
    }

    public void DebugPrintSaveDataContents(QuestSaveData saveData)
    {
        Debug.Log($"=== QUEST LOADER DEBUG: Save Data Contents ===");
        Debug.Log($"Last Save Time: {saveData.lastSaveTime}");
        Debug.Log($"Player Coins: {saveData.playerCoins}");
        Debug.Log($"Talked To NPCs: {saveData.talkedToNPCs?.Count ?? 0}");
        if (saveData.talkedToNPCs != null && saveData.talkedToNPCs.Count > 0)
        {
            foreach (var npc in saveData.talkedToNPCs)
            {
                Debug.Log($"  - {npc}");
            }
        }

        Debug.Log($"Quest Progress Entries: {saveData.questProgress?.Count ?? 0}");
        if (saveData.questProgress != null)
        {
            foreach (var entry in saveData.questProgress)
            {
                Debug.Log($"  Quest: {entry.questId}, State: {entry.state}");
                Debug.Log($"    Triggers: [{string.Join(", ", entry.triggers)}]");
            }
        }
        else
        {
            Debug.LogWarning("Quest Progress is NULL!");
        }

        Debug.Log($"Active Quest IDs: {saveData.activeQuestIds?.Count ?? 0}");
        if (saveData.activeQuestIds != null)
        {
            foreach (var id in saveData.activeQuestIds)
            {
                Debug.Log($"  - {id}");
            }
        }
        
        Debug.Log($"Completed Quest IDs: {saveData.completedQuestIds?.Count ?? 0}");
        if (saveData.completedQuestIds != null)
        {
            foreach (var id in saveData.completedQuestIds)
            {
                Debug.Log($"  - {id}");
            }
        }
    }

    public void DebugQuestProgressUpdate(QuestData quest)
    {
        Debug.Log($"=== DEBUG: Updating Quest Progress ===");
        Debug.Log($"Quest: {quest.questId}");
        Debug.Log($"State: {quest.state}");
        Debug.Log($"Completion Triggers: {quest.completionTriggers.Count}");
        for (int i = 0; i < quest.completionTriggers.Count; i++)
        {
            var trigger = quest.completionTriggers[i];
            Debug.Log($"  Trigger {i}: {trigger.currentAmount}/{trigger.requiredAmount} ({trigger.targetId})");
        }
    }

    public string GetSaveFilePath()
    {
        return saveFilePath;
    }
}