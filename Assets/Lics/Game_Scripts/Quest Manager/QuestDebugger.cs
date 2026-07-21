using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestDebugger
{
    private readonly QuestManager manager;

    public QuestDebugger(QuestManager manager)
    {
        this.manager = manager;
    }

    [ContextMenu("Debug: Complete ORIENTATION_DAY_0")]
    public void DebugCompleteOrientationDay0()
    {
        var quest = manager.GetQuest("ORIENTATION_DAY_0");
        if (quest != null)
        {
            foreach (var trigger in quest.completionTriggers)
            {
                trigger.currentAmount = trigger.requiredAmount;
            }

            manager.CheckQuestCompletion(quest);

            if (quest.state == QuestState.COMPLETED)
            {
                manager.InvokeQuestCompleted(quest);
            }
        }
    }

    [ContextMenu("Debug: Activate STEWARDSHIP_CHALLENGE_1")]
    public void DebugActivateStewardshipChallenge()
    {
        var quest = manager.GetQuest("STEWARDSHIP_CHALLENGE_1");
        if (quest != null && quest.state == QuestState.LOCKED)
        {
            var orientationQuest = manager.GetQuest("ORIENTATION_DAY_0");
            if (orientationQuest != null && orientationQuest.state != QuestState.COMPLETED)
            {
                DebugCompleteOrientationDay0();
            }

            if (quest.state == QuestState.LOCKED)
            {
                var saveData = manager.GetSaveData();
                quest.state = QuestState.ACTIVE;

                if (!saveData.activeQuestIds.Contains(quest.questId))
                {
                    saveData.activeQuestIds.Add(quest.questId);
                }

                manager.InvokeQuestStarted(quest);
            }
        }
    }

    [ContextMenu("Debug: Complete First Cast Quest (2A)")]
    public void DebugCompleteFirstCast()
    {
        var quest = manager.GetQuest("FIRST_CAST_2A");
        if (quest != null && quest.state == QuestState.ACTIVE)
        {
            foreach (var trigger in quest.completionTriggers)
            {
                trigger.currentAmount = trigger.requiredAmount;
            }

            manager.CheckQuestCompletion(quest);
        }
    }

    [ContextMenu("Debug: Complete First Hatchery Quest (2B)")]
    public void DebugCompleteFirstHatchery()
    {
        var quest = manager.GetQuest("FIRST_HATCHERY_2B");
        if (quest != null && quest.state == QuestState.ACTIVE)
        {
            foreach (var trigger in quest.completionTriggers)
            {
                trigger.currentAmount = trigger.requiredAmount;
            }

            manager.CheckQuestCompletion(quest);
        }
    }

    [ContextMenu("Debug: Give 5000 Coins for SEASON_REVIEW")]
    public void DebugGiveSeasonReviewCoins()
    {
        var quest = manager.GetQuest("SEASON_REVIEW_8");
        if (quest != null)
        {
            var saveData = manager.GetSaveData();
            saveData.playerCoins = 5000;
            manager.GetComponent<QuestTriggerHandler>()?.UpdateCoins(5000);
        }
    }

    [ContextMenu("Debug: Complete All Quests in Chain")]
    public void DebugCompleteAllQuestsInChain()
    {
        DebugCompleteOrientationDay0();

        string[] questChain = {
            "STEWARDSHIP_CHALLENGE_1",
            "FIRST_CAST_2A",
            "FIRST_HATCHERY_2B",
            "MARKET_DYNAMICS_3",
            "ART_OF_VALUE_4",
            "TOOLS_OF_TRADE_5",
            "TEST_OF_VALUES_6",
            "WARDENS_WATCH_7",
            "SEASON_REVIEW_8",
            "GRADUATION_DAY_9"
        };

        foreach (var questId in questChain)
        {
            var quest = manager.GetQuest(questId);
            if (quest != null)
            {
                if (quest.state == QuestState.LOCKED)
                {
                    quest.state = QuestState.ACTIVE;

                    var saveData = manager.GetSaveData();
                    if (!saveData.activeQuestIds.Contains(quest.questId))
                        saveData.activeQuestIds.Add(quest.questId);

                    manager.InvokeQuestStarted(quest);
                }

                foreach (var trigger in quest.completionTriggers)
                {
                    trigger.currentAmount = trigger.requiredAmount;
                }

                manager.CheckQuestCompletion(quest);
            }
        }
    }

    public void DebugPrintAllQuestStates()
    {
        Debug.Log("=== CURRENT QUEST STATES ===");
        var allQuests = manager.GetAllQuests();

        int activeCount = 0;
        int completedCount = 0;
        int lockedCount = 0;

        foreach (var quest in allQuests)
        {
            Debug.Log($"Quest: {quest.questName} (ID: {quest.questId}) - State: {quest.state}");

            if (quest.state == QuestState.ACTIVE || quest.state == QuestState.COMPLETED)
            {
                Debug.Log($"  Progress: {quest.GetProgressText()}");
            }

            switch (quest.state)
            {
                case QuestState.ACTIVE: activeCount++; break;
                case QuestState.COMPLETED: completedCount++; break;
                case QuestState.LOCKED: lockedCount++; break;
            }
        }

        Debug.Log($"=== SUMMARY ===");
        Debug.Log($"Active: {activeCount}, Completed: {completedCount}, Locked: {lockedCount}");
        Debug.Log($"Total: {allQuests.Count}");
    }

    public void DebugPrintActiveQuestDetails()
    {
        var activeQuests = manager.GetActiveQuests();

        Debug.Log($"=== ACTIVE QUESTS ({activeQuests.Count}) ===");

        if (activeQuests.Count == 0)
        {
            Debug.Log("No active quests.");
            return;
        }

        foreach (var quest in activeQuests)
        {
            Debug.Log($"{quest.questName}");
            Debug.Log($"  Description: {quest.description}");
            Debug.Log($"  Giver: {quest.giverNPC}");
            Debug.Log($"  Location Hint: {quest.locationHint}");
            Debug.Log($"  Progress: {quest.GetProgressText()}");

            // REMOVED XP from reward display
            if (quest.rewardCoins > 0)
            {
                Debug.Log($"  Rewards: {quest.rewardCoins} coins");
            }

            Debug.Log($"---");
        }
    }

    public void DebugPrintQuestChain(string startQuestId)
    {
        var eventDispatcher = manager.GetComponent<QuestEventDispatcher>();
        if (eventDispatcher != null)
        {
            var chain = eventDispatcher.GetQuestChain(startQuestId);
            var allQuests = manager.GetAllQuests();

            Debug.Log($"=== QUEST CHAIN FOR {startQuestId} ===");

            for (int i = 0; i < chain.Count; i++)
            {
                var quest = manager.GetQuest(chain[i]);
                if (quest != null)
                {
                    string prefix = "";
                    if (i > 0) prefix = "└─ ";

                    Debug.Log($"{prefix}[{i + 1}] {quest.questName} (ID: {quest.questId}) - {quest.state}");
                }
            }

            int progress = eventDispatcher.GetQuestChainProgress(startQuestId);
            Debug.Log($"Chain Progress: {progress}%");
        }
    }

    public void DebugPrintSaveDataInfo()
    {
        var saveData = manager.GetSaveData();

        Debug.Log("=== SAVE DATA INFO ===");
        Debug.Log($"Quest Progress Entries: {saveData.questProgress.Count}");
        Debug.Log($"Active Quest IDs: {saveData.activeQuestIds.Count}");
        Debug.Log($"Completed Quest IDs: {saveData.completedQuestIds.Count}");
        Debug.Log($"Player Coins: {saveData.playerCoins}");
        Debug.Log($"Talked To NPCs: {saveData.talkedToNPCs.Count}");
        Debug.Log($"Inventory Items: {saveData.inventory.Count}");
        Debug.Log($"Skill Levels: {saveData.skills.Count}");
        Debug.Log($"Last Save Time: {saveData.lastSaveTime}");
    }

    public void DebugPrintNPCQuestStatus(string npcName)
    {
        var questLoader = manager.GetComponent<QuestLoader>();
        var saveData = manager.GetSaveData();

        if (questLoader != null)
        {
            bool hasAvailable = questLoader.HasAvailableQuestForNPC(npcName, saveData);
            bool isInvolved = questLoader.IsNPCInvolvedInActiveQuest(npcName, saveData);

            Debug.Log($"=== NPC QUEST STATUS: {npcName} ===");
            Debug.Log($"Has Available Quest: {hasAvailable}");
            Debug.Log($"Is Involved in Active Quest: {isInvolved}");

            Debug.Log($"--- Related Quests ---");
            var allQuests = manager.GetAllQuests();
            foreach (var quest in allQuests)
            {
                if (quest.giverNPC == npcName ||
                    quest.completionTriggers.Exists(t =>
                        t.triggerType == QuestTriggerType.TALK_TO_NPC &&
                        t.targetId == npcName))
                {
                    Debug.Log($"{quest.questName} (State: {quest.state})");
                }
            }
        }
    }

    public void ToggleDebugMode()
    {
        manager.debugMode = !manager.debugMode;
        Debug.Log($"Quest Debug Mode: {manager.debugMode}");
    }

    public void ForceSave()
    {
        manager.SaveData();
        Debug.Log("Force saved quest data");
    }

    public void ForceLoad()
    {
        DebugPrintAllQuestStates();
    }

    public void SimulateFishCatch(int amount)
    {
        var triggerHandler = manager.GetComponent<QuestTriggerHandler>();
        if (triggerHandler != null)
        {
            triggerHandler.RecordFishCaught(amount);
            Debug.Log($"Simulated catching {amount} fish");
        }
    }

    public void SimulateFishSale(int amount)
    {
        var triggerHandler = manager.GetComponent<QuestTriggerHandler>();
        if (triggerHandler != null)
        {
            triggerHandler.RecordFishSold(amount);
            Debug.Log($"Simulated selling {amount} fish");
        }
    }

    public void SimulateNPCConversation(string npcName)
    {
        var triggerHandler = manager.GetComponent<QuestTriggerHandler>();
        if (triggerHandler != null)
        {
            triggerHandler.RecordTalkedToNPC(npcName);
            Debug.Log($"Simulated talking to {npcName}");
        }
    }
}