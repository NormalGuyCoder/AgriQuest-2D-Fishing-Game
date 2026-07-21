// FishCatchBridge.cs (CORRECTED)
using UnityEngine;
using System.Collections.Generic;

public class FishCatchBridge : MonoBehaviour
{
    public static FishCatchBridge Instance { get; private set; }
    
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    
    [SerializeField] private int totalFishCaught = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"FishCatchBridge initialized.");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Call this from your fishing minigame when a fish is caught
    public void OnFishCaught(FishData fishData)
    {
        if (fishData == null)
        {
            Debug.LogWarning("FishCatchBridge: FishData is null!");
            return;
        }
        
        totalFishCaught++;
        
        if (enableDebugLogs)
        {
            Debug.Log($"=== FISH CAUGHT ({totalFishCaught} total) ===");
            
            // ADJUST BASED ON YOUR FishData FIELDS:
            // Try common field names
            if (fishData.name != null)
                Debug.Log($"Fish: {fishData.name}");
            else if (fishData.fishName != null)
                Debug.Log($"Fish: {fishData.fishName}");
            else
                Debug.Log($"Fish: (unknown)");
        }
        
        // Update DetailedFishInventory
        if (DetailedFishInventory.Instance != null)
        {
            DetailedFishInventory.Instance.AddCatchRecord(fishData);
            Debug.Log($"Added to inventory.");
        }
        else
        {
            Debug.LogWarning("DetailedFishInventory.Instance is null!");
        }
        
        // Update QuestManager
        if (QuestManager.Instance != null)
        {
            // Try different methods based on your QuestManager
            QuestManager.Instance.RecordFishCaught(1); // If this exists
            
            Debug.Log("Updated quest fish count");
            
            // Check if FIRST_CAST_2A quest exists and check completion
            var quest = QuestManager.Instance.GetQuest("FIRST_CAST_2A");
            if (quest != null)
            {
                Debug.Log($"FIRST_CAST_2A state: {quest.state}");
                
                // Try to check completion - adjust based on your QuestManager
                QuestManager.Instance.CheckQuestCompletion(quest); // If this exists
                // OR
                // quest.CheckCompletion(); // If QuestData has this method
            }
        }
        else
        {
            Debug.LogWarning("QuestManager.Instance is null!");
        }
    }
    
    // Test method - simulate catching fish
    public void TestCatchFish(string fishName = "Test Fish")
    {
        // Create a test fish
        var fishData = ScriptableObject.CreateInstance<FishData>();
        
        // ADJUST BASED ON YOUR FishData FIELDS:
        // Set the most common field names
        fishData.name = fishName; // or fishData.fishName
        
        Debug.Log($"🧪 TEST: Simulating catch of {fishName}");
        OnFishCaught(fishData);
    }
    
    // Quick check if Vincent's quest should be complete
    public bool CheckVincentQuestCompletion()
    {
        if (totalFishCaught >= 1)
        {
            Debug.Log($"✅ Vincent's quest condition met! (Caught {totalFishCaught} fish)");
            return true;
        }
        else
        {
            Debug.Log($"❌ Vincent's quest not complete. Need 1 fish, have {totalFishCaught}");
            return false;
        }
    }
}