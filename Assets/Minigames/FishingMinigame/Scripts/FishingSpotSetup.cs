using UnityEngine;

public class FishingSpotSetup : MonoBehaviour
{
    [Header("Fishing Spot Settings")]
    public Vector3 spotOffset = new Vector3(0.5f, 0f, 0f); // Offset from player
    public Color gizmoColor = Color.yellow;
    public float gizmoRadius = 0.2f;

    private void Awake()
    {
        CreateFishingSpot();
    }

    private void CreateFishingSpot()
    {
        // Check if FishingSpot already exists
        Transform existingSpot = transform.Find("FishingSpot");
        if (existingSpot != null)
        {
            Debug.Log("FishingSpot already exists!");
            return;
        }

        // Create new FishingSpot
        GameObject fishingSpot = new GameObject("FishingSpot");
        fishingSpot.transform.SetParent(transform);
        fishingSpot.transform.localPosition = spotOffset;

        // Add FishingSpotIndicator
        FishingSpotIndicator indicator = fishingSpot.AddComponent<FishingSpotIndicator>();
        indicator.gizmoColor = gizmoColor;
        indicator.gizmoRadius = gizmoRadius;

        Debug.Log("FishingSpot created successfully!");
    }
} 