using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestTriggerHandler
{
    private readonly QuestManager manager;
    private bool hasInventorySubscribed = false;

    public QuestTriggerHandler(QuestManager manager)
    {
        this.manager = manager;
    }

    public void InitializeInventoryListeners()
    {
        if (DetailedFishInventory.Instance != null)
        {
            DetailedFishInventory.Instance.OnInventoryUpdated += OnFishInventoryUpdated;
            UpdateFishQuestTriggers();
            InitializeAllActiveQuestTriggers();
            hasInventorySubscribed = true;
        }
    }

    public void CleanupInventoryListeners()
    {
        if (hasInventorySubscribed && DetailedFishInventory.Instance != null)
        {
            DetailedFishInventory.Instance.OnInventoryUpdated -= OnFishInventoryUpdated;
            hasInventorySubscribed = false;
        }
    }

    public void InitializeAllActiveQuestTriggers()
    {
        var saveData = manager.GetSaveData();
        // Create a copy to avoid modification during enumeration
        var activeQuestIdsCopy = new List<string>(saveData.activeQuestIds);
        foreach (var questId in activeQuestIdsCopy)
        {
            var quest = manager.GetQuest(questId);
            if (quest != null)
            {
                InitializeQuestTriggersOnActivation(quest);
            }
        }
    }

    public void InitializeQuestTriggersOnActivation(QuestData quest)
    {
        if (quest == null) return;

        var saveData = manager.GetSaveData();

        foreach (var trigger in quest.completionTriggers)
        {
            switch (trigger.triggerType)
            {
                case QuestTriggerType.HAVE_ITEM:
                    InitializeItemTrigger(trigger, saveData);
                    break;
                case QuestTriggerType.HAVE_COINS:
                    trigger.currentAmount = Mathf.Min(saveData.playerCoins, trigger.requiredAmount);
                    break;
                case QuestTriggerType.HAVE_SKILL_LEVEL:
                    trigger.currentAmount = Mathf.Min(
                        saveData.skills.GetValueOrDefault(trigger.targetId, 0),
                        trigger.requiredAmount
                    );
                    break;
            }
        }

        manager.UpdateQuestProgress(quest);
    }

    private void InitializeItemTrigger(QuestTrigger trigger, QuestSaveData saveData)
    {
        switch (trigger.targetId)
        {
            case "Fish":
                int currentFishCount = 0;
                if (DetailedFishInventory.Instance != null)
                {
                    currentFishCount = DetailedFishInventory.Instance.CurrentInventoryCount;
                }
                trigger.currentAmount = Mathf.Min(currentFishCount, trigger.requiredAmount);
                break;

            case "Sold Fish":
                int soldFishCount = saveData.inventory.GetValueOrDefault("SoldFish", 0);
                trigger.currentAmount = Mathf.Min(soldFishCount, trigger.requiredAmount);
                break;

            case "Processed Fish":
                int processedFishCount = saveData.inventory.GetValueOrDefault("ProcessedFish", 0);
                trigger.currentAmount = Mathf.Min(processedFishCount, trigger.requiredAmount);
                break;

            case "Fishing Equipment":
                int equipmentCount = saveData.inventory.GetValueOrDefault("FishingEquipment", 0);
                trigger.currentAmount = Mathf.Min(equipmentCount, trigger.requiredAmount);
                break;

            case "Field Journal":
                int journalCount = saveData.inventory.GetValueOrDefault("FieldJournal", 0);
                trigger.currentAmount = Mathf.Min(journalCount, trigger.requiredAmount);
                break;

            default:
                if (trigger.targetId.StartsWith("Side Quest"))
                {
                    int sideQuestCount = saveData.completedQuestIds.Contains(trigger.targetId) ? 1 : 0;
                    trigger.currentAmount = Mathf.Min(sideQuestCount, trigger.requiredAmount);
                }
                else
                {
                    int itemCount = saveData.inventory.GetValueOrDefault(trigger.targetId, 0);
                    trigger.currentAmount = Mathf.Min(itemCount, trigger.requiredAmount);
                }
                break;
        }
    }

    private void OnFishInventoryUpdated()
    {
        UpdateFishQuestTriggers();
    }

    private void UpdateFishQuestTriggers()
    {
        if (DetailedFishInventory.Instance == null) 
        {
            Debug.LogWarning("UpdateFishQuestTriggers: DetailedFishInventory.Instance is null!");
            return;
        }

        int totalFishCount = DetailedFishInventory.Instance.CurrentInventoryCount;
        
        var saveData = manager.GetSaveData();
        
        // FIXED: Create copy before iteration to avoid modification during enumeration
        var activeQuestIdsCopy = new List<string>(saveData.activeQuestIds);
        
        foreach (var questId in activeQuestIdsCopy)
        {
            var quest = manager.GetQuest(questId);
            if (quest != null)
            {
                bool updated = false;
                foreach (var trigger in quest.completionTriggers)
                {
                    if (trigger.triggerType == QuestTriggerType.HAVE_ITEM && trigger.targetId == "Fish")
                    {
                        int previousAmount = trigger.currentAmount;
                        trigger.currentAmount = Mathf.Min(totalFishCount, trigger.requiredAmount);
                        
                        if (previousAmount != trigger.currentAmount)
                        {
                            updated = true;
                        }
                    }
                }
                
                if (updated)
                {
                    manager.UpdateQuestProgress(quest);
                    manager.InvokeQuestProgressUpdated(quest);
                    manager.CheckQuestCompletion(quest);
                }
            }
        }
    }

    private void RemoveFishFromInventoryForQuest(QuestData quest)
    {
        var saveData = manager.GetSaveData();

        foreach (var trigger in quest.completionTriggers)
        {
            if (trigger.triggerType == QuestTriggerType.HAVE_ITEM && trigger.targetId == "Fish")
            {
                int fishToRemove = trigger.requiredAmount;

                if (DetailedFishInventory.Instance != null && fishToRemove > 0)
                {
                    bool success = DetailedFishInventory.Instance.RemoveFishFromInventory("any", fishToRemove, out float weightRemoved);

                    if (success)
                    {
                        if (saveData.inventory.ContainsKey("Fish"))
                        {
                            saveData.inventory["Fish"] = Mathf.Max(0, saveData.inventory["Fish"] - fishToRemove);
                        }
                        else
                        {
                            saveData.inventory["Fish"] = 0;
                        }

                        UpdateFishQuestTriggers();
                    }
                }
            }
        }
    }

    public int GetFishCountForQuest(string targetId)
    {
        var saveData = manager.GetSaveData();

        if (DetailedFishInventory.Instance == null) return 0;

        switch (targetId)
        {
            case "Fish":
                return DetailedFishInventory.Instance.CurrentInventoryCount;
            case "Sold Fish":
                return saveData.inventory.GetValueOrDefault("SoldFish", 0);
            case "Processed Fish":
                return saveData.inventory.GetValueOrDefault("ProcessedFish", 0);
            default:
                if (DetailedFishInventory.Instance.SpeciesInventoryCounts != null &&
                    DetailedFishInventory.Instance.SpeciesInventoryCounts.ContainsKey(targetId))
                {
                    return DetailedFishInventory.Instance.SpeciesInventoryCounts[targetId];
                }
                return 0;
        }
    }

    public void RecordTalkedToNPC(string npcName)
    {
        var saveData = manager.GetSaveData();

        if (npcName == "Professor Clark" && manager.GetQuest("ORIENTATION_DAY_0")?.state == QuestState.ACTIVE)
        {
            if (!saveData.talkedToNPCs.Contains(npcName))
                saveData.talkedToNPCs.Add(npcName);

            if (manager.autoSave) manager.SaveData();
            return;
        }

        if (!saveData.talkedToNPCs.Contains(npcName))
            saveData.talkedToNPCs.Add(npcName);

        // FIXED: Create copy before iteration to avoid modification during enumeration
        var activeQuestIdsCopy = new List<string>(saveData.activeQuestIds);

        foreach (var questId in activeQuestIdsCopy)
        {
            var quest = manager.GetQuest(questId);
            if (quest != null)
            {
                var talkTriggers = new List<QuestTrigger>();
                var otherTriggers = new List<QuestTrigger>();

                foreach (var trigger in quest.completionTriggers)
                {
                    if (trigger.triggerType == QuestTriggerType.TALK_TO_NPC &&
                        trigger.targetId == npcName)
                    {
                        talkTriggers.Add(trigger);
                    }
                    else
                    {
                        otherTriggers.Add(trigger);
                    }
                }

                foreach (var talkTrigger in talkTriggers)
                {
                    if (talkTrigger.currentAmount >= talkTrigger.requiredAmount)
                        continue;

                    if (talkTrigger.sequential)
                    {
                        bool allOtherTriggersComplete = true;

                        foreach (var otherTrigger in otherTriggers)
                        {
                            if (otherTrigger.currentAmount < otherTrigger.requiredAmount)
                            {
                                allOtherTriggersComplete = false;
                                break;
                            }
                        }

                        if (allOtherTriggersComplete)
                        {
                            talkTrigger.currentAmount = Mathf.Min(talkTrigger.currentAmount + 1, talkTrigger.requiredAmount);
                            manager.UpdateQuestProgress(quest);
                            manager.InvokeQuestProgressUpdated(quest);
                            manager.CheckQuestCompletion(quest);
                        }
                    }
                    else
                    {
                        talkTrigger.currentAmount = Mathf.Min(talkTrigger.currentAmount + 1, talkTrigger.requiredAmount);
                        manager.UpdateQuestProgress(quest);
                        manager.InvokeQuestProgressUpdated(quest);
                        manager.CheckQuestCompletion(quest);
                    }
                }
            }
        }

        if (manager.autoSave) manager.SaveData();
    }

    public void RecordItemObtained(string itemId, int amount)
    {
        var saveData = manager.GetSaveData();

        if (!saveData.inventory.ContainsKey(itemId))
            saveData.inventory[itemId] = 0;
        saveData.inventory[itemId] += amount;

        // FIXED: Create copy before iteration to avoid modification during enumeration
        var activeQuestIdsCopy = new List<string>(saveData.activeQuestIds);

        foreach (var questId in activeQuestIdsCopy)
        {
            var quest = manager.GetQuest(questId);
            if (quest != null)
            {
                foreach (var trigger in quest.completionTriggers)
                {
                    if (trigger.triggerType == QuestTriggerType.HAVE_ITEM &&
                        trigger.targetId == itemId)
                    {
                        trigger.currentAmount = Mathf.Min(
                            saveData.inventory.GetValueOrDefault(itemId, 0),
                            trigger.requiredAmount
                        );
                        manager.UpdateQuestProgress(quest);
                        manager.InvokeQuestProgressUpdated(quest);
                        manager.CheckQuestCompletion(quest);
                    }
                }
            }
        }

        if (manager.autoSave) manager.SaveData();
    }

    public void RecordFishCaught(int amount = 1)
    {
        RecordItemObtained("Fish", amount);
    }

    public void RecordFishSold(int amount = 1)
    {
        var saveData = manager.GetSaveData();

        if (!saveData.inventory.ContainsKey("SoldFish"))
            saveData.inventory["SoldFish"] = 0;
        saveData.inventory["SoldFish"] += amount;

        // FIXED: Create copy before iteration to avoid modification during enumeration
        var activeQuestIdsCopy = new List<string>(saveData.activeQuestIds);

        foreach (var questId in activeQuestIdsCopy)
        {
            var quest = manager.GetQuest(questId);
            if (quest != null)
            {
                foreach (var trigger in quest.completionTriggers)
                {
                    if (trigger.triggerType == QuestTriggerType.HAVE_ITEM &&
                        trigger.targetId == "Sold Fish")
                    {
                        trigger.currentAmount = Mathf.Min(
                            saveData.inventory.GetValueOrDefault("SoldFish", 0),
                            trigger.requiredAmount
                        );
                        manager.UpdateQuestProgress(quest);
                        manager.InvokeQuestProgressUpdated(quest);
                        manager.CheckQuestCompletion(quest);
                    }
                }
            }
        }

        if (manager.autoSave) manager.SaveData();
    }

    public void RecordFishProcessed(int amount = 1)
    {
        var saveData = manager.GetSaveData();

        if (!saveData.inventory.ContainsKey("ProcessedFish"))
            saveData.inventory["ProcessedFish"] = 0;
        saveData.inventory["ProcessedFish"] += amount;

        // FIXED: Create copy before iteration to avoid modification during enumeration
        var activeQuestIdsCopy = new List<string>(saveData.activeQuestIds);

        foreach (var questId in activeQuestIdsCopy)
        {
            var quest = manager.GetQuest(questId);
            if (quest != null)
            {
                foreach (var trigger in quest.completionTriggers)
                {
                    if (trigger.triggerType == QuestTriggerType.HAVE_ITEM &&
                        trigger.targetId == "Processed Fish")
                    {
                        trigger.currentAmount = Mathf.Min(
                            saveData.inventory.GetValueOrDefault("ProcessedFish", 0),
                            trigger.requiredAmount
                        );
                        manager.UpdateQuestProgress(quest);
                        manager.InvokeQuestProgressUpdated(quest);
                        manager.CheckQuestCompletion(quest);
                    }
                }
            }
        }

        if (manager.autoSave) manager.SaveData();
    }

    public void UpdateCoins(int coins)
    {
        var saveData = manager.GetSaveData();
        saveData.playerCoins = coins;

        // Update ALL quests (including locked ones) to ensure proper initialization
        var allQuestIds = new List<string>();
        foreach (var entry in saveData.questProgress)
        {
            allQuestIds.Add(entry.questId);
        }

        foreach (var questId in allQuestIds)
        {
            var quest = manager.GetQuest(questId);
            if (quest != null)
            {
                foreach (var trigger in quest.completionTriggers)
                {
                    if (trigger.triggerType == QuestTriggerType.HAVE_COINS)
                    {
                        // Calculate proper current amount (cap at required amount)
                        int currentAmount = Mathf.Min(saveData.playerCoins, trigger.requiredAmount);
                        if (trigger.currentAmount != currentAmount)
                        {
                            trigger.currentAmount = currentAmount;
                            manager.UpdateQuestProgress(quest);
                            manager.InvokeQuestProgressUpdated(quest);
                            manager.CheckQuestCompletion(quest);
                        }
                    }
                }
            }
        }

        if (manager.autoSave) manager.SaveData();
    }

    public void UpdateSkillLevel(string skill, int level)
    {
        var saveData = manager.GetSaveData();

        saveData.skills[skill] = level;

        // FIXED: Create copy before iteration to avoid modification during enumeration
        var activeQuestIdsCopy = new List<string>(saveData.activeQuestIds);

        foreach (var questId in activeQuestIdsCopy)
        {
            var quest = manager.GetQuest(questId);
            if (quest != null)
            {
                foreach (var trigger in quest.completionTriggers)
                {
                    if (trigger.triggerType == QuestTriggerType.HAVE_SKILL_LEVEL &&
                        trigger.targetId == skill)
                    {
                        trigger.currentAmount = Mathf.Min(level, trigger.requiredAmount);
                        manager.UpdateQuestProgress(quest);
                        manager.InvokeQuestProgressUpdated(quest);
                        manager.CheckQuestCompletion(quest);
                    }
                }
            }
        }

        if (manager.autoSave) manager.SaveData();
    }

    public void DeductGold(int amount)
    {
        if (amount <= 0) return;

        var saveData = manager.GetSaveData();
        saveData.playerCoins = Mathf.Max(0, saveData.playerCoins - amount);
        UpdateCoins(saveData.playerCoins);
    }

    public bool HasTalkedToNPC(string npcName)
    {
        var saveData = manager.GetSaveData();
        return saveData.talkedToNPCs.Contains(npcName);
    }

    public int GetSkillLevel(string skill, QuestSaveData saveData)
    {
        if (saveData.skills.TryGetValue(skill, out int level))
            return level;
        return 0;
    }
}