using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FishMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float directionChangeInterval = 2f;
    public float boundaryPadding = 0.5f;

    [Header("Rendering Settings")]
    public int sortingOrder = 5; // Higher number = more in front

    private Rigidbody2D rb;
    private BoxCollider2D bounds;
    private Vector2 currentDirection;
    private float directionChangeTimer;
    private SpriteRenderer spriteRenderer;

    public void Initialize(BoxCollider2D spawnBounds)
    {
        bounds = spawnBounds;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Configure Rigidbody2D
        rb.gravityScale = 0f;
        rb.linearDamping = 0.5f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Configure SpriteRenderer
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
        
        // Set initial direction
        SetNewRandomDirection();
    }

    private void Update()
    {
        if (bounds == null) return;

        // Update direction change timer
        directionChangeTimer -= Time.deltaTime;
        if (directionChangeTimer <= 0f)
        {
            SetNewRandomDirection();
        }

        // Check boundaries and adjust direction if needed
        CheckBoundaries();

        // Update sprite direction
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = currentDirection.x < 0;
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            // Apply movement
            rb.linearVelocity = currentDirection * moveSpeed;
        }
    }

    private void SetNewRandomDirection()
    {
        // Generate random direction
        float angle = Random.Range(0f, 360f);
        currentDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        
        // Reset timer
        directionChangeTimer = directionChangeInterval;
    }

    private void CheckBoundaries()
    {
        Bounds fishBounds = GetComponent<Collider2D>().bounds;
        Bounds areaBounds = bounds.bounds;

        // Check X boundaries
        if (fishBounds.min.x < areaBounds.min.x + boundaryPadding)
        {
            currentDirection.x = Mathf.Abs(currentDirection.x);
        }
        else if (fishBounds.max.x > areaBounds.max.x - boundaryPadding)
        {
            currentDirection.x = -Mathf.Abs(currentDirection.x);
        }

        // Check Y boundaries
        if (fishBounds.min.y < areaBounds.min.y + boundaryPadding)
        {
            currentDirection.y = Mathf.Abs(currentDirection.y);
        }
        else if (fishBounds.max.y > areaBounds.max.y - boundaryPadding)
        {
            currentDirection.y = -Mathf.Abs(currentDirection.y);
        }

        // Normalize direction
        currentDirection.Normalize();
    }

    private void OnDestroy()
    {
        // Notify spawner when fish is destroyed
        FishSpawner spawner = FindObjectOfType<FishSpawner>();
        if (spawner != null)
        {
            spawner.OnFishDestroyed();
        }
    }
} 