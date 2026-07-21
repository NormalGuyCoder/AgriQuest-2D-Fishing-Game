using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class FishingStatsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TextMeshProUGUI totalFishCaughtText;
    [SerializeField] private TextMeshProUGUI averageSessionTimeText;
    [SerializeField] private TextMeshProUGUI mostCaughtFishText;
    [SerializeField] private TextMeshProUGUI leastCaughtFishText;
    [SerializeField] private Transform fishListContent;
    [SerializeField] private GameObject fishStatEntryPrefab;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
        }
        
        HidePanel();
    }

    public void ShowStats()
    {
        statsPanel.SetActive(true);
        UpdateStats();
    }

    public void HidePanel()
    {
        statsPanel.SetActive(false);
    }

    private void UpdateStats()
    {
        var stats = FishingStats.Instance;
        if (stats == null) return;

        var allStats = stats.GetAllFishStats();
        int totalFish = allStats.Values.Sum(x => x.catchCount);
        
        // Update total fish caught
        if (totalFishCaughtText != null)
        {
            totalFishCaughtText.text = $"Total Fish Caught: {totalFish}";
        }

        // Update average session time
        if (averageSessionTimeText != null)
        {
            float avgTime = stats.GetAverageSessionTime();
            averageSessionTimeText.text = $"Average Session Time: {avgTime:F1} minutes";
        }

        // Update most caught fish
        if (mostCaughtFishText != null)
        {
            var mostCaught = stats.GetMostCaughtFish();
            if (mostCaught != null)
            {
                int count = allStats[mostCaught].catchCount;
                mostCaughtFishText.text = $"Most Caught: {mostCaught.fishName} ({count})";
            }
        }

        // Update least caught fish
        if (leastCaughtFishText != null)
        {
            var leastCaught = stats.GetLeastCaughtFish();
            if (leastCaught != null)
            {
                int count = allStats[leastCaught].catchCount;
                leastCaughtFishText.text = $"Least Caught: {leastCaught.fishName} ({count})";
            }
        }

        // Update fish list
        UpdateFishList(allStats);
    }

    private void UpdateFishList(System.Collections.Generic.Dictionary<FishData, FishingStats.FishCatchData> stats)
    {
        // Clear existing entries
        foreach (Transform child in fishListContent)
        {
            Destroy(child.gameObject);
        }

        // Create new entries
        foreach (var stat in stats.OrderByDescending(x => x.Value.catchCount))
        {
            GameObject entry = Instantiate(fishStatEntryPrefab, fishListContent);
            var entryUI = entry.GetComponent<FishStatEntryUI>();
            if (entryUI != null)
            {
                entryUI.Setup(stat.Key, stat.Value);
            }
        }
    }
} 