using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central registry of all fish catalog entries. Provides lookup utilities.
/// </summary>
[CreateAssetMenu(fileName = "FishCatalogDatabase", menuName = "Fish Catalog/Database")]
public class FishCatalogDatabase : ScriptableObject
{
    [SerializeField]
    private List<FishCatalogEntry> entries = new List<FishCatalogEntry>();

    private Dictionary<string, FishCatalogEntry> idLookup;
    private bool isInitialized;

    private void OnEnable()
    {
        BuildLookup();
    }

    public IReadOnlyList<FishCatalogEntry> Entries => entries;

    public FishCatalogEntry GetEntry(string fishIdOrAlias)
    {
        if (string.IsNullOrWhiteSpace(fishIdOrAlias))
        {
            return null;
        }

        BuildLookup();

        string sanitized = Sanitize(fishIdOrAlias);
        if (idLookup.TryGetValue(sanitized, out var entry))
        {
            return entry;
        }

        return null;
    }

    public void BuildLookup()
    {
        if (isInitialized && idLookup != null)
            return;

        idLookup = new Dictionary<string, FishCatalogEntry>();

        foreach (var entry in entries)
        {
            if (entry == null) continue;
            string id = entry.GetFishId();
            if (string.IsNullOrWhiteSpace(id)) continue;

            idLookup[id] = entry;

            if (entry.aliases != null)
            {
                foreach (var alias in entry.aliases)
                {
                    string aliasKey = Sanitize(alias);
                    if (!string.IsNullOrEmpty(aliasKey))
                    {
                        idLookup[aliasKey] = entry;
                    }
                }
            }
        }

        isInitialized = true;
    }

    private string Sanitize(string value)
    {
        return value?.Trim().ToLower().Replace(" ", "_");
    }
}

