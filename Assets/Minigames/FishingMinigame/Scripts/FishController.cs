using UnityEngine;

public class FishController : MonoBehaviour
{
    public float speed = 2f;
    private Vector2 direction;
    private bool isCaught = false;

    void Start()
    {
        // Randomly choose left or right
        direction = Random.value > 0.5f ? Vector2.left : Vector2.right;
        // Flip sprite if needed
        if (direction.x < 0 && GetComponent<SpriteRenderer>())
            GetComponent<SpriteRenderer>().flipX = true;
    }

    void Update()
    {
        if (!isCaught)
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    public void CatchFish(Transform hook)
    {
        isCaught = true;
        transform.SetParent(hook);
        transform.localPosition = Vector3.zero;
        // Optionally disable collider, animation, etc.
        if (GetComponent<Collider2D>())
            GetComponent<Collider2D>().enabled = false;
    }
} 