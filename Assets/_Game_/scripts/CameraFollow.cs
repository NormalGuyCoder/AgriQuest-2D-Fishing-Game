using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // The player to follow
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 10f; // How smooth the camera movement should be
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // Offset from the player (z should be -10 for 2D games)
    
    [Header("Boundaries")]
    [SerializeField] private bool useBoundaries = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;

    private void Start()
    {
        // If no target is set, try to find the player
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("No player found with tag 'Player'. Please assign a target for the camera to follow.");
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Calculate the desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Apply boundaries if enabled
        if (useBoundaries)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }
        
        // Smoothly move the camera towards the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Keep the z-position from the offset (important for 2D games)
        smoothedPosition.z = offset.z;
        
        // Update the camera position
        transform.position = smoothedPosition;
    }

    // Public method to set new boundaries at runtime
    public void SetBoundaries(float newMinX, float newMaxX, float newMinY, float newMaxY)
    {
        minX = newMinX;
        maxX = newMaxX;
        minY = newMinY;
        maxY = newMaxY;
    }

    // Public method to set new target at runtime
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Public method to set new offset at runtime
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
} 