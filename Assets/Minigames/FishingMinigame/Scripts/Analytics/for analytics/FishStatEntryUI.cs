using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishStatEntryUI : MonoBehaviour
{
    [SerializeField] private Image fishIcon;
    [SerializeField] private TextMeshProUGUI fishNameText;
    [SerializeField] private TextMeshProUGUI catchCountText;
    [SerializeField] private TextMeshProUGUI lastCaughtText;

    public void Setup(FishData fish, FishingStats.FishCatchData stats)
    {
        if (fish == null || stats == null) return;

        if (fishIcon != null) fishIcon.sprite = fish.fishSprite;
        if (fishNameText != null) fishNameText.text = fish.fishName;
        if (catchCountText != null) catchCountText.text = $"Caught: {stats.catchCount}";
        
        if (lastCaughtText != null && stats.catchTimes.Count > 0)
        {
            var lastCaught = stats.catchTimes[stats.catchTimes.Count - 1];
            lastCaughtText.text = $"Last caught: {lastCaught.ToString("MM/dd/yyyy HH:mm")}";
        }
    }
} 