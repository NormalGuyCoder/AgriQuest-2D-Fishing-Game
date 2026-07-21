using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Fish", menuName = "Fish Deboner/Fish Definition")]
public class FishDefinition : ScriptableObject
{
    [Header("Fish Information")]
    public string fishType;
    public string displayName;
    public DifficultyLevel difficulty;
    [TextArea(3, 5)]
    public string educationalInfo;
    [TextArea(2, 4)]
    public string description; // Brief description for library cards
    
    [Header("Challenge Settings")]
    public int timeLimitSeconds = 90; // Time limit for this fish
    public int spineBoneCount;
    public int ribBoneCount;
    public int pinBoneCount;
    
    [Header("Habitat & Cooking")]
    [TextArea(2, 3)]
    public string habitat; // Where the fish lives
    [TextArea(3, 5)]
    public string cookingTips; // Cooking and preparation tips
    
    [Header("Visual")]
    public Sprite fishSprite;
    public Sprite fishIcon; // Small icon for library cards
    [Tooltip("Optional: Prefab that contains positioned BoneMarker components for manual placement")] 
    public GameObject fishPrefab;
    
    [Header("Shared Catalog")]
    public FishCatalogEntry sharedCatalogEntry;
    [SerializeField] private string sharedFishId;
    
    [Header("Bone Data")]
    public List<BoneData> bones = new List<BoneData>();

    public enum DifficultyLevel
    {
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3
    }

    public string GetDifficultyText()
    {
        switch (difficulty)
        {
            case DifficultyLevel.Beginner: return "Beginner";
            case DifficultyLevel.Intermediate: return "Intermediate";
            case DifficultyLevel.Advanced: return "Advanced";
            default: return "Unknown";
        }
    }

    public int GetTotalBoneCount()
    {
        // Return the sum of declared bone counts, or fallback to bones list count
        int declaredCount = spineBoneCount + ribBoneCount + pinBoneCount;
        if (declaredCount > 0)
            return declaredCount;
        return bones.Count;
    }

    public int GetRemovedBoneCount()
    {
        int count = 0;
        foreach (var bone in bones)
        {
            if (bone.isRemoved) count++;
        }
        return count;
    }

    public bool AreAllBonesRemoved()
    {
        return GetRemovedBoneCount() == GetTotalBoneCount();
    }

    public void ResetBones()
    {
        foreach (var bone in bones)
        {
            bone.isRemoved = false;
        }
    }

    public string GetSharedFishId()
    {
        if (!string.IsNullOrEmpty(sharedFishId))
            return sharedFishId;

        if (sharedCatalogEntry != null)
        {
            sharedFishId = sharedCatalogEntry.GetFishId();
            return sharedFishId;
        }

        if (!string.IsNullOrWhiteSpace(fishType))
        {
            sharedFishId = fishType.Trim().ToLower().Replace(" ", "_");
        }

        return sharedFishId;
    }

    private void OnValidate()
    {
        if (sharedCatalogEntry != null)
        {
            sharedFishId = sharedCatalogEntry.GetFishId();
        }
        else if (!string.IsNullOrWhiteSpace(sharedFishId))
        {
            sharedFishId = sharedFishId.Trim().ToLower().Replace(" ", "_");
        }
        else if (!string.IsNullOrWhiteSpace(fishType))
        {
            sharedFishId = fishType.Trim().ToLower().Replace(" ", "_");
        }
    }
}



