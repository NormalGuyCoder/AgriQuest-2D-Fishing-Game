using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class QuestListUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questPanel;
    public GameObject questSlotPrefab;
    public Transform contentParent;

    [Header("Right Panel Details")]
    public GameObject detailsPanel;
    public TextMeshProUGUI detailQuestNameText;
    public TextMeshProUGUI detailDescriptionText;
    public TextMeshProUGUI detailObjectivesText;  // Now shows both objectives and progress
    public TextMeshProUGUI detailRewardsText;
    public TextMeshProUGUI detailLocationHintText;
    public TextMeshProUGUI detailGiverNPCText;

    [Header("Display Settings")]
    public bool showCompletedQuests = false;
    public Color selectedQuestColor = Color.yellow;
    public Color defaultQuestColor = Color.white;
    public Color activeQuestColor = Color.yellow;
    public Color completedQuestColor = Color.green;
    public Color lockedQuestColor = Color.gray;

    [Header("Debug")]
    public bool debugMode = true;

    [Header("Testing Tools")]
    [Tooltip("IMPORTANT: Uncheck this in normal gameplay. Only use for testing.")]
    public bool resetQuestsOnStart = false;

    private Dictionary<string, GameObject> questSlots = new Dictionary<string, GameObject>();
    private bool hasSubscribedToEvents = false;
    private List<string> displayedQuestIds = new List<string>();
    private bool isInitialized = false;
    private Coroutine initializationCoroutine;

    // Track selected quest
    private string selectedQuestId = "";
    private GameObject selectedQuestSlot = null;

    void Start()
    {
        if (debugMode) Debug.Log("=== QuestListUI: Start ===");

        // Validate UI references
        if (questPanel == null)
        {
            Debug.LogError("QuestListUI: questPanel is not assigned!");
            return;
        }

        if (questSlotPrefab == null)
        {
            Debug.LogError("QuestListUI: questSlotPrefab is not assigned!");
            return;
        }

        if (contentParent == null)
        {
            Debug.LogError("QuestListUI: contentParent is not assigned!");
            return;
        }

        // Validate details panel references
        if (detailsPanel == null)
        {
            Debug.LogError("QuestListUI: detailsPanel is not assigned!");
            return;
        }

        // Initialize panels
        questPanel.SetActive(true);
        detailsPanel.SetActive(false); // Start with details panel hidden

        if (resetQuestsOnStart)
        {
            Debug.LogError("ERROR: resetQuestsOnStart is checked! Uncheck this in the Inspector!");
            resetQuestsOnStart = false;
        }

        // Start delayed initialization
        initializationCoroutine = StartCoroutine(DelayedInitialization());
    }

    IEnumerator DelayedInitialization()
    {
        bool isNewGame = PlayerPrefs.GetInt("GameStarted", 0) == 0;

        if (isNewGame && debugMode)
        {
            Debug.Log("QuestListUI: Detected new game start");
            yield return new WaitForSeconds(1.0f);
        }

        // Wait for QuestManager to initialize
        int attempts = 0;
        while (QuestManager.Instance == null && attempts < 30)
        {
            if (debugMode && attempts % 10 == 0) Debug.Log("QuestListUI: Waiting for QuestManager...");
            yield return new WaitForSeconds(0.1f);
            attempts++;
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestListUI: QuestManager not found after waiting!");
            CreateNoQuestsMessage("Quest system not available. Starting new game...");
            yield break;
        }

        if (debugMode) Debug.Log("QuestListUI: QuestManager found, initializing...");

        bool forcedNewGame = PlayerPrefs.GetInt("NewGameRequested", 0) == 1;
        if (forcedNewGame)
        {
            if (debugMode) Debug.Log("QuestListUI: Forced new game detected - performing complete refresh");
            PlayerPrefs.SetInt("NewGameRequested", 0);
            PlayerPrefs.Save();
            yield return new WaitForSeconds(1.0f);
            ForceCompleteUIRefresh();
            yield break;
        }

        // CRITICAL: If this is a new game, force QuestManager to reload from JSON
        if (isNewGame)
        {
            yield return new WaitForSeconds(0.5f);
            ForceQuestManagerLoadWithRetry();
            PlayerPrefs.SetInt("GameStarted", 1);
            PlayerPrefs.Save();
        }

        yield return new WaitForSeconds(0.5f);

        // FIXED: Direct check and activation of first quest if needed
        var activeQuests = QuestManager.Instance.GetActiveQuests();
        if (activeQuests.Count == 0)
        {
            if (debugMode) Debug.Log("QuestListUI: No active quests found, checking for first quest...");
            var firstQuest = QuestManager.Instance.GetQuest("ORIENTATION_DAY_0");
            if (firstQuest != null && firstQuest.state == QuestState.LOCKED)
            {
                QuestManager.Instance.ActivateQuestManually("ORIENTATION_DAY_0");
                if (debugMode) Debug.Log("QuestListUI: Manually activated first quest");
                yield return new WaitForSeconds(0.5f);
                activeQuests = QuestManager.Instance.GetActiveQuests();
            }
        }

        // Initialize UI
        RefreshQuestList();
        SubscribeToEvents();
        isInitialized = true;

        if (debugMode) Debug.Log("QuestListUI: Initialization complete");
    }

    // NEW: Complete UI Refresh Method for New Game Reset
    public void ForceCompleteUIRefresh()
    {
        if (debugMode) Debug.Log("=== QuestListUI: Force Complete UI Refresh ===");

        if (initializationCoroutine != null)
        {
            StopCoroutine(initializationCoroutine);
            initializationCoroutine = null;
        }

        ClearAllSlots();
        isInitialized = false;
        displayedQuestIds.Clear();
        questSlots.Clear();
        selectedQuestId = "";
        selectedQuestSlot = null;

        if (contentParent != null)
        {
            Transform message = contentParent.Find("NoQuestsMessage");
            if (message != null) Destroy(message.gameObject);
        }

        detailsPanel.SetActive(false);

        StartCoroutine(DelayedRefreshAfterNewGame());
    }

    private IEnumerator DelayedRefreshAfterNewGame()
    {
        yield return null;

        int attempts = 0;
        while (QuestManager.Instance == null && attempts < 30)
        {
            yield return new WaitForSeconds(0.1f);
            attempts++;
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestListUI: QuestManager not found after new game reset!");
            CreateNoQuestsMessage("Quest system not available. Please restart.");
            yield break;
        }

        yield return new WaitForSeconds(0.5f);

        var firstQuest = QuestManager.Instance.GetQuest("ORIENTATION_DAY_0");
        if (firstQuest != null && firstQuest.state != QuestState.ACTIVE)
        {
            QuestManager.Instance.ActivateQuestManually("ORIENTATION_DAY_0");
            yield return new WaitForSeconds(0.2f);
        }

        RefreshQuestList();
        SubscribeToEvents();
        isInitialized = true;

        if (debugMode) Debug.Log("QuestListUI: Complete refresh finished after new game");
    }

    private void ForceQuestManagerLoadWithRetry()
    {
        bool success = false;
        int retryCount = 0;

        while (!success && retryCount < 3)
        {
            try
            {
                ForceQuestManagerLoad();
                success = true;
            }
            catch (System.Exception e)
            {
                retryCount++;
                Debug.LogWarning($"QuestListUI: ForceQuestManagerLoad failed (attempt {retryCount}): {e.Message}");
                if (retryCount < 3)
                {
                    StartCoroutine(DelayedRetry(retryCount * 0.5f));
                }
            }
        }
    }

    private IEnumerator DelayedRetry(float delay)
    {
        yield return new WaitForSeconds(delay);
        ForceQuestManagerLoad();
    }

    private void ForceQuestManagerLoad()
    {
        var questManager = QuestManager.Instance;
        if (questManager != null)
        {
            System.Type questManagerType = questManager.GetType();
            var loadMethod = questManagerType.GetMethod("Load", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (loadMethod != null)
            {
                loadMethod.Invoke(questManager, null);
                if (debugMode) Debug.Log("QuestListUI: Forced QuestManager to load quests");
            }
        }
    }

    void SubscribeToEvents()
    {
        if (QuestManager.Instance == null) return;

        if (!hasSubscribedToEvents)
        {
            QuestManager.OnQuestStarted += OnQuestStarted;
            QuestManager.OnQuestProgressUpdated += OnQuestProgressUpdated;
            QuestManager.OnQuestCompleted += OnQuestCompleted;
            QuestManager.OnQuestTurnedIn += OnQuestTurnedIn;
            QuestManager.OnNewGameReset += OnNewGameReset;

            hasSubscribedToEvents = true;
            if (debugMode) Debug.Log("QuestListUI: Subscribed to quest events");
        }
    }

    void OnNewGameReset()
    {
        if (debugMode) Debug.Log("QuestListUI: New game reset detected, refreshing list");
        ForceCompleteUIRefresh();
    }

    public void RefreshQuestList()
    {
        // Clean up any destroyed references first
        CleanupDestroyedSlots();

        if (!isInitialized && QuestManager.Instance == null)
        {
            if (debugMode) Debug.Log("QuestListUI: Not initialized yet, skipping refresh");
            return;
        }

        if (debugMode) Debug.Log("=== QuestListUI: Refreshing Quest List ===");

        ClearAllSlots();
        displayedQuestIds.Clear();

        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestListUI: QuestManager is null!");
            CreateNoQuestsMessage("Quest system not available");
            return;
        }

        List<QuestData> questsToShow = new List<QuestData>();
        HashSet<string> addedQuestIds = new HashSet<string>();

        // Get active quests
        var activeQuests = QuestManager.Instance.GetActiveQuests();
        if (debugMode) Debug.Log($"Found {activeQuests.Count} active quests");

        foreach (var quest in activeQuests)
        {
            if (quest == null) continue;

            if (!addedQuestIds.Contains(quest.questId))
            {
                questsToShow.Add(quest);
                addedQuestIds.Add(quest.questId);
                if (debugMode) Debug.Log($"Adding active quest: {quest.questName} (State: {quest.state})");
            }
        }

        // Get completed quests if enabled
        if (showCompletedQuests)
        {
            var completedQuests = QuestManager.Instance.GetCompletedQuests();
            foreach (var quest in completedQuests)
            {
                if (quest == null) continue;

                if (!addedQuestIds.Contains(quest.questId))
                {
                    questsToShow.Add(quest);
                    addedQuestIds.Add(quest.questId);
                }
            }
        }

        // If no quests found, show a helpful message
        if (questsToShow.Count == 0)
        {
            CreateNoQuestsMessage("No active quests. Talk to Professor Clark to begin!");

            // Hide details panel
            detailsPanel.SetActive(false);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene == "Auditorium" || currentScene == "Riverside")
            {
                StartCoroutine(ForceInitialQuest());
            }
        }
        else
        {
            // Create UI slots for each quest
            foreach (var quest in questsToShow)
            {
                if (quest != null)
                {
                    CreateQuestSlot(quest);
                    displayedQuestIds.Add(quest.questId);
                }
            }

            // Auto-select first quest if none selected
            if (string.IsNullOrEmpty(selectedQuestId) && questsToShow.Count > 0)
            {
                SelectQuest(questsToShow[0].questId);
            }
            else if (!string.IsNullOrEmpty(selectedQuestId))
            {
                // Reselect the previously selected quest if it exists
                if (questsToShow.Exists(q => q.questId == selectedQuestId))
                {
                    SelectQuest(selectedQuestId);
                }
                else
                {
                    // Select first quest if previous selection is gone
                    SelectQuest(questsToShow[0].questId);
                }
            }
        }

        if (debugMode) Debug.Log($"Displayed {questsToShow.Count} quests");
    }

    private IEnumerator ForceInitialQuest()
    {
        yield return new WaitForSeconds(1.0f);

        bool questStarted = false;

        try
        {
            var questManager = QuestManager.Instance;
            if (questManager != null)
            {
                System.Type questManagerType = questManager.GetType();
                var startMethod = questManagerType.GetMethod("StartQuest",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    null,
                    new System.Type[] { typeof(string) },
                    null);

                if (startMethod != null)
                {
                    startMethod.Invoke(questManager, new object[] { "ORIENTATION_DAY_0" });
                    Debug.Log("QuestListUI: Forced start of initial quest");
                    questStarted = true;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"QuestListUI: Could not force initial quest: {e.Message}");
        }

        if (questStarted)
        {
            yield return new WaitForSeconds(0.5f);
            RefreshQuestList();
        }
    }

    void CreateQuestSlot(QuestData quest)
    {
        if (quest == null || questSlotPrefab == null || contentParent == null) return;

        if (questSlots.ContainsKey(quest.questId))
        {
            UpdateQuestSlot(quest);
            return;
        }

        GameObject slot = Instantiate(questSlotPrefab, contentParent);
        slot.SetActive(true);
        slot.name = $"QuestSlot_{quest.questId}";

        // Add button component if not present
        Button button = slot.GetComponent<Button>();
        if (button == null)
        {
            button = slot.AddComponent<Button>();
        }

        // Set button click listener
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SelectQuest(quest.questId));

        // Find UI elements
        TextMeshProUGUI questNameText = null;
        Transform nameTransform = slot.transform.Find("Quest Name");
        if (nameTransform != null)
        {
            questNameText = nameTransform.GetComponent<TextMeshProUGUI>();
        }

        // Fallback: search for TextMeshProUGUI
        if (questNameText == null)
        {
            TextMeshProUGUI[] allTexts = slot.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (allTexts.Length >= 1) questNameText = allTexts[0];
        }

        // Set quest name with state indicator
        if (questNameText != null)
        {
            string stateIndicator = "";
            Color stateColor = GetQuestStateColor(quest.state);

            switch (quest.state)
            {
                case QuestState.ACTIVE:
                    stateIndicator = "> ";  // Using > for active (like "next up")
                    break;
                case QuestState.COMPLETED:
                    stateIndicator = "[X] ";  // Using [X] for completed (more font-friendly)
                    break;
                case QuestState.LOCKED:
                    stateIndicator = "[ ] ";  // Using [ ] for locked
                    break;
            }

            questNameText.text = stateIndicator + quest.questName;

            // Apply selection color if this is the selected quest
            if (quest.questId == selectedQuestId)
            {
                questNameText.color = selectedQuestColor;
            }
            else
            {
                questNameText.color = stateColor;
            }
        }

        questSlots[quest.questId] = slot;

        if (debugMode) Debug.Log($"Created quest slot: {quest.questName}");
    }

    Color GetQuestStateColor(QuestState state)
    {
        switch (state)
        {
            case QuestState.ACTIVE:
                return activeQuestColor;
            case QuestState.COMPLETED:
                return completedQuestColor;
            case QuestState.LOCKED:
                return lockedQuestColor;
            default:
                return defaultQuestColor;
        }
    }

    void CreateNoQuestsMessage(string message)
    {
        if (contentParent == null) return;

        Transform existingMessage = contentParent.Find("NoQuestsMessage");
        if (existingMessage != null) Destroy(existingMessage.gameObject);

        GameObject messageObj = new GameObject("NoQuestsMessage");
        messageObj.transform.SetParent(contentParent);
        messageObj.transform.SetAsLastSibling();

        RectTransform rect = messageObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 60);
        rect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI text = messageObj.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.fontSize = 14;
        text.color = new Color(0.7f, 0.7f, 0.7f);
        text.alignment = TextAlignmentOptions.Center;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
    }

    void ClearAllSlots()
    {
        if (contentParent == null) return;

        List<GameObject> childrenToDestroy = new List<GameObject>();
        foreach (Transform child in contentParent)
        {
            if (child.gameObject != questSlotPrefab && child.name != "NoQuestsMessage")
            {
                childrenToDestroy.Add(child.gameObject);
            }
        }

        foreach (GameObject child in childrenToDestroy)
        {
            Destroy(child);
        }

        questSlots.Clear();
        displayedQuestIds.Clear();
        selectedQuestSlot = null;

        Transform message = contentParent.Find("NoQuestsMessage");
        if (message != null) Destroy(message.gameObject);
    }

    // Method to clean up destroyed GameObjects from the dictionary
    void CleanupDestroyedSlots()
    {
        List<string> destroyedKeys = new List<string>();

        foreach (var kvp in questSlots)
        {
            if (kvp.Value == null)
            {
                destroyedKeys.Add(kvp.Key);
            }
        }

        foreach (var key in destroyedKeys)
        {
            questSlots.Remove(key);
            displayedQuestIds.Remove(key);

            if (selectedQuestId == key)
            {
                selectedQuestId = "";
                selectedQuestSlot = null;
            }
        }

        if (destroyedKeys.Count > 0 && debugMode)
        {
            Debug.Log($"QuestListUI: Cleaned up {destroyedKeys.Count} destroyed slot references");
        }
    }

    // NEW: Method to select a quest and show its details
    void SelectQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId)) return;

        // Deselect previous quest - ADD NULL CHECKS
        if (selectedQuestSlot != null && questSlots.ContainsKey(selectedQuestId))
        {
            var prevSlot = questSlots[selectedQuestId];
            // Check if the GameObject still exists
            if (prevSlot != null)
            {
                var prevText = prevSlot.GetComponentInChildren<TextMeshProUGUI>();
                if (prevText != null)
                {
                    var prevQuest = QuestManager.Instance.GetQuest(selectedQuestId);
                    if (prevQuest != null)
                    {
                        prevText.color = GetQuestStateColor(prevQuest.state);
                    }
                }
            }
            else
            {
                // Clean up destroyed reference
                questSlots.Remove(selectedQuestId);
            }
        }

        // Select new quest - ADD NULL CHECKS
        selectedQuestId = questId;
        if (questSlots.ContainsKey(questId))
        {
            selectedQuestSlot = questSlots[questId];
            // Check if the GameObject still exists
            if (selectedQuestSlot != null)
            {
                var selectedText = selectedQuestSlot.GetComponentInChildren<TextMeshProUGUI>();
                if (selectedText != null)
                {
                    selectedText.color = selectedQuestColor;
                }
            }
            else
            {
                // Clean up destroyed reference
                questSlots.Remove(questId);
                selectedQuestSlot = null;
            }
        }

        // Update details panel
        UpdateQuestDetails(questId);

        if (debugMode) Debug.Log($"Selected quest: {questId}");
    }

    // NEW: Method to update the details panel
    void UpdateQuestDetails(string questId)
    {
        if (QuestManager.Instance == null) return;

        var quest = QuestManager.Instance.GetQuest(questId);
        if (quest == null)
        {
            if (detailsPanel != null)
                detailsPanel.SetActive(false);
            return;
        }

        if (detailsPanel != null)
            detailsPanel.SetActive(true);

        // Set basic quest info
        if (detailQuestNameText != null)
            detailQuestNameText.text = quest.questName;

        if (detailDescriptionText != null)
            detailDescriptionText.text = quest.description;

        if (detailGiverNPCText != null)
            detailGiverNPCText.text = $"From: {quest.giverNPC}";

        if (detailLocationHintText != null)
            detailLocationHintText.text = $"Location: {quest.locationHint}";

        // Set objectives with progress (merged)
        if (detailObjectivesText != null)
        {
            string objectivesAndProgress = "";

            if (quest.state == QuestState.ACTIVE || quest.state == QuestState.COMPLETED)
            {
                // Get the progress text which shows current/required amounts
                objectivesAndProgress = GetCombinedObjectivesAndProgress(quest);
            }
            else
            {
                // For locked quests, just show what's required
                objectivesAndProgress = GetObjectivesList(quest);
            }

            detailObjectivesText.text = objectivesAndProgress;
        }

        // Set rewards
        if (detailRewardsText != null)
        {
            string rewards = "";
            if (quest.rewardCoins > 0)
            {
                rewards += $"{quest.rewardCoins} coins\n";
            }
            foreach (var item in quest.itemRewards)
            {
                rewards += $"{item.quantity}x {item.itemName}\n";
            }
            if (string.IsNullOrEmpty(rewards))
            {
                rewards = "No tangible rewards";
            }
            detailRewardsText.text = rewards;
        }

            // NEW: Add debug info for coins
        if (quest.questId == "SEASON_REVIEW_8" && debugMode)
        {
            var saveData = QuestManager.Instance.GetSaveData();
            Debug.Log($"SEASON_REVIEW_8 Debug: Player Coins: {saveData?.playerCoins ?? 0}");
            
            foreach (var trigger in quest.completionTriggers)
            {
                if (trigger.triggerType == QuestTriggerType.HAVE_COINS)
                {
                    Debug.Log($"Coin Trigger: {trigger.currentAmount}/{trigger.requiredAmount}");
                }
            }
        }
    }

    string GetCombinedObjectivesAndProgress(QuestData quest)
    {
        string combinedText = "";

        foreach (var trigger in quest.completionTriggers)
        {
            string objectiveText = GetObjectiveText(trigger);
            string status = "";

            if (trigger.currentAmount >= trigger.requiredAmount)
            {
                status = "<color=green>[X]</color> "; // Using [X] instead of ✓ for font compatibility
            }
            else if (quest.state == QuestState.ACTIVE)
            {
                // Show progress for active quests
                status = $"[{trigger.currentAmount}/{trigger.requiredAmount}] ";
            }
            else
            {
                // For future objectives, just show what's needed
                status = $"[0/{trigger.requiredAmount}] ";
            }

            combinedText += $"{status}{objectiveText}\n";
        }

        // Add a summary if the quest is completed
        if (quest.state == QuestState.COMPLETED)
        {
            combinedText += "\n<color=green>Quest Completed!</color>";
        }

        return combinedText;
    }

    string GetObjectivesList(QuestData quest)
    {
        string objectives = "";

        foreach (var trigger in quest.completionTriggers)
        {
            string objectiveText = GetObjectiveText(trigger);
            objectives += $"[ ] {objectiveText}\n";
        }

        return objectives;
    }

    string GetObjectiveText(QuestTrigger trigger)
    {
        switch (trigger.triggerType)
        {
            case QuestTriggerType.TALK_TO_NPC:
                return $"Talk to {trigger.targetId}";
            case QuestTriggerType.GO_TO_LOCATION:
                return $"Go to {trigger.targetId}";
            case QuestTriggerType.HAVE_ITEM:
                if (trigger.targetId == "Fish")
                    return $"Catch {trigger.requiredAmount} fish";
                else if (trigger.targetId == "Sold Fish")
                    return $"Sell {trigger.requiredAmount} fish";
                else if (trigger.targetId == "Processed Fish")
                    return $"Process {trigger.requiredAmount} fish";
                else if (trigger.targetId == "OrientationComplete")
                    return $"Complete orientation";
                else if (trigger.targetId == "ProcessingLessonComplete")
                    return $"Complete processing tutorial";
                else if (trigger.targetId == "HelpRhysbryn")
                    return $"Help Rhysbryn sustainably";
                else
                    return $"Collect {trigger.requiredAmount} {trigger.targetId}";
            case QuestTriggerType.HAVE_COINS:
                return $"Collect {trigger.requiredAmount} coins";
            case QuestTriggerType.HAVE_SKILL_LEVEL:
                return $"Reach {trigger.targetId} level {trigger.requiredAmount}";
            default:
                return $"Complete objective";
        }
    }

    void OnQuestStarted(QuestData quest)
    {
        // Check if this MonoBehaviour is still active
        if (this == null || !isActiveAndEnabled) return;

        if (debugMode) Debug.Log($"QuestListUI: Quest started - {quest?.questName}");
        RefreshQuestList();
    }

    void OnQuestProgressUpdated(QuestData quest)
    {
        // Check if this MonoBehaviour is still active
        if (this == null || !isActiveAndEnabled) return;

        if (quest != null)
        {
            if (questSlots.ContainsKey(quest.questId))
            {
                UpdateQuestSlot(quest);
            }

            // Update details if this is the selected quest
            if (quest.questId == selectedQuestId)
            {
                UpdateQuestDetails(selectedQuestId);
            }
        }
    }

    void OnQuestCompleted(QuestData quest)
    {
        // Check if this MonoBehaviour is still active
        if (this == null || !isActiveAndEnabled) return;

        if (debugMode) Debug.Log($"QuestListUI: Quest completed - {quest?.questName}");
        RefreshQuestList();
    }

    void OnQuestTurnedIn(QuestData quest)
    {
        // Check if this MonoBehaviour is still active
        if (this == null || !isActiveAndEnabled) return;

        if (debugMode) Debug.Log($"QuestListUI: Quest turned in - {quest?.questName}");
        RefreshQuestList();
    }

    void UpdateQuestSlot(QuestData quest)
    {
        if (quest == null) return;

        if (questSlots.TryGetValue(quest.questId, out GameObject slot))
        {
            // Check if slot still exists
            if (slot == null)
            {
                questSlots.Remove(quest.questId);
                return;
            }

            TextMeshProUGUI questNameText = null;
            Transform nameTransform = slot.transform.Find("Quest Name");

            if (nameTransform != null)
                questNameText = nameTransform.GetComponent<TextMeshProUGUI>();

            if (questNameText == null)
            {
                TextMeshProUGUI[] allTexts = slot.GetComponentsInChildren<TextMeshProUGUI>(true);
                if (allTexts.Length >= 1) questNameText = allTexts[0];
            }

            if (questNameText != null)
            {
                string stateIndicator = "";
                Color stateColor = GetQuestStateColor(quest.state);

                switch (quest.state)
                {
                    case QuestState.ACTIVE:
                        stateIndicator = "> ";
                        break;
                    case QuestState.COMPLETED:
                        stateIndicator = "[X] ";
                        break;
                    case QuestState.LOCKED:
                        stateIndicator = "[ ] ";
                        break;
                }

                questNameText.text = stateIndicator + quest.questName;

                // Apply selection color if this is the selected quest
                if (quest.questId == selectedQuestId)
                {
                    questNameText.color = selectedQuestColor;
                }
                else
                {
                    questNameText.color = stateColor;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (initializationCoroutine != null)
        {
            StopCoroutine(initializationCoroutine);
        }

        if (hasSubscribedToEvents && QuestManager.Instance != null)
        {
            QuestManager.OnQuestStarted -= OnQuestStarted;
            QuestManager.OnQuestProgressUpdated -= OnQuestProgressUpdated;
            QuestManager.OnQuestCompleted -= OnQuestCompleted;
            QuestManager.OnQuestTurnedIn -= OnQuestTurnedIn;
            QuestManager.OnNewGameReset -= OnNewGameReset;

            hasSubscribedToEvents = false;
        }
    }

    public void ManualRefresh()
    {
        if (debugMode) Debug.Log("QuestListUI: Manual refresh requested");
        isInitialized = false;
        StartCoroutine(DelayedInitialization());
    }

    public void ForceInitialize()
    {
        if (!isInitialized)
        {
            StartCoroutine(DelayedInitialization());
        }
        else
        {
            RefreshQuestList();
        }
    }

    public bool IsUIReady()
    {
        return isInitialized && questPanel != null && contentParent != null;
    }
}