using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Fish Levels")]
    public List<FishDefinition> fishLevels = new List<FishDefinition>();
    
    [Header("References")]
    public FishBoardController boardController;
    public HUDController hudController;
    public ToolController toolController;

    [Header("Game Settings")]
    public int startTimeSeconds = 300; // 5 minutes default
    
    private int currentLevelIndex = 0;
    private int score = 0;
    private int cleanliness = 100;
    private int mistakes = 0;
    private float timeRemaining;
    private bool isGameActive = false;
    private bool isPaused = false;
    private FishDefinition currentFish;
    private bool isFishOpened = false; // Track if fish has been opened with knife

    // Scoring constants
    private const int SPINE_BONE_POINTS = 20;
    private const int RIB_BONE_POINTS = 15;
    private const int PIN_BONE_POINTS = 10;
    private const int TIME_BONUS_MULTIPLIER = 2;
    private const int WRONG_TOOL_PENALTY = 10;
    private const int MISTAKE_POINTS = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize game state
        ResetGameStats();
        
        // Check for missing references
        if (boardController == null)
            Debug.LogError("GameManager: Board Controller is not assigned!");
        if (hudController == null)
            Debug.LogError("GameManager: HUD Controller is not assigned!");
        if (toolController == null)
            Debug.LogError("GameManager: Tool Controller is not assigned!");
        if (fishLevels == null || fishLevels.Count == 0)
            Debug.LogWarning("GameManager: No fish levels assigned! Add FishDefinition assets to Fish Levels list.");

        if (hudController != null)
        {
            hudController.UpdateUI(score, cleanliness, 0, mistakes);
        }
    }

    void Update()
    {
        if (isGameActive && !isPaused)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                HandleTimeUp();
            }
            hudController.UpdateUI(score, cleanliness, Mathf.CeilToInt(timeRemaining), mistakes);
        }
    }

    public void StartGame()
    {
        Debug.Log("StartGame() called!");

        // Check for missing references
        if (boardController == null)
        {
            Debug.LogError("Cannot start game: Board Controller is null!");
            return;
        }
        if (hudController == null)
        {
            Debug.LogError("Cannot start game: HUD Controller is null!");
            return;
        }
        if (toolController == null)
        {
            Debug.LogError("Cannot start game: Tool Controller is null!");
            return;
        }
        if (fishLevels == null || fishLevels.Count == 0)
        {
            Debug.LogError("Cannot start game: No fish levels assigned! Add FishDefinition ScriptableObjects to the Fish Levels list in GameManager.");
            return;
        }

        // Reset game stats
        ResetGameStats();

        currentLevelIndex = 0;
        LoadLevel(currentLevelIndex);
    }

    public void StartGameWithFish(FishDefinition fish)
    {
        if (fish == null)
        {
            Debug.LogError("Cannot start game: Fish is null!");
            return;
        }

        // Check for missing references
        if (boardController == null)
        {
            Debug.LogError("Cannot start game: Board Controller is null!");
            return;
        }
        if (hudController == null)
        {
            Debug.LogError("Cannot start game: HUD Controller is null!");
            return;
        }
        if (toolController == null)
        {
            Debug.LogError("Cannot start game: Tool Controller is null!");
            return;
        }

        // Reset game stats
        ResetGameStats();

        // Set time limit from fish definition
        startTimeSeconds = fish.timeLimitSeconds;

        // Load this specific fish
        currentFish = fish;
        currentFish.ResetBones();
        isFishOpened = false; // Reset fish opened state
        timeRemaining = startTimeSeconds;
        isGameActive = true;
        isPaused = false;

        Debug.Log($"Starting game with: {currentFish.displayName}");

        boardController.SetupFish(currentFish);
        
        if (hudController != null)
        {
            hudController.UpdateUI(score, cleanliness, Mathf.CeilToInt(timeRemaining), mistakes);
            hudController.ShowFishOpeningPrompt(true); // Show prompt to open fish
        }
        
        if (toolController != null)
        {
            toolController.SetTool(ToolType.Knife);
        }
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= fishLevels.Count)
        {
            Debug.Log("All levels completed!");
            ShowGameComplete();
            return;
        }

        currentFish = fishLevels[levelIndex];
        if (currentFish == null)
        {
            Debug.LogError($"Fish level at index {levelIndex} is null!");
            return;
        }

        if (boardController == null)
        {
            Debug.LogError("Cannot load level: Board Controller is null!");
            return;
        }

        currentFish.ResetBones();
        isFishOpened = false; // Reset fish opened state
        timeRemaining = startTimeSeconds;
        isGameActive = true;
        isPaused = false;

        Debug.Log($"Loading level {levelIndex + 1}: {currentFish.displayName}");

        boardController.SetupFish(currentFish);
        
        if (hudController != null)
        {
            hudController.UpdateUI(score, cleanliness, Mathf.CeilToInt(timeRemaining), mistakes);
            hudController.ShowFishOpeningPrompt(true); // Show prompt to open fish
        }
        
        if (toolController != null)
        {
            toolController.SetTool(ToolType.Knife); // Default to knife
        }
    }

    public void SelectTool(string toolName)
    {
        if (toolName == "Knife")
        {
            toolController.SetTool(ToolType.Knife);
        }
        else if (toolName == "Tweezers")
        {
            toolController.SetTool(ToolType.Tweezers);
        }
    }

    public void OnBoneClicked(BoneData bone, BoneButton boneButton)
    {
        if (!isGameActive || isPaused || bone.isRemoved)
            return;

        // Check if fish needs to be opened first (for Spine and Rib bones)
        if (!isFishOpened && (bone.boneType == BoneType.Spine || bone.boneType == BoneType.Rib))
        {
            // Player must open fish first with knife
            ToolType currentTool = toolController.GetCurrentTool();
            if (currentTool == ToolType.Knife)
            {
                // Open the fish with knife
                OpenFishWithKnife();
                // After opening, remove the bone (spine bone removal opens the fish)
                RemoveBone(bone, boneButton);
            }
            else
            {
                // Show message that fish needs to be opened first
                if (hudController != null)
                {
                    hudController.ShowMessage("Open the fish with the knife first!", 2f);
                }
            }
            return;
        }

        ToolType currentTool2 = toolController.GetCurrentTool();
        bool isCorrectTool = IsCorrectToolForBone(bone.boneType, currentTool2);

        if (isCorrectTool)
        {
            RemoveBone(bone, boneButton);
        }
        else
        {
            HandleWrongTool(bone.boneType, currentTool2);
        }
    }

    public void OnFishBoardClicked()
    {
        if (!isGameActive || isPaused || isFishOpened)
            return;

        ToolType currentTool = toolController.GetCurrentTool();
        if (currentTool == ToolType.Knife)
        {
            OpenFishWithKnife();
        }
        else
        {
            if (hudController != null)
            {
                hudController.ShowMessage("Use the knife to open the fish first!", 2f);
            }
        }
    }

    private void OpenFishWithKnife()
    {
        if (isFishOpened)
            return;

        isFishOpened = true;
        Debug.Log("Fish opened with knife!");

        // Trigger opening animation
        if (boardController != null)
        {
            boardController.PlayFishOpeningAnimation();
        }

        // Hide opening prompt
        if (hudController != null)
        {
            hudController.ShowFishOpeningPrompt(false);
            hudController.ShowMessage("Fish opened! Now you can remove bones.", 2f);
        }

        // Play opening sound (if you have one)
        // AudioManager.Instance.PlaySound("FishOpened");
    }

    private bool IsCorrectToolForBone(BoneType boneType, ToolType tool)
    {
        // Pin bones require only tweezers (and fish must be opened)
        if (boneType == BoneType.Pin)
        {
            return tool == ToolType.Tweezers && isFishOpened;
        }
        else if (boneType == BoneType.Spine)
        {
            // Spine bones always require knife (even after opening)
            // First spine bone opens the fish, remaining spine bones need knife
            if (!isFishOpened)
            {
                return tool == ToolType.Knife; // First spine opens fish
            }
            else
            {
                return tool == ToolType.Knife; // Remaining spine bones use knife
            }
        }
        else // Rib bones
        {
            // Rib bones require knife first, then tweezers after opening
            if (!isFishOpened)
            {
                return tool == ToolType.Knife; // Can't remove ribs until fish is opened
            }
            else
            {
                // After opening, ribs can be removed with tweezers
                return tool == ToolType.Tweezers;
            }
        }
    }

    public bool IsFishOpened()
    {
        return isFishOpened;
    }

    private void RemoveBone(BoneData bone, BoneButton boneButton)
    {
        bone.isRemoved = true;
        boneButton.SetRemoved(true);

        // Award points based on bone type
        int points = 0;
        switch (bone.boneType)
        {
            case BoneType.Spine:
                points = SPINE_BONE_POINTS;
                break;
            case BoneType.Rib:
                points = RIB_BONE_POINTS;
                break;
            case BoneType.Pin:
                points = PIN_BONE_POINTS;
                break;
        }

        score += points;
        hudController.ShowPointsPopup(boneButton.transform.position, points);

        // Play success sound (if you have one)
        // AudioManager.Instance.PlaySound("BoneRemoved");

        // Check if all bones are removed
        if (currentFish.AreAllBonesRemoved())
        {
            CompleteLevel();
        }
        else
        {
            hudController.UpdateUI(score, cleanliness, Mathf.CeilToInt(timeRemaining), mistakes);
        }
    }

    private void HandleWrongTool(BoneType boneType, ToolType usedTool)
    {
        cleanliness = Mathf.Max(0, cleanliness - WRONG_TOOL_PENALTY);
        mistakes += MISTAKE_POINTS;
        hudController.UpdateUI(score, cleanliness, Mathf.CeilToInt(timeRemaining), mistakes);
        
        // Show specific message about which tool to use
        string message = GetWrongToolMessage(boneType, usedTool);
        if (hudController != null && !string.IsNullOrEmpty(message))
        {
            hudController.ShowMessage(message, 2.5f);
        }
        
        // Play error sound (if you have one)
        // AudioManager.Instance.PlaySound("WrongTool");
    }

    /// <summary>
    /// Get the appropriate error message when wrong tool is used
    /// </summary>
    private string GetWrongToolMessage(BoneType boneType, ToolType usedTool)
    {
        // Determine which tool should be used for this bone type
        ToolType correctTool = GetRequiredToolForBone(boneType);
        
        // If fish isn't opened yet, some bones might need knife first
        if (!isFishOpened && (boneType == BoneType.Spine || boneType == BoneType.Rib))
        {
            return "Open the fish with the knife first!";
        }
        
        // Generate message based on what tool was used vs what's needed
        if (correctTool == ToolType.Knife)
        {
            return "You can only use a knife here!";
        }
        else if (correctTool == ToolType.Tweezers)
        {
            return "You can only use tweezers here!";
        }
        
        return "Wrong tool!";
    }

    /// <summary>
    /// Get the required tool for a bone type (assuming fish is opened if needed)
    /// </summary>
    private ToolType GetRequiredToolForBone(BoneType boneType)
    {
        switch (boneType)
        {
            case BoneType.Spine:
                return ToolType.Knife; // Spine always needs knife
            case BoneType.Rib:
                // Ribs need tweezers after opening, knife before opening
                return isFishOpened ? ToolType.Tweezers : ToolType.Knife;
            case BoneType.Pin:
                return ToolType.Tweezers; // Pins always need tweezers (after opening)
            default:
                return ToolType.Knife;
        }
    }

    private void CompleteLevel()
    {
        isGameActive = false;

        // Calculate time bonus
        int timeBonus = Mathf.CeilToInt(timeRemaining) * TIME_BONUS_MULTIPLIER;
        score += timeBonus;

        // Calculate time taken
        float timeTaken = startTimeSeconds - timeRemaining;

        // Count bones removed
        int bonesRemoved = currentFish.GetRemovedBoneCount();

        // Save score to ScoreManager with detailed statistics
        if (ScoreManager.Instance != null && currentFish != null)
        {
            ScoreManager.Instance.AddGameScore(
                currentFish.displayName, 
                score, 
                timeBonus, 
                mistakes, 
                cleanliness, 
                timeTaken, 
                bonesRemoved
            );
        }

        hudController.ShowLevelComplete(currentFish, timeBonus);

        if (FishInventoryManager.Instance != null && currentFish != null)
        {
            FishInventoryManager.Instance.RecordCatch(
                currentFish.sharedCatalogEntry,
                1,
                FishCatchSource.Deboning,
                currentFish.GetSharedFishId());
        }

        if (EconomyManager.Instance != null && currentFish != null)
        {
            float fallbackValue = Mathf.Max(100, currentFish.GetTotalBoneCount() * 5);
            EconomyManager.Instance.RecordFishSale(
                currentFish.sharedCatalogEntry,
                currentFish.displayName,
                FishCatchSource.Deboning,
                fallbackValue,
                soldAfterDeboning: true);
        }

        // Record deboning completion for sustainability metrics
        if (SustainableFishingMetrics.Instance != null && currentFish != null)
        {
            SustainableFishingMetrics.Instance.RecordDeboningCompletion(
                currentFish.GetSharedFishId(),
                currentFish.displayName,
                bonesRemoved,
                mistakes,
                timeTaken
            );
        }
        
        // Play completion sound
        // AudioManager.Instance.PlaySound("LevelComplete");
    }

    private void HandleTimeUp()
    {
        isGameActive = false;
        hudController.ShowTimeUp();
    }

    public void NextLevel()
    {
        currentLevelIndex++;
        LoadLevel(currentLevelIndex);
    }

    public void RestartLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public void PauseGame()
    {
        isPaused = true;
        hudController.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        isPaused = false;
        hudController.HidePauseMenu();
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShowGameComplete()
    {
        hudController.ShowGameComplete();
        isGameActive = false;
    }

    private void ResetGameStats()
    {
        score = 0;
        cleanliness = 100;
        mistakes = 0;
        timeRemaining = startTimeSeconds;
        isGameActive = false;
        isPaused = false;
        isFishOpened = false;
    }

    public bool IsGameActive()
    {
        return isGameActive && !isPaused;
    }
}



