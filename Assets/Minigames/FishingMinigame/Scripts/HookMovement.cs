using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HookMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float retractionSpeed = 5f;
    public float rotationSpeed = 200f;
    public float maxYPosition = -9f;
    public float maxXPosition = 8f;
    public float minXPosition = -8f;
    public LayerMask obstacles;

    [Header("Fishing Timer Settings")]
    public float fishingDuration = 5f; // How long the hook stays in water
    private float fishingTimer = 0f;
    private bool isFishingEnabled = false;

    private Vector2 moveDirection;
    private Rigidbody2D rigidBody;
    private bool isRetracting;
    private Transform retractionPoint;
    private bool isMoving = false;

    private void Start()
    {
        isRetracting = false;
        rigidBody = GetComponent<Rigidbody2D>();
        setupRigidBody();
    }

    private void Update()
    {
        // Check for space input to start fishing
        if (Input.GetKeyDown(KeyCode.Space) && !isFishingEnabled && !isRetracting)
        {
            StartFishing();
        }

        if (isFishingEnabled)
        {
            // Update fishing timer
            fishingTimer += Time.deltaTime;
            if (fishingTimer >= fishingDuration)
            {
                StopFishing();
            }
        }

        if (!isRetracting)
        {
            HandlePlayerMovement();
        }
        else
        {
            HandleRetractMovement();
        }
    }

    private void StartFishing()
    {
        isFishingEnabled = true;
        fishingTimer = 0f;
        isRetracting = false;
        EnableControls();
    }

    private void StopFishing()
    {
        isFishingEnabled = false;
        RetractHook(retractionPoint);
    }

    private void HandlePlayerMovement()
    {
        if (!isFishingEnabled) return;

        HandleForwardMovement();
        HandleRotation();
    }

    private void setupRigidBody()
    {
        rigidBody.gravityScale = 0f;
        rigidBody.linearDamping = 0.5f;
    }

    private void HandleForwardMovement()
    {
        // Check for obstacles
        RaycastHit2D raycastHit2D = Physics2D.Raycast(transform.position, -transform.up, 1f, obstacles);
        
        // Only move if no obstacles and within bounds
        if (raycastHit2D.collider == null)
        {
            // Get input for horizontal movement
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            
            // Calculate movement direction based on rotation
            Vector2 forwardDirection = -transform.up;
            Vector2 rightDirection = transform.right;
            
            // Combine forward and horizontal movement
            moveDirection = (forwardDirection + rightDirection * horizontalInput).normalized * moveSpeed;
            
            // Apply movement
            rigidBody.linearVelocity = moveDirection;
            
            // Clamp position within bounds
            Vector3 position = transform.position;
            position.y = Mathf.Min(position.y, maxYPosition);
            position.x = Mathf.Clamp(position.x, minXPosition, maxXPosition);
            transform.position = position;
            
            isMoving = true;
        }
        else
        {
            rigidBody.linearVelocity = Vector2.zero;
            isMoving = false;
        }
    }

    private void HandleRotation()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * -rotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleRetractMovement()
    {
        if (retractionPoint != null)
        {
            Vector2 direction = (retractionPoint.position - transform.position);
            float distance = direction.magnitude;
            
            // Use smooth interpolation to prevent stuttering on different refresh rates
            if (distance > 0.1f)
            {
                moveDirection = direction.normalized * retractionSpeed;
                rigidBody.linearVelocity = moveDirection;
                
                // Calculate rotation to face retraction point
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * 3f * Time.deltaTime);
            }
            else
            {
                // Snap to position when very close to prevent jitter
                transform.position = retractionPoint.position;
                rigidBody.linearVelocity = Vector2.zero;
                isRetracting = false;
                isFishingEnabled = false;
            }
        }
    }

    public void RetractHook(Transform targetRetractionPoint)
    {
        retractionPoint = targetRetractionPoint;
        isRetracting = true;
        isFishingEnabled = false;
    }

    public void EnableControls()
    {
        isRetracting = false;
    }
} 