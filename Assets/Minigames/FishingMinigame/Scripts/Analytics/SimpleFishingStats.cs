using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SimpleFishingStats : MonoBehaviour
{
    public TextMeshProUGUI fishCountText;
    public TextMeshProUGUI sessionTimeText;
    public TextMeshProUGUI fishTypeCountsText;

    private int totalFishCaught = 0;
    private Dictionary<string, int> fishCatchCounts = new Dictionary<string, int>();
    private float sessionTime = 0f;
    private bool sessionActive = false;
    private bool timerPaused = false;

    void Start()
    {
        totalFishCaught = 0;
        fishCatchCounts.Clear();
        sessionTime = 0f;
        sessionActive = true;
        timerPaused = false;
        UpdateUI();
    }

    void Update()
    {
        if (sessionActive && !timerPaused)
        {
            sessionTime += Time.deltaTime;
            UpdateUI(); // Update every frame for timer
        }
    }

    public void OnFishCaught(FishData fish)
    {
        if (fish == null) return;

        totalFishCaught++;
        if (fishCatchCounts.ContainsKey(fish.fishName))
        {
            fishCatchCounts[fish.fishName]++;
        }
        else
        {
            fishCatchCounts[fish.fishName] = 1;
        }
        UpdateUI();
    }

    public void PauseTimer()
    {
        timerPaused = true;
    }

    public void ResumeTimer()
    {
        timerPaused = false;
    }

    private void UpdateUI()
    {
        if (fishCountText != null)
            fishCountText.text = $"Fish Caught: {totalFishCaught}";

        if (sessionTimeText != null)
            sessionTimeText.text = $"Time: {sessionTime:F1} s";

        if (fishTypeCountsText != null)
        {
            if (fishCatchCounts.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var pair in fishCatchCounts.OrderByDescending(pair => pair.Value))
                {
                    sb.AppendLine($"{pair.Key}: {pair.Value}");
                }
                fishTypeCountsText.text = sb.ToString();
            }
            else
            {
                fishTypeCountsText.text = "Fish Counts: None";
            }
        }
    }

    public void EndSession()
    {
        sessionActive = false;
    }
}