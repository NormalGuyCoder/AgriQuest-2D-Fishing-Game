using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Animation Settings")]
    [SerializeField] private float idleThreshold = 0.01f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Dance Settings")]
    [SerializeField] private AudioClip danceSound;
    [SerializeField] private float danceDuration = 2f;
    private bool isDancing = false;
    private bool isMusicPlaying = false;
    private AudioSource audioSource;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private bool isMoving;
    private Vector2 lastDirection;

    // NEW: Dialogue blocking variables
    private bool canMove = true;
    private bool isInDialogue = false;

    // Animation parameter names
    private readonly string ANIM_IS_MOVING = "IsMoving";
    private readonly string ANIM_HORIZONTAL = "Horizontal";
    private readonly string ANIM_VERTICAL = "Vertical";
    private readonly string ANIM_LAST_HORIZONTAL = "LastHorizontal";
    private readonly string ANIM_LAST_VERTICAL = "LastVertical";
    private readonly string ANIM_IS_DANCING = "IsDancing";

    private void Awake()
    {
        // Get required components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        // Get or add SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Ensure proper sprite rendering settings
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 1;
            spriteRenderer.enabled = true;
        }

        // Add AudioSource if it doesn't exist
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.5f;
            audioSource.loop = true;
        }

        // Configure Rigidbody2D for top-down movement
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (animator == null)
        {
            Debug.LogError("Animator component missing from player!");
            return;
        }
    }

    private void Start()
    {
        // Initialize with facing down
        lastDirection = Vector2.down;
        isMoving = false;
        isDancing = false;
        isMusicPlaying = false;
        
        // Ensure animator is properly configured
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
            
            // Set initial animation parameters
            SetIdleAnimation(Vector2.down);
        }

        // Verify sprite renderer is working
        if (spriteRenderer != null && spriteRenderer.sprite == null)
        {
            Debug.LogWarning("No sprite assigned to SpriteRenderer!");
        }

        // Subscribe to dialogue events
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.OnDialogStarted += OnDialogStarted;
            DialogSystem.Instance.OnDialogEnded += OnDialogEnded;
        }
    }

    private void OnDialogStarted(string npcName)
    {
        canMove = false;
        isInDialogue = true;
        
        // Stop any movement
        movement = Vector2.zero;
        isMoving = false;
        
        // Stop physics movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Set idle animation
        SetIdleAnimation(lastDirection);
        
        Debug.Log($"Player: Dialogue started with {npcName}, movement blocked");
    }

    private void OnDialogEnded(string npcName)
    {
        canMove = true;
        isInDialogue = false;
        Debug.Log($"Player: Dialogue ended with {npcName}, movement enabled");
    }

    private void Update()
    {
        // Handle music toggle - only if not in dialogue
        if (Input.GetKeyDown(KeyCode.Y) && !isInDialogue)
        {
            ToggleMusic();
        }

        // Only handle movement if not dancing AND can move
        if (!isDancing && canMove)
        {
            HandleMovementAndAnimation();
        }
        
        // NEW: Prevent dance activation during dialogue
        if (isInDialogue)
        {
            return;
        }
    }

    private void ToggleMusic()
    {
        if (audioSource != null && danceSound != null)
        {
            if (!isMusicPlaying)
            {
                // Start playing music
                audioSource.clip = danceSound;
                audioSource.Play();
                isMusicPlaying = true;
                
                // Start dance animation
                isDancing = true;
                if (animator != null)
                {
                    animator.SetBool(ANIM_IS_DANCING, true);
                }
            }
            else
            {
                // Stop music
                audioSource.Stop();
                isMusicPlaying = false;
                
                // Stop dance animation
                isDancing = false;
                if (animator != null)
                {
                    animator.SetBool(ANIM_IS_DANCING, false);
                    SetIdleAnimation(lastDirection);
                }
            }
        }
    }

    private void HandleMovementAndAnimation()
    {
        // Get input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // Create movement vector and normalize if needed
        Vector2 newMovement = new Vector2(moveX, moveY);
        if (newMovement.magnitude > 1f)
        {
            newMovement.Normalize();
        }

        // Update movement vector
        movement = newMovement;

        // Check if player is actually moving
        bool nowMoving = movement.magnitude > idleThreshold;

        // Update animation state
        if (nowMoving)
        {
            lastDirection = movement.normalized;
            SetMovingAnimation(movement);
        }
        else if (isMoving || !nowMoving)
        {
            SetIdleAnimation(lastDirection);
        }

        // Update movement state
        isMoving = nowMoving;
    }

    private void FixedUpdate()
    {
        // Only move if we can move, not dancing, and have a valid rigidbody
        if (rb != null && !rb.isKinematic && isMoving && !isDancing && canMove)
        {
            // Calculate the movement vector
            Vector2 moveVector = movement * moveSpeed * Time.fixedDeltaTime;
            
            // Move using MovePosition for physics-based movement
            Vector2 newPosition = rb.position + moveVector;
            rb.MovePosition(newPosition);
        }
        else if (!canMove && rb != null)
        {
            // Ensure no residual movement during dialogue
            rb.linearVelocity = Vector2.zero;
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

    private void OnDisable()
    {
        // Stop music when object is disabled
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            isMusicPlaying = false;
        }
        
        // Unsubscribe from events
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.OnDialogStarted -= OnDialogStarted;
            DialogSystem.Instance.OnDialogEnded -= OnDialogEnded;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.OnDialogStarted -= OnDialogStarted;
            DialogSystem.Instance.OnDialogEnded -= OnDialogEnded;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure components are assigned in editor
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }
#endif

    public void SavePlayerPosition()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        PlayerPrefs.SetFloat(currentScene + "_PlayerX", transform.position.x);
        PlayerPrefs.SetFloat(currentScene + "_PlayerY", transform.position.y);
        PlayerPrefs.Save();
    }
    
    // Public method to check if player can move (for other scripts)
    public bool CanPlayerMove()
    {
        return canMove && !isDancing && !isInDialogue;
    }
    
    // Public method to force stop movement (for cutscenes, etc.)
    public void ForceStopMovement()
    {
        canMove = false;
        movement = Vector2.zero;
        isMoving = false;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    // Public method to resume movement
    public void ResumeMovement()
    {
        canMove = true;
    }
}