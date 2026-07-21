using System;
using UnityEngine;

[System.Serializable]
public class DetailedFishCatchRecord
{
    public string recordId;
    public string fishId;
    public string fishName;

    public float sizeCm;
    public float weightKg;
    public DateTime catchTime;
    public float actualValue;

    // NEW: Track fish source
    public string catchSource; // "saltwater" or "freshwater"
    public string location; // Location where caught

    public DetailedFishCatchRecord() { }

    public DetailedFishCatchRecord(FishData fishData)
    {
        recordId = Guid.NewGuid().ToString();
        fishId = fishData.GetSharedFishId();
        fishName = fishData.fishName;

        sizeCm = UnityEngine.Random.Range(fishData.minSize, fishData.maxSize);
        weightKg = Mathf.Pow(sizeCm / 100f, 3) * 1.2f;
        catchTime = DateTime.UtcNow;
        actualValue = fishData.GetSuggestedSaleValue(1f);
        catchSource = "saltwater";
        location = "Saltyshore";
    }

    // NEW: Constructor for freshwater fish
    public DetailedFishCatchRecord(FreshwaterFish freshwaterFish)
    {
        recordId = Guid.NewGuid().ToString();
        fishId = freshwaterFish.name.ToLower().Replace(" ", "_");
        fishName = freshwaterFish.name;

        // Use reasonable defaults for freshwater fish sizes
        sizeCm = UnityEngine.Random.Range(15f, 40f);
        weightKg = Mathf.Pow(sizeCm / 100f, 3) * 1.2f;
        catchTime = DateTime.UtcNow;
        actualValue = freshwaterFish.baseCost;
        catchSource = "freshwater";
        location = "Freshwater Lake";
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }
}