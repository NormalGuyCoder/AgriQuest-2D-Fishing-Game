using UnityEngine;

public class FishingCameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);
    
    [Header("Boundaries")]
    public float minY = -20f;
    public float maxY = 0f;
    public float minX = -10f;
    public float maxX = 10f;

    private Vector3 velocity = Vector3.zero;
    private bool isFollowing = false;

    private void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Clamp the position within boundaries
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        
        // Smoothly move the camera
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        isFollowing = true;
    }

    public void ClearTarget()
    {
        target = null;
        isFollowing = false;
    }
}