using UnityEngine;

[CreateAssetMenu(fileName = "New Fish", menuName = "Fishing/Fish Data")]
public class FishData : ScriptableObject
{
    [Header("Basic Info")]
    public string fishName;
    public string scientificName;
    [TextArea(3, 5)]
    public string description;
    public Sprite fishSprite;
    public Sprite fishIcon;

    [Header("Game Stats")]
    public float rarity; // 0-1, higher means rarer
    public float baseValue; // Base selling price
    public float minSize; // Minimum size in cm
    public float maxSize; // Maximum size in cm

    [Header("Behavior")]
    public float swimSpeed;
    public float catchDifficulty; // 0-1, higher means harder to catch
    public string[] preferredBaits; // Types of bait this fish likes

    [Header("Habitat")]
    public string[] habitats; // Where this fish can be found
    public float preferredDepth; // Preferred depth in the water
    public string[] seasons; // When this fish is available

    public string[] possibleLocations; // Where this fish can be found
    public bool isDiscovered; // Whether the player has discovered this fish

    [Header("Conservation Status")]
    [Tooltip("Force this fish to be treated as endangered even if the shared catalog entry is not flagged.")]
    public bool overrideEndangeredTag = false;

    [Tooltip("If true, popping a warning dialog is required before the player is allowed to keep this fish.")]
    public bool prohibitedCatch = false;

    [Tooltip("Optional high-value override used when an endangered catch should reward extra coins.")]
    public float endangeredValueOverride = 0f;

    [TextArea(2, 4)]
    [Tooltip("Custom body text shown in the endangered warning dialog.")]
    public string conservationWarningCopy;

    [Header("Shared Catalog")]
    public FishCatalogEntry sharedCatalogEntry;
    [SerializeField] private string sharedFishId;

    public string GetSharedFishId()
    {
        if (!string.IsNullOrEmpty(sharedFishId))
            return sharedFishId;

        if (sharedCatalogEntry != null)
        {
            sharedFishId = sharedCatalogEntry.GetFishId();
            return sharedFishId;
        }

        if (!string.IsNullOrWhiteSpace(fishName))
        {
            sharedFishId = fishName.Trim().ToLower().Replace(" ", "_");
        }

        return sharedFishId;
    }

    public bool IsEndangeredSpecies()
    {
        bool catalogFlagged = sharedCatalogEntry != null && sharedCatalogEntry.isEndangered;
        return catalogFlagged || overrideEndangeredTag;
    }

    public bool RequiresCatchWarning()
    {
        return prohibitedCatch || IsEndangeredSpecies();
    }

    public float GetSuggestedSaleValue(float endangeredMultiplier)
    {
        float value = endangeredValueOverride > 0f ? endangeredValueOverride : baseValue;
        if (value <= 0f)
        {
            value = sharedCatalogEntry != null
                ? sharedCatalogEntry.GetSellPrice()
                : 25f;
        }

        if (IsEndangeredSpecies() && endangeredMultiplier > 1f)
        {
            value *= endangeredMultiplier;
        }

        return Mathf.Max(1f, value);
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
        else if (!string.IsNullOrWhiteSpace(fishName))
        {
            sharedFishId = fishName.Trim().ToLower().Replace(" ", "_");
        }
    }
} 