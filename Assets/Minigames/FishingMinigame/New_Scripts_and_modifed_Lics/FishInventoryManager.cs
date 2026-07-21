using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class FishInventoryManager : MonoBehaviour
{
    public static FishInventoryManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private FishCatalogDatabase catalogDatabase;

    [Header("Persistence")]
    [SerializeField] private string saveKey = "FishInventoryData";
    [SerializeField] private bool loadOnAwake = true;
    [SerializeField] private bool saveOnQuit = true;

    private readonly Dictionary<string, FishCatchTotals> catchTotals = new Dictionary<string, FishCatchTotals>();
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadOnAwake)
        {
            LoadFromPrefs();
        }
    }

    private void OnApplicationQuit()
    {
        if (saveOnQuit)
        {
            SaveToPrefs();
        }
    }

    public void RecordCatch(
        FishCatalogEntry catalogEntry,
        int amount,
        FishCatchSource source,
        string fallbackId = null)
    {
        if (amount <= 0)
            return;

        string fishId = ResolveFishId(catalogEntry, fallbackId);
        if (string.IsNullOrEmpty(fishId))
        {
            Debug.LogWarning("FishInventoryManager: Unable to resolve fish id for catch event.");
            return;
        }

        if (!catchTotals.TryGetValue(fishId, out var totals))
        {
            totals = new FishCatchTotals { fishId = fishId, catalogEntry = catalogEntry };
            catchTotals[fishId] = totals;
        }

        totals.totalCaught += amount;
        switch (source)
        {
            case FishCatchSource.Deboning:
                totals.deboningCaught += amount;
                break;
            case FishCatchSource.SaltwaterFishing:
                totals.saltwaterCaught += amount;
                break;
            case FishCatchSource.FreshwaterFishing:
                totals.freshwaterCaught += amount;
                break;
        }

        if (catalogEntry == null && catalogDatabase != null)
        {
            var entry = catalogDatabase.GetEntry(fishId);
            if (entry != null)
            {
                totals.catalogEntry = entry;
            }
        }

        OnInventoryChanged?.Invoke();
    }

    public FishCatchTotals GetTotals(string fishIdOrAlias)
    {
        if (string.IsNullOrEmpty(fishIdOrAlias))
            return null;

        string key = Sanitize(fishIdOrAlias);
        catchTotals.TryGetValue(key, out var totals);
        return totals;
    }

    /// <summary>
    /// Convenience lookup that exposes the assigned catalog database to other systems.
    /// </summary>
    public FishCatalogEntry GetCatalogEntry(string fishIdOrAlias)
    {
        if (catalogDatabase == null || string.IsNullOrWhiteSpace(fishIdOrAlias))
            return null;

        return catalogDatabase.GetEntry(fishIdOrAlias);
    }

    public IEnumerable<FishCatchTotals> GetAllTotals()
    {
        return catchTotals.Values;
    }

    /// <summary>
    /// Completely reset all stored catch totals and remove the PlayerPrefs entry.
    /// Use this when starting a brand new game so that no old catch history remains.
    /// </summary>
    public void ResetAllData()
    {
        catchTotals.Clear();
        OnInventoryChanged?.Invoke();

        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();
    }

    public void ClearInventory(bool save = true)
    {
        catchTotals.Clear();
        if (save)
        {
            SaveToPrefs();
        }
        OnInventoryChanged?.Invoke();
    }

    public void SaveToPrefs()
    {
        FishInventorySaveData data = new FishInventorySaveData();
        foreach (var pair in catchTotals)
        {
            data.records.Add(pair.Value.ToSerializable());
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadFromPrefs()
    {
        catchTotals.Clear();

        if (!PlayerPrefs.HasKey(saveKey))
            return;

        string json = PlayerPrefs.GetString(saveKey);
        if (string.IsNullOrEmpty(json))
            return;

        FishInventorySaveData data = JsonUtility.FromJson<FishInventorySaveData>(json);
        if (data?.records == null)
            return;

        foreach (var record in data.records)
        {
            var totals = new FishCatchTotals
            {
                fishId = record.fishId,
                totalCaught = record.totalCaught,
                deboningCaught = record.deboningCaught,
                saltwaterCaught = record.saltwaterCaught,
                freshwaterCaught = record.freshwaterCaught
            };

            if (catalogDatabase != null)
            {
                totals.catalogEntry = catalogDatabase.GetEntry(record.fishId);
            }

            catchTotals[record.fishId] = totals;
        }

        OnInventoryChanged?.Invoke();
    }

    private string ResolveFishId(FishCatalogEntry entry, string fallbackId)
    {
        if (entry != null)
        {
            string id = entry.GetFishId();
            if (!string.IsNullOrEmpty(id))
                return id;
        }

        if (!string.IsNullOrWhiteSpace(fallbackId))
        {
            return Sanitize(fallbackId);
        }

        return null;
    }

    private string Sanitize(string value)
    {
        return value?.Trim().ToLower().Replace(" ", "_");
    }
}

[System.Serializable]
public class FishCatchTotals
{
    public string fishId;
    public FishCatalogEntry catalogEntry;
    public int totalCaught;
    public int deboningCaught;
    public int saltwaterCaught;
    public int freshwaterCaught;

    public FishCatchRecord ToSerializable()
    {
        return new FishCatchRecord
        {
            fishId = fishId,
            totalCaught = totalCaught,
            deboningCaught = deboningCaught,
            saltwaterCaught = saltwaterCaught,
            freshwaterCaught = freshwaterCaught
        };
    }
}

[System.Serializable]
public class FishCatchRecord
{
    public string fishId;
    public int totalCaught;
    public int deboningCaught;
    public int saltwaterCaught;
    public int freshwaterCaught;
}

[System.Serializable]
public class FishInventorySaveData
{
    public List<FishCatchRecord> records = new List<FishCatchRecord>();
}

