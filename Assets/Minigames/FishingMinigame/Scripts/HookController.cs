using UnityEngine;

public class HookController : MonoBehaviour
{
    public FishingLineController fishingLineController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        FishController fish = other.GetComponent<FishController>();
        if (fish != null)
        {
            fish.CatchFish(transform);
            if (fishingLineController != null)
            {
                fishingLineController.OnFishCaught(fish.gameObject);
            }
        }
    }
} 