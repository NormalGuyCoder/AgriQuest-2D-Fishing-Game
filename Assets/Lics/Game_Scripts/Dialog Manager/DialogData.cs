using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum DialogConditionType
{
    ALWAYS = 0,
    QUEST_STATE = 1,
    QUEST_ACTIVE = 2,
    QUEST_COMPLETED = 3,
    HAS_ITEM = 4,
    SKILL_LEVEL = 5,
    REPUTATION = 6,
    POPULATION_HEALTH = 7,
    PLAYER_CHOICE = 8,
    TIME_OF_DAY = 9,
    HAS_MET_NPC = 10,           // New: Check if player has talked to an NPC
    TUTORIAL_COMPLETED = 11,    // New: Check if tutorial completed
    SKILL_LEVEL_COMPARISON = 12 // New: Compare skill levels
}

[Serializable]
public class DialogCondition
{
    public DialogConditionType conditionType;
    public string conditionKey;
    public string conditionValue;
    public bool expectedBool = true;
    public ComparisonType comparison = ComparisonType.EQUALS; // New: For numeric comparisons
}

// New: For skill level comparisons
public enum ComparisonType
{
    EQUALS,
    GREATER_THAN,
    LESS_THAN,
    GREATER_OR_EQUAL,
    LESS_OR_EQUAL
}

[Serializable]
public class DialogLine
{
    public string speakerName;
    [TextArea(3, 10)]
    public string text;
    public string audioClipName;
    public float displayTime = 3f;
    public List<DialogCondition> showConditions = new List<DialogCondition>();
    public List<string> triggersOnShow = new List<string>();
    public List<DialogChoice> choices = new List<DialogChoice>();
    public string nextDialogId;
    public bool showAsAction = false; // New: Format text as action (visual novel style)
}

[Serializable]
public class DialogChoice
{
    public string choiceText;
    public string resultDialogId;
    public List<string> triggersOnChoose = new List<string>();
    public List<DialogCondition> showConditions = new List<DialogCondition>();
}

[Serializable]
public class DialogTree
{
    public string npcName;
    public string defaultDialogId = "default";
    public Dictionary<string, DialogLine> dialogs = new Dictionary<string, DialogLine>();

    public List<DialogKeyValue> ToSerializableList()
    {
        var list = new List<DialogKeyValue>();
        foreach (var kvp in dialogs)
        {
            list.Add(new DialogKeyValue { key = kvp.Key, value = kvp.Value });
        }
        return list;
    }

    public void FromSerializableList(List<DialogKeyValue> list)
    {
        dialogs = new Dictionary<string, DialogLine>();
        foreach (var item in list)
        {
            dialogs[item.key] = item.value;
        }
    }
}

[Serializable]
public class DialogKeyValue
{
    public string key;
    public DialogLine value;
}

[Serializable]
public class DialogDatabase
{
    public List<NPCDialog> npcDialogs = new List<NPCDialog>();
}

[Serializable]
public class NPCDialog
{
    public string npcName;
    public List<DialogKeyValue> dialogs = new List<DialogKeyValue>();
}