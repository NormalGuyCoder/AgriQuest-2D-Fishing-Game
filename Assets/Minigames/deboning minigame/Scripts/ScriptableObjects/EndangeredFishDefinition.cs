using UnityEngine;

[CreateAssetMenu(fileName = "New Endangered Fish", menuName = "Fish Deboner/Endangered Fish Definition")]
public class EndangeredFishDefinition : ScriptableObject
{
    [Header("Fish Information")]
    public string fishName;
    public string scientificName;
    [TextArea(3, 5)]
    public string description;
    
    [Header("Conservation Status")]
    public ConservationStatus status;
    public string iucnStatus; // e.g., "Critically Endangered"
    public string populationTrend; // e.g., "Decreasing"
    
    [Header("Habitat & Distribution")]
    [TextArea(2, 3)]
    public string habitat;
    [TextArea(2, 3)]
    public string distribution; // Where they are found globally
    
    [Header("Threats & Importance")]
    [TextArea(3, 5)]
    public string threats; // What threatens this species
    [TextArea(3, 5)]
    public string importance; // Ecological importance, cultural significance
    
    [Header("Conservation Efforts")]
    [TextArea(3, 5)]
    public string conservationEfforts; // What's being done to protect them
    
    [Header("Visual")]
    public Sprite fishImage;
    public Sprite fishIcon;

    [Header("Learn More Content")]
    [TextArea(2, 4)]
    public string learnMoreHeadline;

    [TextArea(4, 6)]
    public string learnMoreCopy;

    [TextArea(3, 5)]
    public string learnMoreAction;

    public enum ConservationStatus
    {
        Vulnerable,
        Endangered,
        CriticallyEndangered,
        ExtinctInWild
    }

    public string GetStatusText()
    {
        switch (status)
        {
            case ConservationStatus.Vulnerable: return "Vulnerable";
            case ConservationStatus.Endangered: return "Endangered";
            case ConservationStatus.CriticallyEndangered: return "Critically Endangered";
            case ConservationStatus.ExtinctInWild: return "Extinct in the Wild";
            default: return "Unknown";
        }
    }
}


