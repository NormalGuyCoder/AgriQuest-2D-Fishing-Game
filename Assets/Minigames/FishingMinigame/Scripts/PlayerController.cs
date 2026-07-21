using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Fishing Settings")]
    [SerializeField] private FishingLineController fishingLineController;
    [SerializeField] private Transform fishingSpot; // The position where the player will stand
    [SerializeField] private FishingCameraFollow cameraFollow;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector2 lastDirection = Vector2.right; // Default facing right

    // Animation parameter names
    private readonly string ANIM_IS_MOVING = "IsMoving";
    private readonly string ANIM_HORIZONTAL = "Horizontal";
    private readonly string ANIM_VERTICAL = "Vertical";
    private readonly string ANIM_LAST_HORIZONTAL = "LastHorizontal";
    private readonly string ANIM_LAST_VERTICAL = "LastVertical";

    private void Awake()
    {
        // Find fishing spot if not assigned
        if (fishingSpot == null)
        {
            fishingSpot = transform.Find("FishingSpot");
            if (fishingSpot == null)
            {
                Debug.LogError("FishingSpot not found! Please create a child GameObject named 'FishingSpot' under the Player.");
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Find camera follow if not assigned
        if (cameraFollow == null)
        {
            cameraFollow = Camera.main.GetComponent<FishingCameraFollow>();
            if (cameraFollow == null)
            {
                Debug.LogWarning("FishingCameraFollow component not found on main camera!");
            }
        }
    }

    private void Start()
    {
        // Set initial position to fishing spot
        if (fishingSpot != null)
        {
            transform.position = fishingSpot.position;
        }

        // Set initial animation state
        SetIdleAnimation(lastDirection);
    }

    private void Update()
    {
        HandleFishingInput();
        UpdateFishingAnimation();
    }

    private void HandleFishingInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && fishingLineController != null)
        {
            // Trigger fishing action
            fishingLineController.CastLine();
        }
    }

    private void UpdateFishingAnimation()
    {
        if (fishingLineController != null)
        {
            // Update animation based on fishing line state
            switch (fishingLineController.CurrentState)
            {
                case FishingLineController.FishingState.Casting:
                    SetMovingAnimation(Vector2.down);
                    break;
                case FishingLineController.FishingState.Reeling:
                    SetMovingAnimation(Vector2.up);
                    break;
                default:
                    SetIdleAnimation(lastDirection);
                    break;
            }
        }
    }

    private void SetMovingAnimation(Vector2 direction)
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.SetBool(ANIM_IS_MOVING, true);
            animator.SetFloat(ANIM_HORIZONTAL, direction.x);
            animator.SetFloat(ANIM_VERTICAL, direction.y);
            animator.SetFloat(ANIM_LAST_HORIZONTAL, direction.x);
            animator.SetFloat(ANIM_LAST_VERTICAL, direction.y);
        }
    }

    private void SetIdleAnimation(Vector2 lastDir)
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.SetBool(ANIM_IS_MOVING, false);
            animator.SetFloat(ANIM_HORIZONTAL, 0);
            animator.SetFloat(ANIM_VERTICAL, 0);
            animator.SetFloat(ANIM_LAST_HORIZONTAL, lastDir.x);
            animator.SetFloat(ANIM_LAST_VERTICAL, lastDir.y);
        }
    }
} 