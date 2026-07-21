﻿using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum QuestTriggerType
{
    TALK_TO_NPC = 0,
    GO_TO_LOCATION = 1,
    HAVE_ITEM = 2,
    HAVE_COINS = 3,
    HAVE_SKILL_LEVEL = 4,
    COMBINED = 5
}

[Serializable]
public enum PrerequisiteLogic
{
    ALL,
    ANY
}

[Serializable]
public enum QuestState
{
    LOCKED = 0,
    ACTIVE = 1,
    COMPLETED = 2
}

[Serializable]
public class QuestTrigger
{
    public QuestTriggerType triggerType;
    public string targetId;
    public int requiredAmount = 1;
    public int currentAmount = 0;
    public bool sequential = false;
}

[Serializable]
public class QuestRewardItem
{
    public string itemId;
    public string itemName;
    public int quantity;

    public QuestRewardItem(string id, string name, int qty)
    {
        itemId = id;
        itemName = name;
        quantity = qty;
    }
}

[Serializable]
public class QuestData
{
    public string questId;
    public string questName;
    public string description;
    public List<QuestTrigger> completionTriggers = new List<QuestTrigger>();
    public int rewardCoins;
    public List<QuestRewardItem> itemRewards = new List<QuestRewardItem>();
    public string giverNPC;
    public QuestState state = QuestState.LOCKED;
    public List<string> requiredQuestIds = new List<string>();
    public List<string> nextQuestIds = new List<string>();
    public string locationHint;
    public PrerequisiteLogic prerequisiteLogic = PrerequisiteLogic.ALL;

    public bool IsComplete()
    {
        foreach (var trigger in completionTriggers)
        {
            if (trigger.currentAmount < trigger.requiredAmount)
                return false;
        }
        return true;
    }

    public string GetProgressText()
    {
        if (completionTriggers.Count == 0) return "No objectives";

        var lines = new List<string>();
        foreach (var trigger in completionTriggers)
        {
            string progress = $"{trigger.currentAmount}/{trigger.requiredAmount}";

            switch (trigger.triggerType)
            {
                case QuestTriggerType.TALK_TO_NPC:
                    lines.Add($"Talk to {trigger.targetId}: {(trigger.currentAmount >= 1 ? "✓" : progress)}");
                    break;
                case QuestTriggerType.GO_TO_LOCATION:
                    lines.Add($"Go to {trigger.targetId}: {(trigger.currentAmount >= 1 ? "✓" : progress)}");
                    break;
                case QuestTriggerType.HAVE_ITEM:
                    lines.Add($"Have {trigger.targetId}: {progress}");
                    break;
                case QuestTriggerType.HAVE_COINS:
                    lines.Add($"Have coins: {progress}");
                    break;
                case QuestTriggerType.HAVE_SKILL_LEVEL:
                    lines.Add($"{trigger.targetId} Level: {progress}");
                    break;
            }
        }
        return string.Join("\n", lines);
    }
}