using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestEventDispatcher
{
    private readonly QuestManager manager;

    public QuestEventDispatcher(QuestManager manager)
    {
        this.manager = manager;
    }

    public void CheckQuestCompletion(QuestData quest)
    {
        var saveData = manager.GetSaveData();

        if (quest.IsComplete() && quest.state == QuestState.ACTIVE)
        {
            CompleteQuest(quest, saveData);
        }
    }

    private void CompleteQuest(QuestData quest, QuestSaveData saveData)
    {
        if (manager.debugMode) Debug.Log($"QuestEventDispatcher: Starting completion for quest '{quest.questName}'");

        quest.state = QuestState.COMPLETED;

        saveData.activeQuestIds.Remove(quest.questId);
        saveData.completedQuestIds.Add(quest.questId);

        RemoveFishFromInventoryForQuest(quest);
        GrantRewards(quest, saveData);

        manager.UpdateQuestProgress(quest);
        ActivateNextQuests(quest, saveData);
        manager.InvokeQuestCompleted(quest);

        if (manager.autoSave) manager.SaveData();

        if (manager.debugMode)
            Debug.Log($"QuestEventDispatcher: Completed quest '{quest.questName}' and activated next quests");
    }

    private void RemoveFishFromInventoryForQuest(QuestData quest)
    {
        var triggerHandler = manager.GetComponent<QuestTriggerHandler>();
        if (triggerHandler == null) return;
    }

    private void GrantRewards(QuestData quest, QuestSaveData saveData)
    {
        if (quest.rewardCoins > 0)
        {
            int oldCoins = saveData.playerCoins;
            saveData.playerCoins += quest.rewardCoins;
            if (manager.debugMode)
                Debug.Log($"QuestEventDispatcher: Quest '{quest.questName}' reward: {quest.rewardCoins} coins (old: {oldCoins}, new: {saveData.playerCoins})");
        }

        if (quest.itemRewards.Count > 0)
        {
            foreach (var item in quest.itemRewards)
            {
                if (!saveData.inventory.ContainsKey(item.itemId))
                    saveData.inventory[item.itemId] = 0;
                saveData.inventory[item.itemId] += item.quantity;

                if (manager.debugMode)
                    Debug.Log($"QuestEventDispatcher: Quest '{quest.questName}' reward: {item.quantity}x {item.itemName}");
            }
        }
    }

    private void ActivateNextQuests(QuestData quest, QuestSaveData saveData)
    {
        if (manager.debugMode)
            Debug.Log($"QuestEventDispatcher: Checking for next quests after '{quest.questName}' ({quest.nextQuestIds.Count} possible)");

        foreach (var nextId in quest.nextQuestIds)
        {
            if (manager.debugMode) Debug.Log($"QuestEventDispatcher: Checking next quest ID: {nextId}");

            var nextQuest = manager.GetQuest(nextId);
            if (nextQuest != null)
            {
                if (manager.debugMode) Debug.Log($"QuestEventDispatcher: Found next quest '{nextQuest.questName}', state: {nextQuest.state}");

                if (nextQuest.state == QuestState.LOCKED && ArePrerequisitesMet(nextQuest, saveData))
                {
                    if (manager.debugMode) Debug.Log($"QuestEventDispatcher: Activating '{nextQuest.questName}'");
                    ActivateQuest(nextQuest, saveData);
                }
                else if (manager.debugMode)
                {
                    Debug.Log($"QuestEventDispatcher: Cannot activate '{nextQuest.questName}': " +
                             $"state={nextQuest.state}, prerequisitesMet={ArePrerequisitesMet(nextQuest, saveData)}");
                }
            }
            else
            {
                Debug.LogWarning($"QuestEventDispatcher: Next quest '{nextId}' not found in definitions");
            }
        }
    }

    private bool ArePrerequisitesMet(QuestData quest, QuestSaveData saveData)
    {
        if (quest.requiredQuestIds.Count == 0) return true;

        switch (quest.prerequisiteLogic)
        {
            case PrerequisiteLogic.ANY:
                foreach (var reqId in quest.requiredQuestIds)
                {
                    var reqQuest = manager.GetQuest(reqId);
                    if (reqQuest != null && reqQuest.state == QuestState.COMPLETED)
                    {
                        return true;
                    }
                }
                return false;

            case PrerequisiteLogic.ALL:
            default:
                foreach (var reqId in quest.requiredQuestIds)
                {
                    var reqQuest = manager.GetQuest(reqId);
                    if (reqQuest == null || reqQuest.state != QuestState.COMPLETED)
                    {
                        return false;
                    }
                }
                return true;
        }
    }

    private void ActivateQuest(QuestData quest, QuestSaveData saveData)
    {
        quest.state = QuestState.ACTIVE;

        var triggerHandler = manager.GetComponent<QuestTriggerHandler>();
        triggerHandler?.InitializeQuestTriggersOnActivation(quest);

        if (!saveData.activeQuestIds.Contains(quest.questId))
        {
            saveData.activeQuestIds.Add(quest.questId);
        }

        manager.UpdateQuestProgress(quest);
        manager.InvokeQuestStarted(quest);
    }

    public void CompleteQuestManually(string questId)
    {
        var saveData = manager.GetSaveData();
        var quest = manager.GetQuest(questId);

        if (quest != null)
        {
            foreach (var trigger in quest.completionTriggers)
            {
                trigger.currentAmount = trigger.requiredAmount;
            }

            CheckQuestCompletion(quest);
        }
    }

    public void ActivateQuestManually(string questId)
    {
        var saveData = manager.GetSaveData();
        var quest = manager.GetQuest(questId);

        if (quest != null)
        {
            if (quest.state == QuestState.LOCKED)
            {
                if (!ArePrerequisitesMet(quest, saveData))
                {
                    Debug.LogWarning($"QuestEventDispatcher: Cannot activate {quest.questName}: Prerequisites not met");
                    return;
                }

                ActivateQuest(quest, saveData);
            }
        }
    }

    public void ResetQuestManually(string questId)
    {
        var saveData = manager.GetSaveData();
        var quest = manager.GetQuest(questId);

        if (quest != null)
        {
            quest.state = QuestState.LOCKED;

            foreach (var trigger in quest.completionTriggers)
            {
                trigger.currentAmount = 0;
            }

            saveData.activeQuestIds.Remove(questId);
            saveData.completedQuestIds.Remove(questId);

            manager.UpdateQuestProgress(quest);
        }
    }

    public List<string> GetQuestChain(string startQuestId)
    {
        var chain = new List<string>();

        string currentId = startQuestId;
        while (!string.IsNullOrEmpty(currentId))
        {
            chain.Add(currentId);

            var quest = manager.GetQuest(currentId);
            if (quest != null && quest.nextQuestIds.Count > 0)
            {
                currentId = quest.nextQuestIds[0];
            }
            else
            {
                currentId = null;
            }
        }

        return chain;
    }

    public int GetQuestChainProgress(string startQuestId)
    {
        var chain = GetQuestChain(startQuestId);
        int completed = 0;

        foreach (var questId in chain)
        {
            var quest = manager.GetQuest(questId);
            if (quest != null && quest.state == QuestState.COMPLETED)
                completed++;
        }

        return chain.Count > 0 ? (completed * 100) / chain.Count : 0;
    }

    public void DebugPrintQuestChain(string startQuestId)
    {
        var chain = GetQuestChain(startQuestId);

        Debug.Log($"=== QUEST CHAIN for {startQuestId} ===");
        for (int i = 0; i < chain.Count; i++)
        {
            var quest = manager.GetQuest(chain[i]);
            if (quest != null)
            {
                Debug.Log($"[{i}] {quest.questName} ({quest.questId}) - State: {quest.state}");
                Debug.Log($"    Next: {string.Join(", ", quest.nextQuestIds)}");
                Debug.Log($"    Required: {string.Join(", ", quest.requiredQuestIds)}");
            }
        }
        Debug.Log($"Chain length: {chain.Count}");
    }

    public void DebugCheckPrerequisites(string questId)
    {
        var quest = manager.GetQuest(questId);
        if (quest != null)
        {
            Debug.Log($"=== PREREQUISITES for {quest.questName} ===");
            Debug.Log($"Required: {string.Join(", ", quest.requiredQuestIds)}");
            Debug.Log($"Logic: {quest.prerequisiteLogic}");

            var saveData = manager.GetSaveData();
            bool met = ArePrerequisitesMet(quest, saveData);
            Debug.Log($"Prerequisites met: {met}");
        }
    }
}