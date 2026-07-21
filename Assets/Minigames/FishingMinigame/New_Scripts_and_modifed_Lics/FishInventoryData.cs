using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FishInventoryData
{
    public Dictionary<string, FishInventoryItem> fishInventory = new Dictionary<string, FishInventoryItem>();
    public int totalFishCaught = 0;
    public float totalWeightCaught = 0f;
    public DateTime createdDate;
    public DateTime lastUpdated;

    public FishInventoryData()
    {
        createdDate = DateTime.UtcNow;
        lastUpdated = DateTime.UtcNow;
    }

    public void AddFish(FishData fishData, string location = "Saltwater")
    {
        if (fishData == null) return;

        string fishId = fishData.GetSharedFishId();

        if (!fishInventory.ContainsKey(fishId))
        {
            fishInventory[fishId] = new FishInventoryItem(fishId, fishData.fishName, location);
        }

        float size = UnityEngine.Random.Range(fishData.minSize, fishData.maxSize);
        float weight = Mathf.Pow(size / 100f, 3) * 1.5f;

        fishInventory[fishId].AddCatch(Mathf.RoundToInt(size), weight);

        totalFishCaught++;
        totalWeightCaught += weight;
        lastUpdated = DateTime.UtcNow;
    }

    public int GetFishCount(string fishId)
    {
        return fishInventory.ContainsKey(fishId) ? fishInventory[fishId].count : 0;
    }

    public List<FishInventoryItem> GetAllFish()
    {
        return new List<FishInventoryItem>(fishInventory.Values);
    }

    public void ClearInventory()
    {
        fishInventory.Clear();
        totalFishCaught = 0;
        totalWeightCaught = 0f;
        lastUpdated = DateTime.UtcNow;
    }
}