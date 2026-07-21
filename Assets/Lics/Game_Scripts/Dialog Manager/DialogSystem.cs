using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogSystem : MonoBehaviour
{
    public static DialogSystem Instance { get; private set; }

    [Header("Dialog Files")]
    public List<TextAsset> dialogFiles = new List<TextAsset>();

    [Header("Debug")]
    public bool debugMode = true;
    public bool logConditionChecks = true;
    public bool logTriggerProcessing = true;

    private Dictionary<string, DialogTree> dialogTrees = new Dictionary<string, DialogTree>();
    private DialogTree currentDialogTree;
    private DialogLine currentDialogLine;
    private string currentDialogId;
    private string currentNPCName;
    private bool isDialogActive = false;
    private List<DialogChoice> currentChoices = new List<DialogChoice>();

    // Events for UI communication
    public event Action<string, DialogLine> OnDialogLineReady;
    public event Action<List<DialogChoice>> OnChoicesReady;
    public event Action<string> OnDialogStarted;
    public event Action<string> OnDialogEnded;
    public event Action<string> OnDialogTrigger;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Initialize()
    {
        LoadDialogDatabase();
    }

    void LoadDialogDatabase()
    {
        if (dialogFiles == null || dialogFiles.Count == 0)
        {
            Debug.LogError("DialogSystem: No dialog files assigned!");
            return;
        }

        try
        {
            int loadedFiles = 0;
            int loadedNPCs = 0;

            foreach (var file in dialogFiles)
            {
                if (file == null)
                {
                    if (debugMode) Debug.LogWarning("DialogSystem: Skipping null dialog file");
                    continue;
                }

                var database = JsonUtility.FromJson<DialogDatabase>(file.text);

                if (database.npcDialogs == null)
                {
                    Debug.LogWarning($"DialogSystem: File '{file.name}' has no NPC dialogs!");
                    continue;
                }

                foreach (var npcDialog in database.npcDialogs)
                {
                    if (string.IsNullOrEmpty(npcDialog.npcName))
                    {
                        Debug.LogWarning($"DialogSystem: NPC in file '{file.name}' has no name!");
                        continue;
                    }

                    var dialogTree = new DialogTree
                    {
                        npcName = npcDialog.npcName
                    };

                    dialogTree.FromSerializableList(npcDialog.dialogs);

                    if (dialogTrees.ContainsKey(npcDialog.npcName))
                    {
                        if (debugMode) Debug.LogWarning($"DialogSystem: NPC '{npcDialog.npcName}' already exists! Overwriting.");
                        dialogTrees[npcDialog.npcName] = dialogTree;
                    }
                    else
                    {
                        dialogTrees.Add(npcDialog.npcName, dialogTree);
                    }

                    loadedNPCs++;
                }

                loadedFiles++;
            }

            if (debugMode)
            {
                Debug.Log($"DialogSystem: Loaded {loadedNPCs} NPC dialogs from {loadedFiles} files");
                Debug.Log($"Available NPCs: {string.Join(", ", dialogTrees.Keys)}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"DialogSystem: Failed to load dialog database: {e.Message}\n{e.StackTrace}");
        }
    }

    public bool StartDialog(string npcName, string startingDialogId = null)
    {
        if (debugMode) Debug.Log($"=== DIALOG START REQUEST ===");
        if (debugMode) Debug.Log($"NPC: {npcName}, Starting Dialog ID: {startingDialogId ?? "null"}");

        if (!dialogTrees.TryGetValue(npcName, out currentDialogTree))
        {
            Debug.LogWarning($"DialogSystem: No dialog tree found for NPC: {npcName}");
            if (debugMode) Debug.Log($"Available NPCs: {string.Join(", ", dialogTrees.Keys)}");
            return false;
        }

        // ADDED: Special debug for Vincent
        if (npcName == "Vincent" && debugMode)
        {
            Debug.Log($"=== VINCENT DEBUG INFO ===");
            Debug.Log($"Available dialogues for Vincent:");
            foreach (var kvp in currentDialogTree.dialogs)
            {
                bool conditionsPass = CheckDialogConditions(kvp.Value);
                Debug.Log($"  - {kvp.Key}: conditions pass = {conditionsPass}");
                
                if (kvp.Value.showConditions != null)
                {
                    foreach (var condition in kvp.Value.showConditions)
                    {
                        Debug.Log($"    Condition: Type={condition.conditionType}, Key={condition.conditionKey}, Value={condition.conditionValue}");
                    }
                }
            }
            
            // Check FIRST_CAST_2A quest status
            var firstCastQuest = QuestManager.Instance?.GetQuest("FIRST_CAST_2A");
            if (firstCastQuest != null)
            {
                Debug.Log($"FIRST_CAST_2A Quest Status: {firstCastQuest.state}");
                Debug.Log($"Progress: {firstCastQuest.GetProgressText()}");
            }
            else
            {
                Debug.Log($"FIRST_CAST_2A Quest not found in QuestManager");
            }
        }

        if (string.IsNullOrEmpty(startingDialogId))
        {
            startingDialogId = currentDialogTree.defaultDialogId;
            if (debugMode) Debug.Log($"Using default dialog ID: {startingDialogId}");
        }

        // Check if the starting dialog is available
        bool useStartingDialog = false;
        if (!string.IsNullOrEmpty(startingDialogId) && 
            currentDialogTree.dialogs.ContainsKey(startingDialogId))
        {
            if (CheckDialogConditions(currentDialogTree.dialogs[startingDialogId]))
            {
                useStartingDialog = true;
                if (debugMode) Debug.Log($"Starting dialog '{startingDialogId}' is available");
            }
            else
            {
                if (debugMode) Debug.Log($"Starting dialog '{startingDialogId}' conditions FAIL");
            }
        }

        if (!useStartingDialog)
        {
            if (debugMode) Debug.Log($"Starting dialog ID '{startingDialogId}' not available, looking for alternative...");
            startingDialogId = FindFirstAvailableDialog(npcName);
        }

        if (string.IsNullOrEmpty(startingDialogId))
        {
            Debug.LogWarning($"DialogSystem: No available dialog for NPC: {npcName}");
            return false;
        }

        isDialogActive = true;
        currentNPCName = npcName;
        currentDialogId = startingDialogId;

        // Record NPC interaction for quests
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.RecordTalkedToNPC(npcName);
        }

        OnDialogStarted?.Invoke(npcName);
        ShowDialogLine(startingDialogId);

        if (debugMode) Debug.Log($"=== DIALOG STARTED SUCCESSFULLY ===");
        if (debugMode) Debug.Log($"NPC: {npcName}, Dialog ID: {startingDialogId}");
        return true;
    }

    private string FindFirstAvailableDialog(string npcName)
    {
        if (!dialogTrees.TryGetValue(npcName, out var tree))
            return null;

        if (debugMode) Debug.Log($"=== FINDING FIRST AVAILABLE DIALOG FOR {npcName} ===");
        
        // MODIFIED: Check ALL non-default dialogues first, in the order they appear in JSON
        foreach (var kvp in tree.dialogs)
        {
            if (kvp.Key == "default") 
            {
                if (debugMode) Debug.Log($"Skipping 'default' for now - checking it last");
                continue;
            }
            
            if (debugMode) Debug.Log($"Checking non-default dialog: {kvp.Key}");
            
            if (CheckDialogConditions(kvp.Value))
            {
                if (debugMode) Debug.Log($"✓ Found available dialog: {kvp.Key}");
                return kvp.Key;
            }
            else
            {
                if (debugMode) Debug.Log($"✗ Dialog '{kvp.Key}' conditions not met");
            }
        }

        // ONLY check "default" if NO other dialogue was found
        if (tree.dialogs.ContainsKey("default"))
        {
            if (debugMode) Debug.Log($"No non-default dialogues available, checking 'default'...");
            
            if (CheckDialogConditions(tree.dialogs["default"]))
            {
                if (debugMode) Debug.Log($"✓ Found available default dialog");
                return "default";
            }
            else
            {
                if (debugMode) Debug.Log($"✗ 'default' dialog conditions not met");
            }
        }

        if (debugMode) Debug.Log($"No available dialogs found for {npcName}");
        return null;
    }

    private void ShowDialogLine(string dialogId)
    {
        if (debugMode) Debug.Log($"=== SHOWING DIALOG LINE ===");
        if (debugMode) Debug.Log($"Dialog ID: {dialogId}");

        if (!currentDialogTree.dialogs.TryGetValue(dialogId, out currentDialogLine))
        {
            Debug.LogWarning($"DialogSystem: Dialog ID {dialogId} not found for NPC {currentDialogTree.npcName}");
            EndDialog();
            return;
        }

        // Process triggers for this line
        if (currentDialogLine.triggersOnShow.Count > 0)
        {
            if (debugMode) Debug.Log($"Executing {currentDialogLine.triggersOnShow.Count} triggers on show...");
            foreach (var trigger in currentDialogLine.triggersOnShow)
            {
                ProcessTrigger(trigger);
            }
        }

        // Clear current choices
        currentChoices.Clear();

        // Check for choices and cache them
        if (currentDialogLine.choices.Count > 0)
        {
            var availableChoices = currentDialogLine.choices.Where(c => CheckChoiceConditions(c)).ToList();
            if (availableChoices.Count > 0)
            {
                currentChoices = availableChoices;
                if (debugMode) Debug.Log($"Found {currentChoices.Count} available choices (from {currentDialogLine.choices.Count} total)");
            }
        }

        // Notify UI with dialog line - dialogue should start typing immediately
        OnDialogLineReady?.Invoke(currentDialogTree.npcName, currentDialogLine);
    }

    public void ShowAvailableChoices()
    {
        if (debugMode) Debug.Log($"=== SHOWING AVAILABLE CHOICES ===");

        if (currentChoices.Count > 0)
        {
            if (debugMode) Debug.Log($"Sending {currentChoices.Count} choices to UI");
            OnChoicesReady?.Invoke(new List<DialogChoice>(currentChoices)); // Send a copy to prevent modification
        }
        else if (string.IsNullOrEmpty(currentDialogLine.nextDialogId))
        {
            // No choices and no next dialog, end the conversation
            if (debugMode) Debug.Log($"No choices and no next dialog, ending dialog");
            EndDialog();
        }
        else
        {
            // No choices but has next dialog, auto-advance
            if (debugMode) Debug.Log($"No choices, advancing to next dialog");
            AdvanceDialog();
        }
    }

    public void SelectChoice(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= currentChoices.Count)
        {
            Debug.LogError($"DialogSystem: Invalid choice index {choiceIndex}. Current choices count: {currentChoices.Count}");
            return;
        }

        var choice = currentChoices[choiceIndex];
        if (debugMode) Debug.Log($"=== CHOICE SELECTED ===");
        if (debugMode) Debug.Log($"Choice Text: {choice.choiceText}");
        if (debugMode) Debug.Log($"Result Dialog ID: {choice.resultDialogId ?? "null"}");

        // Process triggers for this choice
        if (choice.triggersOnChoose.Count > 0)
        {
            if (debugMode) Debug.Log($"Executing {choice.triggersOnChoose.Count} triggers on choose...");
            foreach (var trigger in choice.triggersOnChoose)
            {
                ProcessTrigger(trigger);
            }
        }

        currentChoices.Clear();

        if (!string.IsNullOrEmpty(choice.resultDialogId))
        {
            currentDialogId = choice.resultDialogId;
            ShowDialogLine(currentDialogId);
        }
        else
        {
            EndDialog();
        }
    }

    public void AdvanceDialog()
    {
        if (debugMode) Debug.Log($"=== ADVANCING DIALOG ===");

        if (string.IsNullOrEmpty(currentDialogLine.nextDialogId))
        {
            if (debugMode) Debug.Log($"No next dialog ID, ending dialog");
            EndDialog();
            return;
        }

        if (debugMode) Debug.Log($"Next dialog ID: {currentDialogLine.nextDialogId}");
        ShowDialogLine(currentDialogLine.nextDialogId);
    }

    public void EndDialog()
    {
        if (debugMode) Debug.Log($"=== ENDING DIALOG ===");
        if (debugMode) Debug.Log($"NPC: {currentDialogTree?.npcName ?? "Unknown"}");

        isDialogActive = false;
        currentChoices.Clear();

        OnDialogEnded?.Invoke(currentNPCName);

        currentDialogTree = null;
        currentDialogLine = null;
        currentDialogId = null;
        currentNPCName = null;

        if (debugMode) Debug.Log($"Dialog ended successfully");
    }

    #region Condition Checking

    private bool CheckDialogConditions(DialogLine dialogLine)
    {
        if (dialogLine.showConditions == null || dialogLine.showConditions.Count == 0)
            return true;

        if (debugMode && logConditionChecks) Debug.Log($"Checking {dialogLine.showConditions.Count} conditions for dialog line...");

        foreach (var condition in dialogLine.showConditions)
        {
            if (!CheckCondition(condition))
            {
                if (debugMode && logConditionChecks)
                    Debug.Log($"Condition failed - Type: {condition.conditionType}, Key: {condition.conditionKey}, Value: {condition.conditionValue}");
                return false;
            }
        }

        if (debugMode && logConditionChecks) Debug.Log($"All conditions passed!");
        return true;
    }

    private bool CheckCondition(DialogCondition condition)
    {
        bool result = false;

        switch (condition.conditionType)
        {
            case DialogConditionType.ALWAYS:  // 0
                result = true;
                break;

            case DialogConditionType.QUEST_STATE:  // 1
                var quest = QuestManager.Instance?.GetQuest(condition.conditionKey);
                if (quest == null)
                {
                    if (debugMode && logConditionChecks) Debug.Log($"QUEST_STATE: Quest '{condition.conditionKey}' not found");
                    result = false;
                }
                else
                {
                    result = quest.state.ToString() == condition.conditionValue;
                    if (debugMode && logConditionChecks)
                        Debug.Log($"QUEST_STATE: Quest '{quest.questName}' state is {quest.state}, required: {condition.conditionValue} => {result}");
                }
                break;

            case DialogConditionType.QUEST_ACTIVE:  // 2
                var activeQuest = QuestManager.Instance?.GetQuest(condition.conditionKey);
                result = activeQuest != null && activeQuest.state == QuestState.ACTIVE;
                if (debugMode && logConditionChecks)
                    Debug.Log($"QUEST_ACTIVE: Quest '{condition.conditionKey}' is {(result ? "ACTIVE" : "not active or not found")}");
                break;

            case DialogConditionType.QUEST_COMPLETED:  // 3
                var completedQuest = QuestManager.Instance?.GetQuest(condition.conditionKey);
                result = completedQuest != null && completedQuest.state == QuestState.COMPLETED;
                if (debugMode && logConditionChecks)
                    Debug.Log($"QUEST_COMPLETED: Quest '{condition.conditionKey}' is {(result ? "COMPLETED" : "not completed or not found")}");
                break;

            case DialogConditionType.HAS_ITEM:  // 4
                if (QuestManager.Instance == null)
                {
                    result = false;
                    if (debugMode && logConditionChecks) Debug.Log($"HAS_ITEM: QuestManager not available");
                }
                else if (condition.conditionKey == "any_fish" || condition.conditionKey.Contains("fish"))
                {
                    // FIXED: Check DetailedFishInventory for fish count
                    int currentAmount = 0;
                    if (DetailedFishInventory.Instance != null)
                    {
                        currentAmount = DetailedFishInventory.Instance.CurrentInventoryCount;
                    }
                    else
                    {
                        // Fallback to fish count from save file
                        if (condition.conditionKey == "any_fish")
                        {
                            // Try to get fish count from quest save data
                            var fishQuest = QuestManager.Instance?.GetQuest("FIRST_CAST_2A");
                            if (fishQuest != null)
                            {
                                foreach (var trigger in fishQuest.completionTriggers)
                                {
                                    if (trigger.triggerType == QuestTriggerType.HAVE_ITEM && 
                                        (trigger.targetId.Contains("fish") || trigger.targetId == "Fish"))
                                    {
                                        currentAmount = trigger.currentAmount;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    
                    int requiredAmount = int.TryParse(condition.conditionValue, out int req) ? req : 1;
                    result = currentAmount >= requiredAmount;
                    if (debugMode && logConditionChecks)
                        Debug.Log($"HAS_ITEM (FISH): Have {currentAmount} fish, need {requiredAmount} => {result}");
                }
                else
                {
                    // Original quest item check
                    int currentAmount = QuestManager.Instance.GetFishCountForQuest(condition.conditionKey);
                    int requiredAmount = int.TryParse(condition.conditionValue, out int req) ? req : 1;
                    result = currentAmount >= requiredAmount;
                    if (debugMode && logConditionChecks)
                        Debug.Log($"HAS_ITEM (QUEST): {condition.conditionKey} - Have {currentAmount}, need {requiredAmount} => {result}");
                }
                break;

            case DialogConditionType.SKILL_LEVEL:  // 5
                if (QuestManager.Instance == null)
                {
                    result = false;
                }
                else
                {
                    int skillLevel = QuestManager.Instance.GetSkillLevel(condition.conditionKey);
                    int requiredLevel = int.TryParse(condition.conditionValue, out int reqLevel) ? reqLevel : 0;

                    switch (condition.comparison)
                    {
                        case ComparisonType.EQUALS:
                            result = skillLevel == requiredLevel;
                            break;
                        case ComparisonType.GREATER_THAN:
                            result = skillLevel > requiredLevel;
                            break;
                        case ComparisonType.LESS_THAN:
                            result = skillLevel < requiredLevel;
                            break;
                        case ComparisonType.GREATER_OR_EQUAL:
                            result = skillLevel >= requiredLevel;
                            break;
                        case ComparisonType.LESS_OR_EQUAL:
                            result = skillLevel <= requiredLevel;
                            break;
                        default:
                            result = skillLevel >= requiredLevel;
                            break;
                    }

                    if (debugMode && logConditionChecks)
                        Debug.Log($"SKILL_LEVEL: {condition.conditionKey} level is {skillLevel}, required: {requiredLevel} ({condition.comparison}) => {result}");
                }
                break;

            case DialogConditionType.REPUTATION:  // 6
                if (QuestManager.Instance == null)
                {
                    result = false;
                }
                else
                {
                    int reputation = QuestManager.Instance.GetReputation(condition.conditionKey);
                    int requiredRep = int.TryParse(condition.conditionValue, out int reqRep) ? reqRep : 0;

                    switch (condition.comparison)
                    {
                        case ComparisonType.EQUALS:
                            result = reputation == requiredRep;
                            break;
                        case ComparisonType.GREATER_THAN:
                            result = reputation > requiredRep;
                            break;
                        case ComparisonType.LESS_THAN:
                            result = reputation < requiredRep;
                            break;
                        case ComparisonType.GREATER_OR_EQUAL:
                            result = reputation >= requiredRep;
                            break;
                        case ComparisonType.LESS_OR_EQUAL:
                            result = reputation <= requiredRep;
                            break;
                        default:
                            result = reputation >= requiredRep;
                            break;
                    }

                    if (debugMode && logConditionChecks)
                        Debug.Log($"REPUTATION: {condition.conditionKey} is {reputation}, required: {requiredRep} ({condition.comparison}) => {result}");
                }
                break;

            case DialogConditionType.POPULATION_HEALTH:  // 7
                if (QuestManager.Instance == null)
                {
                    result = false;
                }
                else
                {
                    float health = QuestManager.Instance.GetPopulationHealth(condition.conditionKey);
                    float requiredHealth = float.TryParse(condition.conditionValue, out float reqHealth) ? reqHealth : 0f;

                    switch (condition.comparison)
                    {
                        case ComparisonType.EQUALS:
                            result = Mathf.Approximately(health, requiredHealth);
                            break;
                        case ComparisonType.GREATER_THAN:
                            result = health > requiredHealth;
                            break;
                        case ComparisonType.LESS_THAN:
                            result = health < requiredHealth;
                            break;
                        case ComparisonType.GREATER_OR_EQUAL:
                            result = health >= requiredHealth;
                            break;
                        case ComparisonType.LESS_OR_EQUAL:
                            result = health <= requiredHealth;
                            break;
                        default:
                            result = health >= requiredHealth;
                            break;
                    }

                    if (debugMode && logConditionChecks)
                        Debug.Log($"POPULATION_HEALTH: {condition.conditionKey} health is {health}, required: {requiredHealth} ({condition.comparison}) => {result}");
                }
                break;

            case DialogConditionType.PLAYER_CHOICE:  // 8
                string playerChoice = PlayerPrefs.GetString("PlayerPathChoice", "");
                result = playerChoice == condition.conditionValue;
                if (debugMode && logConditionChecks)
                    Debug.Log($"PLAYER_CHOICE: Current choice is '{playerChoice}', required: '{condition.conditionValue}' => {result}");
                break;

            case DialogConditionType.TIME_OF_DAY:  // 9
                float currentHour = UnityEngine.Random.Range(0f, 24f); // Placeholder - use your time system
                float requiredHour = float.TryParse(condition.conditionValue, out float reqHour) ? reqHour : 0f;
                result = Mathf.Abs(currentHour - requiredHour) <= 1f;
                if (debugMode && logConditionChecks)
                    Debug.Log($"TIME_OF_DAY: Current hour is {currentHour}, required: {requiredHour} => {result}");
                break;

            case DialogConditionType.HAS_MET_NPC:  // 10
                if (QuestManager.Instance == null)
                {
                    result = false;
                    if (debugMode && logConditionChecks) Debug.Log($"HAS_MET_NPC: QuestManager not available");
                }
                else
                {
                    result = QuestManager.Instance.HasTalkedToNPC(condition.conditionKey);
                    if (debugMode && logConditionChecks)
                        Debug.Log($"HAS_MET_NPC: Has met {condition.conditionKey} => {result}");
                }
                break;

            case DialogConditionType.TUTORIAL_COMPLETED:  // 11
                if (QuestManager.Instance == null)
                {
                    result = false;
                }
                else
                {
                    result = QuestManager.Instance.IsTutorialCompleted(condition.conditionKey);
                    if (debugMode && logConditionChecks)
                        Debug.Log($"TUTORIAL_COMPLETED: Tutorial '{condition.conditionKey}' completed => {result}");
                }
                break;

            case DialogConditionType.SKILL_LEVEL_COMPARISON:  // 12
                if (QuestManager.Instance == null)
                {
                    result = false;
                }
                else
                {
                    int skillLevelComp = QuestManager.Instance.GetSkillLevel(condition.conditionKey);
                    int requiredLevelComp = int.TryParse(condition.conditionValue, out int reqLevelComp) ? reqLevelComp : 0;

                    switch (condition.comparison)
                    {
                        case ComparisonType.EQUALS:
                            result = skillLevelComp == requiredLevelComp;
                            break;
                        case ComparisonType.GREATER_THAN:
                            result = skillLevelComp > requiredLevelComp;
                            break;
                        case ComparisonType.LESS_THAN:
                            result = skillLevelComp < requiredLevelComp;
                            break;
                        case ComparisonType.GREATER_OR_EQUAL:
                            result = skillLevelComp >= requiredLevelComp;
                            break;
                        case ComparisonType.LESS_OR_EQUAL:
                            result = skillLevelComp <= requiredLevelComp;
                            break;
                        default:
                            result = skillLevelComp >= requiredLevelComp;
                            break;
                    }

                    if (debugMode && logConditionChecks)
                        Debug.Log($"SKILL_LEVEL_COMPARISON: {condition.conditionKey} level is {skillLevelComp}, required: {requiredLevelComp} ({condition.comparison}) => {result}");
                }
                break;

            default:
                result = true;
                if (debugMode && logConditionChecks)
                    Debug.Log($"Condition type {condition.conditionType} not fully implemented, defaulting to true");
                break;
        }

        return result == condition.expectedBool;
    }

    public void DebugVincentConditions()
    {
        if (!dialogTrees.TryGetValue("Vincent", out var tree))
        {
            Debug.LogError("No dialog tree for Vincent!");
            return;
        }
        
        Debug.Log("=== VINCENT DIALOGUE CONDITIONS DEBUG ===");
        
        // Check FIRST_CAST_2A quest status
        var quest = QuestManager.Instance?.GetQuest("FIRST_CAST_2A");
        if (quest != null)
        {
            Debug.Log($"FIRST_CAST_2A Quest Status:");
            Debug.Log($"  State: {quest.state}");
            Debug.Log($"  Progress: {quest.GetProgressText()}");
            
            // Check fish count
            int fishCount = DetailedFishInventory.Instance?.CurrentInventoryCount ?? 0;
            Debug.Log($"  Fish in inventory: {fishCount}");
        }
        else
        {
            Debug.LogWarning("FIRST_CAST_2A quest not found in QuestManager!");
        }
        
        // Check each dialogue condition
        foreach (var kvp in tree.dialogs)
        {
            Debug.Log($"\nDialogue: {kvp.Key}");
            bool conditionsPass = CheckDialogConditions(kvp.Value);
            Debug.Log($"  All conditions pass: {conditionsPass}");
            
            if (kvp.Value.showConditions != null)
            {
                foreach (var condition in kvp.Value.showConditions)
                {
                    bool conditionResult = CheckCondition(condition);
                    Debug.Log($"  Condition: {condition.conditionType} {condition.conditionKey}={condition.conditionValue} => {conditionResult}");
                }
            }
        }
    }

    private bool CheckChoiceConditions(DialogChoice choice)
    {
        if (choice.showConditions == null || choice.showConditions.Count == 0)
            return true;

        if (debugMode && logConditionChecks) Debug.Log($"Checking conditions for choice: {choice.choiceText}");

        foreach (var condition in choice.showConditions)
        {
            if (!CheckCondition(condition))
            {
                if (debugMode && logConditionChecks)
                    Debug.Log($"Choice condition failed - Type: {condition.conditionType}, Key: {condition.conditionKey}");
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Trigger Processing

    private void ProcessTrigger(string trigger)
    {
        if (string.IsNullOrEmpty(trigger))
        {
            if (debugMode && logTriggerProcessing) Debug.Log($"Empty trigger, skipping");
            return;
        }

        string[] parts = trigger.Split(':');
        if (parts.Length == 0)
        {
            if (debugMode && logTriggerProcessing) Debug.Log($"Invalid trigger format: {trigger}");
            return;
        }

        string triggerType = parts[0];
        string triggerValue = parts.Length > 1 ? parts[1] : "";

        if (debugMode && logTriggerProcessing)
            Debug.Log($"Processing trigger: {triggerType} -> {triggerValue}");

        OnDialogTrigger?.Invoke(trigger);

        switch (triggerType)
        {
            case "StartQuest":
                if (QuestManager.Instance != null)
                {
                    var quest = QuestManager.Instance.GetQuest(triggerValue);
                    if (quest != null && quest.state == QuestState.LOCKED)
                    {
                        QuestManager.Instance.ActivateQuestManually(triggerValue);
                        if (debugMode) Debug.Log($"Started quest: {triggerValue}");
                    }
                }
                break;

            case "CompleteQuest":
                if (QuestManager.Instance != null)
                {
                    var quest = QuestManager.Instance.GetQuest(triggerValue);
                    if (quest != null && quest.state == QuestState.ACTIVE)
                    {
                        QuestManager.Instance.CompleteQuestManually(triggerValue);
                        if (debugMode) Debug.Log($"Completed quest: {triggerValue}");
                    }
                }
                break;

            case "RecordChoice":
                PlayerPrefs.SetString("PlayerPathChoice", triggerValue);
                if (debugMode) Debug.Log($"Recorded player choice: {triggerValue}");
                PlayerPrefs.Save();
                break;

            case "RecordTalkedToNPC":
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.RecordTalkedToNPC(triggerValue);
                    if (debugMode) Debug.Log($"Recorded talking to NPC: {triggerValue}");
                }
                break;

            case "GiveItem":
                string[] itemParts = triggerValue.Split(',');
                if (itemParts.Length >= 2)
                {
                    string itemId = itemParts[0];
                    int amount = int.Parse(itemParts[1]);
                    if (QuestManager.Instance != null)
                    {
                        QuestManager.Instance.RecordItemObtained(itemId, amount);
                        if (debugMode) Debug.Log($"Gave item: {itemId} x{amount}");
                    }
                }
                break;

            case "DeductGold":
                if (int.TryParse(triggerValue, out int goldAmount))
                {
                    if (QuestManager.Instance != null)
                    {
                        QuestManager.Instance.DeductGold(goldAmount);
                        if (debugMode) Debug.Log($"Deducted {goldAmount} gold");
                    }
                }
                break;

            case "OpenShop":
                // Find the DialogInteractor for this NPC and open the shop
                var dialogInteractor = FindObjectsOfType<DialogInteractor>()
                    .FirstOrDefault(di => di.npcName == currentNPCName);

                if (dialogInteractor != null)
                {
                    dialogInteractor.OpenShop();
                    EndDialog(); // Close the dialog when opening shop
                }
                else
                {
                    Debug.LogWarning($"DialogSystem: Could not find DialogInteractor for NPC {currentNPCName} to open shop");
                }
                break;

            case "UnlockAchievement":
                // Placeholder for achievement system
                if (debugMode) Debug.Log($"Unlocked achievement: {triggerValue}");
                break;

            case "SetSkill":
                string[] skillParts = triggerValue.Split(':');
                if (skillParts.Length >= 2)
                {
                    string skillName = skillParts[0];
                    if (int.TryParse(skillParts[1], out int skillLevel))
                    {
                        if (QuestManager.Instance != null)
                        {
                            QuestManager.Instance.UpdateSkillLevel(skillName, skillLevel);
                            if (debugMode) Debug.Log($"Set skill {skillName} to level {skillLevel}");
                        }
                    }
                }
                break;

            case "RecordFishProcessed":
                if (QuestManager.Instance != null)
                {
                    int amount = 1;
                    if (!string.IsNullOrEmpty(triggerValue))
                        int.TryParse(triggerValue, out amount);
                    QuestManager.Instance.RecordFishProcessed(amount);
                    if (debugMode && logTriggerProcessing)
                        Debug.Log($"RecordFishProcessed: {amount} fish processed");
                }
                break;

            case "LoadScene":
                if (debugMode && logTriggerProcessing)
                    Debug.Log($"Loading scene: {triggerValue}");

                // End the current dialog before scene change
                EndDialog();

                // Save player position if player exists
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    // Try to get Player component with SavePlayerPosition method
                    MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
                    foreach (MonoBehaviour script in scripts)
                    {
                        System.Type type = script.GetType();
                        var method = type.GetMethod("SavePlayerPosition",
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.Instance);
                        if (method != null)
                        {
                            method.Invoke(script, null);
                            if (debugMode) Debug.Log($"Saved player position before loading {triggerValue}");
                            break;
                        }
                    }
                }

                // Load the scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(triggerValue);
                break;

            case "PlayMusic":
                if (debugMode && logTriggerProcessing)
                    Debug.Log($"Playing music: {triggerValue}");

                // Try to find MusicManager instance
                MusicManager musicManager = GameObject.FindObjectOfType<MusicManager>();
                if (musicManager != null)
                {
                    System.Type type = musicManager.GetType();
                    var method = type.GetMethod("PlayMusic",
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance,
                        null,
                        new System.Type[] { typeof(string) },
                        null);

                    if (method != null)
                    {
                        method.Invoke(musicManager, new object[] { triggerValue });
                        if (debugMode) Debug.Log($"Invoked PlayMusic with: {triggerValue}");
                    }
                    else
                    {
                        // Try alternative method signature
                        method = type.GetMethod("PlayMusic",
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.Instance);

                        if (method != null)
                        {
                            method.Invoke(musicManager, new object[] { triggerValue });
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"MusicManager not found in scene. Cannot play music: {triggerValue}");
                }
                break;

            default:
                if (debugMode && logTriggerProcessing)
                    Debug.Log($"Trigger type '{triggerType}' handled by external systems via OnDialogTrigger event");
                break;
        }
    }

    #endregion

    #region Public Methods

    public bool IsDialogActive()
    {
        return isDialogActive;
    }

    public string GetCurrentNPC()
    {
        return currentNPCName;
    }

    public void AddDialogFile(TextAsset file)
    {
        if (file != null && !dialogFiles.Contains(file))
        {
            dialogFiles.Add(file);
            ReloadDialogDatabase();
        }
    }

    public void RemoveDialogFile(TextAsset file)
    {
        if (dialogFiles.Contains(file))
        {
            dialogFiles.Remove(file);
            ReloadDialogDatabase();
        }
    }

    public void ReloadDialogDatabase()
    {
        dialogTrees.Clear();
        LoadDialogDatabase();
        if (debugMode) Debug.Log("DialogSystem: Reloaded all dialog files");
    }

    public bool HasNPCDialog(string npcName)
    {
        return dialogTrees.ContainsKey(npcName);
    }

    public List<string> GetAllLoadedNPCs()
    {
        return dialogTrees.Keys.ToList();
    }

    public List<DialogChoice> GetCurrentChoices()
    {
        return new List<DialogChoice>(currentChoices); // Return a copy
    }

    public bool HasAvailableChoices()
    {
        return currentChoices.Count > 0;
    }

    #endregion
}