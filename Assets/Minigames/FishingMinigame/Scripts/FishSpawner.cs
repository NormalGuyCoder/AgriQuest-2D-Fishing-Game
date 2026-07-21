using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish Settings")]
    public GameObject[] fishPrefabs;
    public float spawnInterval = 2f;
    public int minFishCount = 3;
    public int maxFishCount = 8;
    
    [Header("Spawn Area")]
    public float minY = -2f, maxY = -18f;
    public float minX = -8f, maxX = 8f;
    public BoxCollider2D spawnArea;

    private int currentFishCount = 0;

    private void Start()
    {
        // Validate fish prefabs
        if (fishPrefabs == null || fishPrefabs.Length == 0)
        {
            Debug.LogError("No fish prefabs assigned to FishSpawner! Please assign fish prefabs in the inspector.");
            return;
        }

        // Validate spawn area
        if (spawnArea == null)
        {
            Debug.LogError("No spawn area BoxCollider2D assigned! Please assign a BoxCollider2D in the inspector.");
            return;
        }

        // Initial spawn
        SpawnInitialFish();
        
        // Start periodic spawning
        InvokeRepeating(nameof(CheckAndSpawnFish), 1f, spawnInterval);
    }

    private void SpawnInitialFish()
    {
        int initialCount = Random.Range(minFishCount, maxFishCount + 1);
        for (int i = 0; i < initialCount; i++)
        {
            SpawnFish();
        }
    }

    private void CheckAndSpawnFish()
    {
        if (currentFishCount < minFishCount)
        {
            int fishToSpawn = Mathf.Min(maxFishCount - currentFishCount, minFishCount - currentFishCount);
            for (int i = 0; i < fishToSpawn; i++)
            {
                SpawnFish();
            }
        }
    }

    void SpawnFish()
    {
        // Check if we have fish prefabs
        if (fishPrefabs == null || fishPrefabs.Length == 0)
        {
            Debug.LogWarning("Cannot spawn fish: No fish prefabs assigned!");
            return;
        }

        // Get random position within spawn area bounds
        Vector2 spawnPosition = GetRandomPositionInBounds();

        // Select random fish prefab
        int fishIndex = Random.Range(0, fishPrefabs.Length);
        GameObject fishPrefab = fishPrefabs[fishIndex];

        // Check if the selected prefab is valid
        if (fishPrefab == null)
        {
            Debug.LogWarning($"Fish prefab at index {fishIndex} is null!");
            return;
        }

        // Spawn the fish
        GameObject fish = Instantiate(fishPrefab, spawnPosition, Quaternion.identity);
        fish.tag = "Fish"; // Ensure the fish has the correct tag

        // Add FishMovement component if it doesn't exist
        if (fish.GetComponent<FishMovement>() == null)
        {
            FishMovement movement = fish.AddComponent<FishMovement>();
            movement.Initialize(spawnArea);
        }

        currentFishCount++;
    }

    private Vector2 GetRandomPositionInBounds()
    {
        if (spawnArea != null)
        {
            Bounds bounds = spawnArea.bounds;
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            return new Vector2(x, y);
        }
        else
        {
            // Fallback to manual bounds if no collider
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            return new Vector2(x, y);
        }
    }

    public void OnFishDestroyed()
    {
        currentFishCount--;
    }

    private void OnValidate()
    {
        // Validate spawn area
        if (minY > maxY)
        {
            float temp = minY;
            minY = maxY;
            maxY = temp;
        }
        if (minX > maxX)
        {
            float temp = minX;
            minX = maxX;
            maxX = temp;
        }
        
        // Validate fish count
        if (minFishCount > maxFishCount)
        {
            int temp = minFishCount;
            minFishCount = maxFishCount;
            maxFishCount = temp;
        }
    }
} 