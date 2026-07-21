using UnityEngine;

public class HookAttacking : MonoBehaviour
{
    public int damage = 1;
    public string targetTag = "Fish";
    public float attackCoolDown = 0.7f;
    private float attacktimer = 0.7f;
    public bool isRetracting;

    private void Start()
    {
        attacktimer = attackCoolDown;
    }

    private void Update()
    {
        attacktimer -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag(targetTag) && attacktimer <= 0f && !isRetracting)
        {
            attacktimer = attackCoolDown;
            FishHealth fishHealth = collision.GetComponent<FishHealth>();
            if (fishHealth != null)
            {
                fishHealth.dealDamage(damage);
                Debug.Log("Hit a fish");
            }
        }
    }
} 