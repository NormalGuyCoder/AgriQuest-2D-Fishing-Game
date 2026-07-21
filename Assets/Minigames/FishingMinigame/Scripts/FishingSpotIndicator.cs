using UnityEngine;

public class FishingSpotIndicator : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color gizmoColor = Color.yellow;
    public float gizmoRadius = 0.2f;

    private void OnDrawGizmos()
    {
        // Draw a yellow sphere to represent the fishing spot
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoRadius);
    }
} 