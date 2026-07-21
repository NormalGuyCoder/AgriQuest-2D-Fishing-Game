﻿using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestSaveData
{
    public string lastSaveTime;
    public int playerCoins = 0;
    public List<string> talkedToNPCs = new List<string>();
    public Dictionary<string, int> inventory = new Dictionary<string, int>();
    public Dictionary<string, int> skills = new Dictionary<string, int>();
    public List<string> completedTutorials = new List<string>();
    public Dictionary<string, float> populationHealth = new Dictionary<string, float>();
    public Dictionary<string, int> reputation = new Dictionary<string, int>();
    public Dictionary<string, string> gameFlags = new Dictionary<string, string>();
    public List<QuestProgressEntry> questProgress = new List<QuestProgressEntry>();  // Single list structure

    [NonSerialized] public List<string> activeQuestIds = new List<string>();
    [NonSerialized] public List<string> completedQuestIds = new List<string>();

    public void UpdateDerivedLists()
    {
        activeQuestIds.Clear();
        completedQuestIds.Clear();

        foreach (var entry in questProgress)
        {
            switch (entry.state)
            {
                case QuestState.ACTIVE:
                    activeQuestIds.Add(entry.questId);
                    break;
                case QuestState.COMPLETED:
                    completedQuestIds.Add(entry.questId);
                    break;
            }
        }
    }
}

[Serializable]
public class QuestProgressEntry
{
    public string questId;
    public QuestState state = QuestState.LOCKED;
    public List<int> triggers = new List<int>();
}