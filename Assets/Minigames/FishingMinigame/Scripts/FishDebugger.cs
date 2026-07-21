using UnityEngine;

public class FishDebugger : MonoBehaviour
{
    private void Start()
    {
        // Check if fish has required components
        FishHealth health = GetComponent<FishHealth>();
        if (health == null)
        {
            Debug.LogError($"Fish {gameObject.name} is missing FishHealth component!");
        }

        // Check if fish has collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogError($"Fish {gameObject.name} is missing Collider2D component!");
        }

        // Check if fish has correct tag
        if (!gameObject.CompareTag("Fish"))
        {
            Debug.LogError($"Fish {gameObject.name} does not have the 'Fish' tag!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug when fish enters any trigger
        Debug.Log($"Fish {gameObject.name} entered trigger with {other.gameObject.name}");
        
        // Check if it's the hook
        HookAttacking hook = other.GetComponent<HookAttacking>();
        if (hook != null)
        {
            Debug.Log($"Fish {gameObject.name} collided with hook!");
        }
    }
} 