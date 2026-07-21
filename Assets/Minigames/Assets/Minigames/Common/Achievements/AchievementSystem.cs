using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages achievement tracking and unlocking
/// </summary>
public class AchievementSystem : MonoBehaviour
{
    public static AchievementSystem Instance { get; private set; }

    [Header("Achievement Database")]
    public List<AchievementDefinition> allAchievements = new List<AchievementDefinition>();

    private HashSet<string> unlockedAchievements = new HashSet<string>();
    private Dictionary<string, float> achievementProgress = new Dictionary<string, float>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Check and update achievements based on current metrics
    /// </summary>
    public void UpdateAchievements(SustainabilityReport report)
    {
        if (report == null) return;

        foreach (var achievement in allAchievements)
        {
            if (achievement == null) continue;
            if (unlockedAchievements.Contains(achievement.achievementId)) continue;

            bool shouldUnlock = CheckAchievement(achievement, report);
            
            if (shouldUnlock)
            {
                UnlockAchievement(achievement);
            }
            else
            {
                UpdateProgress(achievement, report);
            }
        }
    }

    private bool CheckAchievement(AchievementDefinition achievement, SustainabilityReport report)
    {
        switch (achievement.type)
        {
            case AchievementDefinition.AchievementType.TotalCatches:
                return report.totalCatches >= achievement.requiredValue;

            case AchievementDefinition.AchievementType.SpeciesDiversity:
                return report.speciesDiversity >= achievement.requiredValue;

            case AchievementDefinition.AchievementType.SustainabilityScore:
                return report.sustainabilityScore >= achievement.requiredValue;

            case AchievementDefinition.AchievementType.NoOverfishing:
                return report.overfishedSpecies.Count == 0 && report.totalCatches >= achievement.requiredValue;

            case AchievementDefinition.AchievementType.NoEndangeredCatch:
                return report.endangeredSpeciesCaught == 0 && report.totalCatches >= achievement.requiredValue;

            case AchievementDefinition.AchievementType.GoodDecisionsCount:
                return report.goodDecisions.Count >= achievement.requiredValue;

            case AchievementDefinition.AchievementType.BadDecisionsCount:
                return report.badDecisions.Count <= achievement.requiredValue;

            case AchievementDefinition.AchievementType.PerfectSession:
                return report.sustainabilityScore >= 90f && report.badDecisions.Count == 0;

            case AchievementDefinition.AchievementType.LearningComplete:
                return report.goodDecisions.Count(d => d.description.Contains("Learned")) >= achievement.requiredValue;

            case AchievementDefinition.AchievementType.ConservationMaster:
                return report.sustainabilityScore >= 95f && 
                       report.overfishedSpecies.Count == 0 && 
                       report.endangeredSpeciesCaught == 0;

            default:
                return false;
        }
    }

    private void UpdateProgress(AchievementDefinition achievement, SustainabilityReport report)
    {
        float progress = CalculateProgress(achievement, report);
        achievementProgress[achievement.achievementId] = progress;
    }

    private void UnlockAchievement(AchievementDefinition achievement)
    {
        unlockedAchievements.Add(achievement.achievementId);
        achievementProgress[achievement.achievementId] = 1f;
        
        Debug.Log($"Achievement Unlocked: {achievement.title} - {achievement.description}");
        
        // Trigger achievement unlocked event
        OnAchievementUnlocked?.Invoke(achievement);
        
        SaveAchievements();
    }

    public bool IsUnlocked(string achievementId)
    {
        return unlockedAchievements.Contains(achievementId);
    }

    public float GetProgress(string achievementId)
    {
        // If progress is already calculated, return it
        if (achievementProgress.ContainsKey(achievementId))
        {
            return achievementProgress[achievementId];
        }
        
        // If not calculated yet, try to calculate it now
        var achievement = allAchievements.FirstOrDefault(a => a != null && a.achievementId == achievementId);
        if (achievement != null && SustainableFishingMetrics.Instance != null)
        {
            var report = SustainableFishingMetrics.Instance.GetReport();
            if (report != null)
            {
                float progress = CalculateProgress(achievement, report);
                achievementProgress[achievementId] = progress;
                return progress;
            }
        }
        
        return 0f;
    }
    
    /// <summary>
    /// Calculate progress for an achievement based on current report
    /// </summary>
    private float CalculateProgress(AchievementDefinition achievement, SustainabilityReport report)
    {
        if (report == null || achievement == null) return 0f;
        
        switch (achievement.type)
        {
            case AchievementDefinition.AchievementType.TotalCatches:
                return Mathf.Clamp01((float)report.totalCatches / achievement.requiredValue);
                
            case AchievementDefinition.AchievementType.SpeciesDiversity:
                return Mathf.Clamp01((float)report.speciesDiversity / achievement.requiredValue);
                
            case AchievementDefinition.AchievementType.SustainabilityScore:
                return Mathf.Clamp01(report.sustainabilityScore / achievement.requiredValue);
                
            case AchievementDefinition.AchievementType.NoOverfishing:
                if (report.overfishedSpecies.Count == 0)
                    return Mathf.Clamp01((float)report.totalCatches / achievement.requiredValue);
                return 0f;
                
            case AchievementDefinition.AchievementType.NoEndangeredCatch:
                if (report.endangeredSpeciesCaught == 0)
                    return Mathf.Clamp01((float)report.totalCatches / achievement.requiredValue);
                return 0f;
                
            case AchievementDefinition.AchievementType.GoodDecisionsCount:
                return Mathf.Clamp01((float)report.goodDecisions.Count / achievement.requiredValue);
                
            case AchievementDefinition.AchievementType.BadDecisionsCount:
                // For bad decisions, progress is inverse (fewer is better)
                return Mathf.Clamp01(1f - ((float)report.badDecisions.Count / achievement.requiredValue));
                
            case AchievementDefinition.AchievementType.PerfectSession:
                float scoreProgress = report.sustainabilityScore / 90f;
                float noBadDecisions = report.badDecisions.Count == 0 ? 1f : 0f;
                return Mathf.Clamp01((scoreProgress + noBadDecisions) / 2f);
                
            case AchievementDefinition.AchievementType.LearningComplete:
                int learnedCount = report.goodDecisions.Count(d => d.description.Contains("Learned"));
                return Mathf.Clamp01((float)learnedCount / achievement.requiredValue);
                
            case AchievementDefinition.AchievementType.ConservationMaster:
                float score = report.sustainabilityScore / 95f;
                float noOverfishing = report.overfishedSpecies.Count == 0 ? 1f : 0f;
                float noEndangered = report.endangeredSpeciesCaught == 0 ? 1f : 0f;
                return Mathf.Clamp01((score + noOverfishing + noEndangered) / 3f);
                
            default:
                return 0f;
        }
    }

    public List<AchievementDefinition> GetUnlockedAchievements()
    {
        return allAchievements.Where(a => a != null && unlockedAchievements.Contains(a.achievementId)).ToList();
    }

    public List<AchievementDefinition> GetLockedAchievements()
    {
        return allAchievements.Where(a => a != null && !unlockedAchievements.Contains(a.achievementId)).ToList();
    }

    public List<string> GetUnlockedAchievementIds()
    {
        return unlockedAchievements.ToList();
    }

    public System.Action<AchievementDefinition> OnAchievementUnlocked;

    private void SaveAchievements()
    {
        // Primary persistence: JSON via AchievementsDataStore
        if (AchievementsDataStore.Instance != null)
        {
            AchievementsDataStore.Instance.SetUnlockedAchievementIds(unlockedAchievements.ToList());
        }

        // Legacy fallback: keep PlayerPrefs in sync so older builds still work
        string unlockedIds = string.Join(",", unlockedAchievements);
        PlayerPrefs.SetString("UnlockedAchievements", unlockedIds);
        PlayerPrefs.Save();
    }

    private void LoadAchievements()
    {
        // Prefer JSON-backed data from AchievementsDataStore so that
        // deleting the JSON file truly resets achievements.
        if (AchievementsDataStore.Instance != null && AchievementsDataStore.Instance.HasSavedData)
        {
            var idsFromStore = AchievementsDataStore.Instance.GetUnlockedAchievementIds();
            if (idsFromStore != null && idsFromStore.Count > 0)
            {
                unlockedAchievements = new HashSet<string>(idsFromStore);

                // Keep legacy PlayerPrefs in sync for compatibility
                string syncedIds = string.Join(",", unlockedAchievements);
                PlayerPrefs.SetString("UnlockedAchievements", syncedIds);
                PlayerPrefs.Save();
                return;
            }
        }

        // Fallback: legacy PlayerPrefs-only data (older saves)
        string unlockedIds = PlayerPrefs.GetString("UnlockedAchievements", "");
        if (!string.IsNullOrEmpty(unlockedIds))
        {
            unlockedAchievements = new HashSet<string>(unlockedIds.Split(','));

            // Immediately mirror into JSON so future loads use the file instead
            if (AchievementsDataStore.Instance != null)
            {
                AchievementsDataStore.Instance.SetUnlockedAchievementIds(unlockedAchievements.ToList());
            }
        }
    }
	
    /// <summary>
    /// Reset all achievements and progress for a new game.
    /// </summary>
	public void ResetAchievements()
	{
		unlockedAchievements.Clear();
		achievementProgress.Clear();

		// Clear JSON-backed data
		if (AchievementsDataStore.Instance != null)
		{
			AchievementsDataStore.Instance.SetUnlockedAchievementIds(new System.Collections.Generic.List<string>());
		}

		// Clear legacy PlayerPrefs key as well
		PlayerPrefs.DeleteKey("UnlockedAchievements");
		PlayerPrefs.Save();

		Debug.Log("AchievementSystem: All achievements reset.");
    }
}

