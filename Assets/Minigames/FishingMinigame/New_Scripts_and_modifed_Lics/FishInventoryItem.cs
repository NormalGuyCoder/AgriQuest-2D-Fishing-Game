using System;

[Serializable]
public class FishInventoryItem
{
    public string fishId;
    public string fishName;
    public int count;
    public float totalWeight;
    public int biggestCatch;
    public DateTime firstCaught;
    public DateTime lastCaught;
    public string catchLocation;

    public FishInventoryItem() { }

    public FishInventoryItem(string fishId, string fishName, string location = "Unknown")
    {
        this.fishId = fishId;
        this.fishName = fishName;
        this.count = 0;
        this.totalWeight = 0f;
        this.biggestCatch = 0;
        this.firstCaught = DateTime.UtcNow;
        this.lastCaught = DateTime.UtcNow;
        this.catchLocation = location;
    }

    public void AddCatch(int sizeCm, float weightKg)
    {
        count++;
        totalWeight += weightKg;

        if (sizeCm > biggestCatch)
        {
            biggestCatch = sizeCm;
        }

        lastCaught = DateTime.UtcNow;
    }

    public float GetAverageWeight()
    {
        return count > 0 ? totalWeight / count : 0f;
    }
}