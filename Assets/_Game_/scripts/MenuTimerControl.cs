using UnityEngine;

public class MenuTimerControl : MonoBehaviour
{
    public void PauseFishingTimer()
    {
        var stats = FindObjectOfType<SimpleFishingStats>();
        if (stats != null) stats.PauseTimer();
    }

    public void ResumeFishingTimer()
    {
        var stats = FindObjectOfType<SimpleFishingStats>();
        if (stats != null) stats.ResumeTimer();
    }
}