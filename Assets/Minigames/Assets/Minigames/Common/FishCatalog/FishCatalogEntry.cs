using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Canonical fish definition shared across all minigames.
/// </summary>
[CreateAssetMenu(fileName = "FishCatalogEntry", menuName = "Fish Catalog/Entry")]
public class FishCatalogEntry : ScriptableObject
{
    [Tooltip("Unique, lowercase identifier (e.g. sardine).")]
    public string fishId;

    [Header("Display")]
    public string displayName;
    public string scientificName;
    [TextArea(2, 4)]
    public string description;
    public Sprite mainSprite;
    public Sprite iconSprite;

    [Header("Metadata")]
    public string[] tags;
    
    [Tooltip("Is this fish an endangered species?")]
    public bool isEndangered = false;

    [Tooltip("Alternate names that should resolve to this fish.")]
    public List<string> aliases = new List<string>();

    [Header("Economy")]
    [Tooltip("Base sell price when this fish is sold fresh. Applies to every minigame unless overridden.")]
    [Min(0)]
    public int baseSellPrice = 40;

    [Tooltip("Multiplier that applies when the fish is processed/deboned before selling.")]
    [Range(1f, 3f)]
    public float processedValueMultiplier = 1.35f;

    [Tooltip("Multiplier added on top of base price when the player sells endangered fish (high profit, unethical).")]
    [Range(1f, 4f)]
    public float endangeredValueMultiplier = 2f;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(fishId) && !string.IsNullOrWhiteSpace(displayName))
        {
            fishId = Sanitize(displayName);
        }

        if (!string.IsNullOrWhiteSpace(fishId))
        {
            fishId = Sanitize(fishId);
        }
    }

    public string GetFishId()
    {
        if (string.IsNullOrWhiteSpace(fishId))
        {
            fishId = Sanitize(displayName);
        }
        return fishId;
    }

    /// <summary>
    /// Calculates a sell price using the configured multipliers.
    /// </summary>
    public float GetSellPrice(bool processed = false, bool treatAsEndangered = false, float fallbackValue = 0f)
    {
        float price = baseSellPrice > 0 ? baseSellPrice : fallbackValue;

        if (price <= 0f)
        {
            price = 25f;
        }

        if (processed)
        {
            price *= processedValueMultiplier;
        }

        if (treatAsEndangered || isEndangered)
        {
            price *= endangeredValueMultiplier;
        }

        return Mathf.Max(1f, price);
    }

    private string Sanitize(string value)
    {
        return value
            .Trim()
            .ToLower()
            .Replace(" ", "_");
    }
}

