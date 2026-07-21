using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DetailedFishInventory : MonoBehaviour
{
    public static DetailedFishInventory Instance { get; private set; }

    [Header("Save Settings")]
    [SerializeField] private string saveFileName = "detailed_fish_inventory.json";
    [SerializeField] private int maxRecords = 1000;
    [SerializeField] private bool autoSave = true;

    [Header("Coin Settings")]
    [SerializeField] private int startingCoins = 100;

    private List<DetailedFishCatchRecord> catchRecords = new List<DetailedFishCatchRecord>();
    private string savePath;

    private int coins = 0;
    private int totalFishCaught = 0;
    private int totalFishReleased = 0;
    private int totalFishSold = 0;
    private float totalWeightCaught = 0f;

    private int currentInventoryCount = 0;
    private float currentInventoryWeight = 0f;
    private Dictionary<string, int> speciesInventoryCounts = new Dictionary<string, int>();
    private Dictionary<string, float> speciesInventoryWeights = new Dictionary<string, float>();

    private Dictionary<string, int> fishCountBySpecies = new Dictionary<string, int>();
    private Dictionary<string, float> biggestCatchBySpecies = new Dictionary<string, float>();

    public event Action OnInventoryUpdated;
    public event Action<int> OnCoinsChanged;

    public int CurrentInventoryCount => currentInventoryCount;
    public float CurrentInventoryWeight => currentInventoryWeight;
    public Dictionary<string, int> SpeciesInventoryCounts => new Dictionary<string, int>(speciesInventoryCounts);
    public Dictionary<string, float> SpeciesInventoryWeights => new Dictionary<string, float>(speciesInventoryWeights);
    public int Coins => coins;
    public int TotalFishSold => totalFishSold;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);

        // Always load on awake
        LoadInventory();

        // If coins are 0, set to starting coins
        if (coins == 0)
        {
            coins = startingCoins;
            SaveInventory();
        }
    }

    // NEW: Complete memory reset
    public void ResetMemory()
    {
        Debug.Log("DetailedFishInventory: Resetting memory state...");

        catchRecords.Clear();
        coins = startingCoins;
        currentInventoryCount = 0;
        currentInventoryWeight = 0f;
        totalFishCaught = 0;
        totalFishReleased = 0;
        totalFishSold = 0;
        totalWeightCaught = 0f;
        speciesInventoryCounts.Clear();
        speciesInventoryWeights.Clear();
        fishCountBySpecies.Clear();
        biggestCatchBySpecies.Clear();

        // Trigger events
        OnInventoryUpdated?.Invoke();
        OnCoinsChanged?.Invoke(coins);

        Debug.Log("DetailedFishInventory: Memory reset complete");
    }

    // UPDATED: ClearInventory now calls ResetMemory
    public void ClearInventory()
    {
        ResetMemory();

        // Also save to disk
        SaveInventory();
    }

    public bool AddCoins(int amount)
    {
        if (amount <= 0) return false;

        coins += amount;
        OnCoinsChanged?.Invoke(coins);

        if (autoSave) SaveInventory();
        return true;
    }

    public bool RemoveCoins(int amount)
    {
        if (amount <= 0 || coins < amount) return false;

        coins -= amount;
        OnCoinsChanged?.Invoke(coins);

        if (autoSave) SaveInventory();
        return true;
    }

    public bool HasEnoughCoins(int amount)
    {
        return coins >= amount;
    }

    public int GetCoins()
    {
        return coins;
    }

    public void SetCoins(int amount)
    {
        if (amount < 0) return;

        coins = amount;
        OnCoinsChanged?.Invoke(coins);

        if (autoSave) SaveInventory();
    }

    public void AddCatchRecord(FishData fishData)
    {
        if (fishData == null)
        {
            Debug.LogWarning("Cannot add null fish data to detailed inventory");
            return;
        }

        var record = new DetailedFishCatchRecord(fishData);
        catchRecords.Add(record);

        currentInventoryCount++;
        currentInventoryWeight += record.weightKg;
        totalFishCaught++;
        totalWeightCaught += record.weightKg;

        if (!speciesInventoryCounts.ContainsKey(record.fishId))
        {
            speciesInventoryCounts[record.fishId] = 0;
            speciesInventoryWeights[record.fishId] = 0f;
        }
        speciesInventoryCounts[record.fishId]++;
        speciesInventoryWeights[record.fishId] += record.weightKg;

        if (!fishCountBySpecies.ContainsKey(record.fishId))
        {
            fishCountBySpecies[record.fishId] = 0;
        }
        fishCountBySpecies[record.fishId]++;

        if (!biggestCatchBySpecies.ContainsKey(record.fishId) ||
            record.sizeCm > biggestCatchBySpecies[record.fishId])
        {
            biggestCatchBySpecies[record.fishId] = record.sizeCm;
        }

        if (catchRecords.Count > maxRecords)
        {
            var oldestRecord = catchRecords[0];
            RemoveRecordFromStats(oldestRecord);
            catchRecords.RemoveAt(0);
        }

        if (autoSave) SaveInventory();
        OnInventoryUpdated?.Invoke();
    }

    // NEW: Method to add freshwater fish catch
    public void AddFreshwaterCatchRecord(FreshwaterFish freshwaterFish)
    {
        if (freshwaterFish == null)
        {
            Debug.LogWarning("Cannot add null freshwater fish to detailed inventory");
            return;
        }

        var record = new DetailedFishCatchRecord(freshwaterFish);
        catchRecords.Add(record);

        currentInventoryCount++;
        currentInventoryWeight += record.weightKg;
        totalFishCaught++;
        totalWeightCaught += record.weightKg;

        if (!speciesInventoryCounts.ContainsKey(record.fishId))
        {
            speciesInventoryCounts[record.fishId] = 0;
            speciesInventoryWeights[record.fishId] = 0f;
        }
        speciesInventoryCounts[record.fishId]++;
        speciesInventoryWeights[record.fishId] += record.weightKg;

        if (!fishCountBySpecies.ContainsKey(record.fishId))
        {
            fishCountBySpecies[record.fishId] = 0;
        }
        fishCountBySpecies[record.fishId]++;

        if (!biggestCatchBySpecies.ContainsKey(record.fishId) ||
            record.sizeCm > biggestCatchBySpecies[record.fishId])
        {
            biggestCatchBySpecies[record.fishId] = record.sizeCm;
        }

        if (catchRecords.Count > maxRecords)
        {
            var oldestRecord = catchRecords[0];
            RemoveRecordFromStats(oldestRecord);
            catchRecords.RemoveAt(0);
        }

        if (autoSave) SaveInventory();
        OnInventoryUpdated?.Invoke();

        Debug.Log($"Added freshwater fish to inventory: {freshwaterFish.name}");
    }

    public void AddReleaseRecord(FishData fishData)
    {
        if (fishData == null) return;

        totalFishCaught++;
        totalFishReleased++;

        if (autoSave) SaveInventory();
        OnInventoryUpdated?.Invoke();
    }

    // NEW: Add release record for freshwater fish
    public void AddFreshwaterReleaseRecord(FreshwaterFish freshwaterFish)
    {
        if (freshwaterFish == null) return;

        totalFishCaught++;
        totalFishReleased++;

        if (autoSave) SaveInventory();
        OnInventoryUpdated?.Invoke();
    }

    private void RemoveRecordFromStats(DetailedFishCatchRecord record)
    {
        currentInventoryCount--;
        currentInventoryWeight -= record.weightKg;
        totalWeightCaught -= record.weightKg;

        if (speciesInventoryCounts.ContainsKey(record.fishId))
        {
            speciesInventoryCounts[record.fishId]--;
            speciesInventoryWeights[record.fishId] -= record.weightKg;

            if (speciesInventoryCounts[record.fishId] <= 0)
            {
                speciesInventoryCounts.Remove(record.fishId);
                speciesInventoryWeights.Remove(record.fishId);
            }
        }

        if (fishCountBySpecies.ContainsKey(record.fishId))
        {
            fishCountBySpecies[record.fishId]--;
            if (fishCountBySpecies[record.fishId] <= 0)
            {
                fishCountBySpecies.Remove(record.fishId);
            }
        }
    }

    public bool RemoveFishFromInventory(string fishId, int amount, out float totalWeightRemoved)
    {
        totalWeightRemoved = 0f;

        if (amount <= 0 || currentInventoryCount < amount) return false;

        if (fishId == "any")
        {
            List<string> speciesList = new List<string>(speciesInventoryCounts.Keys);
            int remainingToRemove = amount;

            foreach (var species in speciesList)
            {
                if (remainingToRemove <= 0) break;

                int speciesCount = speciesInventoryCounts[species];
                int toRemoveFromThisSpecies = Mathf.Min(speciesCount, remainingToRemove);

                var toRemove = catchRecords
                    .Where(r => r.fishId == species)
                    .OrderByDescending(r => r.catchTime)
                    .Take(toRemoveFromThisSpecies)
                    .ToList();

                float speciesWeightRemoved = 0f;
                foreach (var record in toRemove)
                {
                    speciesWeightRemoved += record.weightKg;
                    catchRecords.Remove(record);
                }

                speciesInventoryCounts[species] -= toRemoveFromThisSpecies;
                speciesInventoryWeights[species] -= speciesWeightRemoved;

                if (speciesInventoryCounts[species] <= 0)
                {
                    speciesInventoryCounts.Remove(species);
                    speciesInventoryWeights.Remove(species);
                }

                totalWeightRemoved += speciesWeightRemoved;
                remainingToRemove -= toRemoveFromThisSpecies;
            }

            currentInventoryCount -= amount;
            currentInventoryWeight -= totalWeightRemoved;

            if (autoSave) SaveInventory();
            OnInventoryUpdated?.Invoke();
            return true;
        }
        else
        {
            if (!speciesInventoryCounts.ContainsKey(fishId) || speciesInventoryCounts[fishId] < amount)
                return false;

            var toRemove = catchRecords
                .Where(r => r.fishId == fishId)
                .OrderByDescending(r => r.catchTime)
                .Take(amount)
                .ToList();

            if (toRemove.Count != amount) return false;

            foreach (var record in toRemove)
            {
                totalWeightRemoved += record.weightKg;
                catchRecords.Remove(record);
            }

            speciesInventoryCounts[fishId] -= amount;
            speciesInventoryWeights[fishId] -= totalWeightRemoved;
            currentInventoryCount -= amount;
            currentInventoryWeight -= totalWeightRemoved;

            if (speciesInventoryCounts[fishId] <= 0)
            {
                speciesInventoryCounts.Remove(fishId);
                speciesInventoryWeights.Remove(fishId);
            }

            if (fishCountBySpecies.ContainsKey(fishId))
            {
                fishCountBySpecies[fishId] -= amount;
                if (fishCountBySpecies[fishId] <= 0)
                {
                    fishCountBySpecies.Remove(fishId);
                }
            }

            if (autoSave) SaveInventory();
            OnInventoryUpdated?.Invoke();
            return true;
        }
    }

    public bool SellFish(string fishId, int amount, out float totalWeightSold, out int totalCoinsEarned)
    {
        totalWeightSold = 0f;
        totalCoinsEarned = 0;

        if (amount <= 0 || currentInventoryCount < amount)
        {
            Debug.LogWarning($"Cannot sell {amount} fish. Inventory only has {currentInventoryCount}");
            return false;
        }

        if (!speciesInventoryCounts.ContainsKey(fishId) || speciesInventoryCounts[fishId] < amount)
        {
            Debug.LogWarning($"Cannot sell {amount} of {fishId}. Only have {speciesInventoryCounts.GetValueOrDefault(fishId, 0)}");
            return false;
        }

        var fishToSell = catchRecords
            .Where(r => r.fishId == fishId)
            .OrderByDescending(r => r.catchTime)
            .Take(amount)
            .ToList();

        if (fishToSell.Count != amount)
        {
            Debug.LogWarning($"Failed to find {amount} records of {fishId}. Found {fishToSell.Count}");
            return false;
        }

        foreach (var record in fishToSell)
        {
            totalWeightSold += record.weightKg;
            totalCoinsEarned += Mathf.RoundToInt(record.actualValue);
            catchRecords.Remove(record);
        }

        // Update inventory counts
        speciesInventoryCounts[fishId] -= amount;
        speciesInventoryWeights[fishId] -= totalWeightSold;
        currentInventoryCount -= amount;
        currentInventoryWeight -= totalWeightSold;
        totalFishSold += amount;

        // Remove species from dictionaries if count reaches zero
        if (speciesInventoryCounts[fishId] <= 0)
        {
            speciesInventoryCounts.Remove(fishId);
            speciesInventoryWeights.Remove(fishId);
        }

        // Update coins in this inventory
        coins += totalCoinsEarned;
        OnCoinsChanged?.Invoke(coins);

        // Also notify EconomyManager so debt panel and wallet update
        if (EconomyManager.Instance != null)
        {
            // Get catalog entry if available for proper pricing
            FishCatalogEntry catalogEntry = null;
            if (FishInventoryManager.Instance != null)
            {
                catalogEntry = FishInventoryManager.Instance.GetCatalogEntry(fishId);
            }

            // Record each fish sale in the economy system
            // This will update wallet, auto-pay debt, and fire OnEconomyChanged
            for (int i = 0; i < amount; i++)
            {
                float perFishValue = (float)totalCoinsEarned / amount;
                EconomyManager.Instance.RecordFishSale(
                    catalogEntry,
                    fishId,
                    FishCatchSource.SaltwaterFishing,
                    perFishValue,
                    false,
                    false);
            }
        }

        // Update quest system
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.RecordFishSold(amount);
        }

        // Update sustainability metrics for selling fish
        if (SustainableFishingMetrics.Instance != null)
        {
            SustainableFishingMetrics.Instance.RecordDecision(
                $"Sold {amount} {fishId} for {totalCoinsEarned} coins",
                "Selling your catch helps pay off debts and supports sustainable fishing economy.",
                true,
                DecisionSeverity.Low);
        }

        if (autoSave) SaveInventory();
        OnInventoryUpdated?.Invoke();

        Debug.Log($"Sold {amount} {fishId}: Weight={totalWeightSold:F1}kg, Coins={totalCoinsEarned}");
        return true;
    }

    public List<DetailedFishCatchRecord> GetAllRecords()
    {
        return new List<DetailedFishCatchRecord>(catchRecords);
    }

    // NEW: Get records by fish source
    public List<DetailedFishCatchRecord> GetRecordsBySource(string source)
    {
        return catchRecords.FindAll(record => record.catchSource == source);
    }

    public List<DetailedFishCatchRecord> GetRecordsByFishId(string fishId)
    {
        return catchRecords.FindAll(record => record.fishId == fishId);
    }

    public void SaveInventory()
    {
        try
        {
            var speciesCountsList = new List<SpeciesCountData>();
            var speciesWeightsList = new List<SpeciesWeightData>();

            foreach (var kvp in speciesInventoryCounts)
                speciesCountsList.Add(new SpeciesCountData { fishId = kvp.Key, count = kvp.Value });

            foreach (var kvp in speciesInventoryWeights)
                speciesWeightsList.Add(new SpeciesWeightData { fishId = kvp.Key, weight = kvp.Value });

            var saveData = new DetailedFishInventorySaveData
            {
                records = catchRecords,
                coins = coins,
                currentInventoryCount = currentInventoryCount,
                currentInventoryWeight = currentInventoryWeight,
                totalFishCaught = totalFishCaught,
                totalFishReleased = totalFishReleased,
                totalFishSold = totalFishSold,
                totalWeightCaught = totalWeightCaught,
                speciesInventoryCounts = speciesCountsList,
                speciesInventoryWeights = speciesWeightsList,
                lastSaved = DateTime.UtcNow
            };

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(savePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save detailed fish inventory: {e.Message}");
        }
    }

    public void LoadInventory()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                var saveData = JsonUtility.FromJson<DetailedFishInventorySaveData>(json);

                if (saveData != null)
                {
                    catchRecords = saveData.records ?? new List<DetailedFishCatchRecord>();
                    coins = saveData.coins;

                    currentInventoryCount = saveData.currentInventoryCount;
                    currentInventoryWeight = saveData.currentInventoryWeight;
                    totalFishCaught = saveData.totalFishCaught;
                    totalFishReleased = saveData.totalFishReleased;
                    totalFishSold = saveData.totalFishSold;
                    totalWeightCaught = saveData.totalWeightCaught;

                    speciesInventoryCounts = new Dictionary<string, int>();
                    speciesInventoryWeights = new Dictionary<string, float>();
                    fishCountBySpecies = new Dictionary<string, int>();
                    biggestCatchBySpecies = new Dictionary<string, float>();

                    if (saveData.speciesInventoryCounts != null)
                        foreach (var item in saveData.speciesInventoryCounts)
                            speciesInventoryCounts[item.fishId] = item.count;

                    if (saveData.speciesInventoryWeights != null)
                        foreach (var item in saveData.speciesInventoryWeights)
                            speciesInventoryWeights[item.fishId] = item.weight;

                    RecalculateStats();
                }
                else
                {
                    InitializeFresh();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load detailed fish inventory: {e.Message}");
                InitializeFresh();
            }
        }
        else
        {
            InitializeFresh();
        }
    }

    private void InitializeFresh()
    {
        catchRecords = new List<DetailedFishCatchRecord>();
        coins = startingCoins;
        currentInventoryCount = 0;
        currentInventoryWeight = 0f;
        totalFishCaught = 0;
        totalFishReleased = 0;
        totalFishSold = 0;
        totalWeightCaught = 0f;
        speciesInventoryCounts = new Dictionary<string, int>();
        speciesInventoryWeights = new Dictionary<string, float>();
        fishCountBySpecies = new Dictionary<string, int>();
        biggestCatchBySpecies = new Dictionary<string, float>();
    }

    private void RecalculateStats()
    {
        fishCountBySpecies.Clear();
        biggestCatchBySpecies.Clear();

        int calculatedInventory = catchRecords.Count;
        float calculatedInventoryWeight = 0f;
        float calculatedWeight = 0f;
        var calculatedSpeciesCounts = new Dictionary<string, int>();
        var calculatedSpeciesWeights = new Dictionary<string, float>();

        foreach (var record in catchRecords)
        {
            calculatedInventoryWeight += record.weightKg;
            calculatedWeight += record.weightKg;

            if (!calculatedSpeciesCounts.ContainsKey(record.fishId))
            {
                calculatedSpeciesCounts[record.fishId] = 0;
                calculatedSpeciesWeights[record.fishId] = 0f;
            }
            calculatedSpeciesCounts[record.fishId]++;
            calculatedSpeciesWeights[record.fishId] += record.weightKg;

            if (!fishCountBySpecies.ContainsKey(record.fishId))
                fishCountBySpecies[record.fishId] = 0;
            fishCountBySpecies[record.fishId]++;

            if (!biggestCatchBySpecies.ContainsKey(record.fishId) ||
                record.sizeCm > biggestCatchBySpecies[record.fishId])
            {
                biggestCatchBySpecies[record.fishId] = record.sizeCm;
            }
        }

        currentInventoryCount = calculatedInventory;
        currentInventoryWeight = calculatedInventoryWeight;
        totalWeightCaught = calculatedWeight;
        speciesInventoryCounts = calculatedSpeciesCounts;
        speciesInventoryWeights = calculatedSpeciesWeights;
    }

    private void OnApplicationQuit()
    {
        if (autoSave) SaveInventory();
    }
}

[System.Serializable]
public class DetailedFishInventorySaveData
{
    public List<DetailedFishCatchRecord> records = new List<DetailedFishCatchRecord>();
    public int coins = 0;
    public int currentInventoryCount = 0;
    public float currentInventoryWeight = 0f;
    public int totalFishCaught = 0;
    public int totalFishReleased = 0;
    public int totalFishSold = 0;
    public float totalWeightCaught = 0f;
    public List<SpeciesCountData> speciesInventoryCounts = new List<SpeciesCountData>();
    public List<SpeciesWeightData> speciesInventoryWeights = new List<SpeciesWeightData>();
    public DateTime lastSaved;
}

[System.Serializable]
public class SpeciesCountData
{
    public string fishId;
    public int count;
}

[System.Serializable]
public class SpeciesWeightData
{
    public string fishId;
    public float weight;
}