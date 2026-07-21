using UnityEngine;

/// <summary>
/// Definition of an achievement that can be unlocked by players.
/// Create instances of this ScriptableObject to define achievements.
/// </summary>
[CreateAssetMenu(fileName = "New Achievement", menuName = "Achievements/Achievement Definition")]
public class AchievementDefinition : ScriptableObject
{
    [Header("Achievement Info")]
    public string achievementId;
    public string title;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    
    [Header("Requirements")]
    public AchievementType type;
    public int requiredValue;
    public string requiredFishId; // For fish-specific achievements
    
    [Header("Rewards")]
    public int experiencePoints = 0;
    public string rewardDescription = "";

    public enum AchievementType
    {
        TotalCatches,
        SpeciesDiversity,
        SustainabilityScore,
        NoOverfishing,
        NoEndangeredCatch,
        GoodDecisionsCount,
        BadDecisionsCount,
        PerfectSession,
        LearningComplete,
        ConservationMaster
    }
}

