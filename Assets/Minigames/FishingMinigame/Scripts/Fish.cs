using UnityEngine;

public class Fish : MonoBehaviour
{
    [SerializeField] private FishData fishData; // Assign this in the prefab inspector

    public FishData GetFishData()
    {
        return fishData;
    }

    public bool IsEndangered => fishData != null && fishData.IsEndangeredSpecies();

    public bool RequiresCatchWarning => fishData != null && fishData.RequiresCatchWarning();
} 